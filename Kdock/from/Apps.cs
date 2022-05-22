using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class Apps : Form
    {
        public Apps()
        {
            InitializeComponent();
        }

        public Point pbpoint;
        // Public file As String
        public string mode = "";
        public int sy = 0;
        private struct SHFILEINFO
        {
            public IntPtr hIcon;            // : icon
            public int iIcon;           // : icondex
            public int dwAttributes;    // : SFGAO_ flags
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, int cbFileInfo, int uFlags);

        private const int SHGFI_ICON = 0x100;
        private const int SHGFI_SMALLICON = 0x1;
        private const int SHGFI_LARGEICON = 0x0;

        private void Apps_Load(object sender, EventArgs e)
        {
            this.Opacity = double.Parse(KGlobal.setting.opa);
            PictureBox1.BackColor = KGlobal.kdockmain.lblapps.BackColor;
            papps.BackColor = KGlobal.kdockmain.lblapps.BackColor;
            Panel2.BackColor = KGlobal.kdockmain.lblapps.BackColor;
            Label1.ForeColor = KGlobal.appfont;
            Label2.ForeColor = KGlobal.appfont;
            this.Activate();

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Apps\"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Apps\");
                System.IO.File.SetAttributes(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Apps\", System.IO.FileAttributes.Hidden);
            }
            if (KGlobal.kdockmain.Location.X >= 600)
            {
                pbpoint = new Point(480, 0);

                this.Location = new Point(KGlobal.kdockmain.Location.X - 480, KGlobal.kdockmain.Location.Y + 134);
            }
            else
            {
                pbpoint = new Point(0, 0);
                this.Location = new Point(KGlobal.kdockmain.Location.X + 171, KGlobal.kdockmain.Location.Y + 134);
            }
            PictureBox1.Location = pbpoint;
            PictureBox1.Visible = true;
            locOpen(true);

            if (mode == "A")
                LoadApps();
            else if (mode == "W")
            {
                if (KGlobal.setting.wurls != "")
                {
                    string[] shrt = KGlobal.setting.wurls.Split(KGlobal.midsep);
                    for (int i = 0; i <= shrt.Length - 2; i++)
                    {
                        ucShortcut sc = new ucShortcut();
                        sc.sctype = "";
                        sc.Tag = shrt[i].Split(KGlobal.endsep)[0];
                        sc.Name = shrt[i].Split(KGlobal.endsep)[1];
                        sc.MouseMove += Apps_MouseMove;
                        sc.MouseLeave += Apps_mouseL;
                        sc.Label2.MouseMove += Apps_MouseMove;
                        sc.Label2.MouseLeave += Apps_mouseL;
                        sc.lbl1.MouseMove += Apps_MouseMove;
                        sc.lbl1.MouseLeave += Apps_mouseL;
                        papps.Controls.Add(sc);
                    }
                    shrt = null;
                }
            }
            else if (mode == "L")
            {
                if (!BackgroundWorker1.IsBusy)
                    BackgroundWorker1.RunWorkerAsync();
            }
        }
        private TcpClient Client;

        public void FindingThreats()
        {
            // DirectoryEntry childEntry;
            DirectoryEntry ParentEntry = new DirectoryEntry();
            try
            {
                ParentEntry.Path = "WinNT:";
                foreach (DirectoryEntry childEntry in ParentEntry.Children)
                {
                    switch (childEntry.SchemaClassName)
                    {
                        case "Domain":
                            {
                                DirectoryEntry SubParentEntry = new DirectoryEntry();
                                SubParentEntry.Path = "WinNT://" + childEntry.Name;
                                foreach (DirectoryEntry SubChildEntry in SubParentEntry.Children)
                                {
                                    switch (SubChildEntry.SchemaClassName)
                                    {
                                        case "Computer":
                                            {
                                                LanPC uc = new LanPC();
                                                uc.Name = "uc" + SubChildEntry.Name;
                                                ApplyMouseMove(uc);
                                                ApplyMouseMove(uc.ContextMenuStrip);
                                                uc.LoadSystem(SubChildEntry.Name);
                                                try
                                                {
                                                    papps.Controls.Add(uc);
                                                }
                                                catch (Exception ex)
                                                {
                                                    BeginInvoke((MethodInvoker)delegate { papps.Controls.Add(uc); });
                                                }

                                                break;
                                            }
                                    }
                                }

                                break;
                            }
                    }
                }
            }


            // For Each ctrl As Control In papps.Controls
            // Dim host As String = CType(ctrl, LanPC).lblPC.Text
            // Try
            // Client = New TcpClient(host, PortString)
            // If Client.Connected Then
            // Dim sw As New StreamWriter(Client.GetStream())
            // sw.Write(">??")
            // sw.Flush()
            // sw.Close()
            // End If
            // Catch ex As Exception
            // End Try
            // Next
            catch (Exception Excep)
            {
            }
            // MsgBox("Error While Reading Directories : " + Excep.Message.ToString)
            finally
            {
                ParentEntry = null/* TODO Change to default(_) if this is not a reference type */;
            }
        }

        public void LoadApps()
        {
            papps.Controls.Clear();
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Apps\");
            System.IO.FileInfo[] diar1 = di.GetFiles();
            for (int i = 0; i <= 4; i++)
            {
                ucShortcut sc = new ucShortcut();

                switch (i)
                {
                    case 0:
                        {
                            sc.sctype = "";
                            sc.Name = "calc";
                            sc.Tag = "Calculator";
                            break;
                        }

                    case 1:
                        {
                            sc.sctype = "";
                            sc.Name = "cmd";
                            sc.Tag = "CMD";
                            break;
                        }

                    case 2:
                        {
                            sc.sctype = "A";
                            sc.Name = "cmd";
                            sc.Tag = "CMD (Admin)";
                            break;
                        }

                    case 3:
                        {
                            sc.sctype = "";
                            sc.Name = "notepad";
                            sc.Tag = "Note Pad";
                            break;
                        }

                    case 4:
                        {
                            sc.sctype = "";
                            sc.Name = "snippingtool";
                            sc.Tag = "Snipping Tool";
                            break;
                        }
                }


                sc.MouseMove += Apps_MouseMove;
                sc.MouseLeave += Apps_mouseL;
                sc.Label2.MouseMove += Apps_MouseMove;
                sc.Label2.MouseLeave += Apps_mouseL;
                sc.lbl1.MouseMove += Apps_MouseMove;
                sc.lbl1.MouseLeave += Apps_mouseL;
                papps.Controls.Add(sc);
            }
            foreach (System.IO.FileInfo dra in diar1)
            {
                string path = dra.DirectoryName + @"\" + dra.Name;
                createIcon(path);
            }
        }



        public void createIcon(string path)
        {
            IntPtr hImgLarge;  // The handle to the system image list.
            SHFILEINFO shinfo;
            shinfo = new SHFILEINFO();


            shinfo.szDisplayName = new string((char)0, 260);
            shinfo.szTypeName = new string((char)0, 80);

            // Use this to get the small icon.
            // hImgSmall = SHGetFileInfo(fName, 0, shinfo, Marshal.SizeOf(shinfo), SHGFI_ICON Or SHGFI_SMALLICON)

            // Use this to get the large icon.
            hImgLarge = SHGetFileInfo(path, 0, ref shinfo, Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);

            // The icon is returned in the hIcon member of the shinfo struct.
            System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
            KShortcut pic = new KShortcut();
            string[] st;
            st = path.Split('\\');
            // pic.Size = New Size(60, 60)
            pic.Parent = papps;
            // pic.Location = scp
            pic.Label1.Text = st[st.Length - 1];
            pic.Label1.ForeColor = KGlobal.appfont;
            pic.Label2.Image = myIcon.ToBitmap();
            // pic.Button1.BackgroundImage = myIcon.ToBitmap
            // pic.Button1.BackgroundImageLayout = ImageLayout.Center
            pic.Label2.Name = path;
            pic.DeleteItemToolStripMenuItem.Name = path;
            pic.CopyToDesktopToolStripMenuItem.Name = path;
            pic.MoveToDesktopToolStripMenuItem.Name = path;
            pic.OpenToolStripMenuItem.Name = path;
            // pic.Button1.Name = path
            pic.Label1.Name = path;
            // AddHandler pic.Button1.Click, AddressOf DynamicClick
            pic.Label2.Click += DynamicClick1;
            // AddHandler pic.Label1.MouseClick, AddressOf PicMouseMove
            pic.ContextMenuStrip1.MouseMove += Apps_MouseMove;
            pic.OpenToolStripMenuItem.Click += OpenItem;
            pic.DeleteItemToolStripMenuItem.Click += Deleteitem;
            pic.MoveToDesktopToolStripMenuItem.Click += Moveitem;
            pic.CopyToDesktopToolStripMenuItem.Click += Copyitem;
            pic.MouseMove += Apps_MouseMove;
            // AddHandler pic.Button1.MouseMove, AddressOf Apps_MouseMove
            pic.Label1.MouseMove += Apps_MouseMove;
            pic.Label2.MouseMove += Apps_MouseMove;
            pic.MouseLeave += Apps_mouseL;
            // AddHandler pic.Button1.mouseL, AddressOf Apps_mouseL
            pic.Label1.MouseLeave += Apps_mouseL;
            pic.Label2.MouseLeave += Apps_mouseL;
            st = null;
        }
        public void OpenItem(object sender, EventArgs e)
        {
            ToolStripMenuItem btn = (ToolStripMenuItem)sender;
            Getlocation(btn.Name);
        }
        public void Copyitem(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            System.IO.FileInfo foundFileInfo = new System.IO.FileInfo(item.Name);
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name))
            {
                if (MessageBox.Show("Same file " + foundFileInfo.Name + " is present, do you want to replace it ?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name);
                    File.Copy(item.Name, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name);
                }
            }
            else
                File.Copy(item.Name, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name);
        }
        public void Moveitem(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            System.IO.FileInfo foundFileInfo = new System.IO.FileInfo(item.Name);
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name))
            {
                if (MessageBox.Show("Same file " + foundFileInfo.Name + " is present, do you want to replace it ?", "Kdock", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name);
                    File.Move(item.Name, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name);
                }
            }
            else
                File.Move(item.Name, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + foundFileInfo.Name);
        }
        public void Deleteitem(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            File.Delete(item.Name);
        }
        // Public Sub DynamicClick(ByVal sender As Object, ByVal e As EventArgs)
        // Dim btn As Button = CType(sender, Button)
        //  KGlobal .kdockmain.Timer4.Enabled = False
        //  KGlobal .kdockmain.Timer3.Enabled = True

        // locClose(False)
        // locOpen(True)
        // '  Panel2.Visible = True
        // ' Panel1.Visible = True
        // Getlocation(btn.Name)
        // End Sub
        public void DynamicClick1(object sender, EventArgs e)
        {
            Label btn = (Label)sender;
            //  KGlobal .kdockmain.Timer4.Enabled = False
            //  KGlobal .kdockmain.Timer3.Enabled = True

            // locClose(False)
            // locOpen(True)
            // '  Panel2.Visible = True
            // ' Panel1.Visible = True
            Getlocation(btn.Name);
        }
        private void Apps_MouseMove(object sender, MouseEventArgs e)
        {
            locClose(false);
            locOpen(true);
            KGlobal.kdockmain.TimerDrawer2.Enabled = false;
            KGlobal.kdockmain.TimerDrawer1.Enabled = true;
        }


        private void Apps_mouseL(object sender, EventArgs e)
        {
            locOpen(false);
            locClose(true);
        }

        public void Getlocation(string path)
        {
            try
            {
                string[] extn = path.Split('.');

                if (extn[extn.Length - 1].ToUpper() != "LNK")
                    Process.Start(path);
                else
                {

                    // WshShellClass shell = new WshShellClass();
                    IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell(); //Create a new WshShell Interface
                    IWshRuntimeLibrary.IWshShortcut link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(path); //Link the interface to our shortcut

                   // MessageBox.Show(link.TargetPath); //Show the target in a MessageBox using IWshShortcut

                    // Display Target Path
                    string st;
                    st = link.TargetPath.Replace("Program Files (x86)", "Program Files");
                    // st = wshLink.TargetPath.ToString.Split("\")
                    try
                    {
                        Process.Start(link.TargetPath);
                    }
                    catch (Exception ex)
                    {
                        Process.Start(st);
                    }

                    // MsgBox(wshLink.TargetPath)
                    // 
                    // Housekeeping
                    link = null;
                    shell = null;
                }
                extn = null;
            }
            catch (Exception ex)
            {
            }
        }


        private int i = 0;
        private void TimerRight1_Tick(object sender, EventArgs e)
        {
            if (PictureBox1.Width <= 480)
            {
                pbpoint.X -= 50;
                PictureBox1.Width += 50;
                PictureBox1.Location = pbpoint;
                papps.Location = new Point(30, 0);
            }
            else if (papps.Location.X > 0)
            {
                Panel2.Visible = true;
                papps.Visible = true;
                papps.Focus();
                i += 4;
                papps.Location = new Point(30 - i, 0);
            }
            else
            {
                i = 0;
                TimerRight1.Enabled = false;
            }
        }

        private void TimerRight2_Tick(object sender, EventArgs e)
        {
            i = 0;

            if (PictureBox1.Width >= 10)
            {
                papps.Visible = false;
                Panel2.Visible = false;


                pbpoint.X += 50;
                PictureBox1.Location = pbpoint;
                PictureBox1.Width -= 50;
            }
            else
            {
                TimerRight2.Enabled = false;
                PictureBox1.Location = new Point(480, 0);
                PictureBox1.Size = new Size(1, 266);
                this.Close();
                this.Dispose();
                GC.Collect();
                int countApp = 0;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm is Apps)
                    {
                        countApp += 1;
                        ((Apps)frm).locOpen(false);
                        ((Apps)frm).locClose(true);
                    }
                }
                if (countApp == 0)
                {
                    KGlobal.kdockmain.TimerDrawer1.Enabled = false;
                    KGlobal.kdockmain.TimerDrawer2.Enabled = true;
                    KGlobal.kdockmain.lblDay.Visible = false;
                }
            }
        }

        private void TimerLeft1_Tick(object sender, EventArgs e)
        {
            if (PictureBox1.Width <= 480)
            {
                PictureBox1.Width += 30;
                PictureBox1.Location = pbpoint;
                papps.Location = new Point(-30, 0);
            }
            else if (papps.Location.X < 0)
            {
                Panel2.Visible = true;
                papps.Visible = true;
                papps.Focus();
                i += 2;
                Panel2.Location = new Point(423 + i, 0);
                papps.Location = new Point(-30 + i, 0);
            }
            else
            {
                i = 0;
                TimerLeft1.Enabled = false;
            }
        }

        private void TimerLeft2_Tick(object sender, EventArgs e)
        {
            i = 0;

            if (PictureBox1.Width >= 10)
            {
                papps.Visible = false;
                Panel2.Visible = false;
                PictureBox1.Width -= 30;
                PictureBox1.Location = pbpoint;
            }
            else
            {
                TimerLeft2.Enabled = false;
                PictureBox1.Location = new Point(0, 0);
                PictureBox1.Size = new Size(1, 266);
                this.Close();
                this.Dispose();
                GC.Collect();
                KGlobal.kdockmain.TimerDrawer1.Enabled = false;
                KGlobal.kdockmain.TimerDrawer2.Enabled = true;
                KGlobal.kdockmain.lblDay.Visible = false;
            }
        }

        public void locOpen(bool b)
        {
            if (b == true)
            {
                if (KGlobal.kdockmain.Location.X >= 600)
                    TimerRight1.Enabled = true;
                else
                    TimerLeft1.Enabled = true;
            }
            else if (KGlobal.kdockmain.Location.X >= 600)
                TimerRight1.Enabled = false;
            else
                TimerLeft1.Enabled = false;
        }

        public void locClose(bool b)
        {
            if (b == true)
            {
                if (KGlobal.kdockmain.Location.X >= 600)
                    TimerRight2.Enabled = true;
                else
                    TimerLeft2.Enabled = true;
            }
            else if (KGlobal.kdockmain.Location.X >= 600)
                TimerRight2.Enabled = false;
            else
                TimerLeft2.Enabled = false;
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            if (sy < 1)
                sy = 0;
            else
            {
                sy -= 150;

                papps.AutoScrollPosition = new Point(0, sy);
            }
        }

        private void Label2_Click(object sender, EventArgs e)
        {
            if (sy > papps.VerticalScroll.Maximum - 150)
                sy = papps.VerticalScroll.Maximum - 150;
            else
            {
                sy += 150;

                papps.AutoScrollPosition = new Point(0, sy);
            }
        }

        private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            papps.Controls.Clear();
            if (KGlobal.setting.LPC != "")
            {
                string[] shrt = KGlobal.setting.LPC.Split(KGlobal.midsep);
                for (int i = 0; i <= shrt.Length - 2; i++)
                {
                    LanPC sc = new LanPC();
                    sc.sctype = "D";
                    sc.Tag = shrt[i].Split(KGlobal.endsep)[1];
                    sc.Name = shrt[i].Split(KGlobal.endsep)[0];

                    ApplyMouseMove(sc);
                    ApplyMouseMove(sc.ContextMenuStrip1);

                    try
                    {
                        papps.Controls.Add(sc);
                    }
                    catch (Exception ex)
                    {
                        BeginInvoke((MethodInvoker)delegate { papps.Controls.Add(sc); });
                    }
                }
            }
            Label lbl = new Label();
            lbl.Text = "Auto detected";
            lbl.AutoSize = false;
            lbl.Width = papps.Width - 30;
            lbl.Height = 20;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new System.Drawing.Font("Segoe UI Semibold", 12.0F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            try
            {
                BeginInvoke((MethodInvoker)delegate
           {
               papps.Controls.Add(lbl);
                    // lbl.BackColor =  KGlobal .kdockmain.PanelDrawer.BackColor
                    lbl.ForeColor = KGlobal.kdockmain.PanelMain.BackColor;
               lbl.Margin = new Padding(3, 6, 3, 0);
           });
            }
            catch (Exception ex)
            {
                papps.Controls.Add(lbl);
                // lbl.BackColor =  KGlobal .kdockmain.PanelDrawer.BackColor
                lbl.ForeColor = KGlobal.kdockmain.PanelMain.BackColor;
                lbl.Margin = new Padding(3, 6, 3, 0);
            }
            lbl.MouseMove += Apps_MouseMove;
            lbl.MouseLeave += Apps_mouseL;
            FindingThreats();
        }

        private void ApplyMouseMove(Control ctrl)
        {
            ctrl.MouseMove += Apps_MouseMove;
            ctrl.MouseLeave += Apps_mouseL;
            foreach (Control crl in ctrl.Controls)
            {
                crl.MouseMove += Apps_MouseMove;
                crl.MouseLeave += Apps_mouseL;
                if (crl is Panel | crl is FlowLayoutPanel)
                    ApplyMouseMove(crl);
            }
        }

        private void Apps_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mode == "L")
                KGlobal.StopLANThreads(papps);
        }
    }
}
