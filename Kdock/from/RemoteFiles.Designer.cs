namespace Kdock
{
    partial class RemoteFiles
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.PasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RefreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PasteFromThisPCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Button2 = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.Panel2 = new System.Windows.Forms.Panel();
            this.presize = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.ContextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.Panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // FlowLayoutPanel1
            // 
            this.FlowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowLayoutPanel1.AutoScroll = true;
            this.FlowLayoutPanel1.ContextMenuStrip = this.ContextMenuStrip1;
            this.FlowLayoutPanel1.Location = new System.Drawing.Point(4, 34);
            this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            this.FlowLayoutPanel1.Size = new System.Drawing.Size(497, 292);
            this.FlowLayoutPanel1.TabIndex = 2;
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PasteToolStripMenuItem,
            this.MoveHereToolStripMenuItem,
            this.RefreshToolStripMenuItem,
            this.PasteFromThisPCToolStripMenuItem});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(177, 92);
            // 
            // PasteToolStripMenuItem
            // 
            this.PasteToolStripMenuItem.Name = "PasteToolStripMenuItem";
            this.PasteToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.PasteToolStripMenuItem.Text = "Paste";
            this.PasteToolStripMenuItem.Visible = false;
            this.PasteToolStripMenuItem.Click += new System.EventHandler(this.PasteToolStripMenuItem_Click);
            // 
            // MoveHereToolStripMenuItem
            // 
            this.MoveHereToolStripMenuItem.Name = "MoveHereToolStripMenuItem";
            this.MoveHereToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.MoveHereToolStripMenuItem.Text = "Move Here";
            this.MoveHereToolStripMenuItem.Visible = false;
            this.MoveHereToolStripMenuItem.Click += new System.EventHandler(this.MoveHereToolStripMenuItem_Click);
            // 
            // RefreshToolStripMenuItem
            // 
            this.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem";
            this.RefreshToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.RefreshToolStripMenuItem.Text = "Refresh";
            this.RefreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItem_Click);
            // 
            // PasteFromThisPCToolStripMenuItem
            // 
            this.PasteFromThisPCToolStripMenuItem.Name = "PasteFromThisPCToolStripMenuItem";
            this.PasteFromThisPCToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.PasteFromThisPCToolStripMenuItem.Text = "Paste From This PC";
            this.PasteFromThisPCToolStripMenuItem.Click += new System.EventHandler(this.PasteFromThisPCToolStripMenuItem_Click);
            // 
            // Button2
            // 
            this.Button2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button2.FlatAppearance.BorderSize = 0;
            this.Button2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button2.Location = new System.Drawing.Point(4, 3);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(40, 31);
            this.Button2.TabIndex = 3;
            this.Button2.Text = "Up";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(46, 10);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(11, 17);
            this.Label1.TabIndex = 4;
            this.Label1.Text = ":";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(16, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(33, 17);
            this.lblTitle.TabIndex = 6;
            this.lblTitle.Text = "Title";
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Blue;
            this.PictureBox1.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.PictureBox1.Location = new System.Drawing.Point(0, 0);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(10, 10);
            this.PictureBox1.TabIndex = 63;
            this.PictureBox1.TabStop = false;
            this.PictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseMove);
            // 
            // Panel2
            // 
            this.Panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel2.Controls.Add(this.Button2);
            this.Panel2.Controls.Add(this.FlowLayoutPanel1);
            this.Panel2.Controls.Add(this.Label1);
            this.Panel2.Location = new System.Drawing.Point(3, 30);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(504, 329);
            this.Panel2.TabIndex = 64;
            // 
            // presize
            // 
            this.presize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.presize.BackColor = System.Drawing.Color.Black;
            this.presize.Cursor = System.Windows.Forms.Cursors.Cross;
            this.presize.Location = new System.Drawing.Point(493, 345);
            this.presize.Name = "presize";
            this.presize.Size = new System.Drawing.Size(17, 17);
            this.presize.TabIndex = 65;
            this.presize.MouseMove += new System.Windows.Forms.MouseEventHandler(this.presize_MouseMove);
            // 
            // lblClose
            // 
            this.lblClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClose.AutoSize = true;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClose.Location = new System.Drawing.Point(494, 0);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(16, 17);
            this.lblClose.TabIndex = 66;
            this.lblClose.Text = "X";
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // RemoteFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 362);
            this.Controls.Add(this.lblClose);
            this.Controls.Add(this.Panel2);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.presize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RemoteFiles";
            this.Text = "RemoteFiles";
            this.Load += new System.EventHandler(this.RemoteFiles_Load);
            this.ContextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.Panel2.ResumeLayout(false);
            this.Panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;

        private System.Windows.Forms.Button Button2;

        private System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;

        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.PictureBox PictureBox1;

        private System.Windows.Forms.Panel Panel2;

        private System.Windows.Forms.Panel presize;

        private System.Windows.Forms.Label lblClose;

        #endregion

        public System.Windows.Forms.ToolStripMenuItem PasteToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem MoveHereToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem RefreshToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem PasteFromThisPCToolStripMenuItem;
        public System.Windows.Forms.Label Label1;
    }
}