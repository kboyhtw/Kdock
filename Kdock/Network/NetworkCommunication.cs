using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kdock
{
    public class NetworkCommunication
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private PackageAssembler _assembler = new PackageAssembler();

        public string Tag { get; internal set; }

        public event Action<NetworkPackage> PackageReceived;
        public event Action<string> StatusUpdated;

        public async Task StartAsync(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            _assembler.PackageCompleted += delegate (NetworkPackage pkg)
            {
                if (PackageReceived != null) PackageReceived(pkg);
            };

            if (StatusUpdated != null) StatusUpdated("Connected.");
            await ListenAsync(_cts.Token);
        }
        private async Task ListenAsync(CancellationToken token)
        {
            byte[] buffer = new byte[64 * 1024];

            try
            {
                while (!token.IsCancellationRequested && _client != null && _client.Connected)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read <= 0)
                    {
                        if (StatusUpdated != null) StatusUpdated("Remote client disconnected.");
                        break;
                    }

                    byte[] incoming = new byte[read];
                    Array.Copy(buffer, incoming, read);
                    _assembler.FeedBytes(incoming);
                }
            }
            catch (Exception ex)
            {
                if (StatusUpdated != null) StatusUpdated("Listener error: " + ex.Message);
            }

            if (StatusUpdated != null) StatusUpdated("Listener exited.");
        }


        private async Task<byte[]> ReadExactAsync(NetworkStream stream, int length, CancellationToken token)
        {
            byte[] buffer = new byte[length];
            int offset = 0;
            int timeoutMs = 5000;
            CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeoutCts.CancelAfter(timeoutMs);

            try
            {
                while (offset < length)
                {
                    int read = await stream.ReadAsync(buffer, offset, length - offset, timeoutCts.Token);
                    if (read == 0)
                        return null; // Remote closed
                    offset += read;
                }
                return buffer;
            }
            catch (OperationCanceledException)
            {
                StatusUpdated?.Invoke("Read timeout after " + timeoutMs + "ms");
                return null;
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke("Read error: " + ex.Message);
                return null;
            }
        }



        public async Task SendAsync(NetworkPackage package)
        {
            if (_stream != null && _stream.CanWrite)
            {
                var bytes = package.ToFrame();
                await _stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            if (_stream != null) _stream.Dispose();
            if (_client != null) _client.Close();
        }
    }
}
