using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Kdock
{
    public partial class frmChat : Form
    {
        public frmChat()
        {
            InitializeComponent();
        }
        public TcpClient Client;
        // Dim receiveBuffer(bufferSize) As Byte
        // Dim sendBuffer(bufferSize) As Byte
        private Color recColor = Color.IndianRed;
        private Color senColor = Color.Blue;


        public void GotMsg(string msg)
        {
            TextBox lbl = new TextBox();
            lbl.BorderStyle = BorderStyle.None;
            lbl.ReadOnly = true;
            lbl.Text = this.Tag + " : " + msg;
            lbl.BackColor = recColor;
            lbl.ForeColor = this.BackColor;
            lbl.MinimumSize = new Size(197, 19);
            lbl.MaximumSize = new Size(197, 0);
            lbl.Margin = new Padding(5, 5, 0, 5);
            lbl.Multiline = true;
            lbl.AutoSize = true;
            lbl.TextAlign = HorizontalAlignment.Left;
            lbl.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            lbl.Height = TextRenderer.MeasureText(lbl.Text, lbl.Font, new Size(lbl.ClientSize.Width, 1000), TextFormatFlags.WordBreak).Height + 10;
            BeginInvoke((MethodInvoker)delegate
            {
                flpChat.Controls.Add(lbl);
                flpChat.VerticalScroll.Value = flpChat.VerticalScroll.Maximum;
            });
        }

        public void GotMsg(string msg, FileObject File)
        {
            ucChatFile lbl = new ucChatFile();

            lbl.Text = this.Tag + " : " + msg;
            lbl.BackColor = recColor;
            lbl.ForeColor = this.BackColor;
            lbl.File = File;
            //lbl.MinimumSize = new Size(197, 19);
            //lbl.MaximumSize = new Size(197, 0);
            //lbl.Margin = new Padding(5, 5, 0, 5);
            //lbl.Multiline = true;
            //lbl.AutoSize = true;
            //lbl.TextAlign = HorizontalAlignment.Left;
            lbl.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            //  lbl.Height = TextRenderer.MeasureText(lbl.Text, lbl.Font, new Size(lbl.ClientSize.Width, 1000), TextFormatFlags.WordBreak).Height + 10;

            Invoke((MethodInvoker)delegate
             {
                 flpChat.Controls.Add(lbl);
                 flpChat.VerticalScroll.Value = flpChat.VerticalScroll.Maximum;
                 this.Refresh();
             });
        }


        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                btnSend_Click(btnSend, new EventArgs());
        }
        private void UpdateTheme()
        {
            this.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            txtMsg.ForeColor = KGlobal.kdockmain.PanelMain.BackColor;
            btnSend.ForeColor = KGlobal.kdockmain.PanelMain.BackColor;
            flpChat.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            Label1.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            Label2.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            lblHost.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            lblClose.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            lblMin.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            txtMsg.BackColor = KGlobal.kdockmain.lblLAN.BackColor;
            btnSend.BackColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            recColor = KGlobal.kdockmain.lblLAN.BackColor;
            senColor = KGlobal.kdockmain.PanelDrawer.BackColor;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = Cursor.Position;
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            tmrClose.Start();
        }

        private void frmLanChat_Load(object sender, EventArgs e)
        {
            UpdateTheme();
            this.Opacity = KGlobal.kdockmain.Opacity;
            lblHost.Text = this.Text;
            if (File.Exists(KGlobal.path + "InMsg.k"))
            {
                string[] str = File.ReadAllText(KGlobal.path + "InMsg.k").Split(KGlobal.mainsep);
                string sInmsg = "";
                for (int i = 0; i <= str.Length - 1; i++)
                {
                    if (!str[i].Contains(this.Tag.ToString()))
                    {
                        if (str[i].Trim() != "")
                            sInmsg += str[i] + KGlobal.mainsep;
                    }
                    else
                        GotMsg(str[i].Split(KGlobal.smallsep)[2]);
                }
                File.WriteAllText(KGlobal.path + "InMsg.k", sInmsg);
            }
            tmrOpen.Start();
            txtMsg.Focus();
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = e.KeyChar == '\r';
        }
        private int sy = 0;
        private void Label1_Click(object sender, EventArgs e)
        {
            if (sy < 1)
                sy = 0;
            else
            {
                sy -= 150;

                flpChat.AutoScrollPosition = new Point(0, sy);
            }
        }

        private void Label2_Click(object sender, EventArgs e)
        {
            if (sy > flpChat.VerticalScroll.Maximum - 150)
            {
                sy = flpChat.VerticalScroll.Maximum - 150;
            }
            else
            {
                sy += 150;
                flpChat.AutoScrollPosition = new Point(0, sy);
            }
        }
        private int j = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            tmrClose.Stop();
            if (this.Height < System.Convert.ToInt32(420))
            {
                this.Height += 30;
                Panel1.Location = new Point(0, 0);
            }
            else
            {
                this.Height = System.Convert.ToInt32(420);
                if (Panel1.Location.Y < 22)
                {
                    j += 1;
                    Panel1.Location = new Point(0, j);
                }
                else
                {
                    tmrOpen.Stop();
                    j = 0;
                }
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            tmrOpen.Stop();
            if (this.Height >= 30)
                this.Height -= 30;
            else
            {
                tmrClose.Stop();
                this.Dispose();
                GC.Collect();
            }
        }

        private void lblMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMsg.Text.Trim() != "")
            {
                try
                {
                    // Client = New TcpClient(Me.Tag, PortString)
                    if (Client.Connected)
                    {
                        //StreamWriter sw = new StreamWriter(Client.GetStream());

                        BinaryWriter br = new BinaryWriter(Client.GetStream());
                        NetworkPackage np = new NetworkPackage("MSG:" + Environment.MachineName, txtMsg.Text.Replace("\r\n", "\n"));
                        br.Write(np.ToByte());
                        br.Flush();
                    }
                    //else
                    //{
                    //    Client.Connect(Client.Client.RemoteEndPoint.AddressFamily.ToString().Split(':')[0], int.Parse(Client.Client.RemoteEndPoint.AddressFamily.ToString().Split(':')[1]));

                    //    BinaryWriter br = new BinaryWriter(Client.GetStream());
                    //    NetworkPackage np = new NetworkPackage("MSG:" + Environment.MachineName, txtMsg.Text.Replace("\r\n", "\n"));
                    //    br.Write(np.ToByte());
                    //    br.Flush();
                    //}
                    TextBox lbl = new TextBox();
                    lbl.BorderStyle = BorderStyle.None;
                    lbl.Text = "Me :  " + txtMsg.Text;
                    lbl.ReadOnly = true;
                    lbl.BackColor = senColor;
                    lbl.ForeColor = this.BackColor;
                    lbl.MinimumSize = new Size(197, 19);
                    lbl.MaximumSize = new Size(197, 0);
                    lbl.AutoSize = true;
                    lbl.Multiline = true;
                    lbl.Margin = new Padding(25, 5, 0, 5);
                    lbl.TextAlign = HorizontalAlignment.Left;
                    lbl.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
                    // Dim iHeight As Integer = TextRenderer.MeasureText(Me.Text, Me.Font, New Size(Me.ClientSize.Width, 1000), TextFormatFlags.WordBreak).Height

                    lbl.Height = TextRenderer.MeasureText(lbl.Text, lbl.Font, new Size(lbl.ClientSize.Width, 1000), TextFormatFlags.WordBreak).Height + 10;
                    flpChat.Controls.Add(lbl);
                    flpChat.VerticalScroll.Value = flpChat.VerticalScroll.Maximum;
                    txtMsg.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        int FileID = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            //string File = "H:\\TechStock - demo college.rar";
            //int TotalLength = 0;
            //bool bStop = false;
            //int BufferSize = 1024;
            if (Client.Connected)
            {
                OpenFileDialog op = new OpenFileDialog();
                if (op.ShowDialog() == DialogResult.OK)
                {
                    string Directory = op.FileName.Substring(0, op.FileName.LastIndexOf('\\')) + "\\";
                    string Filename = op.FileName.Split('\\').Last();
                    FileID++;

                    Thread t1 = new Thread(new ThreadStart(delegate
                    {

                        FileObject fobj = new FileObject(FileID, Filename, Directory);
                        FileSender fs = new FileSender(fobj, Client);
                        fs.Send();
                    }));
                    t1.Start();
                }

            }
        }
    }
}
