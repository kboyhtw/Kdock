using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;

namespace Kdock
{
    public partial class ucFIle : UserControl
    {
        public ucFIle()
        {
            InitializeComponent();
        }

        public void LoadMe(string sFile)
        {
            this.Tag = sFile;
            this.Name = "uc" + sFile;
            Label1.Text = sFile.Split('\\').Last();
            ToolTip1.SetToolTip(Label1, Label1.Text);
            Label2.Visible = false;
            Button2.Visible = false;
            ShellIcons.PutOn(Button1, ShellIcons.ForFile(Label1.Text));
            ContextMenuStrip = ContextMenuStrip1;
        }

        public void ApplyView(bool list, int rowWidth)
        {
            Button2.Visible = false;
            Label2.Visible = false;
            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.White;
            Label1.ForeColor = Color.White;
            Label1.Font = new Font("Segoe UI", list ? 10F : 8.5F);
            if (list)
            {
                Size = new Size(Math.Max(220, rowWidth), 36);
                Button1.SetBounds(10, 4, 28, 28);
                Label1.TextAlign = ContentAlignment.MiddleLeft;
                Label1.SetBounds(46, 4, Math.Max(40, Width - 56), 28);
            }
            else
            {
                Size = new Size(104, 96);
                Button1.SetBounds(28, 8, 48, 48);
                Label1.TextAlign = ContentAlignment.TopCenter;
                Label1.SetBounds(2, 58, 100, 34);
            }
        }

        private void MoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Tag = "movefile:" + this.Tag;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = false;
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Tag = "copyfile:" + this.Tag;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = false;
        }
        private TcpClient Client;
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Safely resolve parent and host IP
                RemoteFiles parentForm = this.ParentForm as RemoteFiles;
                if (parentForm == null || parentForm.Tag == null || this.Tag == null)
                    return;

                string host = parentForm.Tag.ToString();
                string filePath = this.Tag.ToString();

                if (parentForm.Session != null)
                {
                    parentForm.Session.Send(new NetworkPackage(
                        PackageType.String,
                        "files:deletefile:" + Environment.MachineName,
                        Encoding.UTF8.GetBytes(filePath)));
                }
            }
            catch (Exception ex)
            {
                // Optional: log or show feedback
                // MessageBox.Show("File delete failed: " + ex.Message);
            }
        }

        private void CopyToThisPCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            if (fb.ShowDialog() == DialogResult.OK)
            {
                RemoteFiles parent = this.ParentForm as RemoteFiles;
                if (parent != null && parent.Session != null)
                {
                    parent.Session.IncomingFileDirectory = fb.SelectedPath;
                    parent.Session.Send(new NetworkPackage(
                        PackageType.String,
                        "files:getfile",
                        Encoding.UTF8.GetBytes(this.Tag.ToString())));
                }
            }
        }
    }
}
