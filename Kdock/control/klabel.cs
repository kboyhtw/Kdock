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
    public partial class Klabel : Label 
    {
        public Klabel()
        {
            InitializeComponent();
        }
        public Color cmouseH = Control.DefaultForeColor;
        public Color cmouseL = Control.DefaultForeColor;
        private string clipb = "";
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        public Color HoverColor
        {
            get
            {
                return cmouseH;
            }
            set
            {
                cmouseH = value;
            }
        }

        public Color LeaveColor
        {
            get
            {
                return cmouseL;
            }
            set
            {
                this.ForeColor = value;
                cmouseL = value;
            }
        }

        private void klabel_Click(object sender, EventArgs e)
        {
            clipb = Clipboard.GetText();
        }

        private void klabel_DoubleClick(object sender, EventArgs e)
        {
            if (clipb != "")
                Clipboard.SetText(clipb);
        }

        private void klabel_MouseLeave(object sender, EventArgs e)
        {
            if (cmouseL != Control.DefaultForeColor)
                this.ForeColor = cmouseL;
        }

        private void klabel_MouseMove(object sender, MouseEventArgs e)
        {
            if (cmouseH != Control.DefaultForeColor)
                this.ForeColor = cmouseH;
        }
    }
}
