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
            if (Label1.Text.Split('.').Last().ToUpper() == "TXT")
                Label2.Visible = false;
            else
            {
                Label2.Text = Label1.Text.Split('.').Last().ToUpper();
                if (Label2.Text.Length > 4)
                    Label2.Text = Label1.Text.Split('.').Last().ToUpper().Substring(0, 4);
                Label2.Visible = true;
            }
        }

        private void MoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Tag = "F<" + this.Tag;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = false;
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Tag = "F+" + this.Tag;
            ((RemoteFiles)this.ParentForm).PasteToolStripMenuItem.Visible = true;
            ((RemoteFiles)this.ParentForm).MoveHereToolStripMenuItem.Visible = false;
        }
        private TcpClient Client;
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string host =( (RemoteFiles)this.ParentForm).Tag.ToString ();
                Client = new TcpClient(host,KGlobal . PortString);
                if (Client.Connected)
                {
                    StreamWriter sw = new StreamWriter(Client.GetStream());
                    sw.Write("<-" + "F-" + this.Tag + "->");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void CopyToThisPCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            if (fb.ShowDialog() == DialogResult.OK)
            {
                KGlobal.ClientConnect.savefile = fb.SelectedPath + @"\" + this.Tag.ToString().Split('\\').Last();
                KGlobal.ClientConnect.SourceFile = this.Tag.ToString();
                try
                {
                    string host = ((RemoteFiles)this.ParentForm).Tag.ToString() ;
                    Client = new TcpClient(host,KGlobal . PortString);
                    if (Client.Connected)
                    {
                        StreamWriter sw = new StreamWriter(Client.GetStream());
                        sw.Write("<@" + this.Tag + "@R@>");
                        sw.Flush();
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
