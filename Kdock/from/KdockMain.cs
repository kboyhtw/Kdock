using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;
using System.Net.Sockets;
using Microsoft.VisualBasic.Devices;

namespace Kdock
{
    public partial class KdockMain : Form
    {
        public KdockMain()
        {
            InitializeComponent();
        }



        // <DllImport("user32.dll", SetLastError:=True)> _
        // Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As Integer
        // End Function
        // <DllImport("user32.dll", SetLastError:=True)> _
        // Private Shared Function FindWindow(lpWindowClass As String, lpWindowName As String) As IntPtr
        // End Function
        // <DllImport("user32.dll", SetLastError:=True)> _
        // Private Shared Function FindWindowEx(parentHandle As IntPtr, childAfter As IntPtr, className As String, windowTitle As String) As IntPtr
        // End Function
        // Const GWL_HWNDPARENT As Integer = -8
        // Private Const SHERB_NOCONFIRMATION As Long = &H1
        //  Private Const SHERB_NOPROGRESSUI As Long = &H2
        //  Private Const SHERB_NOSOUND As Long = &H4
        //  Private Structure ULARGE_INTEGER
        // Dim LowPart As Integer
        //  Dim HighPart As Integer
        //   End Structure
        //  Private Structure SHQUERYRBINFO
        // Dim cbSize As Int32
        //  Dim i64Size As ULARGE_INTEGER
        //  Dim i64NumItems As ULARGE_INTEGER
        //  End Structure
        //  Private Structure details
        // Dim numberOFitems As Integer
        // Dim itemsSize As String
        // End Structure
        private string GetIPv4Address()
        {
            string mac = String.Empty;
            string strHostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry iphe = System.Net.Dns.GetHostEntry(strHostName);
            foreach (System.Net.IPAddress ipheal in iphe.AddressList)
            {
                if ((ipheal.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    mac = ipheal.ToString();
                    break;
                }

            }
            return mac;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Dim hprog As IntPtr = FindWindowEx(FindWindowEx(FindWindow("Progman", "Program Manager"), IntPtr.Zero, "SHELLDLL_DefView", ""), IntPtr.Zero, "SysListView32", "FolderView")
            // SetWindowLong(Me.Handle, GWL_HWNDPARENT, hprog)    
            //List<string> processes = new List<string>();
            //foreach (var item in Process .GetProcesses())
            //{
            //    processes.Add(item.ProcessName);
            //}

            Bitmap img = KGlobal.GetScreenShot(new Size(1920, 1080), new Point(0, 0));
            NetworkPackage np = new NetworkPackage("Hello kboy", img);
            byte[] result = np.ToByte();

            NetworkPackage np1 = new NetworkPackage(ref result);
            ((Bitmap)np1.Data).Save("test123.jpg");  

            KGlobal.kdockmain = this;
            this.getTheme();

            KGlobal.LoadAccessTable();
            if (KGlobal.readvalue == "")
            {
                ONToolStripMenuItem.Enabled = true;
                OFFToolStripMenuItem.Enabled = false;
            }
            else
            {
                ONToolStripMenuItem.Enabled = false;
                OFFToolStripMenuItem.Enabled = true;
            }

            PictureBox1.Visible = false;
            LockToolStripMenuItem.Text = "Unlock";
            Label4.Text = " " + Environment.MachineName + " ";
            Label14.Text = " " + (new ComputerInfo()).OSFullName + " ";
            Label15.Text = " " + (new ComputerInfo()).OSVersion + " ";
            if (!Directory.Exists(KGlobal.path))
            {
                Directory.CreateDirectory(KGlobal.path);
            }

            string s = (KGlobal.path + "\\settings.k");
            if (!File.Exists(s))
            {
                KGlobal.setting.Save(KGlobal.path);
                System.IO.File.SetAttributes(s, System.IO.FileAttributes.Hidden);
            }

            try
            {
                this.SettingUp(s);
            }
            catch (Exception ex)
            {
                try
                {
                    File.Copy(KGlobal.path + "\\Backsettings.k", KGlobal.path + "\\settings.k");
                    this.SettingUp(s);
                }
                catch (Exception ex1)
                {
                    KGlobal.setting.Save(KGlobal.path);
                    this.SettingUp(s);
                }

            }

            KGlobal.appd.Close();
            KGlobal.Webd.Close();
            KGlobal.LANd.Close();
        }

        public void SettingUp(string s)
        {
            KGlobal.setting.Load(KGlobal.path);
            this.Location = KGlobal.setting.apppos;
            this.Opacity = double.Parse(KGlobal.setting.opa);
            switch (KGlobal.setting.lay)
            {
                case "l":
                    this.leftside();
                    break;
                case "r":
                    this.rightside();
                    break;
                case "c":
                    this.centerside();
                    break;
            }
            if (!(KGlobal.setting.ap == ""))
            {
                ShowAP(KGlobal.setting.ap);
            }

            if ((KGlobal.setting.LanEng == "1"))
            {
                if (KGlobal.ClientConnect == null)
                {
                    Connect frm = new Connect();
                    KGlobal.ClientConnect = frm;
                }
                KGlobal.ClientConnect.Show();
                KGlobal.ClientConnect.Hide();
                GoOnlineToolStripMenuItem.Text = "Go Offline";
            }
            else
            {
                GoOnlineToolStripMenuItem.Text = "Go Online";
            }

        }

        public void getTheme()
        {
            KGlobal.ggetTheme();
            PanelMain.BackColor = KGlobal.GTheme.clr0;
            lblDateTime.LeaveColor = KGlobal.GTheme.clr0;
            lblDay.LeaveColor = KGlobal.GTheme.clr0;
            lblShutdown.BackColor = KGlobal.GTheme.clr0;
            lblRestart.BackColor = KGlobal.GTheme.clr0;
            lblLogout.BackColor = KGlobal.GTheme.clr0;
            Klabel1.BackColor = KGlobal.GTheme.clr0;
            KGlobal.mainfont = KGlobal.GTheme.clr1;
            Label4.LeaveColor = KGlobal.GTheme.clr2;
            Label14.LeaveColor = KGlobal.GTheme.clr2;
            Label15.LeaveColor = KGlobal.GTheme.clr2;
            lblShutdown.HoverColor = KGlobal.GTheme.clr2;
            lblRestart.HoverColor = KGlobal.GTheme.clr2;
            lblLogout.HoverColor = KGlobal.GTheme.clr2;
            Label3.LeaveColor = KGlobal.GTheme.clr3;
            Label6.LeaveColor = KGlobal.GTheme.clr3;
            lblShutdown.LeaveColor = KGlobal.GTheme.clr3;
            Klabel1.LeaveColor = KGlobal.GTheme.clr3;
            lblapps.BackColor = KGlobal.GTheme.clr4;
            lblweb.BackColor = KGlobal.GTheme.clr4;
            lblLAN.BackColor = KGlobal.GTheme.clr4;
            KGlobal.appfont = KGlobal.GTheme.clr5;
            PanelDrawer.BackColor = KGlobal.GTheme.clr6;
            lblDateTime.BackColor = KGlobal.GTheme.clr6;
            lblDay.BackColor = KGlobal.GTheme.clr6;
            lblmypc.BackColor = KGlobal.GTheme.clr6;
            lblcp.BackColor = KGlobal.GTheme.clr6;
            lbluser.BackColor = KGlobal.GTheme.clr6;
            lblrb.BackColor = KGlobal.GTheme.clr6;
            lblnetwork.BackColor = KGlobal.GTheme.clr6;
            flpsh.BackColor = KGlobal.GTheme.clr6;
            lblsrl.BackColor = KGlobal.GTheme.clr6;
            lblRestart.LeaveColor = KGlobal.GTheme.clr6;
            lblLogout.LeaveColor = KGlobal.GTheme.clr6;
            KGlobal.mouseH = KGlobal.GTheme.clr7;
            lblmypc.HoverColor = KGlobal.mouseH;
            lblcp.HoverColor = KGlobal.mouseH;
            lbluser.HoverColor = KGlobal.mouseH;
            lblrb.HoverColor = KGlobal.mouseH;
            lblweb.HoverColor = KGlobal.mouseH;
            lblnetwork.HoverColor = KGlobal.mouseH;
            lblapps.HoverColor = KGlobal.mouseH;
            lblsrl.HoverColor = KGlobal.mouseH;
            lblLAN.HoverColor = KGlobal.mouseH;
            KGlobal.mouseL = KGlobal.GTheme.clr8;
            PanelDrawer.ForeColor = KGlobal.mouseL;
            lblmypc.LeaveColor = KGlobal.mouseL;
            lblcp.LeaveColor = KGlobal.mouseL;
            lbluser.LeaveColor = KGlobal.mouseL;
            lblrb.LeaveColor = KGlobal.mouseL;
            lblweb.LeaveColor = KGlobal.mouseL;
            lblnetwork.LeaveColor = KGlobal.mouseL;
            lblapps.LeaveColor = KGlobal.mouseL;
            lblsrl.LeaveColor = KGlobal.mouseL;
            lblLAN.LeaveColor = KGlobal.mouseL;
        }

        public void setTheme()
        {
            //KGlobal.gsetTheme();
            PanelMain.BackColor = KGlobal.GTheme.clr0;
            lblDateTime.LeaveColor = KGlobal.GTheme.clr0;
            lblDay.LeaveColor = KGlobal.GTheme.clr0;
            lblShutdown.BackColor = KGlobal.GTheme.clr0;
            lblRestart.BackColor = KGlobal.GTheme.clr0;
            lblLogout.BackColor = KGlobal.GTheme.clr0;
            Klabel1.BackColor = KGlobal.GTheme.clr0;
            KGlobal.mainfont = KGlobal.GTheme.clr1;
            Label4.LeaveColor = KGlobal.GTheme.clr2;
            Label14.LeaveColor = KGlobal.GTheme.clr2;
            Label15.LeaveColor = KGlobal.GTheme.clr2;
            lblShutdown.HoverColor = KGlobal.GTheme.clr2;
            lblRestart.HoverColor = KGlobal.GTheme.clr2;
            lblLogout.HoverColor = KGlobal.GTheme.clr2;
            Label3.LeaveColor = KGlobal.GTheme.clr3;
            Label6.LeaveColor = KGlobal.GTheme.clr3;
            lblShutdown.LeaveColor = KGlobal.GTheme.clr3;
            Klabel1.LeaveColor = KGlobal.GTheme.clr3;
            lblapps.BackColor = KGlobal.GTheme.clr4;
            lblweb.BackColor = KGlobal.GTheme.clr4;
            lblLAN.BackColor = KGlobal.GTheme.clr4;
            KGlobal.appfont = KGlobal.GTheme.clr5;
            PanelDrawer.BackColor = KGlobal.GTheme.clr6;
            lblDateTime.BackColor = KGlobal.GTheme.clr6;
            lblDay.BackColor = KGlobal.GTheme.clr6;
            lblmypc.BackColor = KGlobal.GTheme.clr6;
            lblcp.BackColor = KGlobal.GTheme.clr6;
            lbluser.BackColor = KGlobal.GTheme.clr6;
            lblrb.BackColor = KGlobal.GTheme.clr6;
            lblnetwork.BackColor = KGlobal.GTheme.clr6;
            flpsh.BackColor = KGlobal.GTheme.clr6;
            lblsrl.BackColor = KGlobal.GTheme.clr6;
            lblRestart.LeaveColor = KGlobal.GTheme.clr6;
            lblLogout.LeaveColor = KGlobal.GTheme.clr6;
            KGlobal.mouseH = KGlobal.GTheme.clr7;
            lblmypc.HoverColor = KGlobal.mouseH;
            lblcp.HoverColor = KGlobal.mouseH;
            lbluser.HoverColor = KGlobal.mouseH;
            lblrb.HoverColor = KGlobal.mouseH;
            lblweb.HoverColor = KGlobal.mouseH;
            lblnetwork.HoverColor = KGlobal.mouseH;
            lblapps.HoverColor = KGlobal.mouseH;
            lblsrl.HoverColor = KGlobal.mouseH;
            lblLAN.HoverColor = KGlobal.mouseH;
            KGlobal.mouseL = KGlobal.GTheme.clr8;
            PanelDrawer.ForeColor = KGlobal.mouseL;
            lblmypc.LeaveColor = KGlobal.mouseL;
            lblcp.LeaveColor = KGlobal.mouseL;
            lbluser.LeaveColor = KGlobal.mouseL;
            lblrb.LeaveColor = KGlobal.mouseL;
            lblweb.LeaveColor = KGlobal.mouseL;
            lblnetwork.LeaveColor = KGlobal.mouseL;
            lblapps.LeaveColor = KGlobal.mouseL;
            lblsrl.LeaveColor = KGlobal.mouseL;
            lblLAN.LeaveColor = KGlobal.mouseL;
            if (Application.OpenForms.OfType<Note>().Any())
            {
                KGlobal.oNote.Opacity = this.Opacity;
                KGlobal.oNote.BackColor = PanelMain.BackColor;
                KGlobal.oNote.txtnote.BackColor = PanelMain.BackColor;
                KGlobal.oNote.txtnote.ForeColor = Label4.ForeColor;
                KGlobal.oNote.lblAttached.ForeColor = Label4.ForeColor;
            }

            if (Application.OpenForms.OfType<web>().Any())
            {
                KGlobal.oweb.updatetheme();
            }

        }

        private System.Drawing.Bitmap BMP = new System.Drawing.Bitmap(1, 1);

        private System.Drawing.Graphics GFX;

        private int i1 = 0;

        private void TimerPro_Tick(object sender, EventArgs e)
        {
            int c;
            int ram;
            // If My.Computer.Network.IsAvailable = True Then
            //     Label5.ForeColor = Color.Gainsboro
            //     ToolTip1.SetToolTip(Label5, "Network Is Connected")
            //     i1 += 1
            //     If i1 = 1 Then
            //         Label5.Text = "(("))"
            //     ElseIf i1 = 2 Then
            //         Label5.Text = "(")"
            //         i1 = 0
            //     End If
            // Else
            //     Label5.Text = """
            //     ToolTip1.SetToolTip(Label5, "Network Is Disconnected")
            //     Label5.ForeColor = Color.Red
            // End If
            //  Console.Beep()
            try
            {
                if (KGlobal.GTheme.Auto)
                {
                    GFX = System.Drawing.Graphics.FromImage(BMP);
                    GFX.CopyFromScreen(new System.Drawing.Point((this.Location.X - 1), (this.Location.Y - 1)), new Point(0, 0), BMP.Size);
                    Color Pixel = BMP.GetPixel(0, 0);
                    if (KGlobal.PixColor != Pixel)
                    {
                        KGlobal.GTheme.clr0 = Color.FromArgb(Pixel.A, GetValue(Pixel.R), GetValue(Pixel.G), GetValue(Pixel.B));
                        KGlobal.GTheme.clr1 = Color.FromArgb(Pixel.A, GetValue(Pixel.R + 191), GetValue(Pixel.G + 191), GetValue(Pixel.B + 191));
                        KGlobal.GTheme.clr2 = Color.FromArgb(Pixel.A, GetValue(Pixel.R - 64), GetValue(Pixel.G + 127), GetValue(Pixel.B + 191));
                        KGlobal.GTheme.clr3 = Color.FromArgb(Pixel.A, GetValue(Pixel.R + 191), GetValue(Pixel.G - 64), GetValue(Pixel.B - 64));
                        KGlobal.GTheme.clr4 = Color.FromArgb(Pixel.A, GetValue(Pixel.R + 191), GetValue(Pixel.G + 191), GetValue(Pixel.B + 191));
                        KGlobal.GTheme.clr5 = Color.FromArgb(Pixel.A, GetValue(Pixel.R + 64), GetValue(Pixel.G - 64), GetValue(Pixel.B - 64));
                        KGlobal.GTheme.clr6 = Color.FromArgb(Pixel.A, GetValue(Pixel.R + 156), GetValue(Pixel.G + 156), GetValue(Pixel.B + 156));
                        KGlobal.GTheme.clr7 = Color.FromArgb(Pixel.A, GetValue(Pixel.R - 64), GetValue(Pixel.G + 127), GetValue(Pixel.B + 191));
                        KGlobal.GTheme.clr8 = Color.FromArgb(Pixel.A, GetValue(Pixel.R - 64), GetValue(Pixel.G + 64), GetValue(Pixel.B + 191));
                        KGlobal.GTheme.Auto = true;
                        string theme;
                        theme = KGlobal.GTheme.ToString();
                        if (!File.Exists((KGlobal.path + "\\theme.k")))
                        {
                            File.AppendAllText((KGlobal.path + "\\theme.k"), theme);
                            File.SetAttributes((KGlobal.path + "\\theme.k"), FileAttributes.Hidden);
                        }
                        else
                        {
                            File.Delete((KGlobal.path + "\\theme.k"));
                            File.AppendAllText((KGlobal.path + "\\theme.k"), theme);
                            File.SetAttributes((KGlobal.path + "\\theme.k"), FileAttributes.Hidden);
                        }

                        this.setTheme();

                        KGlobal.PixColor = Pixel;
                    }

                }

            }
            catch (Exception ex)
            {
            }

            c = Convert.ToInt32(performanceCounter1.NextValue());

            float totalRAM = (float)(new ComputerInfo()).TotalPhysicalMemory;
            float availRAM = (float)(new ComputerInfo()).AvailablePhysicalMemory;
            ram = Convert.ToInt32((totalRAM - availRAM) * (100 / totalRAM));
            if (KGlobal.setl == true)
            {
                this.Location = KGlobal.setting.apppos;
            }

            lblDateTime.Text = DateTime.Now.ToString();
            lblDay.Text = DateTime.Now.DayOfWeek.ToString();
            Label3.Text = "CPU : " + c.ToString() + "%";
            Label6.Text = "Ram : " + ram.ToString() + "% ";
            if (File.Exists((KGlobal.path + "InMsg.k")))
            {
                Klabel1.Text = (File.ReadAllText((KGlobal.path + "InMsg.k")).Split(KGlobal.mainsep).Count() - 1).ToString();
            }

        }

        public int GetValue(int no)
        {
            if ((no < 0))
            {
                no = (no * -1);
            }

            if ((no > 255))
            {
                no = (no - 255);
            }

            return no;
        }

        private void Label10_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/n, ::{645FF040-5081-101B-9F08-00AA002F954E}");
        }

        private void SystemPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("control", "system");
        }

        private void Label7_Click(object sender, EventArgs e)
        {
            // Shell("explorer.exe """, AppWinStyle.NormalNoFocus)
            System.Diagnostics.Process.Start("explorer.exe", "/e,/select,c:");
        }

        private void Label9_Click(object sender, EventArgs e)
        {
            string s;
            s = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            System.Diagnostics.Process.Start(s);
        }

        private void Label8_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("Control");
        }

        [DllImport("shell32.dll", EntryPoint = "SHEmptyRecycleBinA")]
        static extern long SHEmptyRecycleBin(int hwnd, string pszRootPath, int dwFlags);

        [DllImport("shell32.dll")]
        static extern long SHUpdateRecycleBinIcon();

        private void EmptyRecycleBin()
        {
            SHEmptyRecycleBin(this.Handle.ToInt32(), "", 1);
            SHUpdateRecycleBinIcon();
        }

        private void EmptyRecycleBinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.EmptyRecycleBin();
        }

        // Private Sub ContextMenuStrip2_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStrip2.Opening
        // Dim SHQBI1 As SHQUERYRBINFO
        // If SHQBI1.i64NumItems.LowPart > 0 Then
        //     EmptyRecycleBinToolStripMenuItem.Enabled = True
        //   End If
        //  End Sub
        //  Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        //  Dim counter As Integer = System.IO.Directory.GetFiles("C:\$Recycle.Bin\S-1-5-21-132158221-3295442736-3676110784-1001").Length
        // Dim c As Integer = CStr(counter.Count)
        //   Dim counter1 = My.Computer.FileSystem.GetDirectories("C:\$Recycle.Bin\S-1-5-21-132158221-3295442736-3676110784-1001")
        //  Dim c1 As Integer = CStr(counter1.Count)
        // c = c / 2
        //  Dim c2 As Integer = counter + c1
        //  If c2 > 0 Then
        //  EmptyRecycleBinToolStripMenuItem.Enabled = True
        // ToolTip2.ToolTipTitle = counter & " Files " & c1 & " Folders" & " found that you have deleted"
        //  clr = Color.Red
        //  Else : EmptyRecycleBinToolStripMenuItem.Enabled = False
        //  ToolTip2.ToolTipTitle = "Recycle Bin is empty"
        //  clr = Color.Blue
        //  End If
        //   End Sub
        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void ToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Verb = "runas";
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception ex)
            {
            }

        }

        private void Label12_Click(object sender, EventArgs e)
        {
            Process.Start("shell:NetworkPlacesFolder");
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("::{208D2C60-3AEA-1069-A2D7-08002B30309D}");
        }

        private void NetworkAdeptorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/n,::{7007ACC7-3202-11D1-AAD2-00805FC1270E}");
        }

        private void ManageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("compmgmt.msc");
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = Cursor.Position;

        }

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            System.IO.File.SetAttributes((Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\Apps"), System.IO.FileAttributes.Normal);
        }

        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            System.IO.File.SetAttributes((Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\Apps"), System.IO.FileAttributes.Hidden);
        }

        private void LockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((PictureBox1.Visible == true))
            {
                PictureBox1.Visible = false;
                KGlobal.setting.apppos.X = this.Location.X;
                KGlobal.setting.apppos.Y = this.Location.Y;
                KGlobal.setting.Save(KGlobal.path);
                KGlobal.setting.apppos = this.Location;
                KGlobal.setl = true;
                LockToolStripMenuItem.Text = "Unlock";
            }
            else if ((PictureBox1.Visible == false))
            {
                PictureBox1.Visible = true;
                LockToolStripMenuItem.Text = "Lock";
                KGlobal.setl = false;
            }

        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private int idr = 0;

        private void TimerDrawer1_Tick(object sender, EventArgs e)
        {
            //  PictureBox4.Width = 173
            if ((PanelDrawer.Height < 300))
            {
                PanelDrawer.Height += 50;
            }
            else if (((PanelDrawer.Height >= 300)
                        && (PanelDrawer.Height < 330)))
            {
                PanelDrawer.Height += 2;
                idr += 2;
                flpsh.Visible = true;
                flpsh.Location = new Point(0, (137 + idr));
            }
            else if ((PanelDrawer.Height >= 330))
            {
                idr = 0;
                TimerDrawer1.Stop();
                lblsrl.Visible = true;
                PanelDrawer.Height = 330;
                flpsh.Visible = true;
                lblLAN.Visible = true;
            }

        }

        private void TimerDrawer2_Tick(object sender, EventArgs e)
        {
            this.TopMost = false;
            idr = 0;
            lblsrl.Visible = false;
            flpsh.Visible = false;
            lblShutdown.Visible = false;
            lblRestart.Visible = false;
            lblLogout.Visible = false;
            PanelDrawer.Height -= 50;
            if ((PanelDrawer.Height <= 10))
            {
                TimerDrawer2.Stop();
                PanelDrawer.Height = 10;
            }

        }





        private void ToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.25;
            KGlobal.setting.opa = "0.25";
            KGlobal.setting.Save(KGlobal.path);
        }

        private void ToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.5;
            KGlobal.setting.opa = "0.5";
            KGlobal.setting.Save(KGlobal.path);
        }

        private void ToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            this.Opacity = 0.75;
            KGlobal.setting.opa = "0.75";
            KGlobal.setting.Save(KGlobal.path);
        }

        private void ToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            this.Opacity = 1;
            KGlobal.setting.opa = "1";
            KGlobal.setting.Save(KGlobal.path);
        }

        public void leftside()
        {
            Label4.TextAlign = ContentAlignment.MiddleLeft;
            Label14.TextAlign = ContentAlignment.MiddleLeft;
            Label15.TextAlign = ContentAlignment.MiddleLeft;
            lblmypc.TextAlign = ContentAlignment.MiddleLeft;
            lblcp.TextAlign = ContentAlignment.MiddleLeft;
            lbluser.TextAlign = ContentAlignment.MiddleLeft;
            lblrb.TextAlign = ContentAlignment.MiddleLeft;
            lblweb.TextAlign = ContentAlignment.MiddleLeft;
            lblnetwork.TextAlign = ContentAlignment.MiddleLeft;
            lblapps.TextAlign = ContentAlignment.MiddleLeft;
            lblLAN.TextAlign = ContentAlignment.MiddleLeft;
            // Dim p As New Point
            // apppos.x = 15
            // apppos.y = 177
            // Label7.Location = p
            // apppos.y = 207
            // Label8.Location = p
            // apppos.y = 147
            // Label9.Location = p
            // apppos.y = 237
            // Label10.Location = p
            // apppos.y = 327
            // Label11.Location = p
            // apppos.y = 267
            // Label12.Location = p
            // apppos.y = 297
            // Label13.Location = p
        }

        public void rightside()
        {
            Label4.TextAlign = ContentAlignment.MiddleRight;
            Label14.TextAlign = ContentAlignment.MiddleRight;
            Label15.TextAlign = ContentAlignment.MiddleRight;
            lblmypc.TextAlign = ContentAlignment.MiddleRight;
            lblcp.TextAlign = ContentAlignment.MiddleRight;
            lbluser.TextAlign = ContentAlignment.MiddleRight;
            lblrb.TextAlign = ContentAlignment.MiddleRight;
            lblweb.TextAlign = ContentAlignment.MiddleRight;
            lblnetwork.TextAlign = ContentAlignment.MiddleRight;
            lblapps.TextAlign = ContentAlignment.MiddleRight;
            lblLAN.TextAlign = ContentAlignment.MiddleRight;
            // Dim p As New Point
            // apppos.x = 50
            // apppos.y = 177
            // Label7.Location = p
            // apppos.x = 49
            // apppos.y = 207
            // Label8.Location = p
            // apppos.x = 106
            // apppos.y = 147
            // Label9.Location = p
            // apppos.x = 61
            // apppos.y = 237
            // Label10.Location = p
            // apppos.x = 111
            // apppos.y = 327
            // Label11.Location = p
            // apppos.x = 86
            // apppos.y = 267
            // Label12.Location = p
            // apppos.x = 107
            // apppos.y = 297
            // Label13.Location = p
        }

        public void centerside()
        {
            Label4.TextAlign = ContentAlignment.MiddleCenter;
            Label14.TextAlign = ContentAlignment.MiddleCenter;
            Label15.TextAlign = ContentAlignment.MiddleCenter;
            lblmypc.TextAlign = ContentAlignment.MiddleCenter;
            lblcp.TextAlign = ContentAlignment.MiddleCenter;
            lbluser.TextAlign = ContentAlignment.MiddleCenter;
            lblrb.TextAlign = ContentAlignment.MiddleCenter;
            lblweb.TextAlign = ContentAlignment.MiddleCenter;
            lblnetwork.TextAlign = ContentAlignment.MiddleCenter;
            lblapps.TextAlign = ContentAlignment.MiddleCenter;
            lblLAN.TextAlign = ContentAlignment.MiddleCenter;
            // Dim p As New Point
            // apppos.x = 33
            // apppos.y = 177
            // Label7.Location = p
            // apppos.x = 33
            // apppos.y = 207
            // Label8.Location = p
            // apppos.x = 59
            // apppos.y = 147
            // Label9.Location = p
            // apppos.x = 38
            // apppos.y = 237
            // Label10.Location = p
            // apppos.x = 62
            // apppos.y = 327
            // Label11.Location = p
            // apppos.x = 50
            // apppos.y = 267
            // Label12.Location = p
            // apppos.x = 60
            // apppos.y = 297
            // Label13.Location = p
        }

        private void RightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.rightside();
            KGlobal.setting.lay = "r";
            KGlobal.setting.Save(KGlobal.path);
        }

        private void CenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.centerside();
            KGlobal.setting.lay = "c";
            KGlobal.setting.Save(KGlobal.path);
        }

        private void LeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.leftside();
            KGlobal.setting.lay = "l";
            KGlobal.setting.Save(KGlobal.path);
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About frm = new About();
            frm.Show();
        }

        private void AddDesktopAppsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s1 = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string s2 = "C:\\Users\\Public\\Desktop\\";
            string s3 = (s1 + "\\Apps\\");
            string[] files = Directory.GetFiles(s1, "*.*");
            for (int i = 0; (i <= (files.Count() - 1)); i++)
            {
                try
                {
                    System.IO.FileInfo foundFileInfo = new System.IO.FileInfo(files[i]);
                    File.Move(files[i], (s3 + foundFileInfo.Name));
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show("Same file " + files[i] + " is present, do you want to replace it ?", "Kdock", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        System.IO.FileInfo foundFileInfo = new System.IO.FileInfo(files[i]);
                        File.Delete((s3 + foundFileInfo.Name));
                        File.Move(files[i], (s3 + foundFileInfo.Name));
                    }

                }

            }

            files = Directory.GetFiles(s2, "*.*");
            for (int i = 0; i <= files.Count() - 1; i++)
            {
                try
                {
                    System.IO.FileInfo foundFileInfo = new System.IO.FileInfo(files[i]);
                    File.Move(files[i], (s3 + foundFileInfo.Name));
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show("Same file " + files[i] + " is present, do you want to replace it ?", "Kdock", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        System.IO.FileInfo foundFileInfo = new System.IO.FileInfo(files[i]);
                        File.Delete((s3 + foundFileInfo.Name));
                        File.Move(files[i], (s3 + foundFileInfo.Name));
                    }

                }

            }

            KGlobal.appd.LoadApps();
        }

        private void ONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //     My.Computer.Registry.CurrentUser.CreateSubKey("TestKey")
            //   My.Computer.Registry.SetValue("HKEY_CURRENT_USER\TestKey", "TestValue", "This is a test value.")
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "kdock", (this.App_Path() + System.AppDomain.CurrentDomain.FriendlyName));
            OFFToolStripMenuItem.Enabled = true;
            ONToolStripMenuItem.Enabled = false;
        }

        public string App_Path()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        private void OFFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.RegistryKey rg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
            rg.DeleteValue("kdock");
            OFFToolStripMenuItem.Enabled = false;
            ONToolStripMenuItem.Enabled = true;
        }



        private void Label6_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }

        }

        private void PanelMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] MyFiles;
                int i;
                //  Assign the files to an array.
                MyFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                //  Loop through the array and add the files to the list.
                for (i = 0; (i
                            <= (MyFiles.Length - 1)); i++)
                {
                    System.IO.FileInfo foundFileInfo = new System.IO.FileInfo(MyFiles[i]);
                    string s3 = (KGlobal.DesktopPath + "\\Apps\\");
                    if (File.Exists((s3 + foundFileInfo.Name)))
                    {
                        if (MessageBox.Show("Same file " + foundFileInfo.Name + " is present, do you want to replace it ?", "Kdock", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            File.Delete((s3 + foundFileInfo.Name));
                            File.Move(MyFiles[i], (s3 + foundFileInfo.Name));
                        }

                    }
                    else
                    {
                        File.Move(MyFiles[i], (s3 + foundFileInfo.Name));
                    }

                }

            }

        }

        private void ThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTheme frm = new frmTheme();
            frm.Show();
        }

        private void lblShutdown_Click(object sender, EventArgs e)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "ShutDown";
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = " /s /t 00";
            cmd.Start();
            // Process.Start ("ShutDown"," /s /t 00");
        }

        private void lblRestart_Click(object sender, EventArgs e)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "ShutDown";
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = "/r /t 00";
            cmd.Start();
            // Process.Start("ShutDown" ,"/r /t 00");
        }

        private void lblLogout_Click(object sender, EventArgs e)
        {

            Process cmd = new Process();
            cmd.StartInfo.FileName = "ShutDown";
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = "-l";
            cmd.Start();
            // Process.Start("ShutDown","-l");
        }

        private bool Alt_F4 = false;

        private void Kdock_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Alt || (e.KeyValue == 115)))
            {
                Alt_F4 = true;
            }

        }

        private void Kdock_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Alt_F4)
            {
                e.Cancel = true;
                Alt_F4 = false;
            }

        }

        private void Label3_DoubleClick(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<Note>().Any())
            {
                KGlobal.oNote.Timerclose.Start();
            }
            else
            {

                Note frm = new Note();
                KGlobal.oNote = frm;
                frm.Show();
            }

        }

        private void PanelMain_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right))
            {
                if (Application.OpenForms.OfType<web>().Any())
                {
                    KGlobal.oweb.Focus();
                }

                if (Application.OpenForms.OfType<Note>().Any())
                {
                    KGlobal.oNote.Focus();
                }

            }

        }

        private void Kdock_Resize(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void ShowAP(string place)
        {
            if (KGlobal.activeplace_.IsDisposed)
            {
                ActivePlace frm = new ActivePlace();
                KGlobal.activeplace_ = frm;
                KGlobal.activeplace_.Show();
                KGlobal.activeplace_.setPlace(KGlobal.setting.ap);
            }
            else
            {
                KGlobal.activeplace_.Show();
                KGlobal.activeplace_.setPlace(KGlobal.setting.ap);
            }
        }

        private void TopToolStripMenuItem_Click(object sender, EventArgs e)
        {

            KGlobal.setting.ap = "T";
            ShowAP(KGlobal.setting.ap);
            KGlobal.setting.Save(KGlobal.path);
        }

        private void BottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KGlobal.setting.ap = "B";
            ShowAP(KGlobal.setting.ap);
            KGlobal.setting.Save(KGlobal.path);
        }

        private void LeftToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            KGlobal.setting.ap = "L";
            ShowAP(KGlobal.setting.ap);
            KGlobal.setting.Save(KGlobal.path);
        }

        private void RightToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            KGlobal.setting.ap = "R";
            ShowAP(KGlobal.setting.ap);
            KGlobal.setting.Save(KGlobal.path);
        }

        private void NoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KGlobal.setting.ap = "";
            if (Application.OpenForms.OfType<ActivePlace>().Any())
            {
                KGlobal.activeplace_.Dispose();
            }
            KGlobal.setting.Save(KGlobal.path);
        }

        private void ContextMenuStrip5_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (KGlobal.setting.ap)
            {
                case "":
                    NoneToolStripMenuItem.Checked = true;
                    TopToolStripMenuItem.Checked = false;
                    BottomToolStripMenuItem.Checked = false;
                    LeftToolStripMenuItem1.Checked = false;
                    RightToolStripMenuItem1.Checked = false;
                    break;
                case "T":
                    NoneToolStripMenuItem.Checked = false;
                    TopToolStripMenuItem.Checked = true;
                    BottomToolStripMenuItem.Checked = false;
                    LeftToolStripMenuItem1.Checked = false;
                    RightToolStripMenuItem1.Checked = false;
                    break;
                case "B":
                    NoneToolStripMenuItem.Checked = false;
                    TopToolStripMenuItem.Checked = false;
                    BottomToolStripMenuItem.Checked = true;
                    LeftToolStripMenuItem1.Checked = false;
                    RightToolStripMenuItem1.Checked = false;
                    break;
                case "L":
                    NoneToolStripMenuItem.Checked = false;
                    TopToolStripMenuItem.Checked = false;
                    BottomToolStripMenuItem.Checked = false;
                    LeftToolStripMenuItem1.Checked = true;
                    RightToolStripMenuItem1.Checked = false;
                    break;
                case "R":
                    NoneToolStripMenuItem.Checked = false;
                    TopToolStripMenuItem.Checked = false;
                    BottomToolStripMenuItem.Checked = false;
                    LeftToolStripMenuItem1.Checked = false;
                    RightToolStripMenuItem1.Checked = true;
                    break;
            }
            switch (KGlobal.setting.opa)
            {
                case "1":
                    tsmopa100.Checked = true;
                    tsmopa75.Checked = false;
                    tsmopa50.Checked = false;
                    tsmopa25.Checked = false;
                    break;
                case "0.75":
                    tsmopa100.Checked = false;
                    tsmopa75.Checked = true;
                    tsmopa50.Checked = false;
                    tsmopa25.Checked = false;
                    break;
                case "0.5":
                    tsmopa100.Checked = false;
                    tsmopa75.Checked = false;
                    tsmopa50.Checked = true;
                    tsmopa25.Checked = false;
                    break;
                case "0.25":
                    tsmopa100.Checked = false;
                    tsmopa75.Checked = false;
                    tsmopa50.Checked = false;
                    tsmopa25.Checked = true;
                    break;
            }
            switch (KGlobal.setting.lay)
            {
                case "l":
                    LeftToolStripMenuItem.Checked = true;
                    RightToolStripMenuItem.Checked = false;
                    CenterToolStripMenuItem.Checked = false;
                    break;
                case "r":
                    LeftToolStripMenuItem.Checked = false;
                    RightToolStripMenuItem.Checked = true;
                    CenterToolStripMenuItem.Checked = false;
                    break;
                case "c":
                    LeftToolStripMenuItem.Checked = false;
                    RightToolStripMenuItem.Checked = false;
                    CenterToolStripMenuItem.Checked = true;
                    break;
            }
        }

        private void timerflush_Tick(object sender, EventArgs e)
        {
            KGlobal.FlushMemory();
        }


        private void PanelMain_mouseL(object sender, EventArgs e)
        {
            if ((KGlobal.appd.Visible == false) && (KGlobal.Webd.Visible == false) && (KGlobal.LANd.Visible == false))
            {
                TimerDrawer1.Enabled = false;
                TimerDrawer2.Enabled = true;
                lblDay.Visible = false;
            }

            if (sender == lblapps)
            {
                KGlobal.appd.locClose(true);
                KGlobal.appd.locOpen(false);
            }
            if (sender == lblweb)
            {
                KGlobal.Webd.locClose(true);
                KGlobal.Webd.locOpen(false);
            }
            if (sender == lblLAN)
            {
                KGlobal.LANd.locClose(true);
                KGlobal.LANd.locOpen(false);
            }
        }

        private void PanelMain_MouseMove(object sender, MouseEventArgs e)
        {
            this.TopMost = true;
            TimerDrawer2.Enabled = false;
            TimerDrawer1.Enabled = true;
            lblShutdown.Visible = false;
            lblRestart.Visible = false;
            lblLogout.Visible = false;
            this.WindowState = FormWindowState.Normal;

            if (sender == lblapps || sender == ContextMenuStrip4)
            {
                if (!KGlobal.appd.IsDisposed)
                {

                    KGlobal.appd.locOpen(true);
                    KGlobal.appd.locClose(false);
                }
                else
                {
                    KGlobal.appd = new Apps();
                    KGlobal.appd.mode = "A";
                    KGlobal.appd.Show();
                }
            }
            if (sender == lblLAN)
            {
                if (!KGlobal.LANd.IsDisposed)
                {

                    KGlobal.LANd.locOpen(true);
                    KGlobal.LANd.locClose(false);
                }
                else
                {
                    KGlobal.LANd = new Apps();
                    KGlobal.LANd.mode = "L";
                    KGlobal.LANd.Show();
                }
            }
            if (sender == lblweb)
            {
                if (!KGlobal.Webd.IsDisposed)
                {

                    KGlobal.Webd.locOpen(true);
                    KGlobal.Webd.locClose(false);
                }
                else
                {
                    KGlobal.Webd = new Apps();
                    KGlobal.Webd.mode = "W";
                    KGlobal.Webd.Show();
                }
            }



            if (sender == lblDateTime)
            {
                lblDay.Visible = true;
            }
        }

        private void Klabel1_TextChanged(object sender, EventArgs e)
        {
            if ((Klabel1.Text == "0"))
            {
                Klabel1.Visible = false;
            }
            else
            {
                Klabel1.Visible = true;
            }

        }

        private void Klabel1_Click(object sender, EventArgs e)
        {
            if (KGlobal.notify == null)
            {

                frmNotify frm = new frmNotify();
                KGlobal.notify = frm;
                frm.Show();
            }
            else
            {
                KGlobal.notify.NClose();
            }

        }

        private void Kdock_LocationChanged(object sender, EventArgs e)
        {
            if (KGlobal.notify != null)
            {
                KGlobal.notify.NClose();
            }

        }

        private void GoOnlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((GoOnlineToolStripMenuItem.Text == "Go Online"))
            {
                GoOnlineToolStripMenuItem.Text = "Go Offline";
                KGlobal.setting.LanEng = "1";
                KGlobal.setting.Save(KGlobal.path);
                try
                {
                    if (KGlobal.ClientConnect == null)
                    {
                        Connect frm = new Connect();
                        KGlobal.ClientConnect = frm;
                        frm.Show();
                        frm.Hide();
                    }

                }
                catch (Exception ex)
                {
                }

            }
            else
            {
                GoOnlineToolStripMenuItem.Text = "Go Online";
                KGlobal.setting.LanEng = "0";
                KGlobal.setting.Save(KGlobal.path);
                Label15.Text = " " + (new ComputerInfo()).OSVersion + " ";
                try
                {
                    if (KGlobal.ClientConnect != null)
                    {
                        KGlobal.ClientConnect.Close();
                    }
                }
                catch (Exception ex)
                {
                }

            }

        }

        private void lblapps_DoubleClick(object sender, EventArgs e)
        {
            // Apps.Show()
            string s1;
            s1 = (Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\Apps");
            if (!Directory.Exists(s1))
            {
                System.IO.Directory.CreateDirectory(s1);
                System.IO.File.SetAttributes(s1, System.IO.FileAttributes.Hidden);
                Process.Start(s1);
            }
            else
            {
                Process.Start(s1);
            }

        }

        private void lblweb_DoubleClick(object sender, EventArgs e)
        {
            // Process.Start("www.google.com")
            if (!Application.OpenForms.OfType<web>().Any())
            {

                web frm = new web();
                KGlobal.oweb = frm;
                frm.Show();


            }
            else
            {
                KGlobal.oweb.whideurls = false;
                KGlobal.oweb.Timerclose.Start();
            }

        }

        private void lblLAN_DoubleClick(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<LAN>().Any())
            {
                LAN frm = new LAN();
                KGlobal.oLAN = frm;
                frm.Show();
            }
            else
            {
                KGlobal.oLAN.bhidePC = false;
                KGlobal.oLAN.Timerclose.Start();
            }

        }

        private void lblRestart_MouseMove(object sender, MouseEventArgs e)
        {
            TimerDrawer2.Enabled = false;
            TimerDrawer1.Enabled = true;
            lblShutdown.Visible = true;
            lblRestart.Visible = true;
            lblLogout.Visible = true;
        }
    }
}

