using System.IO;
using System.Runtime.InteropServices;

namespace Kdock.App;

public sealed class ClipOffer
{
    public ClipOffer(string name, long size)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "file" : name;
        Size = size < 0 ? 0 : size;
    }

    public string Name { get; }
    public long Size { get; }
}

public static class VirtualClipboard
{
    public static void Offer(IReadOnlyList<ClipOffer> files, Func<int, ChunkStream?> beginDownload)
    {
        VirtualFileClipboard.PutOnClipboard(new VirtualFileClipboard(files, beginDownload));
    }

    public static bool IsCurrent() => VirtualFileClipboard.IsCurrent(null);

    public static void ClearIfOurs() => VirtualFileClipboard.ClearIfOurs();
}

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public sealed class ChunkStream : IStream
{
    private readonly object _gate = new();
    private readonly FileStream _file;
    private readonly long _size;
    private long _written;
    private long _position;
    private bool _done;

    public long Written
    {
        get { lock (_gate) return _written; }
    }

    public ChunkStream(long size)
    {
        _size = size < 0 ? 0 : size;
        string path = Path.Combine(Path.GetTempPath(), "kdock-" + Guid.NewGuid().ToString("N"));
        _file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 65536, FileOptions.DeleteOnClose);
    }

    public void Add(byte[] data)
    {
        if (data.Length == 0)
            return;
        lock (_gate)
        {
            _file.Position = _written;
            _file.Write(data, 0, data.Length);
            _written += data.Length;
            Monitor.PulseAll(_gate);
        }
    }

    public void Complete()
    {
        lock (_gate)
        {
            _done = true;
            Monitor.PulseAll(_gate);
        }
    }

    public int Read(IntPtr pv, int cb, IntPtr pcbRead)
    {
        if (pv == IntPtr.Zero || cb < 0)
            return unchecked((int)0x80030001);
        if (cb > 1024 * 1024)
            cb = 1024 * 1024;
        int copied = 0;
        DateTime deadline = DateTime.UtcNow.AddMinutes(3);
        byte[] buffer = new byte[cb];
        while (copied == 0)
        {
            lock (_gate)
            {
                long available = _written - _position;
                if (available > 0)
                {
                    int count = (int)Math.Min(cb, available);
                    _file.Position = _position;
                    copied = _file.Read(buffer, 0, count);
                    _position += copied;
                }
                else if (_done || DateTime.UtcNow >= deadline)
                {
                    break;
                }
                else
                {
                    Monitor.Wait(_gate, 30);
                }
            }
        }
        if (copied > 0)
            Marshal.Copy(buffer, 0, pv, copied);
        if (pcbRead != IntPtr.Zero)
            Marshal.WriteInt32(pcbRead, copied);
        return 0;
    }

    public int Write(IntPtr pv, int cb, IntPtr pcbWritten) => unchecked((int)0x80030005);

    public int Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
    {
        lock (_gate)
        {
            long next = dwOrigin switch
            {
                0 => dlibMove,
                1 => _position + dlibMove,
                _ => _size + dlibMove
            };
            if (next < 0)
                next = 0;
            _position = next;
            if (plibNewPosition != IntPtr.Zero)
                Marshal.WriteInt64(plibNewPosition, _position);
        }
        return 0;
    }

    public int SetSize(long libNewSize) => 0;
    public int CopyTo(IntPtr pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten) => unchecked((int)0x80004001);
    public int Commit(int grfCommitFlags) => 0;
    public int Revert() => 0;
    public int LockRegion(long libOffset, long cb, int dwLockType) => 0;
    public int UnlockRegion(long libOffset, long cb, int dwLockType) => 0;

    public int Stat(out STATSTG pstatstg, int grfStatFlag)
    {
        pstatstg = new STATSTG { type = 2, cbSize = _size, grfMode = 2 };
        return 0;
    }

    public int Clone(out IStream ppstm)
    {
        ppstm = null!;
        return unchecked((int)0x80004001);
    }
}

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class VirtualFileClipboard : IDataObject
{
    private const int Ok = 0;
    private const int NotImpl = unchecked((int)0x80004001);
    private const int BadFormat = unchecked((int)0x80040064);
    private const int BadTymed = unchecked((int)0x80040069);
    private const int BadIndex = unchecked((int)0x80040068);
    private const int NoAdvise = unchecked((int)0x80040003);
    private const uint TymedHGlobal = 1;
    private const uint TymedStream = 4;

    private static VirtualFileClipboard? _alive;
    private readonly int _generation;
    private readonly List<ClipOffer> _files;
    private readonly Func<int, ChunkStream?> _begin;
    private readonly List<ChunkStream> _streams = new();
    private readonly Dictionary<int, ChunkStream> _byIndex = new();
    private static int _generationSource;
    private static ushort _descriptor;
    private static ushort _contents;
    private static ushort _dropEffect;
    private static ushort _exclude;

    public VirtualFileClipboard(IReadOnlyList<ClipOffer> files, Func<int, ChunkStream?> begin)
    {
        _generation = Interlocked.Increment(ref _generationSource);
        _files = files.ToList();
        _begin = begin;
        EnsureFormats();
    }

    public static void PutOnClipboard(VirtualFileClipboard offer)
    {
        _alive = offer;
        OleSetClipboard(offer);
    }

    public static bool IsCurrent(VirtualFileClipboard? offer)
    {
        offer ??= _alive;
        return offer != null && OleIsCurrentClipboard(offer) == 0;
    }

    public static void ClearIfOurs()
    {
        if (_alive != null && OleIsCurrentClipboard(_alive) == 0)
        {
            OleSetClipboard(null);
            _alive = null;
        }
    }

    public int GetData(ref FORMATETC format, out STGMEDIUM medium)
    {
        medium = new STGMEDIUM();
        try
        {
            EnsureFormats();
            if (format.cfFormat == _descriptor)
            {
                medium.tymed = TymedHGlobal;
                medium.unionmember = BuildDescriptor();
                return Ok;
            }
            if (format.cfFormat == _dropEffect)
            {
                medium.tymed = TymedHGlobal;
                medium.unionmember = AllocBytes(BitConverter.GetBytes(1));
                return Ok;
            }
            if (format.cfFormat == _exclude)
            {
                medium.tymed = TymedHGlobal;
                medium.unionmember = AllocBytes(new byte[] { 0 });
                return Ok;
            }
            if (format.cfFormat == _contents)
            {
                int index = format.lindex;
                if (index < 0 && _files.Count == 1)
                    index = 0;
                if (index < 0 || index >= _files.Count)
                    return BadIndex;
                if (_byIndex.TryGetValue(index, out ChunkStream? existing))
                {
                    medium.tymed = TymedStream;
                    medium.unionmember = Marshal.GetIUnknownForObject(existing);
                    return Ok;
                }
                ChunkStream? stream = _begin(index);
                if (stream == null)
                    return BadIndex;
                _byIndex[index] = stream;
                _streams.Add(stream);
                medium.tymed = TymedStream;
                medium.unionmember = Marshal.GetIUnknownForObject(stream);
                return Ok;
            }
        }
        catch
        {
        }
        return BadFormat;
    }

    public int GetDataHere(ref FORMATETC format, ref STGMEDIUM medium) => NotImpl;

    public int QueryGetData(ref FORMATETC format)
    {
        try
        {
            EnsureFormats();
            uint needed;
            if (format.cfFormat == _descriptor || format.cfFormat == _dropEffect || format.cfFormat == _exclude)
                needed = TymedHGlobal;
            else if (format.cfFormat == _contents)
                needed = TymedStream;
            else
                return BadFormat;
            if (format.tymed != 0 && (format.tymed & needed) == 0)
                return BadTymed;
            if (format.cfFormat == _contents && format.lindex >= _files.Count)
                return BadIndex;
            return Ok;
        }
        catch
        {
            return BadFormat;
        }
    }

    public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
    {
        formatOut = formatIn;
        formatOut.ptd = IntPtr.Zero;
        return BadFormat;
    }

    public int SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release) => NotImpl;

    public int EnumFormatEtc(uint direction, out IEnumFORMATETC ppenum)
    {
        ppenum = null!;
        if (direction != 1)
            return NotImpl;
        try
        {
            EnsureFormats();
            ppenum = new FormatEnum(new[]
            {
                Make(_descriptor, TymedHGlobal),
                Make(_contents, TymedStream),
                Make(_dropEffect, TymedHGlobal),
                Make(_exclude, TymedHGlobal)
            });
            return Ok;
        }
        catch
        {
            return NotImpl;
        }
    }

    public int DAdvise(ref FORMATETC pFormatetc, uint advf, IntPtr adviseSink, out uint connection)
    {
        connection = 0;
        return NoAdvise;
    }

    public int DUnadvise(uint connection) => NoAdvise;

    public int EnumDAdvise(out IntPtr ppenumAdvise)
    {
        ppenumAdvise = IntPtr.Zero;
        return NoAdvise;
    }

    private IntPtr BuildDescriptor()
    {
        int one = Marshal.SizeOf<FileDescriptor>();
        int count = _files.Count;
        IntPtr memory = Marshal.AllocHGlobal(4 + one * Math.Max(1, count));
        Marshal.WriteInt32(memory, count);
        for (int i = 0; i < count; i++)
        {
            long size = _files[i].Size;
            FileDescriptor descriptor = new()
            {
                dwFlags = 0x40 | 0x4,
                dwFileAttributes = 0x80,
                nFileSizeLow = (uint)(size & 0xFFFFFFFF),
                nFileSizeHigh = (uint)(size >> 32),
                cFileName = _files[i].Name
            };
            Marshal.StructureToPtr(descriptor, IntPtr.Add(memory, 4 + one * i), false);
        }
        return memory;
    }

    private static void EnsureFormats()
    {
        if (_descriptor != 0)
            return;
        _descriptor = (ushort)RegisterClipboardFormat("FileGroupDescriptorW");
        _contents = (ushort)RegisterClipboardFormat("FileContents");
        _dropEffect = (ushort)RegisterClipboardFormat("Preferred DropEffect");
        _exclude = (ushort)RegisterClipboardFormat("ExcludeClipboardContentFromMonitorProcessing");
    }

    private static FORMATETC Make(ushort format, uint tymed) => new()
    {
        cfFormat = format,
        dwAspect = 1,
        lindex = -1,
        tymed = tymed
    };

    private static IntPtr AllocBytes(byte[] data)
    {
        IntPtr memory = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, memory, data.Length);
        return memory;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterClipboardFormat(string name);

    [DllImport("ole32.dll")]
    private static extern int OleSetClipboard(IDataObject? data);

    [DllImport("ole32.dll")]
    private static extern int OleIsCurrentClipboard(IDataObject data);
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FileDescriptor
{
    public uint dwFlags;
    public Guid clsid;
    public int cx;
    public int cy;
    public int x;
    public int y;
    public uint dwFileAttributes;
    public int ftCreationLow;
    public int ftCreationHigh;
    public int ftAccessLow;
    public int ftAccessHigh;
    public int ftWriteLow;
    public int ftWriteHigh;
    public uint nFileSizeHigh;
    public uint nFileSizeLow;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string cFileName;
}

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class FormatEnum : IEnumFORMATETC
{
    private readonly FORMATETC[] _items;
    private int _index;

    public FormatEnum(FORMATETC[] items) => _items = items;

    public int Next(uint celt, IntPtr rgelt, IntPtr pceltFetched)
    {
        uint fetched = 0;
        int size = Marshal.SizeOf<FORMATETC>();
        if (rgelt != IntPtr.Zero)
        {
            while (fetched < celt && _index < _items.Length)
            {
                Marshal.StructureToPtr(_items[_index], IntPtr.Add(rgelt, (int)fetched * size), false);
                _index++;
                fetched++;
            }
        }
        if (pceltFetched != IntPtr.Zero)
            Marshal.WriteInt32(pceltFetched, (int)fetched);
        return fetched == celt ? 0 : 1;
    }

    public int Skip(uint celt)
    {
        _index += (int)celt;
        if (_index > _items.Length)
        {
            _index = _items.Length;
            return 1;
        }
        return 0;
    }

    public int Reset()
    {
        _index = 0;
        return 0;
    }

    public int Clone(out IEnumFORMATETC ppenum)
    {
        FormatEnum copy = new(_items) { _index = _index };
        ppenum = copy;
        return 0;
    }
}

[ComImport]
[Guid("0000010E-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDataObject
{
    [PreserveSig] int GetData(ref FORMATETC pFormatetc, out STGMEDIUM pmedium);
    [PreserveSig] int GetDataHere(ref FORMATETC pFormatetc, ref STGMEDIUM pmedium);
    [PreserveSig] int QueryGetData(ref FORMATETC pFormatetc);
    [PreserveSig] int GetCanonicalFormatEtc(ref FORMATETC pFormatetcIn, out FORMATETC pFormatetcOut);
    [PreserveSig] int SetData(ref FORMATETC pFormatetc, ref STGMEDIUM pmedium, [MarshalAs(UnmanagedType.Bool)] bool fRelease);
    [PreserveSig] int EnumFormatEtc(uint dwDirection, out IEnumFORMATETC ppenumFormatEtc);
    [PreserveSig] int DAdvise(ref FORMATETC pFormatetc, uint advf, IntPtr pAdvSink, out uint pdwConnection);
    [PreserveSig] int DUnadvise(uint dwConnection);
    [PreserveSig] int EnumDAdvise(out IntPtr ppenumAdvise);
}

[ComImport]
[Guid("00000103-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IEnumFORMATETC
{
    [PreserveSig] int Next(uint celt, IntPtr rgelt, IntPtr pceltFetched);
    [PreserveSig] int Skip(uint celt);
    [PreserveSig] int Reset();
    [PreserveSig] int Clone(out IEnumFORMATETC ppenum);
}

[ComImport]
[Guid("0000000C-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IStream
{
    [PreserveSig] int Read(IntPtr pv, int cb, IntPtr pcbRead);
    [PreserveSig] int Write(IntPtr pv, int cb, IntPtr pcbWritten);
    [PreserveSig] int Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition);
    [PreserveSig] int SetSize(long libNewSize);
    [PreserveSig] int CopyTo(IntPtr pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);
    [PreserveSig] int Commit(int grfCommitFlags);
    [PreserveSig] int Revert();
    [PreserveSig] int LockRegion(long libOffset, long cb, int dwLockType);
    [PreserveSig] int UnlockRegion(long libOffset, long cb, int dwLockType);
    [PreserveSig] int Stat(out STATSTG pstatstg, int grfStatFlag);
    [PreserveSig] int Clone(out IStream ppstm);
}

[StructLayout(LayoutKind.Sequential)]
internal struct FORMATETC
{
    public ushort cfFormat;
    public IntPtr ptd;
    public uint dwAspect;
    public int lindex;
    public uint tymed;
}

[StructLayout(LayoutKind.Sequential)]
internal struct STGMEDIUM
{
    public uint tymed;
    public IntPtr unionmember;
    public IntPtr pUnkForRelease;
}

[StructLayout(LayoutKind.Sequential)]
public struct STATSTG
{
    public IntPtr pwcsName;
    public uint type;
    public long cbSize;
    public long mtime;
    public long ctime;
    public long atime;
    public uint grfMode;
    public uint grfLocksSupported;
    public Guid clsid;
    public uint grfStateBits;
    public uint reserved;
}
