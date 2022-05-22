namespace Kdock
{
    partial class KShortcut
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
            this.Label1 = new System.Windows.Forms.Label();
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveToDesktopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CopyToDesktopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EventLog1 = new System.Diagnostics.EventLog();
            this.Label2 = new System.Windows.Forms.Label();
            this.ContextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EventLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.BackColor = System.Drawing.Color.Transparent;
            this.Label1.ContextMenuStrip = this.ContextMenuStrip1;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.Location = new System.Drawing.Point(0, 51);
            this.Label1.MaximumSize = new System.Drawing.Size(80, 30);
            this.Label1.MinimumSize = new System.Drawing.Size(80, 30);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(80, 30);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Label1";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.Label1.MouseLeave += new System.EventHandler(this.KShortcut_MouseLeave);
            this.Label1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.KShortcut_MouseMove);
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenToolStripMenuItem,
            this.DeleteItemToolStripMenuItem,
            this.MoveToDesktopToolStripMenuItem,
            this.CopyToDesktopToolStripMenuItem});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.ShowImageMargin = false;
            this.ContextMenuStrip1.Size = new System.Drawing.Size(143, 92);
            // 
            // OpenToolStripMenuItem
            // 
            this.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
            this.OpenToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.OpenToolStripMenuItem.Text = "Open";
            // 
            // DeleteItemToolStripMenuItem
            // 
            this.DeleteItemToolStripMenuItem.Name = "DeleteItemToolStripMenuItem";
            this.DeleteItemToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.DeleteItemToolStripMenuItem.Text = "Delete This Item";
            this.DeleteItemToolStripMenuItem.Click += new System.EventHandler(this.DeleteItemToolStripMenuItem_Click);
            // 
            // MoveToDesktopToolStripMenuItem
            // 
            this.MoveToDesktopToolStripMenuItem.Name = "MoveToDesktopToolStripMenuItem";
            this.MoveToDesktopToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.MoveToDesktopToolStripMenuItem.Text = "Move to Desktop ";
            this.MoveToDesktopToolStripMenuItem.Click += new System.EventHandler(this.MoveToDesktopToolStripMenuItem_Click);
            // 
            // CopyToDesktopToolStripMenuItem
            // 
            this.CopyToDesktopToolStripMenuItem.Name = "CopyToDesktopToolStripMenuItem";
            this.CopyToDesktopToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.CopyToDesktopToolStripMenuItem.Text = "Copy to Desktop";
            // 
            // EventLog1
            // 
            this.EventLog1.SynchronizingObject = this;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.BackColor = System.Drawing.Color.Transparent;
            this.Label2.ContextMenuStrip = this.ContextMenuStrip1;
            this.Label2.Location = new System.Drawing.Point(0, 0);
            this.Label2.MaximumSize = new System.Drawing.Size(80, 50);
            this.Label2.MinimumSize = new System.Drawing.Size(80, 50);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(80, 50);
            this.Label2.TabIndex = 2;
            this.Label2.MouseLeave += new System.EventHandler(this.KShortcut_MouseLeave);
            this.Label2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.KShortcut_MouseMove);
            // 
            // KShortcut
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Margin = new System.Windows.Forms.Padding(8, 8, 0, 0);
            this.Name = "KShortcut";
            this.Size = new System.Drawing.Size(80, 80);
            this.MouseLeave += new System.EventHandler(this.KShortcut_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.KShortcut_MouseMove);
            this.ContextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.EventLog1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Diagnostics.EventLog EventLog1;

        #endregion

        public System.Windows.Forms.Label Label1;
        public System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;
        public System.Windows.Forms.Label Label2;
        public System.Windows.Forms.ToolStripMenuItem DeleteItemToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem MoveToDesktopToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem CopyToDesktopToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem OpenToolStripMenuItem;
    }
}
