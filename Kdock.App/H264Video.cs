using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Kdock.App;

public sealed class H264Encoder : IDisposable
{
    private static int _started;
    private IMFTransform? _transform;
    private ICodecApi? _codec;
    private byte[]? _header;
    private int _width;
    private int _height;
    private int _bitrate;
    private long _stamp;
    private bool _failed;
    public static string LastError { get; internal set; } = string.Empty;

    public bool TryEncode(Bitmap frame, int bitrate, bool keyframe, out byte[] packet)
    {
        packet = Array.Empty<byte>();
        if (_failed)
            return false;
        int width = frame.Width & ~1;
        int height = frame.Height & ~1;
        bitrate = Math.Clamp(bitrate, 400_000, 16_000_000);
        if (width < 2 || height < 2)
            return false;
        if (_transform == null || width != _width || height != _height || bitrate != _bitrate)
        {
            DisposeTransform();
            if (!Open(width, height, bitrate))
            {
                _failed = true;
                return false;
            }
            keyframe = true;
        }
        if (keyframe)
            ForceKey();
        byte[] nv12 = ToNv12(frame, width, height);
        if (!Process(nv12, out byte[] sample) || sample.Length == 0)
        {
            LastError = "process produced no sample";
            return false;
        }
        if (keyframe)
            _header = ReadHeader() ?? _header;
        packet = Pack(keyframe, width, height, keyframe ? _header : null, sample);
        return true;
    }

    public static bool RoundTrip()
    {
        using Bitmap frame = new(160, 96, PixelFormat.Format24bppRgb);
        using (Graphics graphics = Graphics.FromImage(frame))
            graphics.Clear(Color.SeaGreen);
        using H264Encoder encoder = new();
        if (!encoder.TryEncode(frame, 800_000, true, out byte[] packet))
            return false;
        using H264Decoder decoder = new();
        if (!decoder.TryDecode(packet, out byte[] bgr, out int width, out int height))
        {
            Unpack(packet, out _, out _, out _, out byte[] header, out byte[] sample);
            string hex = sample.Length == 0 ? "" : Convert.ToHexString(sample.AsSpan(0, Math.Min(20, sample.Length)));
            LastError = "decode-failed " + LastError + " header=" + header.Length + " " + hex;
            return false;
        }
        return width == 160 && height == 96 && bgr.Length == 160 * 96 * 3;
    }

    public void Dispose() => DisposeTransform();

    private bool Open(int width, int height, int bitrate)
    {
        try
        {
            StartMediaFoundation();
            _transform = TryHardware();
            if (_transform == null || !Configure(_transform, width, height, bitrate))
            {
                string hardwareError = _transform == null ? "none" : LastError;
                if (_transform != null)
                    Marshal.ReleaseComObject(_transform);
                _transform = CreateSoftware();
                if (_transform == null || !Configure(_transform, width, height, bitrate))
                {
                    LastError = "hw=[" + hardwareError + "] sw=[" + (_transform == null ? "create-failed" : LastError) + "]";
                    DisposeTransform();
                    return false;
                }
            }
            _codec = _transform as ICodecApi;
            _transform.ProcessMessage(MessageBeginStreaming, 0);
            _transform.ProcessMessage(MessageStartStream, 0);
            _width = width;
            _height = height;
            _bitrate = bitrate;
            _stamp = 0;
            _header = ReadHeader();
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.GetType().Name + " " + ex.Message;
            return false;
        }
    }

    private bool Process(byte[] nv12, out byte[] sample)
    {
        sample = Array.Empty<byte>();
        if (_transform == null)
            return false;
        IMFSample? input = null;
        IMFSample? output = null;
        try
        {
            input = CreateSample(nv12, _stamp, 333_333);
            _stamp += 333_333;
            int hr = _transform.ProcessInput(0, input, 0);
            if (hr < 0)
            {
                LastError = "ProcessInput " + hr.ToString("X8");
                return false;
            }
            int bufferSize = OutputBufferSize(_transform, nv12.Length);
            output = CreateOutputSample(bufferSize);
            var data = new MftOutputBuffer { Sample = Marshal.GetIUnknownForObject(output) };
            try
            {
                hr = _transform.ProcessOutput(0, 1, ref data, out _);
                if (hr < 0)
                {
                    LastError = "ProcessOutput " + hr.ToString("X8");
                    return false;
                }
                sample = ReadSample(output);
                return sample.Length > 0;
            }
            finally
            {
                if (data.Sample != IntPtr.Zero)
                    Marshal.Release(data.Sample);
                if (data.Events != IntPtr.Zero)
                    Marshal.Release(data.Events);
            }
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            if (input != null)
                Marshal.ReleaseComObject(input);
            if (output != null)
                Marshal.ReleaseComObject(output);
        }
    }

    private byte[]? ReadHeader()
    {
        if (_transform == null)
            return null;
        int hr = _transform.GetOutputCurrentType(0, out IMFMediaType type);
        if (hr < 0 || type == null)
            return null;
        try
        {
            Guid key = MpegSequenceHeader;
            if (type.GetBlobSize(ref key, out int size) < 0 || size <= 0 || size > 4096)
                return null;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                if (type.GetBlob(ref key, buffer, size, out int written) < 0 || written <= 0)
                    return null;
                byte[] blob = new byte[written];
                Marshal.Copy(buffer, blob, 0, written);
                return blob;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(type);
        }
    }

    private void ForceKey()
    {
        if (_codec == null)
            return;
        var value = new PropVariant { vt = 19, i4 = 1 };
        Guid key = CodecForceKey;
        _codec.SetValue(ref key, ref value);
    }

    private void DisposeTransform()
    {
        if (_transform != null)
        {
            try { _transform.ProcessMessage(MessageEndStreaming, 0); } catch { }
            Marshal.ReleaseComObject(_transform);
            _transform = null;
        }
        _codec = null;
        _width = 0;
        _height = 0;
    }

    private static byte[] Pack(bool key, int width, int height, byte[]? header, byte[] sample)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write((byte)1);
        writer.Write((byte)(key ? 1 : 0));
        writer.Write(width);
        writer.Write(height);
        writer.Write(header?.Length ?? 0);
        if (header is { Length: > 0 })
            writer.Write(header);
        writer.Write(sample.Length);
        writer.Write(sample);
        return stream.ToArray();
    }

    public static bool Unpack(byte[] packet, out bool key, out int width, out int height, out byte[] header, out byte[] sample)
    {
        key = false;
        width = 0;
        height = 0;
        header = Array.Empty<byte>();
        sample = Array.Empty<byte>();
        try
        {
            using BinaryReader reader = new(new MemoryStream(packet, writable: false));
            if (reader.ReadByte() != 1)
                return false;
            key = reader.ReadByte() != 0;
            width = reader.ReadInt32();
            height = reader.ReadInt32();
            int headerLength = reader.ReadInt32();
            if (headerLength is < 0 or > 4096 || width is < 2 or > 7680 || height is < 2 or > 4320)
                return false;
            header = headerLength == 0 ? Array.Empty<byte>() : reader.ReadBytes(headerLength);
            int sampleLength = reader.ReadInt32();
            if (sampleLength <= 0 || sampleLength > 8_000_000)
                return false;
            sample = reader.ReadBytes(sampleLength);
            return header.Length == headerLength && sample.Length == sampleLength;
        }
        catch (EndOfStreamException)
        {
            return false;
        }
    }

    private static byte[] ToNv12(Bitmap frame, int width, int height)
    {
        BitmapData data = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            int stride = data.Stride;
            byte[] source = new byte[stride * frame.Height];
            Marshal.Copy(data.Scan0, source, 0, source.Length);
            byte[] nv12 = new byte[width * height * 3 / 2];
            int chroma = width * height;
            for (int y = 0; y < height; y++)
            {
                int row = y * stride;
                int yRow = y * width;
                for (int x = 0; x < width; x++)
                {
                    int i = row + x * 3;
                    nv12[yRow + x] = Luma(source[i + 2], source[i + 1], source[i]);
                }
            }
            for (int y = 0; y < height; y += 2)
            {
                for (int x = 0; x < width; x += 2)
                {
                    int c = 0, d = 0, e = 0, n = 0;
                    Accumulate(source, stride, x, y, ref c, ref d, ref e, ref n);
                    Accumulate(source, stride, x + 1, y, ref c, ref d, ref e, ref n);
                    Accumulate(source, stride, x, y + 1, ref c, ref d, ref e, ref n);
                    Accumulate(source, stride, x + 1, y + 1, ref c, ref d, ref e, ref n);
                    int uv = chroma + (y / 2) * width + x;
                    nv12[uv] = (byte)Math.Clamp(c / n, 0, 255);
                    nv12[uv + 1] = (byte)Math.Clamp(d / n, 0, 255);
                }
            }
            return nv12;
        }
        finally
        {
            frame.UnlockBits(data);
        }
    }

    private static void Accumulate(byte[] source, int stride, int x, int y, ref int u, ref int v, ref int unused, ref int count)
    {
        int i = y * stride + x * 3;
        byte b = source[i];
        byte g = source[i + 1];
        byte r = source[i + 2];
        u += ((-38 * r - 74 * g + 112 * b + 128) >> 8) + 128;
        v += ((112 * r - 94 * g - 18 * b + 128) >> 8) + 128;
        unused += 0;
        count++;
    }

    private static byte Luma(byte r, byte g, byte b)
    {
        int y = ((66 * r + 129 * g + 25 * b + 128) >> 8) + 16;
        return (byte)Math.Clamp(y, 0, 255);
    }

    private static IMFSample CreateSample(byte[] data, long time, long duration)
    {
        if (MFCreateMemoryBuffer(data.Length, out IMFMediaBuffer buffer) < 0)
            throw new InvalidOperationException();
        buffer.Lock(out IntPtr pointer, out _, out _);
        Marshal.Copy(data, 0, pointer, data.Length);
        buffer.Unlock();
        buffer.SetCurrentLength(data.Length);
        if (MFCreateSample(out IMFSample sample) < 0)
            throw new InvalidOperationException();
        sample.AddBuffer(buffer);
        Marshal.ReleaseComObject(buffer);
        if (duration > 0)
        {
            sample.SetSampleTime(time);
            sample.SetSampleDuration(duration);
        }
        return sample;
    }

    private static IMFSample CreateOutputSample(int size)
    {
        if (MFCreateMemoryBuffer(size, out IMFMediaBuffer buffer) < 0)
            throw new InvalidOperationException();
        buffer.SetCurrentLength(0);
        if (MFCreateSample(out IMFSample sample) < 0)
            throw new InvalidOperationException();
        sample.AddBuffer(buffer);
        Marshal.ReleaseComObject(buffer);
        return sample;
    }

    private static byte[] ReadSample(IMFSample sample)
    {
        if (sample.ConvertToContiguousBuffer(out IMFMediaBuffer buffer) < 0)
            return Array.Empty<byte>();
        try
        {
            buffer.Lock(out IntPtr pointer, out _, out int length);
            byte[] copy = new byte[Math.Max(0, length)];
            if (copy.Length > 0)
                Marshal.Copy(pointer, copy, 0, copy.Length);
            buffer.Unlock();
            return copy;
        }
        finally
        {
            Marshal.ReleaseComObject(buffer);
        }
    }

    private static int OutputBufferSize(IMFTransform transform, int fallback)
    {
        if (transform.GetOutputStreamInfo(0, out MftStreamInfo info) < 0 || info.Size < 32)
            return Math.Max(fallback, 256 * 1024);
        return Math.Max(info.Size, 32);
    }

    private static bool SetTypes(IMFTransform transform, int width, int height, int bitrate)
    {
        if (!PrepareOutput(transform, width, height, bitrate))
            return false;
        return PrepareInput(transform, width, height);
    }

    private static bool PrepareOutput(IMFTransform transform, int width, int height, int bitrate)
    {
        int hr = transform.GetOutputAvailableType(0, 0, out IMFMediaType output);
        if (hr < 0)
            hr = MFCreateMediaType(out output);
        if (hr < 0 || output == null)
        {
            LastError = "output type " + hr.ToString("X8");
            return false;
        }
        try
        {
            Guid major = VideoMajor;
            Guid h264 = H264Subtype;
            output.SetGUID(ref MajorType, ref major);
            output.SetGUID(ref Subtype, ref h264);
            output.SetUINT64(ref FrameSize, Pack(width, height));
            output.SetUINT64(ref FrameRate, Pack(30, 1));
            output.SetUINT64(ref Aspect, Pack(1, 1));
            output.SetUINT32(ref Interlace, 2);
            output.SetUINT32(ref Bitrate, bitrate);
            output.SetUINT32(ref Profile, 77);
            hr = output.GetGUID(ref MajorType, out Guid check);
            if (hr < 0 || check != major)
            {
                LastError = "type readback " + hr.ToString("X8");
                return false;
            }
            hr = transform.SetOutputType(0, output, 0);
            if (hr < 0)
            {
                LastError = "SetOutputType " + hr.ToString("X8");
                return false;
            }
            return true;
        }
        finally
        {
            Marshal.ReleaseComObject(output);
        }
    }

    private static bool PrepareInput(IMFTransform transform, int width, int height)
    {
        IMFMediaType? input = null;
        int hr = -1;
        for (int i = 0; i < 8; i++)
        {
            hr = transform.GetInputAvailableType(0, i, out IMFMediaType candidate);
            if (hr < 0 || candidate == null)
                break;
            hr = candidate.GetGUID(ref Subtype, out Guid subtype);
            if (hr >= 0 && subtype == Nv12Subtype)
            {
                input = candidate;
                break;
            }
            Marshal.ReleaseComObject(candidate);
        }
        if (input == null)
        {
            hr = MFCreateMediaType(out input);
            if (hr < 0 || input == null)
            {
                LastError = "input type " + hr.ToString("X8");
                return false;
            }
            Guid major = VideoMajor;
            Guid nv12 = Nv12Subtype;
            input.SetGUID(ref MajorType, ref major);
            input.SetGUID(ref Subtype, ref nv12);
        }
        try
        {
            input.SetUINT64(ref FrameSize, Pack(width, height));
            input.SetUINT64(ref FrameRate, Pack(30, 1));
            input.SetUINT64(ref Aspect, Pack(1, 1));
            input.SetUINT32(ref Interlace, 2);
            hr = transform.SetInputType(0, input, 0);
            if (hr < 0)
                LastError = "SetInputType " + hr.ToString("X8");
            return hr >= 0;
        }
        finally
        {
            Marshal.ReleaseComObject(input);
        }
    }

    private static bool Configure(IMFTransform transform, int width, int height, int bitrate)
    {
        if (transform is IMFAttributes attributes)
            attributes.SetUINT32(ref MfLowLatency, 1);
        ICodecApi? codec = transform as ICodecApi;
        SetCodec(codec, CodecRateMode, 0);
        SetCodec(codec, CodecBitrate, bitrate);
        SetCodec(codec, CodecLowLatency, 1);
        SetCodec(codec, CodecGop, 15);
        SetCodec(codec, CodecBFrames, 0);
        return SetTypes(transform, width, height, bitrate);
    }

    private static IMFTransform? CreateSoftware()
    {
        Guid clsid = SoftwareEncoder;
        Guid iid = typeof(IMFTransform).GUID;
        int hr = CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out IMFTransform software);
        return hr >= 0 ? software : null;
    }

    private static IMFTransform? TryHardware()
    {
        var input = new MftRegisterInfo { Major = VideoMajor, Subtype = Nv12Subtype };
        var output = new MftRegisterInfo { Major = VideoMajor, Subtype = H264Subtype };
        int hr = MFTEnumEx(VideoEncoderCategory, SyncHardware, ref input, ref output, out IntPtr list, out int count);
        if (hr < 0 || count <= 0 || list == IntPtr.Zero)
            return null;
        try
        {
            for (int i = 0; i < count; i++)
            {
                IntPtr activatePointer = Marshal.ReadIntPtr(list, i * IntPtr.Size);
                if (activatePointer == IntPtr.Zero)
                    continue;
                IMFActivate activate = (IMFActivate)Marshal.GetObjectForIUnknown(activatePointer);
                Guid iid = typeof(IMFTransform).GUID;
                if (activate.ActivateObject(ref iid, out IMFTransform transform) < 0 || transform == null)
                    continue;
                if (transform is IMFAttributes attrs && attrs.GetUINT32(ref AsyncAttribute, out int async) >= 0 && async != 0)
                {
                    Marshal.ReleaseComObject(transform);
                    continue;
                }
                return transform;
            }
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            for (int i = 0; i < count; i++)
            {
                IntPtr activatePointer = Marshal.ReadIntPtr(list, i * IntPtr.Size);
                if (activatePointer != IntPtr.Zero)
                    Marshal.Release(activatePointer);
            }
            Marshal.FreeCoTaskMem(list);
        }
        return null;
    }

    private static void SetCodec(ICodecApi? codec, Guid id, int value)
    {
        if (codec == null)
            return;
        var variant = new PropVariant { vt = 19, i4 = value };
        codec.SetValue(ref id, ref variant);
    }

    private static long Pack(int high, int low) => ((long)high << 32) | (uint)low;

    private static void StartMediaFoundation()
    {
        if (Interlocked.Exchange(ref _started, 1) == 0)
            MFStartup(0x00020070, 0);
    }

    private static Guid VideoMajor = new("73646976-0000-0010-8000-00AA00389B71");
    private static Guid Nv12Subtype = new("3231564E-0000-0010-8000-00AA00389B71");
    private static Guid H264Subtype = new("34363248-0000-0010-8000-00AA00389B71");
    private static Guid MajorType = new("48eba18e-f8c9-4687-bf11-0a74c9f96a8d");
    private static Guid Subtype = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
    private static Guid FrameSize = new("1652c33d-d6b2-4012-b834-72030849a37d");
    private static Guid FrameRate = new("c459a2e8-3d2c-4e44-b132-fee5156c7bb0");
    private static Guid Aspect = new("c6376a1e-8d0a-4027-be45-6d9a0ad39bb6");
    private static Guid Interlace = new("e2724bb8-e676-4806-b4b2-a8d6efb44ccd");
    private static Guid Bitrate = new("20332624-fb0d-4d9e-bd0d-cbf6786c102e");
    private static Guid Profile = new("ad76a80b-2d5c-4e0b-b375-64e520137036");
    private static Guid MpegSequenceHeader = new("3c036de7-3ad0-4c9e-9216-ee6d6ac21cb3");
    private static Guid MfLowLatency = new("9c27891a-ed7a-40e1-88e8-b22727a024ee");
    private static Guid AsyncAttribute = new("f81a699a-649a-497d-8c73-29f8fed6ad7a");
    private static Guid VideoEncoderCategory = new("f79eac7d-e545-4387-bdee-3dee89b772bf");
    private static Guid SoftwareEncoder = new("6ca50344-051a-4ded-9779-a43305165e35");
    private static Guid CodecRateMode = new("1c0608e9-370c-4710-8a58-cb6181c42423");
    private static Guid CodecBitrate = new("f7222374-2144-4815-b550-a37f8e12ee52");
    private static Guid CodecLowLatency = new("9c27891a-ed7a-40e1-88e8-b22727a024ee");
    private static Guid CodecGop = new("95f31b26-95a4-41aa-9303-246a7fc7eef1");
    private static Guid CodecBFrames = new("8d390aac-dc5c-4200-b57f-814d04babab2");
    private static Guid CodecForceKey = new("398c1b98-8353-475a-9ef2-8f265d260345");
    private const uint SyncHardware = 0x1 | 0x4 | 0x10 | 0x40;
    private const int MessageBeginStreaming = 0x10000000;
    private const int MessageEndStreaming = 0x10000001;
    private const int MessageStartStream = 0x10000003;

    [DllImport("mfplat.dll")]
    private static extern int MFStartup(uint version, uint flags);

    [DllImport("mfplat.dll")]
    private static extern int MFCreateMediaType(out IMFMediaType type);

    [DllImport("mfplat.dll")]
    private static extern int MFCreateSample(out IMFSample sample);

    [DllImport("mfplat.dll")]
    private static extern int MFCreateMemoryBuffer(int maxLength, out IMFMediaBuffer buffer);

    [DllImport("mfplat.dll")]
    private static extern int MFTEnumEx(Guid category, uint flags, ref MftRegisterInfo input, ref MftRegisterInfo output, out IntPtr activates, out int count);

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(ref Guid clsid, IntPtr outer, uint context, ref Guid iid, out IMFTransform instance);
}

public sealed class H264Decoder : IDisposable
{
    private IMFTransform? _transform;
    private byte[]? _header;
    private int _width;
    private int _height;
    private long _stamp;
    private bool _ready;

    public bool TryDecode(byte[] packet, out byte[] bgr, out int width, out int height)
    {
        bgr = Array.Empty<byte>();
        width = 0;
        height = 0;
        if (!H264Encoder.Unpack(packet, out _, out int frameWidth, out int frameHeight, out byte[] header, out byte[] sample))
            return false;
        if (_transform == null || frameWidth != _width || frameHeight != _height || (header.Length > 0 && !header.AsSpan().SequenceEqual(_header)))
        {
            Dispose();
            if (!Open(frameWidth, frameHeight, header.Length > 0 ? header : _header))
                return false;
        }
        if (!Feed(sample, out byte[] nv12, out int stride))
            return false;
        bgr = FromNv12(nv12, _width, _height, stride);
        width = _width;
        height = _height;
        return bgr.Length == width * height * 3;
    }

    public void Dispose()
    {
        if (_transform != null)
        {
            Marshal.ReleaseComObject(_transform);
            _transform = null;
        }
    }

    private bool Open(int width, int height, byte[]? header)
    {
        try
        {
            if (Interlocked.CompareExchange(ref Started, 1, 0) == 0)
                MFStartup(0x00020070, 0);
            Guid clsid = SoftwareDecoder;
            Guid iid = typeof(IMFTransform).GUID;
            if (CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out IMFTransform transform) < 0)
                return false;
            Guid lowLatency = LowLatency;
            if (transform is IMFAttributes attributes)
                attributes.SetUINT32(ref lowLatency, 1);
            if (transform is ICodecApi codec)
            {
                var variant = new PropVariant { vt = 19, i4 = 1 };
                codec.SetValue(ref lowLatency, ref variant);
            }
            int typeHr = transform.GetInputAvailableType(0, 0, out IMFMediaType input);
            if (typeHr < 0)
            {
                H264Encoder.LastError = "decoder available " + typeHr.ToString("X8");
                Marshal.ReleaseComObject(transform);
                return false;
            }
            if (header is { Length: > 0 })
                SetSequence(input, header);
            typeHr = transform.SetInputType(0, input, 0);
            if (typeHr < 0 && header is { Length: > 0 })
            {
                Marshal.ReleaseComObject(input);
                typeHr = transform.GetInputAvailableType(0, 0, out input);
                if (typeHr >= 0)
                    typeHr = transform.SetInputType(0, input, 0);
            }
            if (input != null)
                Marshal.ReleaseComObject(input);
            if (typeHr < 0)
            {
                H264Encoder.LastError = "decoder input " + typeHr.ToString("X8");
                Marshal.ReleaseComObject(transform);
                return false;
            }
            transform.ProcessMessage(0x10000000, 0);
            transform.ProcessMessage(0x10000003, 0);
            _transform = transform;
            _width = width;
            _height = height;
            if (header is { Length: > 0 })
                _header = header;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool Feed(byte[] sample, out byte[] nv12, out int stride)
    {
        nv12 = Array.Empty<byte>();
        stride = _width;
        if (_transform == null)
            return false;
        IMFSample? input = null;
        IMFSample? output = null;
        try
        {
            input = MakeSample(CloseAccessUnit(sample), _stamp);
            _stamp += 333_333;
            int hr = _transform.ProcessInput(0, input, 0);
            if (hr < 0)
            {
                H264Encoder.LastError = "decoder ProcessInput " + hr.ToString("X8");
                return false;
            }
            if (!_ready && AcceptOutput())
                _ready = true;
            var data = new MftOutputBuffer();
            output = AttachOutputBuffer(ref data);
            try
            {
                hr = _transform.ProcessOutput(0, 1, ref data, out _);
                if (hr == StreamChange || hr == TypeNotSet || hr == NeedMore)
                {
                    ReleaseBuffer(ref data);
                    if (output != null)
                    {
                        Marshal.ReleaseComObject(output);
                        output = null;
                    }
                    if (hr != NeedMore && !AcceptOutput())
                        return false;
                    _ready = hr != NeedMore || AcceptOutput() || _ready;
                    if (hr == NeedMore)
                    {
                        IMFSample mark = MakeSample(AccessUnit, _stamp);
                        _transform.ProcessInput(0, mark, 0);
                        Marshal.ReleaseComObject(mark);
                    }
                    output = AttachOutputBuffer(ref data);
                    hr = _transform.ProcessOutput(0, 1, ref data, out _);
                }
                if (hr < 0)
                {
                    H264Encoder.LastError = "decoder ProcessOutput " + hr.ToString("X8");
                    return false;
                }
                if (output == null && data.Sample != IntPtr.Zero)
                    output = (IMFSample)Marshal.GetObjectForIUnknown(data.Sample);
            }
            finally
            {
                if (data.Sample != IntPtr.Zero)
                    Marshal.Release(data.Sample);
                if (data.Events != IntPtr.Zero)
                    Marshal.Release(data.Events);
            }
            if (output == null)
            {
                H264Encoder.LastError = "decoder produced no sample";
                return false;
            }
            nv12 = CopySample(output);
            stride = OutputStride();
            if (nv12.Length < stride * _height * 3 / 2)
            {
                H264Encoder.LastError = "decoder short " + nv12.Length;
                return false;
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            if (input != null)
                Marshal.ReleaseComObject(input);
            if (output != null)
                Marshal.ReleaseComObject(output);
        }
    }

    private IMFSample? AttachOutputBuffer(ref MftOutputBuffer data)
    {
        if (_transform == null || _transform.GetOutputStreamInfo(0, out MftStreamInfo info) < 0)
            return null;
        if ((info.Flags & 0x100) != 0)
            return null;
        IMFSample sample = MakeOutputSample(Math.Max(info.Size, _width * _height * 3));
        data.Sample = Marshal.GetIUnknownForObject(sample);
        return sample;
    }

    private static void ReleaseBuffer(ref MftOutputBuffer data)
    {
        if (data.Sample != IntPtr.Zero)
        {
            Marshal.Release(data.Sample);
            data.Sample = IntPtr.Zero;
        }
        if (data.Events != IntPtr.Zero)
        {
            Marshal.Release(data.Events);
            data.Events = IntPtr.Zero;
        }
    }

    private bool AcceptOutput()
    {
        if (_transform == null)
            return false;
        int hr = _transform.GetOutputAvailableType(0, 0, out IMFMediaType output);
        if (hr < 0)
        {
            H264Encoder.LastError = "decoder stream type " + hr.ToString("X8");
            return false;
        }
        try
        {
            hr = _transform.SetOutputType(0, output, 0);
            if (hr < 0)
                H264Encoder.LastError = "decoder stream set " + hr.ToString("X8");
            else if (output.GetUINT64(ref FrameSize, out long packed) >= 0)
            {
                int width = (int)(packed >> 32);
                int height = (int)(packed & 0xFFFFFFFF);
                if (width >= 2 && height >= 2)
                {
                    _width = width;
                    _height = height;
                }
            }
            return hr >= 0;
        }
        finally
        {
            Marshal.ReleaseComObject(output);
        }
    }

    private int OutputStride()
    {
        if (_transform == null || _transform.GetOutputCurrentType(0, out IMFMediaType type) < 0)
            return _width;
        try
        {
            if (type.GetUINT32(ref DefaultStride, out int stride) < 0)
                return _width;
            int value = Math.Abs(stride);
            return value >= _width ? value : _width;
        }
        finally
        {
            Marshal.ReleaseComObject(type);
        }
    }

    private static void SetSequence(IMFMediaType type, byte[] header)
    {
        IntPtr blob = Marshal.AllocHGlobal(header.Length);
        try
        {
            Marshal.Copy(header, 0, blob, header.Length);
            type.SetBlob(ref SequenceHeader, blob, header.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(blob);
        }
    }

    private static byte[] CloseAccessUnit(byte[] sample)
    {
        byte[] closed = new byte[sample.Length + AccessUnit.Length];
        Buffer.BlockCopy(sample, 0, closed, 0, sample.Length);
        Buffer.BlockCopy(AccessUnit, 0, closed, sample.Length, AccessUnit.Length);
        return closed;
    }

    private static readonly byte[] AccessUnit = [0, 0, 0, 1, 0x09, 0xF0];

    private static IMFSample MakeSample(byte[] data, long time)
    {
        MFCreateMemoryBuffer(Math.Max(data.Length, 1), out IMFMediaBuffer buffer);
        if (data.Length > 0)
        {
            buffer.Lock(out IntPtr pointer, out _, out _);
            Marshal.Copy(data, 0, pointer, data.Length);
            buffer.Unlock();
        }
        buffer.SetCurrentLength(data.Length);
        MFCreateSample(out IMFSample sample);
        sample.AddBuffer(buffer);
        Marshal.ReleaseComObject(buffer);
        sample.SetSampleTime(time);
        sample.SetSampleDuration(333_333);
        return sample;
    }

    private static IMFSample MakeOutputSample(int size)
    {
        MFCreateMemoryBuffer(size, out IMFMediaBuffer buffer);
        buffer.SetCurrentLength(0);
        MFCreateSample(out IMFSample sample);
        sample.AddBuffer(buffer);
        Marshal.ReleaseComObject(buffer);
        return sample;
    }

    private static byte[] CopySample(IMFSample sample)
    {
        sample.ConvertToContiguousBuffer(out IMFMediaBuffer buffer);
        try
        {
            buffer.Lock(out IntPtr pointer, out _, out int length);
            byte[] copy = new byte[Math.Max(0, length)];
            if (copy.Length > 0)
                Marshal.Copy(pointer, copy, 0, copy.Length);
            buffer.Unlock();
            return copy;
        }
        finally
        {
            Marshal.ReleaseComObject(buffer);
        }
    }

    private static byte[] FromNv12(byte[] nv12, int width, int height, int stride)
    {
        if (stride < width)
            stride = width;
        byte[] bgr = new byte[width * height * 3];
        int chroma = stride * height;
        for (int y = 0; y < height; y++)
        {
            int yRow = y * stride;
            int uvRow = chroma + (y / 2) * stride;
            int dest = y * width * 3;
            for (int x = 0; x < width; x++)
            {
                int c = nv12[yRow + x] - 16;
                int u = nv12[uvRow + (x & ~1)] - 128;
                int v = nv12[uvRow + (x & ~1) + 1] - 128;
                bgr[dest++] = Clamp((298 * c + 516 * u + 128) >> 8);
                bgr[dest++] = Clamp((298 * c - 100 * u - 208 * v + 128) >> 8);
                bgr[dest++] = Clamp((298 * c + 409 * v + 128) >> 8);
            }
        }
        return bgr;
    }

    private static byte Clamp(int value) => (byte)Math.Clamp(value, 0, 255);

    private static int Started;
    private const int TypeNotSet = unchecked((int)0xC00D6D60);
    private const int StreamChange = unchecked((int)0xC00D6D61);
    private const int NeedMore = unchecked((int)0xC00D6D72);
    private static Guid LowLatency = new("9c27891a-ed7a-40e1-88e8-b22727a024ee");
    private static Guid DefaultStride = new("644b4e48-1e02-4516-b0eb-c01ca9d49ac6");
    private static Guid SoftwareDecoder = new("62CE7E72-4C71-4d20-B15D-452831A87D9D");
    private static Guid VideoMajor = new("73646976-0000-0010-8000-00AA00389B71");
    private static Guid H264Subtype = new("34363248-0000-0010-8000-00AA00389B71");
    private static Guid MajorType = new("48eba18e-f8c9-4687-bf11-0a74c9f96a8d");
    private static Guid Subtype = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
    private static Guid FrameSize = new("1652c33d-d6b2-4012-b834-72030849a37d");
    private static Guid FrameRate = new("c459a2e8-3d2c-4e44-b132-fee5156c7bb0");
    private static Guid Interlace = new("e2724bb8-e676-4806-b4b2-a8d6efb44ccd");
    private static Guid SequenceHeader = new("3c036de7-3ad0-4c9e-9216-ee6d6ac21cb3");

    [DllImport("mfplat.dll")]
    private static extern int MFStartup(uint version, uint flags);

    [DllImport("mfplat.dll")]
    private static extern int MFCreateMediaType(out IMFMediaType type);

    [DllImport("mfplat.dll")]
    private static extern int MFCreateSample(out IMFSample sample);

    [DllImport("mfplat.dll")]
    private static extern int MFCreateMemoryBuffer(int maxLength, out IMFMediaBuffer buffer);

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(ref Guid clsid, IntPtr outer, uint context, ref Guid iid, out IMFTransform instance);
}

[StructLayout(LayoutKind.Sequential)]
internal struct MftRegisterInfo
{
    public Guid Major;
    public Guid Subtype;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MftStreamInfo
{
    public int Flags;
    public int Size;
    public int Alignment;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MftOutputBuffer
{
    public uint StreamId;
    public IntPtr Sample;
    public uint Status;
    public IntPtr Events;
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
internal struct PropVariant
{
    [FieldOffset(0)] public ushort vt;
    [FieldOffset(8)] public int i4;
}

[ComImport, Guid("BF94C121-5B05-4E6F-8000-BA598961414D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMFTransform
{
    [PreserveSig] int GetStreamLimits(out int inputMinimum, out int inputMaximum, out int outputMinimum, out int outputMaximum);
    [PreserveSig] int GetStreamCount(out int inputs, out int outputs);
    [PreserveSig] int GetStreamIDs(int inputCount, IntPtr inputIds, int outputCount, IntPtr outputIds);
    [PreserveSig] int GetInputStreamInfo(int id, IntPtr info);
    [PreserveSig] int GetOutputStreamInfo(int id, out MftStreamInfo info);
    [PreserveSig] int GetAttributes(out IntPtr attributes);
    [PreserveSig] int GetInputStreamAttributes(int id, out IntPtr attributes);
    [PreserveSig] int GetOutputStreamAttributes(int id, out IntPtr attributes);
    [PreserveSig] int DeleteInputStream(int id);
    [PreserveSig] int AddInputStreams(int count, IntPtr ids);
    [PreserveSig] int GetInputAvailableType(int stream, int index, out IMFMediaType type);
    [PreserveSig] int GetOutputAvailableType(int stream, int index, out IMFMediaType type);
    [PreserveSig] int SetInputType(int stream, IMFMediaType type, int flags);
    [PreserveSig] int SetOutputType(int stream, IMFMediaType type, int flags);
    [PreserveSig] int GetInputCurrentType(int stream, out IMFMediaType type);
    [PreserveSig] int GetOutputCurrentType(int stream, out IMFMediaType type);
    [PreserveSig] int GetInputStatus(int stream, out int flags);
    [PreserveSig] int GetOutputStatus(out int flags);
    [PreserveSig] int SetOutputBounds(long lower, long upper);
    [PreserveSig] int ProcessEvent(int stream, IntPtr mediaEvent);
    [PreserveSig] int ProcessMessage(int message, IntPtr parameter);
    [PreserveSig] int ProcessInput(int stream, IMFSample sample, int flags);
    [PreserveSig] int ProcessOutput(int flags, int count, ref MftOutputBuffer buffers, out int status);
}

[ComImport, Guid("2CD2D921-C447-44A7-A13C-4ADABFC247E3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMFAttributes
{
    [PreserveSig] int GetItem(ref Guid key, IntPtr value);
    [PreserveSig] int GetItemType(ref Guid key, out int type);
    [PreserveSig] int CompareItem(ref Guid key, IntPtr value, out int result);
    [PreserveSig] int Compare(IMFAttributes other, int matchType, out int result);
    [PreserveSig] int GetUINT32(ref Guid key, out int value);
    [PreserveSig] int GetUINT64(ref Guid key, out long value);
    [PreserveSig] int GetDouble(ref Guid key, out double value);
    [PreserveSig] int GetGUID(ref Guid key, out Guid value);
    [PreserveSig] int GetStringLength(ref Guid key, out int length);
    [PreserveSig] int GetString(ref Guid key, IntPtr value, int size, out int length);
    [PreserveSig] int GetAllocatedString(ref Guid key, out IntPtr value, out int length);
    [PreserveSig] int GetBlobSize(ref Guid key, out int size);
    [PreserveSig] int GetBlob(ref Guid key, IntPtr buffer, int size, out int written);
    [PreserveSig] int GetAllocatedBlob(ref Guid key, out IntPtr buffer, out int size);
    [PreserveSig] int GetUnknown(ref Guid key, ref Guid iid, out IntPtr unknown);
    [PreserveSig] int SetItem(ref Guid key, IntPtr value);
    [PreserveSig] int DeleteItem(ref Guid key);
    [PreserveSig] int DeleteAllItems();
    [PreserveSig] int SetUINT32(ref Guid key, int value);
    [PreserveSig] int SetUINT64(ref Guid key, long value);
    [PreserveSig] int SetDouble(ref Guid key, double value);
    [PreserveSig] int SetGUID(ref Guid key, ref Guid value);
    [PreserveSig] int SetString(ref Guid key, [MarshalAs(UnmanagedType.LPWStr)] string value);
    [PreserveSig] int SetBlob(ref Guid key, IntPtr buffer, int size);
    [PreserveSig] int SetUnknown(ref Guid key, IntPtr unknown);
    [PreserveSig] int LockStore();
    [PreserveSig] int UnlockStore();
    [PreserveSig] int GetCount(out int count);
    [PreserveSig] int GetItemByIndex(int index, out Guid key, IntPtr value);
    [PreserveSig] int CopyAllItems(IMFAttributes destination);
}

[ComImport, Guid("44AE0FA8-EA31-4109-8D2E-4CAE4997C555"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMFMediaType : IMFAttributes
{
    [PreserveSig] new int GetItem(ref Guid key, IntPtr value);
    [PreserveSig] new int GetItemType(ref Guid key, out int type);
    [PreserveSig] new int CompareItem(ref Guid key, IntPtr value, out int result);
    [PreserveSig] new int Compare(IMFAttributes other, int matchType, out int result);
    [PreserveSig] new int GetUINT32(ref Guid key, out int value);
    [PreserveSig] new int GetUINT64(ref Guid key, out long value);
    [PreserveSig] new int GetDouble(ref Guid key, out double value);
    [PreserveSig] new int GetGUID(ref Guid key, out Guid value);
    [PreserveSig] new int GetStringLength(ref Guid key, out int length);
    [PreserveSig] new int GetString(ref Guid key, IntPtr value, int size, out int length);
    [PreserveSig] new int GetAllocatedString(ref Guid key, out IntPtr value, out int length);
    [PreserveSig] new int GetBlobSize(ref Guid key, out int size);
    [PreserveSig] new int GetBlob(ref Guid key, IntPtr buffer, int size, out int written);
    [PreserveSig] new int GetAllocatedBlob(ref Guid key, out IntPtr buffer, out int size);
    [PreserveSig] new int GetUnknown(ref Guid key, ref Guid iid, out IntPtr unknown);
    [PreserveSig] new int SetItem(ref Guid key, IntPtr value);
    [PreserveSig] new int DeleteItem(ref Guid key);
    [PreserveSig] new int DeleteAllItems();
    [PreserveSig] new int SetUINT32(ref Guid key, int value);
    [PreserveSig] new int SetUINT64(ref Guid key, long value);
    [PreserveSig] new int SetDouble(ref Guid key, double value);
    [PreserveSig] new int SetGUID(ref Guid key, ref Guid value);
    [PreserveSig] new int SetString(ref Guid key, [MarshalAs(UnmanagedType.LPWStr)] string value);
    [PreserveSig] new int SetBlob(ref Guid key, IntPtr buffer, int size);
    [PreserveSig] new int SetUnknown(ref Guid key, IntPtr unknown);
    [PreserveSig] new int LockStore();
    [PreserveSig] new int UnlockStore();
    [PreserveSig] new int GetCount(out int count);
    [PreserveSig] new int GetItemByIndex(int index, out Guid key, IntPtr value);
    [PreserveSig] new int CopyAllItems(IMFAttributes destination);
    [PreserveSig] int GetMajorType(out Guid major);
    [PreserveSig] int IsCompressedFormat(out int compressed);
    [PreserveSig] int IsEqual(IMFMediaType other, out int flags);
    [PreserveSig] int GetRepresentation(Guid guid, out IntPtr representation);
    [PreserveSig] int FreeRepresentation(Guid guid, IntPtr representation);
}

[ComImport, Guid("C40A00F2-B93A-4D80-AE8C-5A1C634F58E4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMFSample : IMFAttributes
{
    [PreserveSig] new int GetItem(ref Guid key, IntPtr value);
    [PreserveSig] new int GetItemType(ref Guid key, out int type);
    [PreserveSig] new int CompareItem(ref Guid key, IntPtr value, out int result);
    [PreserveSig] new int Compare(IMFAttributes other, int matchType, out int result);
    [PreserveSig] new int GetUINT32(ref Guid key, out int value);
    [PreserveSig] new int GetUINT64(ref Guid key, out long value);
    [PreserveSig] new int GetDouble(ref Guid key, out double value);
    [PreserveSig] new int GetGUID(ref Guid key, out Guid value);
    [PreserveSig] new int GetStringLength(ref Guid key, out int length);
    [PreserveSig] new int GetString(ref Guid key, IntPtr value, int size, out int length);
    [PreserveSig] new int GetAllocatedString(ref Guid key, out IntPtr value, out int length);
    [PreserveSig] new int GetBlobSize(ref Guid key, out int size);
    [PreserveSig] new int GetBlob(ref Guid key, IntPtr buffer, int size, out int written);
    [PreserveSig] new int GetAllocatedBlob(ref Guid key, out IntPtr buffer, out int size);
    [PreserveSig] new int GetUnknown(ref Guid key, ref Guid iid, out IntPtr unknown);
    [PreserveSig] new int SetItem(ref Guid key, IntPtr value);
    [PreserveSig] new int DeleteItem(ref Guid key);
    [PreserveSig] new int DeleteAllItems();
    [PreserveSig] new int SetUINT32(ref Guid key, int value);
    [PreserveSig] new int SetUINT64(ref Guid key, long value);
    [PreserveSig] new int SetDouble(ref Guid key, double value);
    [PreserveSig] new int SetGUID(ref Guid key, ref Guid value);
    [PreserveSig] new int SetString(ref Guid key, [MarshalAs(UnmanagedType.LPWStr)] string value);
    [PreserveSig] new int SetBlob(ref Guid key, IntPtr buffer, int size);
    [PreserveSig] new int SetUnknown(ref Guid key, IntPtr unknown);
    [PreserveSig] new int LockStore();
    [PreserveSig] new int UnlockStore();
    [PreserveSig] new int GetCount(out int count);
    [PreserveSig] new int GetItemByIndex(int index, out Guid key, IntPtr value);
    [PreserveSig] new int CopyAllItems(IMFAttributes destination);
    [PreserveSig] int GetSampleFlags(out int flags);
    [PreserveSig] int SetSampleFlags(int flags);
    [PreserveSig] int GetSampleTime(out long time);
    [PreserveSig] int SetSampleTime(long time);
    [PreserveSig] int GetSampleDuration(out long duration);
    [PreserveSig] int SetSampleDuration(long duration);
    [PreserveSig] int GetBufferCount(out int count);
    [PreserveSig] int GetBufferByIndex(int index, out IMFMediaBuffer buffer);
    [PreserveSig] int ConvertToContiguousBuffer(out IMFMediaBuffer buffer);
    [PreserveSig] int AddBuffer(IMFMediaBuffer buffer);
    [PreserveSig] int RemoveBufferByIndex(int index);
    [PreserveSig] int RemoveAllBuffers();
    [PreserveSig] int GetTotalLength(out int length);
    [PreserveSig] int CopyToBuffer(IMFMediaBuffer buffer);
}

[ComImport, Guid("045FA593-8799-42B8-BC8D-8968C6453507"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMFMediaBuffer
{
    [PreserveSig] int Lock(out IntPtr buffer, out int maxLength, out int currentLength);
    [PreserveSig] int Unlock();
    [PreserveSig] int GetCurrentLength(out int length);
    [PreserveSig] int SetCurrentLength(int length);
    [PreserveSig] int GetMaxLength(out int length);
}

[ComImport, Guid("7FEE9E9A-4A89-47A6-899C-B6A53A70FB67"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMFActivate : IMFAttributes
{
    [PreserveSig] new int GetItem(ref Guid key, IntPtr value);
    [PreserveSig] new int GetItemType(ref Guid key, out int type);
    [PreserveSig] new int CompareItem(ref Guid key, IntPtr value, out int result);
    [PreserveSig] new int Compare(IMFAttributes other, int matchType, out int result);
    [PreserveSig] new int GetUINT32(ref Guid key, out int value);
    [PreserveSig] new int GetUINT64(ref Guid key, out long value);
    [PreserveSig] new int GetDouble(ref Guid key, out double value);
    [PreserveSig] new int GetGUID(ref Guid key, out Guid value);
    [PreserveSig] new int GetStringLength(ref Guid key, out int length);
    [PreserveSig] new int GetString(ref Guid key, IntPtr value, int size, out int length);
    [PreserveSig] new int GetAllocatedString(ref Guid key, out IntPtr value, out int length);
    [PreserveSig] new int GetBlobSize(ref Guid key, out int size);
    [PreserveSig] new int GetBlob(ref Guid key, IntPtr buffer, int size, out int written);
    [PreserveSig] new int GetAllocatedBlob(ref Guid key, out IntPtr buffer, out int size);
    [PreserveSig] new int GetUnknown(ref Guid key, ref Guid iid, out IntPtr unknown);
    [PreserveSig] new int SetItem(ref Guid key, IntPtr value);
    [PreserveSig] new int DeleteItem(ref Guid key);
    [PreserveSig] new int DeleteAllItems();
    [PreserveSig] new int SetUINT32(ref Guid key, int value);
    [PreserveSig] new int SetUINT64(ref Guid key, long value);
    [PreserveSig] new int SetDouble(ref Guid key, double value);
    [PreserveSig] new int SetGUID(ref Guid key, ref Guid value);
    [PreserveSig] new int SetString(ref Guid key, [MarshalAs(UnmanagedType.LPWStr)] string value);
    [PreserveSig] new int SetBlob(ref Guid key, IntPtr buffer, int size);
    [PreserveSig] new int SetUnknown(ref Guid key, IntPtr unknown);
    [PreserveSig] new int LockStore();
    [PreserveSig] new int UnlockStore();
    [PreserveSig] new int GetCount(out int count);
    [PreserveSig] new int GetItemByIndex(int index, out Guid key, IntPtr value);
    [PreserveSig] new int CopyAllItems(IMFAttributes destination);
    [PreserveSig] int ActivateObject(ref Guid iid, out IMFTransform transform);
    [PreserveSig] int ShutdownObject();
    [PreserveSig] int DetachObject();
}

[ComImport, Guid("901DB4C7-31CE-41A2-85DC-8FA0BF41B8DA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICodecApi
{
    [PreserveSig] int IsSupported(ref Guid key);
    [PreserveSig] int IsModifiable(ref Guid key);
    [PreserveSig] int GetParameterRange(ref Guid key, IntPtr variantMin, IntPtr variantMax, IntPtr stepping);
    [PreserveSig] int GetParameterValues(ref Guid key, out IntPtr values, out int count);
    [PreserveSig] int GetDefaultValue(ref Guid key, IntPtr value);
    [PreserveSig] int GetValue(ref Guid key, IntPtr value);
    [PreserveSig] int SetValue(ref Guid key, ref PropVariant value);
    [PreserveSig] int RegisterForEvent(ref Guid key, IntPtr notify);
    [PreserveSig] int UnregisterForEvent(ref Guid key);
    [PreserveSig] int SetAllDefaults();
    [PreserveSig] int SetValueWithNotify(ref Guid key, ref PropVariant value, out Guid changed, out int count);
    [PreserveSig] int SetAllDefaultsWithNotify(out Guid changed, out int count);
    [PreserveSig] int GetAllSettings(IntPtr stream);
    [PreserveSig] int SetAllSettings(IntPtr stream);
    [PreserveSig] int SetAllSettingsWithNotify(IntPtr stream, out Guid changed, out int count);
}
