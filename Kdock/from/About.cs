using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void Label9_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = Cursor.Position;
        }
        private void About_Load(object sender, EventArgs e)
        {
            this.Opacity = KGlobal.kdockmain.Opacity;
            mytheme();
        }
        public void mytheme()
        {
            this.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            Label1.ForeColor = KGlobal.kdockmain.Label3.ForeColor;
            Label7.ForeColor = KGlobal.kdockmain.Label3.ForeColor;
            Label2.ForeColor = KGlobal.mainfont;
            Label3.ForeColor = KGlobal.mainfont;
            LinkLabel2.ForeColor = KGlobal.mainfont;
            Label5.ForeColor = KGlobal.mainfont;
            Label6.ForeColor = KGlobal.mainfont;
            LinkLabel1.ForeColor = KGlobal.mainfont;
            Label8.ForeColor = KGlobal.mainfont;
            Label9.ForeColor = KGlobal.mainfont;
            Label10.ForeColor = KGlobal.mainfont;
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(((Label)sender).Text);
        }
    }
}
