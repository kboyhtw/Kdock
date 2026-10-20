using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class web : Form
    {
        public web()
        {
            InitializeComponent();
        }


        public bool whideurls;
        private void web_Load(object sender, EventArgs e)
        {
            this.Height = 10;
            Timeropen.Start();
            updatetheme();
            this.Location = KGlobal.setting.wpoint;
            loadsc();
            this.Focus();
            txtsearch.Focus();
        }

        private void pmove_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = Cursor.Position;
                Savechanges();
            }
        }

        public void Savechanges()
        {
            if (this.Location.X > Screen.PrimaryScreen.Bounds.Width - this.Width)
                this.Location = new Point(Screen.GetWorkingArea(new Point(0, 0)).Width - this.Width, this.Location.Y);
            else if (this.Location.X < 0)
            {
                this.Location = new Point(0, this.Location.Y);
                this.Size = new Size(this.Width - this.Location.X, this.Height);
            }

            if (this.Location.Y > Screen.PrimaryScreen.Bounds.Height - this.Height)
                this.Location = new Point(this.Location.X, Screen.GetWorkingArea(new Point(0, 0)).Height - this.Height);
            else if (this.Location.Y < 0)
            {
                this.Location = new Point(this.Location.X, 0);
                this.Size = new Size(this.Width, this.Height - this.Location.Y);
            }

            if (this.Width > Screen.GetWorkingArea(new Point(0, 0)).Width - 10)
                this.Width = Screen.GetWorkingArea(new Point(0, 0)).Width - 10;
            else if (this.Width <= 50)
                this.Width = 50;

            if (this.Height > Screen.GetWorkingArea(new Point(0, 0)).Height - 10)
                this.Height = Screen.GetWorkingArea(new Point(0, 0)).Height - 10;
            else if (this.Height <= 50)
                this.Height = 50;

            KGlobal.setting.wpoint = this.Location;

            KGlobal.setting.Save(KGlobal.path);
        }

        private void lblSearch_MouseMove(object sender, MouseEventArgs e)
        {
            lblSearch.ForeColor = KGlobal.mouseH;
        }

        private void lblSearch_mouseL(object sender, EventArgs e)
        {
            lblSearch.ForeColor = KGlobal.mouseL;
        }

        private void lblSearch_Click(object sender, EventArgs e)
        {
            KGlobal.search(txtsearch.Text);
        }

        private void txtsearch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue  == 13)
                KGlobal.search(txtsearch.Text);
        }



        private void lblClose_Click(object sender, EventArgs e)
        {
            whideurls = false;
            Timerclose.Start();
        }

        private int j = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            Timerclose.Stop();

            if (KGlobal.setting.wshowurls == "Y")
            {
                if (this.Height < System.Convert.ToInt32(300))
                {
                    this.Height += 30;
                    flpUrls.Location = new Point(18, 50);
                }
                else
                {
                    this.Height = System.Convert.ToInt32(300);
                    if (flpUrls.Location.Y < 60)
                    {
                        j += 1;
                        flpUrls.Location = new Point(18, 50 + j);
                    }
                    else
                    {
                        Timeropen.Stop();
                        j = 0;
                    }
                }
            }
            else if (this.Height < System.Convert.ToInt32(55))
                this.Height += 10;
            else
            {
                this.Height = System.Convert.ToInt32(55);
                Timeropen.Stop();
            }
        }

        private void Timerclose_Tick(object sender, EventArgs e)
        {

        }

        private void AddUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flpUrls.Visible = false;
            paddurl.Visible = true;
        }

        private void lblSave_Click(object sender, EventArgs e)
        {
            if (KGlobal.setting.wurls.Split(KGlobal.midsep).Length < 19)
            {
                KGlobal.setting.wurls += txturlname.Text + KGlobal.endsep + txturl.Text + KGlobal.midsep;
                flpUrls.Visible = true;
                paddurl.Visible = false;
                ucShortcut sc = new ucShortcut();
                sc.sctype = "WA";
                sc.Tag = txturlname.Text;
                sc.Name = txturl.Text;
                flpUrls.Controls.Add(sc);
                Savechanges();
            }
            else
               MessageBox .Show ("Please remove some items then try");
        }

        public void RemoveSC(string name)
        {
           ( (ucShortcut)flpUrls.Controls[name]).TimerRemove.Start();
            string[] shrt = KGlobal.setting.wurls.Split(KGlobal.midsep);
            for (int i = 0; i <= shrt.Length - 2; i++)
            {
                if (name == shrt[i].Split(KGlobal.endsep)[1])
                    KGlobal.setting.wurls = KGlobal.setting.wurls.Replace(shrt[i].Split(KGlobal.endsep)[0] + KGlobal.endsep + shrt[i].Split(KGlobal.endsep)[1] + KGlobal.midsep, "");
            }
            Savechanges();
        }

        private void lblCancel_Click(object sender, EventArgs e)
        {
            flpUrls.Visible = true;
            paddurl.Visible = false;
        }

        private void loadsc()
        {
            if (KGlobal.setting.wurls != "")
            {
                string[] shrt = KGlobal.setting.wurls.Split(KGlobal.midsep);
                for (int i = 0; i <= shrt.Length - 2; i++)
                {
                    ucShortcut sc = new ucShortcut();
                    sc.sctype = "W";
                    sc.Tag = shrt[i].Split(KGlobal.endsep)[0];
                    sc.Name = shrt[i].Split(KGlobal.endsep)[1];
                    flpUrls.Controls.Add(sc);
                }
            }
        }

        private void lblshowurls_Click(object sender, EventArgs e)
        {
            if (KGlobal.setting.wshowurls == "")
            {
                KGlobal.setting.wshowurls = "Y";
                Timeropen.Start();
            }
            else
            {
                KGlobal.setting.wshowurls = "";
                whideurls = true;
                Timerclose.Start();
            }
            Savechanges();
        }

        public void updatetheme()
        {
            this.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            txtsearch.BackColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            lblSearch.ForeColor = KGlobal.mouseL;
            lblSave.HoverColor = KGlobal.mouseH;
            lblSave.LeaveColor = KGlobal.mouseL;
            lblCancel.HoverColor = KGlobal.mouseH;
            lblCancel.LeaveColor = KGlobal.mouseL;
            lblname.ForeColor = KGlobal.mouseL;
            lblUrl.ForeColor = KGlobal.mouseL;
            this.Opacity = KGlobal.kdockmain  .Opacity;
            txtsearch.ForeColor = KGlobal.kdockmain.PanelMain.BackColor;
            lblClose.ForeColor = KGlobal.kdockmain.Label4.ForeColor;
            lblShowurls.HoverColor = KGlobal.mouseH;
            lblShowurls.LeaveColor = KGlobal.mouseL;
            foreach (ucShortcut CTRL in flpUrls.Controls)
                CTRL.updatetheme();
        }

    }
}
