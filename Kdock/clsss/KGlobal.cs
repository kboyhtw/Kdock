using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public static class KGlobal
    {

        //static KGlobal()
        //{
        //    kdockmain = new KdockMain();
        //}
        public static KdockMain kdockmain;
        public static Connect ClientConnect;
        public static web oweb;
        public static LAN oLAN;
        public static Note oNote;
        public static ActivePlace activeplace_ = new ActivePlace();
        public static frmNotify notify;
        public static frmAccess access = new frmAccess();

        public static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
        public static Color mainfont = new Color(), appfont = new Color(), mouseH = new Color(), mouseL = new Color();
        public static bool b = false;
        public static bool setl = true;
        public static Color PixColor;
        public static Apps appd = new Apps() { mode = "A" };
        public static Apps Webd = new Apps() { mode = "W" };
        public static Apps LANd = new Apps() { mode = "L" };
        public static string path = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Task\");
        public static string readvalue = "";// Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "kdock", null).ToString();
        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern int GetAsyncKeyState(long vKey);
        public static Theme GTheme = new Theme();
        public static Settings setting = new Settings();
        public static char smallsep = (char)240;
        public static char midsep = (char)241;
        public static char endsep = (char)242;
        public static char mainsep = (char)243;

        public static int FileBufferSize = 1024*500; 

        public static int RmQuality = 50;

        public static string HostName;
        public static int port = 8000;
        public static int PortString = 7777;
        public static int portFile = 8080;
        public static string myAddress;
        public static TcpListener Listener = new TcpListener(System.Net.IPAddress.Any, 8000);
        public static TcpListener NetworkListener = new TcpListener(System.Net.IPAddress.Any, 12604);
        public static TcpListener ListenerString;
        public static TcpListener ListenerFile = new TcpListener(System.Net.IPAddress.Any, 8081);

        public static List<string> AccessingPC = new List<string>();

        public static DataTable tblAccesss = new DataTable();

        public static List<RemoteClient> RemoteClients = new List<RemoteClient>();


        public static bool TRConatins(string tname)
        {
            bool b = false;
            foreach (RemoteClient tr in RemoteClients)
            {
                if ((tr.name == tname))
                {
                    b = true;
                    break;
                }
            }
            return b;
        }

        private static void TRRemove(string tname)
        {
            foreach (RemoteClient tr in RemoteClients)
            {
                if ((tr.name == tname))
                {
                    try
                    {
                        //  tr.Abort()
                    }
                    catch (Exception ex)
                    {
                    }

                    RemoteClients.Remove(tr);
                    break;
                }

            }

        }

        private static void TRStart(string tname)
        {
            foreach (RemoteClient tr in RemoteClients)
            {
                if ((tr.name == tname))
                {
                    try
                    {
                        //  tr.Start()
                    }
                    catch (Exception ex)
                    {
                    }

                    break;
                }

            }

        }


        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }
        public static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void StopLANThreads(Control Pcontrol)
        {
            foreach (var ctrl in Pcontrol.Controls)
            {
                if (ctrl is LanPC)
                    ((LanPC)ctrl).ThreadCheck(false);
            }
        }

        public static Bitmap GetScreenShot(Size Size, Point startpoint)
        {
            Bitmap Bmp = new Bitmap(Size.Width, Size.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            using (Graphics G = Graphics.FromImage(Bmp))
            {
                G.CopyFromScreen(startpoint.X, startpoint.Y, 0, 0, Size);
            }
            return Bmp;
        }
        public static void RemoveLAN(string sname)
        {
            string[] shrt = setting.LPC.Split(midsep);
            bool bRemoved = false;
            for (int i = 0; i <= shrt.Length - 2; i++)
            {
                if (sname == shrt[i].Split(endsep)[0])
                {
                    setting.LPC = setting.LPC.Replace(shrt[i].Split(endsep)[0] + endsep + shrt[i].Split(endsep)[1] + midsep, "");
                    bRemoved = true;
                }
            }
            if (bRemoved == false)
            {
                for (int i = 0; i <= shrt.Length - 2; i++)
                {
                    if (sname == shrt[i].Split(endsep)[1])
                    {
                        setting.LPC = setting.LPC.Replace(shrt[i].Split(endsep)[0] + endsep + shrt[i].Split(endsep)[1] + midsep, "");
                        bRemoved = true;
                    }
                }
            }

            setting.Save(path);
        }
        public static bool WriteTextToStream(NetworkStream CStream, string data)
        {
            try
            {
                data = (data + "\r");
                byte[] databyte = System.Text.ASCIIEncoding.Default.GetBytes(data);
                CStream.Write(databyte, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void ggetTheme()
        {
            if (File.Exists(KGlobal.path + @"\theme.k"))
            {
                GTheme = Theme.Parse(File.ReadAllText(KGlobal.path + @"\theme.k"));
            }
            else
            {
                GTheme = new Theme();
            }
        }

        public static void LoadAccessTable()
        {
            tblAccesss.Columns.Add("IP", typeof(string));
            tblAccesss.Columns.Add("Delete", typeof(bool));
            tblAccesss.Columns.Add("CM", typeof(bool));
            tblAccesss.Columns.Add("Get", typeof(bool));
            tblAccesss.Columns.Add("Give", typeof(bool));
            DataColumn[] primaryKey = new DataColumn[2];
            primaryKey[0] = tblAccesss.Columns[0];
            tblAccesss.PrimaryKey = primaryKey;
        }
        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        public static void Invoke(Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(new MethodInvoker(() => action()), null/* TODO Change to default(_) if this is not a reference type */);
            else
                action.Invoke();
        }






        public static void search(string sdata)
        {
            if (sdata.Contains("https://"))
            {
                if (sdata.Trim().Substring(0, 8) == "https://")
                    Process.Start(sdata.Trim());
                else
                    Process.Start("http://www.google.com/search?hl=en&q=" + sdata.Replace(" ", "+") + "&aq=f&oq=");
            }
            else if (sdata.Contains("http://"))
            {
                if (sdata.Trim().Substring(0, 7) == "http://")
                    Process.Start(sdata.Trim());
                else
                    Process.Start("http://www.google.com/search?hl=en&q=" + sdata.Replace(" ", "+") + "&aq=f&oq=");
            }
            else if (sdata.Contains("www."))
            {
                if (sdata.Trim().Substring(0, 4) == "www.")
                    Process.Start(sdata.Trim());
                else
                    Process.Start("http://www.google.com/search?hl=en&q=" + sdata.Replace(" ", "+") + "&aq=f&oq=");
            }
            else if (sdata.Trim() != "")
                Process.Start("http://www.google.com/search?hl=en&q=" + sdata.Replace(" ", "+") + "&aq=f&oq=");
        }

        public static void FlushMemory()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if ((Environment.OSVersion.Platform == PlatformID.Win32NT))
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
