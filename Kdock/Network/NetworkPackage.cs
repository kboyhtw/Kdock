using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public class NetworkPackage
    {
        public PackageType Type { get; private set; }
        public string Instruction { get; private set; }
        public byte[] Data { get; private set; }

        public NetworkPackage(PackageType type, string instruction, byte[] data)
        {
            Type = type;
            Instruction = instruction ?? "";
            Data = data ?? new byte[0];
        }

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)Type);
                bw.Write(Encoding.UTF8.GetByteCount(Instruction));
                bw.Write(Data.Length);
                bw.Write(Encoding.UTF8.GetBytes(Instruction));
                bw.Write(Data);
                return ms.ToArray();
            }
        }

        public byte[] ToFrame()
        {
            byte[] payload = ToBytes();
            byte[] frame = new byte[4 + payload.Length];
            Buffer.BlockCopy(BitConverter.GetBytes(payload.Length), 0, frame, 0, 4);
            Buffer.BlockCopy(payload, 0, frame, 4, payload.Length);
            return frame;
        }

        public static NetworkPackage FromBytes(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
            {
                var type = (PackageType)br.ReadByte();
                var instrLen = br.ReadInt32();
                var dataLen = br.ReadInt32();
                var instruction = Encoding.UTF8.GetString(br.ReadBytes(instrLen));
                var data = br.ReadBytes(dataLen);
                return new NetworkPackage(type, instruction, data);
            }
        }
    }
    public enum PackageType
    {
        Binary,
        String,
        Image
    }


    public class PackageAssembler
    {
        private MemoryStream _buffer = new MemoryStream();
        public event Action<NetworkPackage> PackageCompleted;

        public void FeedBytes(byte[] incoming)
        {
            if (incoming == null || incoming.Length == 0) return;
            _buffer.Write(incoming, 0, incoming.Length);
            TryAssemble();
        }

        private void TryAssemble()
        {
            while (true)
            {
                byte[] all = _buffer.ToArray();
                if (all.Length < 4) return;

                int length = BitConverter.ToInt32(all, 0);
                if (length <= 0 || length > 20 * 1024 * 1024)
                {
                    _buffer.SetLength(0);
                    return;
                }
                if (all.Length < 4 + length) return;

                byte[] payload = new byte[length];
                Buffer.BlockCopy(all, 4, payload, 0, length);

                _buffer.SetLength(0);
                int remain = all.Length - (4 + length);
                if (remain > 0)
                    _buffer.Write(all, 4 + length, remain);

                try
                {
                    NetworkPackage pkg = NetworkPackage.FromBytes(payload);
                    if (PackageCompleted != null) PackageCompleted(pkg);
                }
                catch
                {
                }
            }
        }
    }
}

