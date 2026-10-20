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
using Kdock.Logic;

namespace Kdock
{
    public partial class frmChat : Form
    {
        public frmChat()
        {
            InitializeComponent();
        }
        public TcpClient Client;
        public LanSession Session;
        // Dim receiveBuffer(bufferSize) As Byte
        // Dim sendBuffer(bufferSize) As Byte
        private Color recColor = Color.IndianRed;
        private Color senColor = Color.Blue;
        private readonly Dictionary<string, ucChatFile> _fileCards = new Dictionary<string, ucChatFile>();


        public void GotMsg(string msg)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                AddBubble(msg, false);
            });
        }

        private void AddBubble(string msg, bool mine)
        {
            int width = Math.Max(160, flpChat.ClientSize.Width - 72);
            TextBox lbl = new TextBox();
            lbl.BorderStyle = BorderStyle.None;
            lbl.ReadOnly = true;
            lbl.Text = msg;
            lbl.BackColor = mine ? Color.FromArgb(0, 103, 192) : Color.FromArgb(243, 243, 243);
            lbl.ForeColor = mine ? Color.White : Color.FromArgb(28, 28, 28);
            lbl.Width = width;
            lbl.Margin = mine ? new Padding(48, 6, 12, 6) : new Padding(12, 6, 48, 6);
            lbl.Multiline = true;
            lbl.Font = new Font("Segoe UI", 10F);
            lbl.Height = TextRenderer.MeasureText(lbl.Text, lbl.Font, new Size(width, 1000), TextFormatFlags.WordBreak).Height + 12;
            flpChat.Controls.Add(lbl);
            flpChat.ScrollControlIntoView(lbl);
        }

        public void ShowIncomingFile(string id, string name, long length)
        {
            if (string.IsNullOrEmpty(id) || _fileCards.ContainsKey(id)) return;
            ucChatFile card = new ucChatFile();
            card.Name = id;
            card.File = new FileObject(id, name, "", length);
            card.Margin = new Padding(12, 6, 12, 6);
            card.Width = Math.Max(180, flpChat.ClientSize.Width - 36);
            _fileCards[id] = card;
            flpChat.Controls.Add(card);
            flpChat.ScrollControlIntoView(card);
        }

        public void UpdateIncomingFile(string id, long done, long total, string savedPath)
        {
            ucChatFile card;
            if (!_fileCards.TryGetValue(id, out card) || card.IsDisposed) return;
            if (!string.IsNullOrEmpty(savedPath))
            {
                string dir = Path.GetDirectoryName(savedPath) ?? "";
                if (dir.Length > 0 && !dir.EndsWith("\\")) dir += "\\";
                long length = total > 0 ? total : done;
                card.File = new FileObject(id, Path.GetFileName(savedPath), dir, length);
            }
            card.UpdateProgress(done);
        }

        public void GotMsg(string msg, FileObject File)
        {
            if (!flpChat.Controls.ContainsKey(File.ID.ToString()))
            {
                ucChatFile lbl = new ucChatFile();
                lbl.Name = File.ID.ToString();
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
            tmrOpen.Stop();
            tmrClose.Stop();
            Close();
        }

        private void frmLanChat_Load(object sender, EventArgs e)
        {
            UpdateTheme();
            StyleChat();
            this.Opacity = 1;
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
            txtMsg.Focus();
            LayoutChat();
        }

        private void StyleChat()
        {
            tmrOpen.Stop();
            tmrClose.Stop();
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(243, 242, 241);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(380, 520);
            ClientSize = new Size(400, 560);
            FormBorderStyle = FormBorderStyle.None;
            lblHost.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblHost.ForeColor = Color.White;
            lblHost.BackColor = Color.FromArgb(92, 46, 145);
            lblClose.Text = "✕";
            lblClose.Font = new Font("Segoe UI", 11F);
            lblClose.ForeColor = Color.White;
            lblClose.BackColor = Color.FromArgb(92, 46, 145);
            lblClose.AutoSize = false;
            lblClose.Size = new Size(32, 28);
            lblClose.TextAlign = ContentAlignment.MiddleCenter;
            lblMin.Text = "—";
            lblMin.Font = new Font("Segoe UI", 11F);
            lblMin.ForeColor = Color.White;
            lblMin.BackColor = Color.FromArgb(92, 46, 145);
            lblMin.AutoSize = false;
            lblMin.Size = new Size(32, 28);
            lblMin.TextAlign = ContentAlignment.MiddleCenter;
            Panel2.Visible = false;
            PictureBox1.Visible = false;
            flpChat.BackColor = Color.FromArgb(243, 242, 241);
            flpChat.FlowDirection = FlowDirection.TopDown;
            flpChat.WrapContents = false;
            flpChat.Padding = new Padding(0, 8, 0, 8);
            Panel1.BackColor = Color.FromArgb(243, 242, 241);
            txtMsg.BorderStyle = BorderStyle.None;
            txtMsg.BackColor = Color.FromArgb(243, 243, 243);
            txtMsg.ForeColor = Color.FromArgb(28, 28, 28);
            txtMsg.Font = new Font("Segoe UI", 10F);
            btnSend.Text = "Send";
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.BackColor = Color.FromArgb(92, 46, 145);
            btnSend.ForeColor = Color.White;
            btnSend.Font = new Font("Segoe UI", 9F);
            button1.Text = "+";
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;
            button1.BackColor = Color.FromArgb(243, 243, 243);
            button1.ForeColor = Color.FromArgb(32, 32, 32);
            button1.Font = new Font("Segoe UI", 16F);
            Label1.Visible = false;
            Label2.Visible = false;
            lblHost.MouseDown += DragChatDown;
            lblHost.MouseMove += DragChatMove;
            lblHost.MouseUp += delegate { _chatDrag = false; };
            Resize += delegate { LayoutChat(); };
        }

        private bool _chatDrag;
        private Point _chatAt;

        private void DragChatDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _chatDrag = true;
            _chatAt = Cursor.Position;
        }

        private void DragChatMove(object sender, MouseEventArgs e)
        {
            if (!_chatDrag) return;
            Point now = Cursor.Position;
            Location = new Point(Location.X + now.X - _chatAt.X, Location.Y + now.Y - _chatAt.Y);
            _chatAt = now;
        }

        private void LayoutChat()
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height;
            lblHost.SetBounds(0, 0, w, 48);
            lblHost.Padding = new Padding(16, 0, 0, 0);
            lblHost.TextAlign = ContentAlignment.MiddleLeft;
            lblMin.SetBounds(w - 80, 10, 32, 28);
            lblClose.SetBounds(w - 42, 10, 32, 28);
            Panel1.SetBounds(0, 48, w, Math.Max(80, h - 48));
            flpChat.SetBounds(0, 0, Panel1.Width, Math.Max(40, Panel1.Height - 72));
            button1.SetBounds(12, Panel1.Height - 58, 40, 40);
            txtMsg.SetBounds(60, Panel1.Height - 54, Math.Max(80, Panel1.Width - 156), 34);
            btnSend.SetBounds(Panel1.Width - 84, Panel1.Height - 58, 72, 40);
            foreach (Control control in flpChat.Controls)
            {
                ucChatFile card = control as ucChatFile;
                if (card != null)
                    card.Width = Math.Max(180, flpChat.ClientSize.Width - 36);
            }
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
            if (this.Height < 520)
            {
                this.Height += 30;
                Panel1.Location = new Point(0, 0);
            }
            else
            {
                this.Height = 520;
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
            if (!string.IsNullOrWhiteSpace(txtMsg.Text) && Session != null && Client != null && Client.Connected)
            {
                try
                {
                    var text = txtMsg.Text.Replace("\r\n", "\n");
                    ChatCommands.SendText(Session, text);

                    AddBubble(text, true);
                    txtMsg.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending message: " + ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Client == null || !Client.Connected || Session == null) return;

            using (var dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string fullPath = dialog.FileName;
                    string directory = Path.GetDirectoryName(fullPath) + Path.DirectorySeparatorChar;
                    string filename = Path.GetFileName(fullPath);
                    string fileID = Guid.NewGuid().ToString();
                    long fileLength = new FileInfo(fullPath).Length;

                    var fobj = new FileObject(fileID, filename, directory, fileLength);

                    var lbl = new ucChatFile
                    {
                        Name = "S" + fobj.ID,
                        Text = this.Tag + " : " + fobj.FileName,
                        BackColor = recColor,
                        ForeColor = this.BackColor,
                        File = fobj,
                        Margin = new Padding(25, 5, 0, 5),
                        Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold)
                    };

                    lbl.Width = Math.Max(180, flpChat.ClientSize.Width - 36);
                    _fileCards[fileID] = lbl;
                    ChatCommands.OfferFile(Session, fileID, filename, fileLength);
                    Session.SendFile(fullPath, fileID, delegate (long done, long total)
                    {
                        if (IsDisposed) return;
                        try
                        {
                            BeginInvoke(new Action(delegate
                            {
                                if (!lbl.IsDisposed)
                                    lbl.UpdateProgress(done);
                            }));
                        }
                        catch
                        {
                        }
                    });

                    flpChat.Controls.Add(lbl);
                    flpChat.VerticalScroll.Value = flpChat.VerticalScroll.Maximum;
                    this.Refresh();
                }
            }
        }
    }
}
