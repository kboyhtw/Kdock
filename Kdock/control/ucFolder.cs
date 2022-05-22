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
        }
        public  TcpClient Client;

        private void Button1_DoubleClick(object sender, EventArgs e)
        {
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string host = ((RemoteFiles)this.ParentForm).Tag.ToString();
                ((RemoteFiles)this.ParentForm).Label1.Text = this.Tag.ToString();
               // Client = new TcpClient(host, KGlobal.PortString);
                if (Client.Connected)
                {
                    StreamWriter sw = new StreamWriter(Client.GetStream());
                    sw.Write("<^" + this.Tag + "^>");
                    sw.Flush();
                   // sw.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void MoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Tag = "D<" + this.Tag;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = false;
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Tag = "D+" + this.Tag;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = false;
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string host = ((RemoteFiles)this.ParentForm).Tag.ToString();
                Client = new TcpClient(host, KGlobal.PortString);
                if (Client.Connected)
                {
                    StreamWriter sw = new StreamWriter(Client.GetStream());
                    sw.Write("<-" + "D-" + this.Tag + "->");
                    sw.Flush();
                   // sw.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
