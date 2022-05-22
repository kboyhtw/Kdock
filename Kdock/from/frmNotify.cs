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
    public partial class frmNotify : Form
    {
        public frmNotify()
        {
            InitializeComponent();
        }
        private int i;
        private Point pbpoint = new Point(800, 300);

        private void TimerRight1_Tick(object sender, EventArgs e)
        {
            if (PictureBox1.Width <= 300)
            {
                pbpoint.X -= 50;
                PictureBox1.Width += 50;
                PictureBox1.Location = pbpoint;
                FlowLayoutPanel1.Location = new Point(30, 0);
            }
            else if (FlowLayoutPanel1.Location.X > 0)
            {
                Panel2.Visible = true;
                FlowLayoutPanel1.Visible = true;
                FlowLayoutPanel1.Focus();
                i += 4;
                FlowLayoutPanel1.Location = new Point(30 - i, 0);
            }
            else
            {
                i = 0;
                TimerRight1.Enabled = false;
            }
        }

        private void frmNotify_Load(object sender, EventArgs e)
        {
            this.Opacity = KGlobal.kdockmain.Opacity;

            PictureBox1.Visible = true;
            UpdateTheme();
            if (KGlobal.kdockmain.Location.X >= 600)
            {
                this.Location = new Point(KGlobal.kdockmain.Location.X - this.Width, KGlobal.kdockmain.Location.Y);
                TimerRight2.Enabled = false;
                TimerRight1.Enabled = true;
                pbpoint = PictureBox1.Location;
            }
            else
            {
                this.Location = new Point(KGlobal.kdockmain.Location.X + KGlobal.kdockmain.Width, KGlobal.kdockmain.Location.Y);
                TimerLeft2.Enabled = false;
                TimerLeft1.Enabled = true;
                pbpoint = new Point(0, 0);
            }
            if (File.Exists(KGlobal . path + "InMsg.k"))
            {
                string[] str = File.ReadAllText(KGlobal . path + "InMsg.k").Split(KGlobal. mainsep);
                for (int i = 0; i <= str.Length - 2; i++)
                {
                    if (FlowLayoutPanel1.Controls.ContainsKey(str[i].Split(KGlobal . smallsep)[1]))
                        ((ucNotification)FlowLayoutPanel1.Controls.Find(str[i].Split(KGlobal . smallsep)[1], true)[0]).AddMsg(str[i].Split(KGlobal . smallsep)[2]);
                    else
                    {
                        ucNotification uc = new ucNotification();
                        uc.Name = str[i].Split(KGlobal . smallsep)[1];
                        uc.loadNoti(str[i].Split(KGlobal . smallsep)[0], str[i].Split(KGlobal . smallsep)[1], str[i].Split(KGlobal . smallsep)[2]);
                        uc.Click += Mouse_Click;
                        uc.lblip.Click += Mouse_Click;
                        uc.lblMsg.Click += Mouse_Click;
                        uc.lblPC.Click += Mouse_Click;
                        BeginInvoke ((MethodInvoker) delegate { FlowLayoutPanel1.Controls.Add(uc); });
                    }
                }
            }
        }

        private void TimerRight2_Tick(object sender, EventArgs e)
        {
            i = 0;

            if (PictureBox1.Width >= 10)
            {
                FlowLayoutPanel1.Visible = false;
                Panel2.Visible = false;


                pbpoint.X += 50;
                PictureBox1.Location = pbpoint;
                PictureBox1.Width -= 50;
            }
            else
            {
                PictureBox1.Location = new Point(290, 0);
                PictureBox1.Size = new Size(1, 135);
                this.Close();
                this.Dispose();
                GC.Collect();
                TimerRight2.Enabled = false;
                KGlobal.kdockmain.TimerDrawer1.Enabled = false;
                KGlobal.kdockmain.TimerDrawer2.Enabled = true;
                KGlobal.kdockmain.lblDay.Visible = false;
            }
        }

        private void Mouse_Click(object sender, EventArgs e)
        {
            NClose();
        }
        public int sy = 0;
        private void Label1_Click(object sender, EventArgs e)
        {
            if (sy < 1)
                sy = 0;
            else
            {
                sy -= 150;

                FlowLayoutPanel1.AutoScrollPosition = new Point(0, sy);
            }
        }

        private void Label2_Click(object sender, EventArgs e)
        {
            if (sy > FlowLayoutPanel1.VerticalScroll.Maximum - 150)
                sy = FlowLayoutPanel1.VerticalScroll.Maximum - 150;
            else
            {
                sy += 150;

                FlowLayoutPanel1.AutoScrollPosition = new Point(0, sy);
            }
        }

        private void UpdateTheme()
        {
            PictureBox1.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            FlowLayoutPanel1.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            Panel2.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            Label1.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            Label2.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            Label1.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            Label2.ForeColor = KGlobal.kdockmain.PanelDrawer.BackColor;
        }

        public void NClose()
        {
            if (KGlobal.kdockmain.Location.X >= 600)
            {
                TimerRight1.Enabled = false;
                TimerRight2.Enabled = true;
            }
            else
            {
                TimerLeft1.Enabled = false;
                TimerLeft2.Enabled = true;
            }
        }

        private void TimerLeft1_Tick(object sender, EventArgs e)
        {
            if (PictureBox1.Width <= 300)
            {
                PictureBox1.Width += 30;
                PictureBox1.Location = pbpoint;
                FlowLayoutPanel1.Location = new Point(-30, 0);
            }
            else if (FlowLayoutPanel1.Location.X < 0)
            {
                Panel2.Visible = true;
                FlowLayoutPanel1.Visible = true;
                FlowLayoutPanel1.Focus();
                i += 2;
                Panel2.Location = new Point(245 + i, 0);
                FlowLayoutPanel1.Location = new Point(-30 + i, 0);
            }
            else
            {
                i = 0;
                TimerLeft1.Enabled = false;
            }
        }

        private void TimerLeft2_Tick(object sender, EventArgs e)
        {
            i = 0;

            if (PictureBox1.Width >= 10)
            {
                FlowLayoutPanel1.Visible = false;
                Panel2.Visible = false;
                PictureBox1.Width -= 30;
                PictureBox1.Location = pbpoint;
            }
            else
            {
                TimerLeft2.Enabled = false;
                PictureBox1.Location = new Point(0, 0);
                PictureBox1.Size = new Size(1, 135);
                this.Close();
                this.Dispose();
                GC.Collect();
                KGlobal.kdockmain.TimerDrawer1.Enabled = false;
                KGlobal.kdockmain.TimerDrawer2.Enabled = true;
                KGlobal.kdockmain.lblDay.Visible = false;
            }
        }
    }
}
