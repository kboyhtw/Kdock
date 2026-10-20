using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public sealed class MeetingClient : IDisposable
{
    private readonly HttpClient _httpClient = CreateHttp();

    private static HttpClient CreateHttp()
    {
        SocketsHttpHandler handler = new()
        {
            UseProxy = false,
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(10),
            Expect100ContinueTimeout = TimeSpan.Zero,
            AllowAutoRedirect = true
        };
        HttpClient client = new(handler)
        {
            Timeout = TimeSpan.FromSeconds(20),
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };
        client.DefaultRequestHeaders.ExpectContinue = false;
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Kdock/1.0");
        client.DefaultRequestHeaders.ConnectionClose = false;
        return client;
    }
    private readonly string _serverPath;
    private readonly CancellationTokenSource _eventsStop = new();
    private ClientWebSocket? _eventsSocket;
    private Task? _eventsTask;

    public MeetingClient(string dataDirectory)
    {
        _serverPath = Path.Combine(dataDirectory, "server.txt");
        ServerAddress = LoadServerAddress();
    }

    public void StartCallEvents(string deskId, Func<IncomingCall, Task> onCall, Func<IncomingPair, Task>? onPair = null)
    {
        if (_eventsTask != null)
            return;
        _eventsTask = EventsLoopAsync(deskId, onCall, onPair, _eventsStop.Token);
    }

    public async Task<string?> AskPairErrorAsync(string fromId, string targetId, CancellationToken cancellationToken = default)
    {
        fromId = Digits(fromId);
        targetId = Digits(targetId);
        try
        {
            using HttpResponseMessage response = await PostJsonAsync("/api/pair/ask", new { fromId, targetId }, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return null;
            return await ReadErrorAsync(response, "That ID is not in the meeting (HTTP " + (int)response.StatusCode + "). Both PCs must show Connected and stay open.").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string path, object body, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        ByteArrayContent content = new(Encoding.UTF8.GetBytes(json));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using HttpRequestMessage request = new(HttpMethod.Post, new Uri(GetBaseUri(), path))
        {
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
            Content = content
        };
        request.Headers.ExpectContinue = false;
        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static string Digits(string value) => new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static void Observe(Task task)
    {
        task.ContinueWith(static completed => _ = completed.Exception, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task EventsLoopAsync(string deskId, Func<IncomingCall, Task> onCall, Func<IncomingPair, Task>? onPair, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Uri baseUri = GetBaseUri();
                UriBuilder builder = new(baseUri)
                {
                    Scheme = baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws",
                    Path = "/api/events",
                    Query = "id=" + Uri.EscapeDataString(Digits(deskId))
                };
                _eventsSocket = CreateSocket();
                await _eventsSocket.ConnectAsync(builder.Uri, cancellationToken).ConfigureAwait(false);
                SetServerConnected(true);
                byte[] buffer = new byte[16 * 1024];
                while (_eventsSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    using MemoryStream message = new();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _eventsSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                            break;
                        message.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    using JsonDocument document = JsonDocument.Parse(message.ToArray());
                    JsonElement root = document.RootElement;
                    if (!root.TryGetProperty("type", out JsonElement type))
                        continue;
                    string kind = type.GetString() ?? string.Empty;
                    if (kind == "ring" && root.TryGetProperty("token", out JsonElement token) && root.TryGetProperty("fromId", out JsonElement fromId))
                        Observe(onCall(new IncomingCall(token.GetString() ?? string.Empty, fromId.GetString() ?? string.Empty)));
                    else if (kind == "pair" && onPair != null && root.TryGetProperty("fromId", out JsonElement pairFrom))
                    {
                        string code = root.TryGetProperty("code", out JsonElement codeEl) ? codeEl.GetString() ?? string.Empty : string.Empty;
                        Observe(onPair(new IncomingPair(pairFrom.GetString() ?? string.Empty, code)));
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception)
            {
            }
            finally
            {
                SetServerConnected(false);
                try { _eventsSocket?.Abort(); } catch { }
                _eventsSocket?.Dispose();
                _eventsSocket = null;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    public Task<MeetingSessionTransport> JoinSessionAsync(string token, CancellationToken cancellationToken = default)
    {
        Uri baseUri = GetBaseUri();
        UriBuilder builder = new(baseUri)
        {
            Scheme = baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws",
            Path = $"/api/join/{Uri.EscapeDataString(token)}"
        };
        return MeetingSessionTransport.ConnectAsync(builder.Uri, cancellationToken);
    }

    public async Task<MeetingVersion?> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MeetingVersion>($"{GetBaseUri()}/api/version", cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<bool> RegisterPairCodeAsync(string fromId, string targetId, string code, CancellationToken cancellationToken = default)
    {
        return await PostPairEndpointAsync("register", fromId, targetId, code, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> PairAsync(string fromId, string targetId, string code, CancellationToken cancellationToken = default)
    {
        return await PostPairEndpointAsync(string.Empty, fromId, targetId, code, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> CallAsync(string fromId, string targetId, string? code = null, CancellationToken cancellationToken = default)
    {
        return (await CallDetailedAsync(fromId, targetId, code, cancellationToken).ConfigureAwait(false)).Token;
    }

    public async Task<(string? Token, string? Error)> CallDetailedAsync(string fromId, string targetId, string? code = null, CancellationToken cancellationToken = default)
    {
        fromId = Digits(fromId);
        targetId = Digits(targetId);
        try
        {
            using HttpResponseMessage response = await PostJsonAsync("/api/call", new { fromId, targetId, code }, cancellationToken).ConfigureAwait(false);
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return (null, ParseError(body, response.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "The other PC is not in the meeting. Open Kdock on that PC and wait until Meeting server says Connected."
                    : "The meeting server refused the call. Check the 6-digit code and try again."));
            using JsonDocument document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            string? token = document.RootElement.TryGetProperty("token", out JsonElement value) ? value.GetString() : null;
            return string.IsNullOrWhiteSpace(token) ? (null, "The meeting server did not return a call token.") : (token, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, string fallback)
    {
        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return ParseError(body, fallback);
    }

    private static string ParseError(string body, string fallback)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            if (document.RootElement.TryGetProperty("error", out JsonElement error))
            {
                string text = error.GetString() ?? string.Empty;
                string want = document.RootElement.TryGetProperty("want", out JsonElement wantEl) ? wantEl.GetString() ?? "" : "";
                string have = "";
                if (document.RootElement.TryGetProperty("have", out JsonElement haveEl) && haveEl.ValueKind == JsonValueKind.Array)
                    have = string.Join(", ", haveEl.EnumerateArray().Select(item => item.GetString()).Where(item => !string.IsNullOrEmpty(item)));
                if (text.Equals("offline", StringComparison.OrdinalIgnoreCase))
                    return "The other PC is not in the meeting."
                        + (want.Length > 0 ? " Looking for ID ending " + want + "." : "")
                        + (have.Length > 0 ? " Meeting currently has IDs ending " + have + "." : " Open Kdock on that PC and wait until Meeting server says Connected.");
                if (text.Length > 0)
                    return text;
            }
        }
        catch (JsonException)
        {
        }
        return fallback;
    }

    private static ClientWebSocket CreateSocket()
    {
        ClientWebSocket socket = new();
        socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
        socket.Options.HttpVersion = HttpVersion.Version11;
        socket.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        return socket;
    }

    private async Task<bool> PostPairEndpointAsync(string suffix, string fromId, string targetId, string code, CancellationToken cancellationToken)
    {
        try
        {
            string endpoint = string.IsNullOrEmpty(suffix) ? "/api/pair" : $"/api/pair/{suffix}";
            using HttpResponseMessage response = await PostJsonAsync(endpoint, new { fromId = Digits(fromId), targetId = Digits(targetId), code }, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public bool IsServerConnected { get; private set; }
    public event Action<bool>? ServerConnectionChanged;
    public string ServerAddress { get; private set; }

    public void SetServerAddress(string value)
    {
        string normalized = NormalizeServerAddress(value);
        bool changed = !string.Equals(ServerAddress, normalized, StringComparison.OrdinalIgnoreCase);
        ServerAddress = normalized;
        File.WriteAllText(_serverPath, normalized);
        if (changed)
        {
            SetServerConnected(false);
            try { _eventsSocket?.Abort(); } catch { }
        }
    }

public sealed class MeetingVersion
{
    public int Protocol { get; set; }
    public string Product { get; set; } = string.Empty;
    public string PackageUrl { get; set; } = string.Empty;
}

    public async Task<bool> IsOnlineAsync(string deskId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deskId))
            return false;

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(
                $"{GetBaseUri()}/api/online/{Uri.EscapeDataString(deskId)}",
                cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return false;

            using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
            return document.RootElement.TryGetProperty("online", out JsonElement online) && online.ValueKind == JsonValueKind.True;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync($"{GetBaseUri()}/api/health", cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    private void SetServerConnected(bool connected)
    {
        if (IsServerConnected == connected)
            return;
        IsServerConnected = connected;
        ServerConnectionChanged?.Invoke(connected);
    }

    private Uri GetBaseUri()
    {
        string raw = (ServerAddress ?? string.Empty).Trim().TrimEnd('/');
        if (raw.Contains("://", StringComparison.Ordinal))
            return new Uri(raw);
        return new Uri((UsesHttps(raw) ? "https://" : "http://") + raw);
    }

    private static bool UsesHttps(string address)
    {
        string host = address;
        int colon = address.LastIndexOf(':');
        if (colon > 0 && int.TryParse(address[(colon + 1)..], out _))
            host = address[..colon];
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || System.Net.IPAddress.TryParse(host, out _))
            return false;
        return host.Contains("runasp.net", StringComparison.OrdinalIgnoreCase)
            || host.Contains("tryasp.net", StringComparison.OrdinalIgnoreCase)
            || (host.Contains('.') && host.Any(char.IsLetter) && colon < 0);
    }

    private string LoadServerAddress()
    {
        if (File.Exists(_serverPath))
        {
            string value = File.ReadAllText(_serverPath).Trim();
            if (!string.IsNullOrWhiteSpace(value))
                return NormalizeServerAddress(value);
        }

        string defaultAddress = $"localhost:{Protocol.MeetingPort}";
        File.WriteAllText(_serverPath, defaultAddress);
        return defaultAddress;
    }

    private static string NormalizeServerAddress(string value)
    {
        string address = (value ?? string.Empty).Trim();
        if (address.Contains("://", StringComparison.Ordinal))
            address = new Uri(address).Authority;
        return string.IsNullOrWhiteSpace(address) ? $"localhost:{Protocol.MeetingPort}" : address.TrimEnd('/');
    }

    public void Dispose()
    {
        _eventsStop.Cancel();
        try { _eventsSocket?.Abort(); } catch { }
        try { _eventsTask?.GetAwaiter().GetResult(); } catch { }
        _eventsSocket?.Dispose();
        _eventsStop.Dispose();
        _httpClient.Dispose();
    }
}

public sealed record IncomingCall(string Token, string FromId);

public sealed record IncomingPair(string FromId, string Code);
