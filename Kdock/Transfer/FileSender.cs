using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kdock
{
    public class FileSender
    {
        public event Action<long> ProgressChanged;

        public async Task SendAsync(FileObject file, TcpClient client, CancellationToken token)
        {
            using (var fs = new FileStream(file.FullPath, FileMode.Open, FileAccess.Read))
            using (var stream = client.GetStream())
            {
                var buffer = new byte[8192];
                long remaining = fs.Length;

                while (remaining > 0 && !token.IsCancellationRequested)
                {
                    int chunkSize = (int)Math.Min(buffer.Length, remaining);
                    int read = await fs.ReadAsync(buffer, 0, chunkSize, token);
                    var chunk = new byte[read];
                    Array.Copy(buffer, chunk, read);

                    var np = new NetworkPackage(PackageType.Binary,
                        "File:" + file.ID + ":" + file.FileName + ":" + fs.Length.ToString(),
                        chunk);

                    await stream.WriteAsync(np.ToBytes(), 0, np.ToBytes().Length, token);
                    remaining -= read;
                    if (ProgressChanged != null) ProgressChanged(fs.Length - remaining);
                }
            }
        }
    }
}
