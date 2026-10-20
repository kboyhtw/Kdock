using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class frmTheme : Form
    {
        public frmTheme()
        {
            InitializeComponent();
        }

        private Color colr;

        private void btnmain_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnmain.BackColor = ColorDialog1.Color;
                pmain.BackColor = ColorDialog1.Color;
                lbldt.ForeColor = ColorDialog1.Color;
                pabout.BackColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr0 = ColorDialog1.Color;
            }
        }

        private void BtnFont_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                BtnFont.BackColor = ColorDialog1.Color;
                Label13.ForeColor = ColorDialog1.Color;
                Label14.ForeColor = ColorDialog1.Color;
                Label16.ForeColor = ColorDialog1.Color;
                Label17.ForeColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr1 = ColorDialog1.Color;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnSysinfo.BackColor = ColorDialog1.Color;
                lblsys.ForeColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr2 = ColorDialog1.Color;
            }
        }


        private void btnprocess_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnprocess.BackColor = ColorDialog1.Color;
                lblpro.ForeColor = ColorDialog1.Color;
                Label15.ForeColor = ColorDialog1.Color;
                Label18.ForeColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr3 = ColorDialog1.Color;
            }
        }

        private void btnApps_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnApps.BackColor = ColorDialog1.Color;
                papps.BackColor = ColorDialog1.Color;
                Label7.BackColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr4 = ColorDialog1.Color;
            }
        }

        private void btnAppsFont_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnAppsFont.BackColor = ColorDialog1.Color;
                Label9.ForeColor = ColorDialog1.Color;
                Label10.ForeColor = ColorDialog1.Color;
                Label11.ForeColor = ColorDialog1.Color;
                Label12.ForeColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr5 = ColorDialog1.Color;
            }
        }

        private void btnDrawerBack_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnDrawerBack.BackColor = ColorDialog1.Color;
                pdraw.BackColor = ColorDialog1.Color;
                lbldt.BackColor = ColorDialog1.Color;
                PmouseH.BackColor = ColorDialog1.Color;
                Label19.ForeColor = ColorDialog1.Color;
                Label20.ForeColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr6 = ColorDialog1.Color;
            }
        }

        private void BtnmouseH_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                btnmouseHover.BackColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr7 = ColorDialog1.Color;
            }
        }

        private void btnmouseL_Click(object sender, EventArgs e)
        {
            if (ColorDialog1.ShowDialog() == DialogResult.OK)
            {
                BtnmouseLeave.BackColor = ColorDialog1.Color;
                Label1.ForeColor = ColorDialog1.Color;
                Label2.ForeColor = ColorDialog1.Color;
                Label4.ForeColor = ColorDialog1.Color;
                Label5.ForeColor = ColorDialog1.Color;
                Label6.ForeColor = ColorDialog1.Color;
                Label7.ForeColor = ColorDialog1.Color;
                Label8.ForeColor = ColorDialog1.Color;
                // BtnSave.Visible = True
                KGlobal.GTheme.clr8 = ColorDialog1.Color;
            }
        }
        public string newcolor(string a, string r, string g, string b)
        {
            string color1;
            color1 = a + "," + r + "," + g + "," + b;
            return color1;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string theme;
            theme = KGlobal.GTheme.ToString();
            if (!File.Exists(KGlobal.path + @"\theme.k"))
            {
                File.AppendAllText(KGlobal.path + @"\theme.k", theme);
                File.SetAttributes(KGlobal.path + @"\theme.k", FileAttributes.Hidden);
                File.Copy(KGlobal.path + @"\theme.k", KGlobal.path + @"\Backtheme.k", true);
            }
            else
            {
                File.Delete(KGlobal.path + @"\theme.k");
                File.AppendAllText(KGlobal.path + @"\theme.k", theme);
                File.SetAttributes(KGlobal.path + @"\theme.k", FileAttributes.Hidden);
                File.Copy(KGlobal.path + @"\theme.k", KGlobal.path + @"\Backtheme.k", true);
            }
            KGlobal.kdockmain.getTheme();
            mytheme();

        }

        private void Theme_Load(object sender, EventArgs e)
        {
            this.Opacity = KGlobal.kdockmain.Opacity;
            getTheme();
            mytheme();
        }
        public void getTheme()
        {
            btnmain.BackColor = KGlobal.GTheme.clr0;
            pmain.BackColor = KGlobal.GTheme.clr0;
            lbldt.ForeColor = KGlobal.GTheme.clr0;
            pabout.BackColor = KGlobal.GTheme.clr0;

            BtnFont.BackColor = KGlobal.GTheme.clr1;
            Label13.ForeColor = KGlobal.GTheme.clr1;
            Label14.ForeColor = KGlobal.GTheme.clr1;
            Label16.ForeColor = KGlobal.GTheme.clr1;
            Label17.ForeColor = KGlobal.GTheme.clr1;

            btnSysinfo.BackColor = KGlobal.GTheme.clr2;
            lblsys.ForeColor = KGlobal.GTheme.clr2;

            btnprocess.BackColor = KGlobal.GTheme.clr3;
            lblpro.ForeColor = KGlobal.GTheme.clr3;
            Label15.ForeColor = KGlobal.GTheme.clr3;
            Label18.ForeColor = KGlobal.GTheme.clr3;

            btnApps.BackColor = KGlobal.GTheme.clr4;
            papps.BackColor = KGlobal.GTheme.clr4;
            Label7.BackColor = KGlobal.GTheme.clr4;

            btnAppsFont.BackColor = KGlobal.GTheme.clr5;
            Label9.ForeColor = KGlobal.GTheme.clr5;
            Label10.ForeColor = KGlobal.GTheme.clr5;
            Label11.ForeColor = KGlobal.GTheme.clr5;
            Label12.ForeColor = KGlobal.GTheme.clr5;

            btnDrawerBack.BackColor = KGlobal.GTheme.clr6;
            pdraw.BackColor = KGlobal.GTheme.clr6;
            lbldt.BackColor = KGlobal.GTheme.clr6;
            Label19.ForeColor = KGlobal.GTheme.clr6;
            Label20.ForeColor = KGlobal.GTheme.clr6;
            PmouseH.BackColor = KGlobal.GTheme.clr6;

            btnmouseHover.BackColor = KGlobal.GTheme.clr7;

            BtnmouseLeave.BackColor = KGlobal.GTheme.clr8;
            Label1.ForeColor = KGlobal.GTheme.clr8;
            Label2.ForeColor = KGlobal.GTheme.clr8;
            Label4.ForeColor = KGlobal.GTheme.clr8;
            Label5.ForeColor = KGlobal.GTheme.clr8;
            Label6.ForeColor = KGlobal.GTheme.clr8;
            Label7.ForeColor = KGlobal.GTheme.clr8;
            Label8.ForeColor = KGlobal.GTheme.clr8;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = Cursor.Position;
        }


        private void btnsetDefault_Click(object sender, EventArgs e)
        {
            if (File.Exists(KGlobal.path + @"\theme.k"))
            {
                File.Delete(KGlobal.path + @"\theme.k");
                File.Delete(KGlobal.path + @"\Backtheme.k");
                getTheme();
                KGlobal.kdockmain.getTheme();
                mytheme();

            }
            else
            {
                getTheme();
                KGlobal.kdockmain.getTheme();
                mytheme();

            }
        }
        public void mytheme()
        {
            this.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            lblexit.ForeColor = KGlobal.mainfont;
            Label21.ForeColor = KGlobal.mainfont;
            Label22.ForeColor = KGlobal.mainfont;
            Label23.ForeColor = KGlobal.mainfont;
            Label24.ForeColor = KGlobal.mainfont;
            Label25.ForeColor = KGlobal.mainfont;
            Label26.ForeColor = KGlobal.mainfont;
            Label27.ForeColor = KGlobal.mainfont;
            Label28.ForeColor = KGlobal.mainfont;
            Label29.ForeColor = KGlobal.mainfont;
            Label3.ForeColor = KGlobal.mainfont;
            BtnCancel.ForeColor = KGlobal.mainfont;
            btnDefault.ForeColor = KGlobal.mainfont;
            BtnSave.ForeColor = KGlobal.mainfont;
            btnAuto.ForeColor = KGlobal.mainfont;
        }

        private void lblexit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAuto_Click(object sender, EventArgs e)
        {
            KGlobal.GTheme.Auto = true;
            string theme;
            theme = KGlobal.GTheme.ToString();
            if (!File.Exists(KGlobal.path + @"\theme.k"))
            {
                File.AppendAllText(KGlobal.path + @"\theme.k", theme);
                File.SetAttributes(KGlobal.path + @"\theme.k", FileAttributes.Hidden);
                File.Copy(KGlobal.path + @"\theme.k", KGlobal.path + @"\Backtheme.k", true);
            }
            else
            {
                File.Delete(KGlobal.path + @"\theme.k");
                File.AppendAllText(KGlobal.path + @"\theme.k", theme);
                File.SetAttributes(KGlobal.path + @"\theme.k", FileAttributes.Hidden);
                File.Copy(KGlobal.path + @"\theme.k", KGlobal.path + @"\Backtheme.k", true);
            }
            KGlobal.PixColor = new Color();
            KGlobal.kdockmain.getTheme();
            mytheme();

        }

    }
}
