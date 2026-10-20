using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kdock
{
    public static class DeskBroker
    {
        private static readonly Dictionary<string, DeskLink> Online = new Dictionary<string, DeskLink>();
        private static readonly Dictionary<string, PairSlot> Waiting = new Dictionary<string, PairSlot>();
        private static readonly Dictionary<string, PairCode> PairCodes = new Dictionary<string, PairCode>();
        private static readonly object Gate = new object();
        private static int _started;

        public static bool IsHosted()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_IIS_PHYSICAL_PATH"))
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APP_POOL_ID"));
        }

        public static bool Ensure(int port)
        {
            if (port < 1 || port > 65535) port = 12605;
            if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
                return true;
            try
            {
                WebApplicationOptions options = new WebApplicationOptions
                {
                    Args = new string[0],
                    ContentRootPath = AppContext.BaseDirectory
                };
                WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
                builder.Logging.ClearProviders();
                if (!IsHosted() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
                    builder.WebHost.UseUrls("http://0.0.0.0:" + port);
                WebApplication app = builder.Build();
                app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(20) });
                Map(app);
                Task.Run(delegate () { app.StartAsync().GetAwaiter().GetResult(); }).GetAwaiter().GetResult();
                return true;
            }
            catch
            {
                Interlocked.Exchange(ref _started, 0);
                return false;
            }
        }

        public static void RunHosted(string[] args)
        {
            if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
                return;
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args ?? new string[0]);
            builder.Logging.ClearProviders();
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
            WebApplication app = builder.Build();
            app.UseForwardedHeaders();
            app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(20) });
            Map(app);
            Console.WriteLine("Kdock meeting API is hosted.");
            app.Run();
        }

        public static void Run(int port)
        {
            if (IsHosted())
            {
                RunHosted(Environment.GetCommandLineArgs());
                return;
            }
            if (port < 1 || port > 65535) port = 12605;
            if (!Ensure(port))
            {
                Console.Error.WriteLine("Could not listen on port " + port + ".");
                return;
            }
            Console.WriteLine("Kdock meeting API is listening.");
            Console.WriteLine("http://0.0.0.0:" + port + "/api/health");
            Console.WriteLine("Leave this process running. Both PCs use this machine's address and port " + port + ".");
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Map(WebApplication app)
        {
            app.MapGet("/", () => Results.Text(
                "Kdock meeting API\r\nGET /api/health\r\nGET /api/online/{id}\r\nPOST /api/hello {\"id\"}\r\nPOST /api/call {\"fromId\",\"targetId\"}\r\nWS /api/events?id=\r\nWS /api/join/{token}\r\n",
                "text/plain"));
            app.MapGet("/api/health", () =>
            {
                string[] tails;
                lock (Gate)
                    tails = Online.Where(item => item.Value.IsOpen).Select(item => Tail(item.Key)).ToArray();
                return Json("{\"status\":\"ok\",\"sockets\":" + tails.Length + ",\"ids\":[" + string.Join(",", tails.Select(item => "\"" + item + "\"")) + "]}");
            });
            app.MapGet("/api/version", () => Json("{\"protocol\":2,\"product\":\"1.0.0\",\"packageUrl\":\"\"}"));
            app.MapGet("/api/online/{id}", (string id) =>
            {
                string desk = Digits(id);
                bool online;
                lock (Gate)
                    online = Online.TryGetValue(desk, out DeskLink link) && link.IsOpen;
                return Json(online ? "{\"online\":true}" : "{\"online\":false}");
            });
            app.MapPost("/api/hello", async (HttpRequest request) =>
            {
                string id = ReadString(await ReadJson(request), "id");
                if (string.IsNullOrEmpty(id))
                    return Results.Text("{\"error\":\"id required\"}", "application/json", Encoding.UTF8, 400);
                return Json("{\"status\":\"ready\",\"id\":\"" + id + "\"}");
            });
            app.MapPost("/api/pair/ask", async (HttpRequest request) =>
            {
                JsonElement body = await ReadJson(request);
                string fromId = Digits(ReadString(body, "fromId"));
                string targetId = Digits(ReadString(body, "targetId"));
                if (fromId.Length == 0 || targetId.Length == 0)
                    return Results.Text("{\"error\":\"fromId and targetId are required\"}", "application/json", Encoding.UTF8, 400);
                string code = Random.Shared.Next(0, 1000000).ToString("D6");
                lock (Gate)
                    PairCodes[targetId + "|" + fromId] = new PairCode(code, DateTime.UtcNow.AddMinutes(10));
                byte[] message = Encoding.UTF8.GetBytes("{\"type\":\"pair\",\"fromId\":\"" + fromId + "\",\"code\":\"" + code + "\"}");
                return Push(targetId, message) ? Json("{\"status\":\"sent\"}") : Offline(targetId);
            });
            app.MapPost("/api/pair/register", async (HttpRequest request) =>
            {
                JsonElement body = await ReadJson(request);
                string fromId = Digits(ReadString(body, "fromId"));
                string targetId = Digits(ReadString(body, "targetId"));
                string code = ReadString(body, "code");
                if (string.IsNullOrEmpty(fromId) || string.IsNullOrEmpty(targetId) || code.Length != 6)
                    return Results.Text("{\"error\":\"fromId, targetId, and a six-digit code are required\"}", "application/json", Encoding.UTF8, 400);

                lock (Gate)
                    PairCodes[targetId + "|" + fromId] = new PairCode(code, DateTime.UtcNow.AddMinutes(10));
                return Json("{\"status\":\"registered\"}");
            });
            app.MapPost("/api/pair", async (HttpRequest request) =>
            {
                JsonElement body = await ReadJson(request);
                string fromId = Digits(ReadString(body, "fromId"));
                string targetId = Digits(ReadString(body, "targetId"));
                string code = ReadString(body, "code");
                bool valid = false;
                lock (Gate)
                {
                    string key = targetId + "|" + fromId;
                    PairCode slot;
                    if (PairCodes.TryGetValue(key, out slot))
                    {
                        valid = slot.ExpiresUtc > DateTime.UtcNow && slot.Code == code;
                        if (valid || slot.ExpiresUtc <= DateTime.UtcNow)
                            PairCodes.Remove(key);
                    }
                }
                return valid
                    ? Json("{\"status\":\"paired\"}")
                    : Results.Text("{\"error\":\"invalid or expired code\"}", "application/json", Encoding.UTF8, 403);
            });
            app.MapPost("/api/call", async (HttpRequest request) =>
            {
                JsonElement body = await ReadJson(request);
                string fromId = Digits(ReadString(body, "fromId"));
                string targetId = Digits(ReadString(body, "targetId"));
                string code = ReadString(body, "code");
                if (!string.IsNullOrEmpty(code))
                {
                    bool valid = false;
                    lock (Gate)
                    {
                        string key = targetId + "|" + fromId;
                        PairCode slot;
                        if (PairCodes.TryGetValue(key, out slot))
                        {
                            valid = slot.ExpiresUtc > DateTime.UtcNow && slot.Code == code;
                            if (valid || slot.ExpiresUtc <= DateTime.UtcNow)
                                PairCodes.Remove(key);
                        }
                    }
                    if (!valid)
                        return Results.Text("{\"error\":\"invalid or expired code\"}", "application/json", Encoding.UTF8, 403);
                }
                string token = Guid.NewGuid().ToString("N");
                byte[] ring = Encoding.UTF8.GetBytes("{\"type\":\"ring\",\"token\":\"" + token + "\",\"fromId\":\"" + fromId + "\"}");
                return Push(targetId, ring) ? Json("{\"token\":\"" + token + "\"}") : Offline(targetId);
            });
            app.Map("/api/events", Events);
            app.Map("/api/join/{token}", Join);
        }

        private static async Task Events(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }
            string id = Digits(context.Request.Query["id"].ToString());
            if (string.IsNullOrEmpty(id))
            {
                context.Response.StatusCode = 400;
                return;
            }
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            DeskLink link = new DeskLink(socket);
            lock (Gate)
                Online[id] = link;
            CancellationToken stop = context.RequestAborted;
            Task send = SendLoop(link, stop);
            try
            {
                link.Enqueue(Encoding.UTF8.GetBytes("{\"type\":\"ready\"}"));
                byte[] buffer = new byte[256];
                while (socket.State == WebSocketState.Open && !stop.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), stop);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                }
            }
            catch
            {
            }
            finally
            {
                link.Close();
                lock (Gate)
                {
                    DeskLink current;
                    if (Online.TryGetValue(id, out current) && current == link)
                        Online.Remove(id);
                }
                try { await send; } catch { }
                try { socket.Abort(); } catch { }
            }
        }

        private static async Task Join(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }
            string token = context.Request.RouteValues["token"] as string;
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 400;
                return;
            }
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            PairSlot slot;
            bool first;
            lock (Gate)
            {
                if (!Waiting.TryGetValue(token, out slot))
                {
                    slot = new PairSlot();
                    slot.Socket = socket;
                    Waiting[token] = slot;
                    first = true;
                }
                else
                {
                    Waiting.Remove(token);
                    first = false;
                }
            }
            try
            {
                if (first)
                {
                    TaskCompletionSource<bool> closed = new TaskCompletionSource<bool>();
                    context.RequestAborted.Register(delegate () { closed.TrySetResult(false); });
                    await Task.WhenAny(slot.Done.Task, closed.Task);
                    if (!slot.Done.Task.IsCompleted)
                    {
                        lock (Gate)
                        {
                            PairSlot current;
                            if (Waiting.TryGetValue(token, out current) && current == slot)
                                Waiting.Remove(token);
                        }
                    }
                    else
                        await slot.Done.Task;
                }
                else
                {
                    await Pump(slot.Socket, socket);
                    slot.Done.TrySetResult(true);
                }
            }
            catch
            {
                slot.Done.TrySetResult(false);
            }
        }

        private static async Task Pump(WebSocket left, WebSocket right)
        {
            Task forward = Copy(left, right);
            Task backward = Copy(right, left);
            await Task.WhenAny(forward, backward);
            try { left.Abort(); } catch { }
            try { right.Abort(); } catch { }
        }

        private static async Task Copy(WebSocket from, WebSocket to)
        {
            byte[] buffer = new byte[32 * 1024];
            try
            {
                while (from.State == WebSocketState.Open && to.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await from.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close || result.Count == 0)
                        break;
                    await to.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Binary, result.EndOfMessage, CancellationToken.None);
                }
            }
            catch
            {
            }
        }

        private static bool Push(string targetId, byte[] message)
        {
            DeskLink link;
            lock (Gate)
            {
                if (!Online.TryGetValue(targetId ?? "", out link) || !link.IsOpen)
                    return false;
            }
            return link.Enqueue(message);
        }

        private static IResult Offline(string targetId)
        {
            string[] have;
            lock (Gate)
                have = Online.Where(item => item.Value.IsOpen).Select(item => Tail(item.Key)).ToArray();
            return Results.Text(
                "{\"error\":\"offline\",\"want\":\"" + Tail(targetId) + "\",\"have\":[" + string.Join(",", have.Select(item => "\"" + item + "\"")) + "]}",
                "application/json",
                Encoding.UTF8,
                404);
        }

        private static async Task SendLoop(DeskLink link, CancellationToken cancellationToken)
        {
            try
            {
                await foreach (byte[] message in link.Outbox.Reader.ReadAllAsync(cancellationToken))
                {
                    if (link.Socket.State != WebSocketState.Open)
                        break;
                    await link.Socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, cancellationToken);
                }
            }
            catch
            {
            }
        }

        private static async Task<JsonElement> ReadJson(HttpRequest request)
        {
            request.EnableBuffering();
            using (System.IO.StreamReader reader = new System.IO.StreamReader(request.Body, Encoding.UTF8, false, 1024, true))
            {
                string text = await reader.ReadToEndAsync();
                try { request.Body.Position = 0; } catch { }
                if (string.IsNullOrWhiteSpace(text))
                    return default(JsonElement);
                try
                {
                    using (JsonDocument document = JsonDocument.Parse(text))
                        return document.RootElement.Clone();
                }
                catch (JsonException)
                {
                    return default(JsonElement);
                }
            }
        }

        private static string Digits(string value) => new string((value ?? "").Where(char.IsDigit).ToArray());

        private static string Tail(string id)
        {
            string digits = Digits(id);
            return digits.Length <= 4 ? digits : digits.Substring(digits.Length - 4);
        }

        private static string ReadString(JsonElement body, string name)
        {
            if (body.ValueKind != JsonValueKind.Object)
                return "";
            foreach (JsonProperty property in body.EnumerateObject())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return property.Value.GetString() ?? "";
            }
            return "";
        }

        private static IResult Json(string json)
        {
            return Results.Text(json, "application/json", Encoding.UTF8);
        }

        private sealed class DeskLink
        {
            public DeskLink(WebSocket socket)
            {
                Socket = socket;
            }

            public WebSocket Socket { get; }
            public Channel<byte[]> Outbox { get; } = Channel.CreateUnbounded<byte[]>();
            public bool IsOpen => Socket != null && Socket.State == WebSocketState.Open;

            public bool Enqueue(byte[] message)
            {
                return Outbox.Writer.TryWrite(message);
            }

            public void Close()
            {
                try { Outbox.Writer.TryComplete(); } catch { }
            }
        }

        private sealed class PairSlot
        {
            public WebSocket Socket;
            public readonly TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();
        }

        private sealed class PairCode
        {
            public PairCode(string code, DateTime expiresUtc)
            {
                Code = code;
                ExpiresUtc = expiresUtc;
            }

            public string Code { get; }
            public DateTime ExpiresUtc { get; }
        }
    }
}
