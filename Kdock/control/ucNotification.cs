using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class ucNotification : UserControl
    {
        public ucNotification()
        {
            InitializeComponent();
        }

        private void lblMsg_Click(object sender, EventArgs e)
        {
            bool b=false ;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == "C" + lblip.Text | frm.Name == "C" + lblPC.Text)
                    b = true;
            }
            if (b == false)
            {
                frmLanChat frmRm = new frmLanChat();
                frmRm.Tag = lblip.Text;
                frmRm.Name = "C" + lblip.Text;
                frmRm.Text = lblPC.Text;
                frmRm.Show();
            }
        }

        public void loadNoti(string ip, string pc, string msg)
        {
            UpdateTheme();
            lblip.Text = ip;
            lblPC.Text = pc;
            lblMsg.Text = msg +"/n";
            lblTotal.Text = "1";
        }

        private void UpdateTheme()
        {
            lblPC.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            lblMsg.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            lblip.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            this.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            PictureBox1.BackColor = KGlobal.kdockmain.PanelDrawer.BackColor;

            lblPC.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            lblMsg.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            lblip.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            this.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
        }

        public void AddMsg(string msg)
        {
            lblTotal.Text = (int.Parse(lblTotal.Text) + 1).ToString ();
            lblMsg.Text += "/n" + msg + "/n";
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            if (lblMsg.MaximumSize.Height == 25)
            {
                lblMsg.MaximumSize = new Size(lblMsg.MaximumSize.Width, 1000);
                Label1.Text = "Less";
            }
            else
            {
                lblMsg.MaximumSize = new Size(lblMsg.MaximumSize.Width, 25);
                Label1.Text = "More";
            }
        }
    }
}
