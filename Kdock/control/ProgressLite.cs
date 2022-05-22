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
    public partial class ProgressLite : Panel 
    {
        public ProgressLite()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        private void ProgressLite_Resize(object sender, EventArgs e)
        {
          Panel1.Size = new Size(Convert.ToInt32((this.Width / iMax) * iVal), this.Height);
        }
    }
}
