using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kdock
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && string.Equals(args[1], "--broker", StringComparison.OrdinalIgnoreCase))
            {
                int port = 12605;
                if (args.Length > 2)
                    int.TryParse(args[2], out port);
                AllocConsole();
                DeskBroker.Run(port);
                return;
            }
            Application.Run(new KdockMain());
        }
    }
}
