namespace Kdock
{
    partial class ucShortcut
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lbl1 = new System.Windows.Forms.Label();
            this.cmsRemoveS = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Label2 = new System.Windows.Forms.Label();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.TimerRemove = new System.Windows.Forms.Timer(this.components);
            this.cmsRemoveS.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl1
            // 
            this.lbl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl1.ContextMenuStrip = this.cmsRemoveS;
            this.lbl1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbl1.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl1.Location = new System.Drawing.Point(0, 0);
            this.lbl1.MinimumSize = new System.Drawing.Size(60, 0);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(80, 42);
            this.lbl1.TabIndex = 0;
            this.lbl1.Text = "0";
            this.lbl1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl1.Click += new System.EventHandler(this.ucShortcut_Click);
            // 
            // cmsRemoveS
            // 
            this.cmsRemoveS.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RemoveToolStripMenuItem});
            this.cmsRemoveS.Name = "cmsRemoveS";
            this.cmsRemoveS.Size = new System.Drawing.Size(118, 26);
            // 
            // RemoveToolStripMenuItem
            // 
            this.RemoveToolStripMenuItem.Name = "RemoveToolStripMenuItem";
            this.RemoveToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.RemoveToolStripMenuItem.Text = "Remove";
            this.RemoveToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItem_Click);
            // 
            // Label2
            // 
            this.Label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label2.ContextMenuStrip = this.cmsRemoveS;
            this.Label2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(0, 42);
            this.Label2.MaximumSize = new System.Drawing.Size(80, 35);
            this.Label2.MinimumSize = new System.Drawing.Size(60, 25);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(80, 35);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "Company master";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label2.Click += new System.EventHandler(this.ucShortcut_Click);
            // 
            // Timer1
            // 
            this.Timer1.Enabled = true;
            this.Timer1.Interval = 1;
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // TimerRemove
            // 
            this.TimerRemove.Interval = 1;
            this.TimerRemove.Tick += new System.EventHandler(this.Timer2_Tick);
            // 
            // ucShortcut
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.cmsRemoveS;
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.lbl1);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(8, 8, 0, 0);
            this.Name = "ucShortcut";
            this.Size = new System.Drawing.Size(80, 80);
            this.Load += new System.EventHandler(this.ucShortcut_Load);
            this.Click += new System.EventHandler(this.ucShortcut_Click);
            this.cmsRemoveS.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.ContextMenuStrip cmsRemoveS;

        private System.Windows.Forms.ToolStripMenuItem RemoveToolStripMenuItem;

        private System.Windows.Forms.Timer Timer1;

        public System.Windows.Forms.Timer TimerRemove;

        #endregion

        public System.Windows.Forms.Label lbl1;
        public System.Windows.Forms.Label Label2;
    }
}
