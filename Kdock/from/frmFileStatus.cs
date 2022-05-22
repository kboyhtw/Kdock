using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class frmFileStatus : Form
    {
        public frmFileStatus()
        {
            InitializeComponent();
        }

        double lastsize = 0;
        public string sMode = "R";


        private void timerSpeed_Tick(object sender, EventArgs e)
        {
            Label2.Text = ((ProgressBar1.Value * 100) / (double)ProgressBar1.Maximum).ToString("0.00") + "%   " + (ProgressBar1.Value / (double)1024).ToString("0.00") + " MB " + "/ " + (ProgressBar1.Maximum / (double)1024).ToString("0.00") + " MB   " + "Transfer Rate : " + ((ProgressBar1.Value - lastsize) / (double)1024).ToString("0.00") + " Mb/s";
            lastsize = ProgressBar1.Value;
        }

        public Int64 Progress
        {
            get
            {
                return ProgressBar1.Value;
            }
            set
            {
                ProgressBar1.Value = value;
            }
        }

        public int MaxProgress
        {
            get
            {
                return Convert.ToInt32(ProgressBar1.Maximum);
            }
            set
            {
                ProgressBar1.Maximum = value;
            }
        }

        public string DisplayText
        {
            get
            {
                return lblSource.Text;
            }
            set
            {
                lblSource.Text = value;
            }
        }

        private TcpClient client;

        private void lblCancelT_Click(object sender, EventArgs e)
        {
            if ((sMode == "R"))
            {
                try
                {
                    client = new TcpClient(this.Tag.ToString(), KGlobal.PortString);
                    if (client.Connected)
                    {
                        StreamWriter sw = new StreamWriter(client.GetStream());
                        sw.Write("</STransR/>");
                        sw.Flush();
                        sw.Close();
                    }

                    KGlobal.ClientConnect.bwReceiver.CancelAsync();
                    //  Connect.Fs.Close()
                    if (File.Exists(KGlobal.ClientConnect.savefile))
                    {
                        File.Delete(KGlobal.ClientConnect.savefile);
                    }

                    //  CType(Application.OpenForms("frm" & Me.Tag), RemoteFiles).FileReceived()
                    this.Close();
                }
                catch (Exception ex)
                {
                }

            }
            else if ((sMode == "S"))
            {
                try
                {
                    client = new TcpClient(this.Tag.ToString(), KGlobal.PortString);
                    if (client.Connected)
                    {
                        StreamWriter sw = new StreamWriter(client.GetStream());
                        sw.Write("</STransS/>");
                        sw.Flush();
                        sw.Close();
                    }

                    KGlobal.ClientConnect.bwSender.CancelAsync();
                    //   Connect.Fs1.Close()
                    if (File.Exists(KGlobal.ClientConnect.savefile))
                    {
                        File.Delete(KGlobal.ClientConnect.savefile);
                    }

                }
                catch (Exception ex)
                {
                }

            }

        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left))
            {
                this.Location = Cursor.Position;
            }

        }
    }
}
