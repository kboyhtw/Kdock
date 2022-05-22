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
    public partial class frmAccess : Form
    {
        public frmAccess()
        {
            InitializeComponent();
        }

        public string ShowMessege(string sip, string msg)
        {
            frmAccess frm = new frmAccess();
            frm.lblMessege.Text = msg;
            frm.Tag = sip;
            frm.ShowDialog();
            return frm.AccessResult;
        }

        private bool accessG = false;
        public bool AccessGrant
        {
            get
            {
                return accessG;
            }
            set
            {
                if (value)
                {
                    string sDel, sCM, sGet, sGive;
                    if (tglDelete.Toggled)
                        sDel = "1";
                    else
                        sDel = "0";
                    if (tglCM.Toggled)
                        sCM = "1";
                    else
                        sCM = "0";
                    if (tglGet.Toggled)
                        sGet = "1";
                    else
                        sGet = "0";
                    if (tglGive.Toggled)
                        sGive = "1";
                    else
                        sGive = "0";
                    AccessResult = this.Tag.ToString() + KGlobal. smallsep + sDel + KGlobal.smallsep + sCM + KGlobal.smallsep + sGet + KGlobal.smallsep + sGive;
                }
                else
                    AccessResult = null;
                accessG = value;
            }
        }

        public string AccessResult = null;

        private void btnYes_Click(object sender, EventArgs e)
        {
            AccessGrant = true;
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            AccessGrant = false;
            this.Close();
        }
    }
}
