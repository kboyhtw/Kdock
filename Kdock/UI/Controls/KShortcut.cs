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
    public partial class KShortcut : UserControl
    {
        public KShortcut()
        {
            InitializeComponent();
        }

        private void DeleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void MoveToDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void KShortcut_MouseMove(object sender, MouseEventArgs e)
        {
            this.BackColor = KGlobal.kdockmain.PanelDrawer.BackColor;
            Cursor = Cursors.Hand;
        }

        private void KShortcut_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.Transparent;
            Cursor = Cursors.Default;
        }
    }
}
