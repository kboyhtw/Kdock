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
            StyleWindow();
        }

        private bool _drag;
        private Point _dragAt;

        private void StyleWindow()
        {
            StartPosition = FormStartPosition.Manual;
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            int stack = 0;
            foreach (Form open in Application.OpenForms)
            {
                if (open is frmFileStatus) stack++;
            }
            Location = new Point(area.Right - Width - 24, area.Bottom - Height - 24 - stack * 28);
            ShowInTaskbar = false;

            MouseEventHandler down = delegate (object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left || sender == lblCancelT) return;
                _drag = true;
                _dragAt = Cursor.Position;
            };
            MouseEventHandler move = delegate (object sender, MouseEventArgs e)
            {
                if (!_drag) return;
                Point now = Cursor.Position;
                Location = new Point(Location.X + now.X - _dragAt.X, Location.Y + now.Y - _dragAt.Y);
                _dragAt = now;
            };
            MouseEventHandler up = delegate { _drag = false; };
            MouseDown += down;
            MouseMove += move;
            MouseUp += up;
            foreach (Control control in pTransfer.Controls)
            {
                control.MouseDown += down;
                control.MouseMove += move;
                control.MouseUp += up;
            }
        }

        double lastsize = 0;
        public string sMode = "R";
        public string TransferId;
        public Action CancelAction;


        private void timerSpeed_Tick(object sender, EventArgs e)
        {
            double max = Math.Max(1, ProgressBar1.Maximum);
            double val = ProgressBar1.Value;
            double percent = val * 100.0 / max;
            double speed = (val - lastsize) / (1024.0 * 1024.0);
            Label2.Text = percent.ToString("0") + "%   ·   " + FormatBytes(val) + " of " + FormatBytes(max) + "   ·   " + speed.ToString("0.0") + " MB/s";
            lastsize = val;
        }

        private static string FormatBytes(double bytes)
        {
            if (bytes >= 1024.0 * 1024.0 * 1024.0)
                return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("0.00") + " GB";
            if (bytes >= 1024.0 * 1024.0)
                return (bytes / (1024.0 * 1024.0)).ToString("0.00") + " MB";
            if (bytes >= 1024.0)
                return (bytes / 1024.0).ToString("0.0") + " KB";
            return bytes.ToString("0") + " B";
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
                lblSource.Text = (value ?? "").Replace("/n", Environment.NewLine);
                lblCopyTitle.Text = sMode == "S" ? "Sending" : "Receiving";
            }
        }

        private void lblCancelT_Click(object sender, EventArgs e)
        {
            try
            {
                if (CancelAction != null)
                    CancelAction();
            }
            catch
            {
            }
            if (!IsDisposed)
                Close();
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
