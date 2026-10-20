using System;
using System.Text;

namespace Kdock.Wire;

public enum WirePacketType
{
    Binary = 0,
    String = 1,
    Image = 2
}

public sealed record WirePacket(WirePacketType Type, string Instruction, byte[] Data)
{
    public static WirePacket Text(string instruction, string data = "")
    {
        return new WirePacket(WirePacketType.String, instruction, Encoding.UTF8.GetBytes(data ?? string.Empty));
    }

    public static WirePacket Binary(string instruction, byte[] data)
    {
        return new WirePacket(WirePacketType.Binary, instruction, data ?? Array.Empty<byte>());
    }
}
