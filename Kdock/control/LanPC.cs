using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;

namespace Kdock
{
    public partial class LanPC
    {
        public LanPC()
        {
            InitializeComponent();
        }


        public string sctype = "";
        private Thread TCheck;
        private Thread tMsg;
        private bool isOl = false;

        private void LanPC_Load(object sender, EventArgs e)
        {
            updatetheme();


            if (sctype == "LA")
            {
                lblPC.Text = this.Name;
                lblip.Text = Tag.ToString();
                RemoveToolStripMenuItem.Visible = true;
                this.Size = new Size(0, 46);
            }
            else if (sctype == "L")
            {
                lblPC.Text = this.Name;
                lblip.Text = this.Tag.ToString();
                RemoveToolStripMenuItem.Visible = true;
                this.Size = new Size(140, 46);
            }
            else if (sctype == "D")
            {
                lblPC.Text = this.Name;
                lblip.Text = this.Tag.ToString();
                RemoveToolStripMenuItem.Visible = false;
                this.Size = new Size(140, 46);
            }
            else
            {
                RemoveToolStripMenuItem.Visible = false;
                this.Size = new Size(140, 46);
            }
            if (KGlobal.setting.LanEng == "1")
                ThreadCheck(true);
        }

        private string sMsg;

        public void SendMsg(string msg)
        {
            sMsg = msg;
            tMsg = new Thread(new ThreadStart(SendMsg));
            tMsg.IsBackground = true;
            tMsg.Start();
        }

        private void SendMsg()
        {
            if (isOnline)
            {
                Client = new TcpClient(lblip.Text, KGlobal.PortString);
                if (Client.Connected)
                {
                    StreamWriter sw = new StreamWriter(Client.GetStream());
                    sw.Write("<*>" + Environment.MachineName + "<*>" + sMsg + "<*>");
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public void ThreadCheck(bool start)
        {
            if (start)
            {
                if (TCheck != null)
                    TCheck.Abort();
                TCheck = new Thread(new ThreadStart(check));
                TCheck.Start();
            }
            else if (TCheck != null)
                TCheck.Abort();
        }

        private void check()
        {
            string Pc = "";
            if (lblip.Text != "0.0.0.0" & lblip.Text != "")
            {
                Pc = lblip.Text;
                this.Name = Pc;
                try
                {
                    Client = new TcpClient(Pc, KGlobal.PortString);
                    if (Client.Connected)
                    {
                        StreamWriter sw = new StreamWriter(Client.GetStream());
                        sw.Write(">??");
                        sw.Flush();
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    isOnline = false;
                    try
                    {
                        lblip.Text = "0.0.0.0";
                    }
                    catch (Exception ex1)
                    {

                        BeginInvoke((MethodInvoker)delegate { lblip.Text = "0.0.0.0"; });


                    }
                    check();
                }
            }
            else
            {
                Pc = lblPC.Text;
                this.Name = Pc;
                try
                {
                    Client = new TcpClient(Pc, KGlobal.PortString);
                    if (Client.Connected)
                    {
                        StreamWriter sw = new StreamWriter(Client.GetStream());
                        sw.Write(">??");
                        sw.Flush();
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    // RemoveLAN(Pc)
                    isOnline = false;
                    if (this.ParentForm is LAN)
                    {
                        try
                        {
                            ;
                            BeginInvoke((MethodInvoker)delegate { lblip.Text = "Can't reach"; });

                        }
                        catch (Exception ex1)
                        {
                            lblip.Text = "Can't reach";
                        }
                        return;
                    }
                    else
                        try
                        {
                            BeginInvoke((MethodInvoker)delegate { tmrRemove.Start(); });
                        }
                        catch (Exception ex1)
                        {
                            tmrRemove.Start();
                        }
                }
            }
        }

        public bool isOnline
        {
            get
            {
                return isOl;
            }
            set
            {
                isOl = value;
                if (isOl)
                    lblStatus.ForeColor = Color.Green;
                else
                    lblStatus.ForeColor = this.ForeColor;
            }
        }


        public void LoadSystem(string sName)
        {
            lblPC.Text = sName;
        }
        private TcpClient Client;

        public void getOnline(string sip)
        {
            lblip.Text = sip;
            string[] shrt = KGlobal.setting.LPC.Split(KGlobal.midsep);

            for (int i = 0; i <= shrt.Length - 2; i++)
            {
                if (Name == shrt[i].Split(KGlobal.endsep)[0])
                    KGlobal.setting.LPC = KGlobal.setting.LPC.Replace(shrt[i].Split(KGlobal.endsep)[0] + KGlobal.endsep + shrt[i].Split(KGlobal.endsep)[1] + KGlobal.midsep, shrt[i].Split(KGlobal.endsep)[0] + KGlobal.endsep + sip + KGlobal.midsep);
            }
            for (int i = 0; i <= shrt.Length - 2; i++)
            {
                if (Name == shrt[i].Split(KGlobal.endsep)[1])
                    KGlobal.setting.LPC = KGlobal.setting.LPC.Replace(shrt[i].Split(KGlobal.endsep)[0] + KGlobal.endsep + shrt[i].Split(KGlobal.endsep)[1] + KGlobal.midsep, shrt[i].Split(KGlobal.endsep)[0] + KGlobal.endsep + sip + KGlobal.midsep);
            }
            KGlobal.setting.Save(KGlobal.path);
            isOnline = true;
        }

        public void updatetheme()
        {
            this.ForeColor = KGlobal.kdockmain.PanelMain.BackColor;
            this.BackColor = KGlobal.kdockmain.PanelDrawer.BackColor;
        }

        private void RemoteControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenRD();
        }

        private void lblStatus_MouseClick(object sender, MouseEventArgs e)
        {
            lblStatus.Focus();
            if (lblip.Text != "0.0.0.0" & lblip.Text != "")
                ThreadCheck(true);
        }

        private void FileExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFE();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            tmrRemove.Stop();
            if (this.Width < 140)
                this.Size = new Size(this.Width + 5, 46);
            else
            {
                this.Size = new Size(140, 46);
                tmrLoad.Stop();
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            ThreadCheck(false);
            tmrLoad.Enabled = false;
            if (this.Width > 0)
                this.Size = new Size(this.Width - 5, 46);
            else
            {
                this.Size = new Size(0, 46);
                tmrRemove.Stop();
                this.Dispose();
            }
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KGlobal.RemoveLAN(this.Name);
            tmrRemove.Start();
        }

        private void ContextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lblip.Text != "0.0.0.0" & lblip.Text != "")
            {
                ChatToolStripMenuItem.Visible = true;
                RemoteControlToolStripMenuItem.Visible = true;
                FileExplorerToolStripMenuItem.Visible = true;
            }
            else
            {
                ChatToolStripMenuItem.Visible = false;
                RemoteControlToolStripMenuItem.Visible = false;
                FileExplorerToolStripMenuItem.Visible = false;
            }
        }

        private void lblChat_Click(object sender, EventArgs e)
        {
            openChat();
        }

        private void lblFileExplorer_Click(object sender, EventArgs e)
        {
            openFE();
        }

        private void lblRD_Click(object sender, EventArgs e)
        {
            OpenRD();
        }
        private bool bOpen;
        private void LanPC_Enter(object sender, EventArgs e)
        {
            if (isOnline)
            {
                bOpen = true;
                tmrOpen.Start();
            }
        }

        private void LanPC_Leave(object sender, EventArgs e)
        {
            bOpen = false;
            tmrOpen.Start();
        }

        private void openChat()
        {
            try
            {
                Client = new TcpClient(lblip.Text, KGlobal.PortString);
                if (Client.Connected)
                {
                    StreamWriter sw = new StreamWriter(Client.GetStream());
                    sw.Write("</Chat/" + Environment.MachineName + "/>");
                    sw.Flush();
                    sw.Close();
                    frmLanChat frmC = new frmLanChat();
                    frmC.Tag = lblip.Text;
                    frmC.Name = "C" + lblip.Text;
                    frmC.Text = lblPC.Text;
                    frmC.Show();
                }
            }
            catch (Exception ex)
            {
                lblip.Text = "0.0.0.0";
                ThreadCheck(true);
            }
        }
        private void openFE()
        {
            if (lblip.Text != "0.0.0.0" & lblip.Text != "")
            {
                try
                {
                    Client = new TcpClient(lblip.Text, KGlobal.PortString);
                    if (Client.Connected)
                    {
                        StreamWriter sw = new StreamWriter(Client.GetStream());
                        sw.Write("<__>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    lblip.Text = "0.0.0.0";
                    ThreadCheck(true);
                }
                KGlobal.kdockmain.Focus();
            }
        }

        private void OpenRD()
        {
            try
            {
                Client = new TcpClient(lblip.Text, KGlobal.PortString);
                if (Client.Connected)
                {
                    StreamWriter sw = new StreamWriter(Client.GetStream());
                    sw.Write("</Remote/>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                lblip.Text = "0.0.0.0";
                ThreadCheck(true);
            }
        }

        private void tmrOpen_Tick(object sender, EventArgs e)
        {
            if (bOpen)
            {
                if (this.Height < 110)
                    this.Height += 4;
                else
                {
                    this.Height = 110;
                    tmrOpen.Stop();
                }
            }
            else if (this.Height > 46)
                this.Height -= 4;
            else
            {
                this.Height = 46;
                tmrOpen.Stop();
            }
        }

        private void ChatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openChat();
        }

        private void VerifyIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lblip.Text = "0.0.0.0";
            ThreadCheck(true);
        }
    }

}

