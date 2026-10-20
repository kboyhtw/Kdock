using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Kdock
{
    public class FileReceiver
    {
        private FileStream _stream;
        private long _received;
        private long _expected;
        public string ID { get; private set; }
        public string SavedPath { get; private set; }
        public bool IsFinished { get; private set; }

        public event Action<long> ProgressUpdated;
        public event Action Completed;

        public void Start(NetworkPackage np, string directory)
        {
            var parts = np.Instruction.Split(':');
            ID = parts[1];
            var name = parts[2];
            _expected = long.Parse(parts[3]);
            _received = 0;

            Directory.CreateDirectory(directory);
            SavedPath = Path.Combine(directory, name);
            _stream = new FileStream(SavedPath, FileMode.Create);
            Write(np.Data);
        }

        public void Abort()
        {
            IsFinished = true;
            try
            {
                if (_stream != null)
                    _stream.Close();
            }
            catch
            {
            }
            try
            {
                if (!string.IsNullOrEmpty(SavedPath) && File.Exists(SavedPath))
                    File.Delete(SavedPath);
            }
            catch
            {
            }
        }

        public void Write(byte[] chunk)
        {
            if (IsFinished) return;
            if (chunk != null && chunk.Length > 0)
            {
                _stream.Write(chunk, 0, chunk.Length);
                _received += chunk.Length;
                if (ProgressUpdated != null) ProgressUpdated(_received);
            }

            if (_received >= _expected)
            {
                IsFinished = true;
                _stream.Flush();
                _stream.Close();
                if (Completed != null) Completed();
            }
        }
    }
}
