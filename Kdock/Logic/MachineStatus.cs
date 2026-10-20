using System.Runtime.InteropServices;

namespace Kdock.Logic
{
    public static class MachineStatus
    {
        public static void ReadMemory(out float total, out float available)
        {
            MemoryStatus status = new MemoryStatus();
            status.Length = (uint)Marshal.SizeOf(typeof(MemoryStatus));
            if (!GlobalMemoryStatusEx(ref status))
            {
                total = 1;
                available = 0;
                return;
            }
            total = status.TotalPhysical;
            available = status.AvailablePhysical;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryStatus
        {
            public uint Length;
            public uint MemoryLoad;
            public ulong TotalPhysical;
            public ulong AvailablePhysical;
            public ulong TotalPageFile;
            public ulong AvailablePageFile;
            public ulong TotalVirtual;
            public ulong AvailableVirtual;
            public ulong AvailableExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
    }
}
