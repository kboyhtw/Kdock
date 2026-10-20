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
using Kdock.Logic;

namespace Kdock
{
    public partial class frmRemote : Form
    {
        public frmRemote()
        {
            InitializeComponent();
        }

        public RemoteClient RClient;
        public LanSession Session;
        public Image LatestImage;
        public NetworkStream ClientStream;
        string key;
        Point cur;
        bool bsize = true;
        DateTime _lastMoveSent = DateTime.MinValue;
        Point _lastSentRemote = new Point(-1, -1);
        Size imgSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        public Size clientImgSize;


        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private bool _full;
        private DateTime _hoverUntil;
        private Timer _hoverTimer;
        private bool _dragBar;
        private Point _dragAt;

        private void Form2_Load(object sender, EventArgs e)
        {
            AutoScroll = false;
            AutoScrollMinSize = Size.Empty;
            StyleRemote();
            PicDisplay.SizeMode = PictureBoxSizeMode.Normal;
            PicDisplay.Image = null;
            PicDisplay.Paint += PicDisplay_Paint;
            Rectangle area = Screen.FromControl(this).WorkingArea;
            Size = new Size((int)(area.Width * 0.75), (int)(area.Height * 0.75));
            MinimumSize = new Size(760, 480);
            PicDisplay.MouseWheel += PicDisplay_MouseWheel;
            KeyPreview = true;
            KeyPress += frmRemote_KeyPress;
            Resize += delegate { FitView(); };
            FitView();
            TextBox1.Text = KGlobal.Listener.LocalEndpoint.AddressFamily.ToString();
            //ImageCodecInfo jpgEncoder = KGlobal.GetEncoder(ImageFormat.Jpeg);
            //System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            //System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
            //System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 5);
            //myEncoderParameters.Param[0] = myEncoderParameter;
            //KGlobal.GetScreenShot(new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)).Save("d:\\dcap.jpg", jpgEncoder, myEncoderParameters);
            RemoteCommands.Ready(Session);
        }

        private void StyleRemote()
        {
            Panel1.Visible = false;
            pnlSettings.Visible = false;
            Text = Tag == null ? "Remote desktop" : Tag.ToString();
            lblRemoteTitle.Text = Text;

            MouseEventHandler down = delegate (object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left || _full) return;
                _dragBar = true;
                _dragAt = Cursor.Position;
            };
            MouseEventHandler move = delegate
            {
                if (!_dragBar) return;
                Point now = Cursor.Position;
                Location = new Point(Location.X + now.X - _dragAt.X, Location.Y + now.Y - _dragAt.Y);
                _dragAt = now;
            };
            MouseEventHandler up = delegate { _dragBar = false; };
            Panel1.MouseDown += down;
            Panel1.MouseMove += move;
            Panel1.MouseUp += up;
            lblRemoteTitle.MouseDown += down;
            lblRemoteTitle.MouseMove += move;
            lblRemoteTitle.MouseUp += up;
            _hoverTimer = new Timer();
            _hoverTimer.Interval = 180;
            _hoverTimer.Tick += delegate { HideActionsIfIdle(); };
            _hoverTimer.Start();
        }

        private void lblSettings_Click(object sender, EventArgs e)
        {
            ToggleSettings();
        }

        private void btnLow_Click(object sender, EventArgs e)
        {
            ApplyPace(22, 240);
        }

        private void btnBalanced_Click(object sender, EventArgs e)
        {
            ApplyPace(45, 100);
        }

        private void btnSharp_Click(object sender, EventArgs e)
        {
            ApplyPace(75, 60);
        }

        private void ApplyPace(int quality, int pace)
        {
            TrackBar1.Value = Math.Max(TrackBar1.Minimum, Math.Min(TrackBar1.Maximum, quality));
            TrackBar1_Scroll(TrackBar1, EventArgs.Empty);
            RemoteCommands.Pace(Session, pace);
        }

        private void ToggleSettings()
        {
            pnlSettings.Visible = !pnlSettings.Visible;
            RevealActions();
            _hoverUntil = DateTime.UtcNow.AddSeconds(3);
        }

        private void Clipboard_CheckedChanged(object sender, EventArgs e)
        {
            RemoteCommands.Clipboard(Session, chkClipboard.Checked);
        }

        private void btnFullScreen_Click(object sender, EventArgs e)
        {
            _full = !_full;
            if (_full)
            {
                pnlSettings.Visible = false;
                Panel1.Visible = false;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                RevealActions();
                _hoverUntil = DateTime.UtcNow.AddSeconds(2);
            }
            FitView();
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
            RemoteCommands.Stop(Session);
        }

        // ----------------- Reciever-----------------
        private string keyinput = "";

        private void frmRemote_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((keyinput == ""))
            {
                string key = RemotePointer.TextKey(e.KeyChar);
                if (key.Length > 0)
                {
                    SendInput(key);
                    e.Handled = true;
                }
            }
            else
            {
                // sw.Write(keyinput)
                keyinput = "";
            }

        }

        private void frmRemote_KeyDown(object sender, KeyEventArgs e)
        {
            keyinput = RemotePointer.SpecialKey(e.KeyValue);
            if (keyinput != "")
            {
                SendInput(keyinput);
                keyinput = "";
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            cur = e.Location;
            if (e.Button == MouseButtons.Right) SendMouse(e.Location, "rd");
            else if (e.Button == MouseButtons.Middle) SendMouse(e.Location, "md");
            else if (e.Clicks >= 2) SendMouse(e.Location, "dbl");
            else SendMouse(e.Location, "ld");
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            cur = e.Location;
            if (e.Clicks >= 2) return;
            if (e.Button == MouseButtons.Right) SendMouse(e.Location, "ru");
            else if (e.Button == MouseButtons.Middle) SendMouse(e.Location, "mu");
            else if (e.Button == MouseButtons.Left) SendMouse(e.Location, "lu");
        }

        private void PicDisplay_MouseWheel(object sender, MouseEventArgs e)
        {
            cur = e.Location;
            SendMouse(e.Location, "wh", e.Delta);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x020A;
            const int WM_NCHITTEST = 0x84;
            if (m.Msg == WM_NCHITTEST && !_full && WindowState == FormWindowState.Normal)
            {
                Point pos = PointToClient(Cursor.Position);
                int grip = 6;
                bool left = pos.X <= grip;
                bool right = pos.X >= ClientSize.Width - grip;
                bool top = pos.Y <= grip;
                bool bottom = pos.Y >= ClientSize.Height - grip;
                if (left && top) { m.Result = (IntPtr)13; return; }
                if (right && top) { m.Result = (IntPtr)14; return; }
                if (left && bottom) { m.Result = (IntPtr)16; return; }
                if (right && bottom) { m.Result = (IntPtr)17; return; }
                if (left) { m.Result = (IntPtr)10; return; }
                if (right) { m.Result = (IntPtr)11; return; }
                if (top) { m.Result = (IntPtr)12; return; }
                if (bottom) { m.Result = (IntPtr)15; return; }
            }
            if (m.Msg == WM_MOUSEWHEEL && PicDisplay != null)
            {
                long wparam = m.WParam.ToInt64();
                int delta = (short)((wparam >> 16) & 0xffff);
                int sx = (short)(m.LParam.ToInt64() & 0xffff);
                int sy = (short)((m.LParam.ToInt64() >> 16) & 0xffff);
                Point local = PicDisplay.PointToClient(new Point(sx, sy));
                SendMouse(local, "wh", delta);
                return;
            }
            base.WndProc(ref m);
        }

        private void PicDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            TrackHover(PointToClient(PicDisplay.PointToScreen(e.Location)));
            cur = e.Location;
            Point remote;
            if (!TryMap(e.Location, out remote)) return;
            if (remote == _lastSentRemote) return;
            if ((DateTime.UtcNow - _lastMoveSent).TotalMilliseconds < 50) return;
            _lastMoveSent = DateTime.UtcNow;
            _lastSentRemote = remote;
            SendMouse(e.Location, "mv");
        }

        public void ShowFrame(Image frame, Size size)
        {
            if (size.Width > 1 && size.Height > 1)
                clientImgSize = size;
            else if (frame != null)
                clientImgSize = frame.Size;
            Image previous = LatestImage;
            LatestImage = frame;
            if (PicDisplay.Image != null)
                PicDisplay.Image = null;
            if (previous != null && previous != frame) previous.Dispose();
            if (Tag != null)
                Text = Tag + "   " + clientImgSize.Width + " x " + clientImgSize.Height;
            FitView();
        }

        internal void ApplyTiles(int width, int height, List<LanSession.ScreenTile> tiles)
        {
            Bitmap canvas = LatestImage as Bitmap;
            if (canvas == null || canvas.Width != width || canvas.Height != height)
            {
                if (LatestImage != null) LatestImage.Dispose();
                canvas = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                LatestImage = canvas;
                clientImgSize = new Size(width, height);
                if (Tag != null)
                    Text = Tag + "   " + width + " x " + height;
                FitView();
            }
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                foreach (LanSession.ScreenTile tile in tiles)
                {
                    if (tile.IsSolid)
                    {
                        using (SolidBrush brush = new SolidBrush(tile.Color))
                            g.FillRectangle(brush, tile.Bounds);
                    }
                    else if (tile.Image != null)
                    {
                        g.DrawImage(tile.Image, tile.Bounds);
                        tile.Image.Dispose();
                        tile.Image = null;
                    }
                }
            }
            PicDisplay.Invalidate();
        }

        private void PicDisplay_Paint(object sender, PaintEventArgs e)
        {
            Image img = LatestImage;
            _shown = ImageRect();
            e.Graphics.Clear(Color.Black);
            if (img == null || _shown.Width < 2 || _shown.Height < 2) return;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            e.Graphics.DrawImage(img, _shown);
        }

        private Rectangle _shown;

        private Size SourceSize()
        {
            int srcW = clientImgSize.Width > 1 ? clientImgSize.Width : (LatestImage != null ? LatestImage.Width : 0);
            int srcH = clientImgSize.Height > 1 ? clientImgSize.Height : (LatestImage != null ? LatestImage.Height : 0);
            return new Size(srcW, srcH);
        }

        private Rectangle ImageRect()
        {
            return RemotePointer.Fit(SourceSize(), PicDisplay.ClientRectangle);
        }

        private void FitView()
        {
            AutoScroll = false;
            AutoScrollMinSize = Size.Empty;
            PicDisplay.Dock = DockStyle.None;
            PicDisplay.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PicDisplay.Bounds = new Rectangle(0, 0, ClientSize.Width, Math.Max(1, ClientSize.Height));
            PicDisplay.SizeMode = PictureBoxSizeMode.Normal;
            PicDisplay.Invalidate();
            PlaceToolbar();
        }

        private void RevealActions()
        {
            int w = Math.Min(560, Math.Max(440, ClientSize.Width - 120));
            Panel1.SetBounds(Math.Max(8, (ClientSize.Width - w) / 2), 8, w, 44);
            btnFullScreen.Text = _full ? "Exit" : "▢";
            btnFullScreen.Size = new Size(_full ? 48 : 32, 28);
            btnClose.Location = new Point(Panel1.Width - 36, 10);
            btnFullScreen.Location = new Point(Panel1.Width - (_full ? 88 : 72), 10);
            lblMinimize.Location = new Point(Panel1.Width - (_full ? 124 : 108), 10);
            Panel1.Visible = true;
            Panel1.BringToFront();
            if (pnlSettings.Visible)
            {
                pnlSettings.SetBounds(Panel1.Left, Panel1.Bottom + 6, Panel1.Width, 150);
                pnlSettings.BringToFront();
            }
        }

        private void TrackHover(Point client)
        {
            bool nearTop = client.Y >= 0 && client.Y <= 22;
            bool center = Math.Abs(client.X - ClientSize.Width / 2) < Math.Max(200, ClientSize.Width / 4);
            bool overBar = Panel1.Visible && Panel1.Bounds.Contains(client);
            bool overSettings = pnlSettings.Visible && pnlSettings.Bounds.Contains(client);
            if ((nearTop && center) || overBar || overSettings)
            {
                RevealActions();
                _hoverUntil = DateTime.UtcNow.AddMilliseconds(900);
            }
        }

        private void HideActionsIfIdle()
        {
            if (!Panel1.Visible) return;
            if (DateTime.UtcNow < _hoverUntil) return;
            if (pnlSettings.Visible) return;
            Point cursor = PointToClient(Cursor.Position);
            if (Panel1.Bounds.Contains(cursor)) return;
            Panel1.Visible = false;
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
        }

        private void SendInput(string input)
        {
            RemoteCommands.Input(Session, input);
        }

        private void SendMouse(Point local, string action, int wheel = 0)
        {
            RemoteCommands.Mouse(Session, local, SourceSize(), ImageRect(), action, wheel);
        }

        private bool TryMap(Point local, out Point remote)
        {
            return RemotePointer.TryMap(local, SourceSize(), ImageRect(), out remote);
        }

        private void PlaceToolbar()
        {
            if (Panel1.Visible)
                RevealActions();
        }

        public void setSize()
        {
            FitView();
        }

        private void frmRemote_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopRM();
            if (Session != null)
                Session.EndRemoteView();
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            SendInput("<%" + TrackBar1.Value + "%>");
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Image frame = LatestImage;
            if (frame == null && RClient != null)
                frame = RClient.RmImage;
            if (frame != null && Tag != null)
                TextBox1.Text = Tag.ToString();

        }
    }
}
