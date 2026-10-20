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
    public partial class ucFolder : UserControl
    {
        public ucFolder()
        {
            InitializeComponent();
        }

        public void LoadMe(string Folder,TcpClient pclient)
        {
            Client = pclient;
            this.Tag = Folder;
            this.Name = "uc" + Folder;
            if (Folder.Split('\\').Last() != "")
                Label1.Text = Folder.Split('\\').Last();
            else
                Label1.Text = Folder;
            ToolTip1.SetToolTip(Label1, Label1.Text);
            Button2.Visible = false;
            ShellIcons.PutOn(Button1, ShellIcons.ForFolder());
            Label1.Click += Button1_Click;
            ContextMenuStrip = ContextMenuStrip1;
        }

        public void ApplyView(bool list, int rowWidth)
        {
            Button2.Visible = false;
            BackColor = Color.FromArgb(32, 32, 32);
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
        public  TcpClient Client;

        private void Button1_DoubleClick(object sender, EventArgs e)
        {
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Safely get parent and target path
                RemoteFiles parentForm = this.ParentForm as RemoteFiles;
                if (parentForm == null || parentForm.Tag == null || this.Tag == null)
                    return;

                string host = parentForm.Tag.ToString();
                string targetPath = this.Tag.ToString();
                parentForm.Label1.Text = targetPath;

                // Reuse shared TcpClient if connected
                if (parentForm.Session != null)
                {
                    parentForm.Session.Send(new NetworkPackage(
                        PackageType.String,
                        "files:getdrives:" + Environment.MachineName,
                        Encoding.UTF8.GetBytes(targetPath)));
                }
            }
            catch (Exception ex)
            {
                // Optional: log or show error
                // MessageBox.Show("Unable to trigger explorer refresh: " + ex.Message);
            }
        }

        private void MoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Tag = "movedirectory:" + this.Tag;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = false;
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Tag = "copydirectory:" + this.Tag;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = false;
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string host = null;
                if (this.ParentForm is RemoteFiles parentForm && parentForm.Tag != null)
                {
                    host = parentForm.Tag.ToString();
                }

                if (!string.IsNullOrEmpty(host))
                {
                    RemoteFiles explorer = this.ParentForm as RemoteFiles;
                    if (explorer != null && explorer.Session != null)
                    {
                        string targetPath = this.Tag != null ? this.Tag.ToString() : "";
                        explorer.Session.Send(new NetworkPackage(
                            PackageType.String,
                            "files:deletedirectory:" + Environment.MachineName,
                            Encoding.UTF8.GetBytes(targetPath)));
                    }
                }
            }
            catch (Exception ex)
            {
                // Optional logging or feedback
                // MessageBox.Show("Delete failed: " + ex.Message);
            }
        }
    }
}
