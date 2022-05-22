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
    public partial class RemoteFiles : Form
    {
        public RemoteFiles()
        {
            InitializeComponent();
        }
        public TcpClient client ;
        public string SourceFile = "";


        public void LoadThis(string sip, string sname)
        {
            this.Name = "frm" + sip;
            this.Text = sname + " (" + sip + ")";
            this.Tag = sip;
            lblTitle.Text = this.Text;
        }

        public void LoadFiles(string files)
        {
            FlowLayoutPanel1.Controls.Clear();
            string[] fls = files.Split(KGlobal.mainsep);
            for (int i = 0; i <= fls.Length - 1; i++)
            {
                string[] str = fls[i].Split(KGlobal.smallsep);
                for (int j = 0; j <= str.Length - 2; j++)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                ucFolder uc = new ucFolder();
                                uc.LoadMe(str[j],client );
                                FlowLayoutPanel1.Controls.Add(uc);
                                break;
                            }

                        case 1:
                            {
                                ucFolder uc = new ucFolder();
                                uc.LoadMe(str[j],client );
                                FlowLayoutPanel1.Controls.Add(uc);
                                break;
                            }

                        case 2:
                            {
                                ucFolder uc = new ucFolder();
                                uc.LoadMe(str[j],client );
                                FlowLayoutPanel1.Controls.Add(uc);
                                break;
                            }

                        case 3:
                            {
                                ucFolder uc = new ucFolder();
                                uc.LoadMe(str[j],client );
                                FlowLayoutPanel1.Controls.Add(uc);
                                break;
                            }

                        case 4:
                            {
                                ucFIle uc = new ucFIle();
                                uc.LoadMe(str[j] );
                                FlowLayoutPanel1.Controls.Add(uc);
                                break;
                            }
                    }
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string upPath;

            if (Label1.Text.Split('\\').Last() == "")
                upPath = "";
            else
                upPath = Label1.Text.Substring(0, Label1.Text.LastIndexOf("\\"));
            if (!upPath.Contains("\\") & upPath != "")
                upPath += "\\";
            Label1.Text = upPath;
           // client = new TcpClient(this.Tag.ToString(), KGlobal.PortString);
            if (client.Connected)
            {
                StreamWriter sw = new StreamWriter(client.GetStream());
                sw.Write("<^" + upPath + "^>");
                sw.Flush();
              //  sw.Close();
            }
        }

        private void MoveHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // client = new TcpClient(this.Tag.ToString(), KGlobal.PortString);
            if (client.Connected)
            {
                StreamWriter sw = new StreamWriter(client.GetStream());
                sw.Write("<<" + MoveHereToolStripMenuItem.Tag + "<" + Label1.Text + "<>");
                sw.Flush();
               // sw.Close();
            }
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // client = new TcpClient(this.Tag.ToString(), KGlobal.PortString);
            if (client.Connected)
            {
                StreamWriter sw = new StreamWriter(client.GetStream());
                sw.Write("<+" + PasteToolStripMenuItem.Tag + "+" + Label1.Text + "+>");
                sw.Flush();
               // sw.Close();
            }
        }

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshDirectory();
        }
        public void RefreshDirectory()
        {
          //  client = new TcpClient(this.Tag.ToString(), KGlobal.PortString);
            if (client.Connected)
            {
                StreamWriter sw = new StreamWriter(client.GetStream());
                sw.Write("<^" + Label1.Text + "^>");
                sw.Flush();
               // sw.Close();
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = Cursor.Position;
        }

        private void presize_MouseMove(object sender, MouseEventArgs e)
        {
            int ax = 1;
            int ay = 1;
            if (e.Button == MouseButtons.Left)
            {
                ax = Cursor.Position.X - this.Location.X;
                ay = Cursor.Position.Y - this.Location.Y;
                this.Width = ax;
                this.Height = ay;
            }
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PasteFromThisPCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KGlobal.ClientConnect.DestiFile = Label1.Text;
           // client = new TcpClient(this.Tag.ToString(), KGlobal.PortString);
            if (client.Connected)
            {
                StreamWriter sw = new StreamWriter(client.GetStream());
                sw.Write("<#" + Clipboard.GetFileDropList()[0] + "#" + Label1.Text + "#>");
                sw.Flush();
               // sw.Close();
            }
        }

        private void ContextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Clipboard.ContainsFileDropList())
                PasteFromThisPCToolStripMenuItem.Visible = true;
            else
                PasteFromThisPCToolStripMenuItem.Visible = false;
        }

        private void RemoteFiles_Load(object sender, EventArgs e)
        {

        }
    }
}
