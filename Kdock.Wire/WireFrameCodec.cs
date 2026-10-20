using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Kdock.Wire;

public static class WireFrameCodec
{
    public const int MaximumPayloadLength = 20 * 1024 * 1024;

    public static byte[] Encode(WirePacket packet)
    {
        byte[] instruction = Encoding.UTF8.GetBytes(packet.Instruction ?? string.Empty);
        byte[] data = packet.Data ?? Array.Empty<byte>();
        int payloadLength = 1 + sizeof(int) + sizeof(int) + instruction.Length + data.Length;
        byte[] frame = new byte[sizeof(int) + payloadLength];
        BinaryPrimitives.WriteInt32LittleEndian(frame, payloadLength);
        frame[4] = (byte)packet.Type;
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(5), instruction.Length);
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(9), data.Length);
        instruction.CopyTo(frame.AsSpan(13));
        data.CopyTo(frame.AsSpan(13 + instruction.Length));
        return frame;
    }

    public static bool TryDecode(ReadOnlySpan<byte> payload, out WirePacket packet)
    {
        packet = null!;
        if (payload.Length < 9)
            return false;

        WirePacketType type = (WirePacketType)payload[0];
        int instructionLength = BinaryPrimitives.ReadInt32LittleEndian(payload[1..]);
        int dataLength = BinaryPrimitives.ReadInt32LittleEndian(payload[5..]);
        if (instructionLength < 0 || dataLength < 0 || instructionLength > payload.Length - 9 || dataLength > payload.Length - 9 - instructionLength)
            return false;

        string instruction = Encoding.UTF8.GetString(payload.Slice(9, instructionLength));
        byte[] data = payload.Slice(9 + instructionLength, dataLength).ToArray();
        packet = new WirePacket(type, instruction, data);
        return true;
    }

    public static async System.Threading.Tasks.Task<WirePacket?> ReadAsync(Stream stream, System.Threading.CancellationToken cancellationToken)
    {
        byte[] lengthBuffer = new byte[4];
        if (!await ReadExactAsync(stream, lengthBuffer, cancellationToken).ConfigureAwait(false))
            return null;

        int length = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
        if (length <= 0 || length > MaximumPayloadLength)
            throw new InvalidDataException($"Invalid Kdock packet length: {length}.");

        byte[] payload = new byte[length];
        if (!await ReadExactAsync(stream, payload, cancellationToken).ConfigureAwait(false))
            return null;
        return TryDecode(payload, out WirePacket packet) ? packet : throw new InvalidDataException("Invalid Kdock packet.");
    }

    private static async System.Threading.Tasks.Task<bool> ReadExactAsync(Stream stream, byte[] buffer, System.Threading.CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken).ConfigureAwait(false);
            if (read == 0)
                return false;
            offset += read;
        }
        return true;
    }
}
