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
    public partial class ActivePlace : Form
    {
        public ActivePlace()
        {
            InitializeComponent();
        }

        public void setPlace(string place)
        {
            switch (place)
            {
                case "T":
                    {
                        this.Location = new Point(Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width / (double)2 - 250), 0);
                        this.Size = new Size(500, 1);
                        break;
                    }

                case "B":
                    {
                        this.Location = new Point(Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width / (double)2 - 250), Screen.PrimaryScreen.Bounds.Height - 1);
                        this.Size = new Size(500, 1);
                        break;
                    }

                case "L":
                    {
                        this.Location = new Point(0, Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height / (double)2 - 250));
                        this.Size = new Size(1, 500);
                        break;
                    }

                case "R":
                    {
                        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 1, Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height / (double)2 - 250));
                        this.Size = new Size(1, 500);
                        break;
                    }
            }
        }


        private void ActivePlace_mouseH(object sender, EventArgs e)
        {
            KGlobal.kdockmain.TopMost = false;
        }

        private void ActivePlace_MouseMove(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            KGlobal.kdockmain.TopMost = true;
            KGlobal.kdockmain.TopMost = false;
            if (Application.OpenForms.OfType<Note>().Any())
            {
                KGlobal.oNote.TopMost = true;
                KGlobal.oNote.TopMost = false;
            }
            if (Application.OpenForms.OfType<web>().Any())
            {
                KGlobal.oweb.TopMost = true;
                KGlobal.oweb.TopMost = false;
            }
            if (Application.OpenForms.OfType<LAN>().Any())
            {
                KGlobal.oLAN.TopMost = true;
                KGlobal.oLAN.TopMost = false;
            }
        }

        private void ActivePlace_Resize(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }
    }
}
