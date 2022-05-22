using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class frmRemote : Form
    {
        public frmRemote()
        {
            InitializeComponent();
        }

        public RemoteClient RClient;
        public NetworkStream ClientStream;
        string key;
        Point cur;
        bool bsize = true;
        Size imgSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        public Size clientImgSize;


        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private void Form2_Load(object sender, EventArgs e)
        {
            TextBox1.Text = KGlobal.Listener.LocalEndpoint.AddressFamily.ToString();
            //ImageCodecInfo jpgEncoder = KGlobal.GetEncoder(ImageFormat.Jpeg);
            //System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            //System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
            //System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 5);
            //myEncoderParameters.Param[0] = myEncoderParameter;
            //KGlobal.GetScreenShot(new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)).Save("d:\\dcap.jpg", jpgEncoder, myEncoderParameters);
            KGlobal.WriteTextToStream(ClientStream, "</ARW/>");
        }

        private void btnFullScreen_Click(object sender, EventArgs e)
        {
            if ((this.FormBorderStyle == FormBorderStyle.None))
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblOC_Click(object sender, EventArgs e)
        {
            if ((Panel1.Size == new Size(345, 5)))
            {
                Panel1.Size = new Size(345, 35);
            }
            else
            {
                Panel1.Size = new Size(345, 5);
            }

        }

        private void lblMaxmimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void bntStop_Click(object sender, EventArgs e)
        {
            StopRM();
        }

        private void StopRM()
        {
            KGlobal.WriteTextToStream(ClientStream, "</Stop/>");
        }

        // ----------------- Reciever-----------------
        private string keyinput = "";

        private void frmRemote_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((keyinput == ""))
            {
                string ip = TextBox1.Text.Substring(0, TextBox1.Text.LastIndexOf(":"));
                KGlobal.WriteTextToStream(ClientStream, e.KeyChar.ToString());
            }
            else
            {
                // sw.Write(keyinput)
                keyinput = "";
            }

        }

        private void frmRemote_KeyDown(object sender, KeyEventArgs e)
        {
            //string str = e.KeyCode;
            switch (e.KeyValue)
            {
                case 8:
                    keyinput = "{BS}";
                    break;
                case 19:
                    keyinput = "{BREAK}";
                    break;
                case 20:
                    keyinput = "{CAPSLOCK}";
                    break;
                case 46:
                    keyinput = "{DELETE}";
                    break;
                case 40:
                    keyinput = "{DOWN}";
                    break;
                case 35:
                    keyinput = "{End}";
                    break;
                case 13:
                    keyinput = "{ENTER}";
                    break;
                case 27:
                    keyinput = "{ESC}";
                    break;
                case 36:
                    keyinput = "{HOME}";
                    break;
                case 45:
                    keyinput = "{INSERT}";
                    break;
                case 37:
                    keyinput = "{LEFT}";
                    break;
                case 144:
                    keyinput = "{NUMLOCK}";
                    break;
                case 34:
                    keyinput = "{PGDN}";
                    break;
                case 33:
                    keyinput = "{PGUP}";
                    break;
                case 39:
                    keyinput = "{RIGHT}";
                    break;
                case 145:
                    keyinput = "{SCROLLLOCK}";
                    break;
                case 9:
                    keyinput = "{TAB}";
                    break;
                case 38:
                    keyinput = "{UP}";
                    break;
                case 112:
                    keyinput = "{F1}";
                    break;
                case 113:
                    keyinput = "{F2}";
                    break;
                case 114:
                    keyinput = "{F3}";
                    break;
                case 115:
                    keyinput = "{F4}";
                    break;
                case 116:
                    keyinput = "{F5}";
                    break;
                case 117:
                    keyinput = "{F6}";
                    break;
                case 118:
                    keyinput = "{F7}";
                    break;
                case 119:
                    keyinput = "{F8}";
                    break;
                case 120:
                    keyinput = "{F9}";
                    break;
                case 121:
                    keyinput = "{F10}";
                    break;
                case 122:
                    keyinput = "{F11}";
                    break;
                case 123:
                    keyinput = "{F12}";
                    break;
                case 124:
                    keyinput = "{F13}";
                    break;
                case 125:
                    keyinput = "{F14}";
                    break;
                case 126:
                    keyinput = "{F15}";
                    break;
                case 127:
                    keyinput = "{F16}";
                    break;
                case 107:
                    keyinput = "{ADD}";
                    break;
                case 109:
                    keyinput = "{SUBTRACT}";
                    break;
                case 106:
                    keyinput = "{MULTIPLY}";
                    break;
                case 111:
                    keyinput = "{DIVIDE}";
                    break;
                case 65552:
                    keyinput = "+";
                    break;
                case 131089:
                    keyinput = "^";
                    break;
                case 262162:
                    keyinput = "%";
                    break;
            }
            if (!(keyinput == ""))
            {
                //   Dim ip As String = TextBox1.Text.Substring(0, TextBox1.Text.LastIndexOf(":"))
                KGlobal.WriteTextToStream(ClientStream, keyinput);
            }

        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                string sinput = "";
                string ip = TextBox1.Text.Substring(0, TextBox1.Text.LastIndexOf(":"));
                Point pt = getCursor();
                if ((e.Button == MouseButtons.Left))
                {
                    sinput = "</" + pt.X.ToString() + ", " + pt.Y.ToString() + "/>--";
                }
                else if ((e.Button == MouseButtons.Right))
                {
                    sinput = "</" + pt.X.ToString() + ", " + pt.Y.ToString() + "/>||";
                }
                else if ((e.Button == MouseButtons.Middle))
                {
                    sinput = "</" + pt.X.ToString() + ", " + pt.Y.ToString() + "/>@@";
                }

                KGlobal.WriteTextToStream(ClientStream, sinput);
            }
            catch (Exception ex)
            {
            }

            tmrCursor.Start();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                string sinput = "";
                string ip = TextBox1.Text.Substring(0, TextBox1.Text.LastIndexOf(":"));
                Point pt = getCursor();
                if ((e.Button == MouseButtons.Left))
                {
                    sinput = "</" + pt.X.ToString() + ", " + pt.Y.ToString() + "/>++";
                }
                else if ((e.Button == MouseButtons.Right))
                {
                    sinput = "</" + pt.X.ToString() + ", " + pt.Y.ToString() + "/>??";
                }
                else if ((e.Button == MouseButtons.Middle))
                {
                    sinput = "</" + pt.X.ToString() + ", " + pt.Y.ToString() + "/>##";
                }

                KGlobal.WriteTextToStream(ClientStream, sinput);
            }
            catch (Exception ex)
            {
            }

            tmrCursor.Stop();
        }

        private void PicDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            cur = e.Location;
        }

        private void PicDisplay_MouseMove(object sender, EventArgs e)
        {
            // If Timer2.Enabled Then
            //     If Listener.Pending = True Then
            //         Dim ip As String = TextBox1.Text.Substring(0, TextBox1.Text.LastIndexOf(":"))
            //         Client = New TcpClient(ip, 8000)
            //         Dim pt As Point = getCursor()
            //         Dim sw As New StreamWriter(Client.GetStream())
            //         sw.Write("</" & pt.X & ", " & pt.Y & "/>**")
            //         sw.Flush()
            //         sw.Close()
            //     End If
            // End If
        }

        private void tmrCursor_Tick(object sender, EventArgs e)
        {
            //    Dim ip As String = TextBox1.Text.Substring(0, TextBox1.Text.LastIndexOf(":"))
            Point pt = getCursor();
            KGlobal.WriteTextToStream(ClientStream, "</" + pt.X.ToString() + ", " + pt.Y.ToString() + "/>**");
        }

        private Point getCursor()
        {
            Point pt = new Point(0, 0);
            pt.X = clientImgSize.Width / PicDisplay.Width * cur.X;
            pt.Y = clientImgSize.Height / PicDisplay.Height * cur.Y;
            return pt;
        }

        public void setSize()
        {
            if ((clientImgSize.Width > Screen.PrimaryScreen.Bounds.Width))
            {
                double dbl = Convert.ToDouble(clientImgSize.Height) / Convert.ToDouble(clientImgSize.Width);
                int wid = Screen.PrimaryScreen.Bounds.Width;
                int hei = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width / dbl);
                PicDisplay.Size = new Size(wid, hei);
                PicDisplay.Location = new Point(0, ((this.Height / 2) - (PicDisplay.Height / 2)));
            }
            else if ((clientImgSize.Height > Screen.PrimaryScreen.WorkingArea.Height) && (this.FormBorderStyle == FormBorderStyle.Sizable))
            {
                PicDisplay.SizeMode = PictureBoxSizeMode.StretchImage;
                double dbl = Convert.ToDouble(clientImgSize.Height) / Convert.ToDouble(clientImgSize.Width);
                int hei = (Screen.PrimaryScreen.WorkingArea.Height - 16);
                int wid = Convert.ToInt32((Screen.PrimaryScreen.WorkingArea.Height - 16) / dbl);
                PicDisplay.Size = new Size(wid, hei);
                PicDisplay.Location = new Point(((this.Width / 2) - (PicDisplay.Width / 2)), 0);
            }
            else if (((clientImgSize.Height > Screen.PrimaryScreen.Bounds.Height) && (this.FormBorderStyle == FormBorderStyle.None)))
            {
                double dbl = Convert.ToDouble(clientImgSize.Height) / Convert.ToDouble(clientImgSize.Width);
                int hei = Screen.PrimaryScreen.Bounds.Height;
                int wid = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height / dbl);
                PicDisplay.Size = new Size(wid, hei);
                PicDisplay.Location = new Point(((this.Width / 2) - (PicDisplay.Width / 2)), 0);
            }
            else
            {
                PicDisplay.SizeMode = PictureBoxSizeMode.AutoSize;
                PicDisplay.Location = new Point(((this.Width / 2) - (PicDisplay.Width / 2)), ((this.Height / 2) - (PicDisplay.Height / 2)));
            }

        }

        private void frmRemote_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopRM();
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            KGlobal.WriteTextToStream(ClientStream, "<%" + TrackBar1.Value + "%>");
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!(RClient == null))
            {
                if (!(RClient.RmImage == null))
                {
                    setSize();
                  try
                    {
                        PicDisplay.Image = RClient.RmImage;
                    }
                    catch { }
                    MinimumSize = new Size((PicDisplay.Width + 16), (PicDisplay.Height + 39));
                    TextBox1.Text = RClient.ClientAddress;
                }

            }

        }
    }
}
