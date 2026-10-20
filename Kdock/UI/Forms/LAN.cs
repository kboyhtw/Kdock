using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kdock.Logic;

namespace Kdock
{
    public partial class LAN : Form
    {
        public LAN()
        {
            InitializeComponent();
        }
        public bool bhidePC;
        private Thread TLoad;

        private void LAN_FormClosing(object sender, FormClosingEventArgs e)
        {
            KGlobal.StopLANThreads(flpPC);
        }
        private void web_Load(object sender, EventArgs e)
        {
            FitForDpi();
            this.Height = S(96);
            Timeropen.Start();
            updatetheme();
            this.Location = KGlobal.setting.LPoint;
            TLoad = new Thread(new ThreadStart(loadsc));
            TLoad.Start();
            this.Focus();
            txtIP.Focus();
            txtBroker.Text = DeskRelay.ReadBroker();
            if (!string.IsNullOrEmpty(DeskRelay.StatusText))
                ShowDeskStatus(DeskRelay.StatusText);
            else if (!string.IsNullOrEmpty(DeskRelay.DeskId))
                ShowDeskStatus("Your ID: " + DeskRelay.Format(DeskRelay.DeskId));
        }

        private int S(int designPixels)
        {
            int dpi = DeviceDpi;
            if (dpi < 96) dpi = 96;
            return designPixels * dpi / 96;
        }

        private void FitForDpi()
        {
        }

        public void ShowDeskStatus(string text)
        {
            lblInternet.Text = text;
            lblInternet.BringToFront();
        }

        private void txtBroker_Leave(object sender, EventArgs e)
        {
            if (txtBroker.Text.Trim() != DeskRelay.ReadBroker())
                DeskRelay.SaveBroker(txtBroker.Text);
        }

        private bool ReadyAddress()
        {
            string text = txtIP.Text.Trim();
            if (text.Length == 0)
            {
                MessageBox.Show("Type the other PC's ID for internet, or its LAN address on the same network.");
                return false;
            }
            return true;
        }

        private void pmove_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = Cursor.Position;
                Savechanges();
            }
        }

        public void Savechanges()
        {
            if (this.Location.X > Screen.PrimaryScreen.Bounds.Width - this.Width)
                this.Location = new Point(Screen.GetWorkingArea(new Point(0, 0)).Width - this.Width, this.Location.Y);
            else if (this.Location.X < 0)
            {
                this.Location = new Point(0, this.Location.Y);
                this.Size = new Size(this.Width - this.Location.X, this.Height);
            }

            if (this.Location.Y > Screen.PrimaryScreen.Bounds.Height - this.Height)
                this.Location = new Point(this.Location.X, Screen.GetWorkingArea(new Point(0, 0)).Height - this.Height);
            else if (this.Location.Y < 0)
            {
                this.Location = new Point(this.Location.X, 0);
                this.Size = new Size(this.Width, this.Height - this.Location.Y);
            }

            if (this.Width > Screen.GetWorkingArea(new Point(0, 0)).Width - 10)
                this.Width = Screen.GetWorkingArea(new Point(0, 0)).Width - 10;
            else if (this.Width <= 50)
                this.Width = 50;

            if (this.Height > Screen.GetWorkingArea(new Point(0, 0)).Height - 10)
                this.Height = Screen.GetWorkingArea(new Point(0, 0)).Height - 10;
            else if (this.Height <= 50)
                this.Height = 50;

            KGlobal.setting.LPoint = this.Location;

            KGlobal.setting.Save(KGlobal.path);
        }


        private void lblSearch_Click(object sender, EventArgs e)
        {
            KGlobal.search(txtIP.Text);
        }

        private void txtsearch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 13)
                KGlobal.search(txtIP.Text);
        }



        private void lblClose_Click(object sender, EventArgs e)
        {
            bhidePC = false;
            Timerclose.Start();
        }

        private int j = 0;

        private void PlaceLan()
        {
            int y = txtIP.Bottom + S(6);
            int brokerW = Math.Min(S(190), Math.Max(S(80), ClientSize.Width / 3));
            txtBroker.SetBounds(ClientSize.Width - brokerW - S(8), y, brokerW, txtBroker.Height);
            lblInternet.SetBounds(S(8), y, Math.Max(S(40), txtBroker.Left - S(16)), Math.Max(lblInternet.Height, S(32)));
            int actions = Math.Max(lblInternet.Bottom, txtBroker.Bottom) + S(8);
            lblChat.Location = new Point(S(12), actions);
            lblFileExpl.Location = new Point(lblChat.Right + S(16), actions);
            lblRD.Location = new Point(lblFileExpl.Right + S(16), actions);
            lblShowPC.Location = new Point(ClientSize.Width - lblShowPC.Width - S(8), actions);
            lblClose.Location = new Point(ClientSize.Width - lblClose.Width - S(6), S(2));
            int top = Math.Max(lblChat.Bottom, lblRD.Bottom) + S(8);
            flpPC.SetBounds(S(8), top, Math.Max(S(80), ClientSize.Width - S(16)), Math.Max(S(40), ClientSize.Height - top - S(8)));
            lblInternet.BringToFront();
            txtBroker.BringToFront();
            txtIP.BringToFront();
            lblChat.BringToFront();
            lblFileExpl.BringToFront();
            lblRD.BringToFront();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Timerclose.Stop();

            int target = KGlobal.setting.LshowPC == "Y" ? S(360) : S(110);
            if (this.Height < target)
                this.Height = Math.Min(target, this.Height + S(40));
            else
            {
                this.Height = target;
                Timeropen.Stop();
                j = 0;
            }
            PlaceLan();
        }

        private void Timerclose_Tick(object sender, EventArgs e)
        {
            Timeropen.Stop();
            if (bhidePC)
            {
                if (this.Height > 60)
                    this.Height -= 20;
                else
                {
                    Timerclose.Stop();
                    this.Height = 55;
                }
            }
            else if (this.Height > 20)
                this.Height -= 20;
            else
            {
                Timerclose.Stop();
                this.Dispose();
                GC.Collect();
            }
        }

        private void AddUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flpPC.Visible = false;
            paddPC.Visible = true;
            pMsg.Visible = false;
        }

        private void lblSave_Click(object sender, EventArgs e)
        {
            if (KGlobal.setting.LPC.Split(KGlobal.midsep).Length < 19)
            {
                KGlobal.setting.LPC += txturlname.Text + KGlobal.endsep + txtipAdd.Text + KGlobal.midsep;
                flpPC.Visible = true;
                paddPC.Visible = false;
                LanPC sc = new LanPC();
                sc.sctype = "LA";
                sc.Tag = txtipAdd.Text;
                sc.Name = txturlname.Text;
                flpPC.Controls.Add(sc);
                Savechanges();
            }
            else
                MessageBox.Show("Please remove some items then try");
        }



        private void lblCancel_Click(object sender, EventArgs e)
        {
            flpPC.Visible = true;
            pMsg.Visible = false;
            paddPC.Visible = false;
        }

        private void loadsc()
        {
            if (KGlobal.setting.LPC != "")
            {
                string[] shrt = KGlobal.setting.LPC.Split(KGlobal.midsep);
                for (int i = 0; i <= shrt.Length - 2; i++)
                {
                    LanPC sc = new LanPC();
                    sc.sctype = "L";
                    sc.Tag = shrt[i].Split(KGlobal.endsep)[1];
                    sc.Name = shrt[i].Split(KGlobal.endsep)[0];
                    BeginInvoke((MethodInvoker)delegate { flpPC.Controls.Add(sc); });
                }
            }
            Label lbl = new Label();
            lbl.Text = "Auto detected";
            lbl.AutoSize = false;
            lbl.Width = flpPC.Width - 30;
            lbl.Height = 20;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new System.Drawing.Font("Segoe UI Semibold", 12.0F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));


            BeginInvoke((MethodInvoker)delegate
            {
                lbl.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
                lbl.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
                flpPC.Controls.Add(lbl);
                lbl.Margin = new Padding(3, 6, 3, 0);
            });


            FindingThreats();
        }
        public void FindingThreats()
        {

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
                                                uc.LoadSystem(SubChildEntry.Name);
                                                try
                                                {
                                                    flpPC.Controls.Add(uc);
                                                }
                                                catch (Exception ex)
                                                {
                                                    BeginInvoke((MethodInvoker)delegate { flpPC.Controls.Add(uc); });
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
        private void lblShowPC_Click(object sender, EventArgs e)
        {
            if (KGlobal.setting.LshowPC == "")
            {
                KGlobal.setting.LshowPC = "Y";
                Timeropen.Start();
            }
            else
            {
                KGlobal.setting.LshowPC = "";
                bhidePC = true;
                Timerclose.Start();
            }
            Savechanges();
        }

        public void updatetheme()
        {
            this.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            txtIP.BackColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            lblChat.HoverColor = KGlobal.mouseH;
            lblChat.LeaveColor = KGlobal.mouseL;
            lblFileExpl.HoverColor = KGlobal.mouseH;
            lblFileExpl.LeaveColor = KGlobal.mouseL;
            lblRD.HoverColor = KGlobal.mouseH;
            lblRD.LeaveColor = KGlobal.mouseL;
            lblSave.HoverColor = KGlobal.mouseH;
            lblSave.LeaveColor = KGlobal.mouseL;
            lblCancel.HoverColor = KGlobal.mouseH;
            lblCancel.LeaveColor = KGlobal.mouseL;
            lblCanMsg.HoverColor = KGlobal.mouseH;
            lblCanMsg.LeaveColor = KGlobal.mouseL;
            lblname.ForeColor = KGlobal.mouseL;
            lblUrl.ForeColor = KGlobal.mouseL;
            this.Opacity = KGlobal.kdockmain.Opacity;
            txtIP.ForeColor = KGlobal.mouseL;
            lblInternet.ForeColor = KGlobal.mouseL;
            lblInternet.BackColor = Color.Transparent;
            lblInternet.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtBroker.BackColor = txtIP.BackColor;
            txtBroker.ForeColor = KGlobal.mouseL;
            lblInternet.BringToFront();
            txtBroker.BringToFront();
            txtIP.BringToFront();
            lblClose.ForeColor = KGlobal.kdockmain.Label4.ForeColor;
            lblShowPC.HoverColor = KGlobal.mouseH;
            lblShowPC.LeaveColor = KGlobal.mouseL;
            foreach (ucShortcut CTRL in flpPC.Controls)
                CTRL.updatetheme();
        }
        private TcpClient Client;
        private void lblChat_Click(object sender, EventArgs e)
        {
            if (!ReadyAddress()) return;
            try
            {
                PeerSession.Open(txtIP.Text, "chat");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //try
            //{
            //    Client = new TcpClient(txtIP.Text, KGlobal.PortString);
            //    if (Client.Connected)
            //    {
            //        string sIncom = Client.Client.RemoteEndPoint.ToString().Replace(" ", "");
            //        StreamWriter sw = new StreamWriter(Client.GetStream());

            //        sw.Write("</start/>/r");
            //        sw.Flush();
            //        sw.Write("</Chat/" + Environment.MachineName + "/>");
            //        sw.Flush();
            //        if (!KGlobal.TRConatins(sIncom.Split(':')[0]))
            //        {
            //            RemoteClient rc = new RemoteClient(sIncom.Split(':')[0], Client);
            //        }
            //        frmLanChat frmC = new frmLanChat();
            //        frmC.Client = Client;
            //        frmC.Tag = txtIP.Text;
            //        frmC.Name = "C" + txtIP.Text;
            //        frmC.Text = txtIP.Text;
            //        frmC.Show();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Can't Connect please check IP address");
            //}
        }

        private void lblFileExpl_Click(object sender, EventArgs e)
        {
            if (!ReadyAddress()) return;
            try
            {
                PeerSession.Open(txtIP.Text, "files");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }




            //try
            //{
            //    Client = new TcpClient(txtIP.Text, KGlobal.PortString);
            //    if (Client.Connected)
            //    {
            //        StreamWriter sw = new StreamWriter(Client.GetStream());
            //        string sIncom = Client.Client.RemoteEndPoint.ToString().Replace(" ", "");
            //        sw.Write("</start/>/r");
            //        sw.Flush();
            //        sw.Write("<__>");
            //        sw.Flush();
            //        if (!KGlobal.TRConatins(sIncom.Split(':')[0]))
            //        {
            //            RemoteClient rc = new RemoteClient(sIncom.Split(':')[0], Client);
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Can't Connect please check IP address");
            //}
        }

        private void lblRD_Click(object sender, EventArgs e)
        {
            if (!ReadyAddress()) return;
            try
            {
                PeerSession.Open(txtIP.Text, "remote");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendMessageToAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flpPC.Visible = false;
            pMsg.Visible = true;
            paddPC.Visible = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (KGlobal.setting.LPC != "")
            {
                string[] shrt = KGlobal.setting.LPC.Split(KGlobal.midsep);
                for (int i = 0; i <= shrt.Length - 2; i++)
                {
                    if (flpPC.Controls.ContainsKey(shrt[i].Split(KGlobal.endsep)[0]))
                    {
                        ((LanPC)flpPC.Controls[shrt[i].Split(KGlobal.endsep)[0]]).SendMsg(txtMsg.Text);
                    }

                }
            }
            flpPC.Visible = true;
            pMsg.Visible = false;
            paddPC.Visible = false;
            txtMsg.Clear();
        }

        private void lblCanMsg_Click(object sender, EventArgs e)
        {
            flpPC.Visible = true;
            pMsg.Visible = false;
            paddPC.Visible = false;
            txtMsg.Clear();
        }
    }
}
