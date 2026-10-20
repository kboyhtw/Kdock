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
    public partial class ucShortcut : UserControl
    {
        public ucShortcut()
        {
            InitializeComponent();
        }

        public Color font, back;
        public string sctype;
        private void ucShortcut_Load(object sender, EventArgs e)
        {
            lbl1.Text = this.Tag.ToString()[0].ToString().ToUpper();
            Label2.Text = this.Tag.ToString();
            updatetheme();
            if (sctype == "WA")
                this.Size = new Size(0, 80);
            else
                this.Size = new Size(80, 80);
        }
        private void ucShortcut_Click(object sender, EventArgs e)
        {
            if (sctype == "W" | sctype == "WA")
                KGlobal.search(this.Name);
            else if (sctype == "A")
            {
                Process process = new Process();
                process.StartInfo.FileName = this.Name;
                process.StartInfo.Verb = "runas";
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            else
                Process.Start(this.Name);
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KGlobal.oweb.RemoveSC(Name);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            TimerRemove.Stop();
            if (this.Width < 70)
                this.Size = new Size(this.Width + 2, 80);
            else
            {
                this.Size = new Size(80, 80);
                Timer1.Stop();
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            Timer1.Enabled = false;
            if (this.Width > 0)
                this.Size = new Size(this.Width - 2, 80);
            else
            {
                this.Size = new Size(0, 80);
                TimerRemove.Stop();
                this.Dispose();
            }
        }

        public void updatetheme()
        {
            this.ForeColor = KGlobal.kdockmain.PanelMain.BackColor;
            this.BackColor = KGlobal.kdockmain.PanelDrawer.BackColor;
        }

        private void cmsRemoveS_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!sctype.Contains("W"))
                e.Cancel = true;
        }
    }
}
