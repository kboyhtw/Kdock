using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Kdock
{
    public partial class ucChatFile : UserControl
    {
        private FileObject _file;

        public ucChatFile()
        {
            InitializeComponent();
        }

        public FileObject File
        {
            get { return _file; }
            set
            {
                _file = value;
                if (_file != null)
                {
                    textBox1.Text = _file.FileName;
                    textBox1.ReadOnly = true;
                    textBox1.Cursor = Cursors.Hand;
                    textBox1.BorderStyle = BorderStyle.None;
                    progressLite1.Maximum = (int)_file.TotalLength;
                    progressLite1.Value = (int)_file.ProcessedData;
                    lblStatus.Text = $"0.00/{_file.TotalLength / 1024.0 / 1024.0:0.00} Mb";
                }
            }
        }

        public void UpdateProgress(long bytesSent)
        {
            if (_file == null) return;

            _file.ProcessedData = bytesSent;
            _file.IsCompleted = bytesSent >= _file.TotalLength;

            double mbSent = bytesSent / 1024.0 / 1024.0;
            double mbTotal = _file.TotalLength / 1024.0 / 1024.0;

            progressLite1.Value = (int)Math.Min(bytesSent, progressLite1.Maximum);
            lblStatus.Text = $"{mbSent:0.00}/{mbTotal:0.00} Mb";

            if (_file.IsCompleted)
            {
                progressLite1.Visible = false;
                lblStatus.Visible = true;
                lblStatus.Text = "Click to open";
                try
                {
                    picIcon.Image = KGlobal.GetIcon(_file.FullPath).ToBitmap();
                }
                catch { /* silently fail if icon load fails */ }
            }
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (_file != null && _file.IsCompleted)
            {
                try { Process.Start(_file.FullPath); }
                catch { /* optional: show error */ }
            }
        }
    }
}

