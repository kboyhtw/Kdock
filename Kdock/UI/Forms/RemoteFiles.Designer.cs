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
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnIcons = new System.Windows.Forms.Button();
            this.lblTransfers = new System.Windows.Forms.Label();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlPath = new System.Windows.Forms.Panel();
            this.Panel2 = new System.Windows.Forms.Panel();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblStatus = new System.Windows.Forms.Label();
            this.presize = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.ContextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlPath.SuspendLayout();
            this.Panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // FlowLayoutPanel1
            // 
            this.FlowLayoutPanel1.AutoScroll = true;
            this.FlowLayoutPanel1.ContextMenuStrip = this.ContextMenuStrip1;
            this.FlowLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            this.FlowLayoutPanel1.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
            this.FlowLayoutPanel1.Size = new System.Drawing.Size(720, 368);
            this.FlowLayoutPanel1.TabIndex = 2;
            this.FlowLayoutPanel1.Visible = false;
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
            this.Button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button2.Location = new System.Drawing.Point(4, 3);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(40, 31);
            this.Button2.TabIndex = 3;
            this.Button2.ForeColor = System.Drawing.Color.White;
            this.Button2.Text = "↑";
            this.Button2.UseVisualStyleBackColor = false;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Label1
            // 
            this.Label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.Label1.Location = new System.Drawing.Point(48, 8);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(660, 20);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "This PC";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(39, 20);
            this.lblTitle.TabIndex = 6;
            this.lblTitle.Text = "Title";
            // 
            // btnDetails
            // 
            this.btnDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(103)))), ((int)(((byte)(192)))));
            this.btnDetails.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDetails.FlatAppearance.BorderSize = 0;
            this.btnDetails.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDetails.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDetails.ForeColor = System.Drawing.Color.White;
            this.btnDetails.Location = new System.Drawing.Point(426, 6);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(78, 28);
            this.btnDetails.TabIndex = 7;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = false;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnIcons
            // 
            this.btnIcons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIcons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnIcons.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIcons.FlatAppearance.BorderSize = 0;
            this.btnIcons.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIcons.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnIcons.ForeColor = System.Drawing.Color.White;
            this.btnIcons.Location = new System.Drawing.Point(512, 6);
            this.btnIcons.Name = "btnIcons";
            this.btnIcons.Size = new System.Drawing.Size(64, 28);
            this.btnIcons.TabIndex = 8;
            this.btnIcons.Text = "Icons";
            this.btnIcons.UseVisualStyleBackColor = false;
            this.btnIcons.Click += new System.EventHandler(this.btnIcons_Click);
            // 
            // lblTransfers
            // 
            this.lblTransfers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTransfers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lblTransfers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblTransfers.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTransfers.ForeColor = System.Drawing.Color.White;
            this.lblTransfers.Location = new System.Drawing.Point(584, 6);
            this.lblTransfers.Name = "lblTransfers";
            this.lblTransfers.Size = new System.Drawing.Size(88, 28);
            this.lblTransfers.TabIndex = 9;
            this.lblTransfers.Text = "Transfers";
            this.lblTransfers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTransfers.Click += new System.EventHandler(this.lblTransfers_Click);
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Blue;
            this.PictureBox1.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.PictureBox1.Location = new System.Drawing.Point(0, 0);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(10, 10);
            this.PictureBox1.Visible = false;
            this.PictureBox1.TabIndex = 63;
            this.PictureBox1.TabStop = false;
            this.PictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseMove);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.btnDetails);
            this.pnlHeader.Controls.Add(this.btnIcons);
            this.pnlHeader.Controls.Add(this.lblTransfers);
            this.pnlHeader.Controls.Add(this.lblClose);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(720, 40);
            this.pnlHeader.TabIndex = 70;
            // 
            // pnlPath
            // 
            this.pnlPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.pnlPath.Controls.Add(this.Button2);
            this.pnlPath.Controls.Add(this.Label1);
            this.pnlPath.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPath.Location = new System.Drawing.Point(0, 40);
            this.pnlPath.Name = "pnlPath";
            this.pnlPath.Size = new System.Drawing.Size(720, 36);
            this.pnlPath.TabIndex = 71;
            // 
            // lvFiles
            // 
            this.lvFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.lvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colDate,
            this.colType,
            this.colSize});
            this.lvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFiles.ForeColor = System.Drawing.Color.White;
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
            this.lvFiles.HideSelection = false;
            this.lvFiles.Location = new System.Drawing.Point(0, 0);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(720, 344);
            this.lvFiles.TabIndex = 3;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 280;
            // 
            // colDate
            // 
            this.colDate.Text = "Date modified";
            this.colDate.Width = 160;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 120;
            // 
            // colSize
            // 
            this.colSize.Text = "Size";
            this.colSize.Width = 90;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.lblStatus.Location = new System.Drawing.Point(0, 344);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblStatus.Size = new System.Drawing.Size(720, 24);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "0 items";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Panel2
            // 
            this.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.Panel2.Controls.Add(this.lvFiles);
            this.Panel2.Controls.Add(this.FlowLayoutPanel1);
            this.Panel2.Controls.Add(this.lblStatus);
            this.Panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel2.Location = new System.Drawing.Point(0, 76);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(720, 368);
            this.Panel2.TabIndex = 64;
            // 
            // presize
            // 
            this.presize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.presize.BackColor = System.Drawing.Color.Black;
            this.presize.Cursor = System.Windows.Forms.Cursors.Cross;
            this.presize.Location = new System.Drawing.Point(703, 463);
            this.presize.Name = "presize";
            this.presize.Size = new System.Drawing.Size(17, 17);
            this.presize.TabIndex = 65;
            this.presize.MouseMove += new System.Windows.Forms.MouseEventHandler(this.presize_MouseMove);
            // 
            // lblClose
            // 
            this.lblClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblClose.ForeColor = System.Drawing.Color.White;
            this.lblClose.Location = new System.Drawing.Point(680, 6);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(32, 28);
            this.lblClose.TabIndex = 66;
            this.lblClose.Text = "✕";
            this.lblClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // RemoteFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ClientSize = new System.Drawing.Size(720, 480);
            this.Controls.Add(this.Panel2);
            this.Controls.Add(this.pnlPath);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.presize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RemoteFiles";
            this.Text = "RemoteFiles";
            this.Load += new System.EventHandler(this.RemoteFiles_Load);
            this.ContextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlPath.ResumeLayout(false);
            this.Panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;

        private System.Windows.Forms.Button Button2;

        private System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;

        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.Button btnDetails;

        private System.Windows.Forms.Button btnIcons;

        private System.Windows.Forms.Label lblTransfers;

        private System.Windows.Forms.PictureBox PictureBox1;

        private System.Windows.Forms.Panel pnlHeader;

        private System.Windows.Forms.Panel pnlPath;

        private System.Windows.Forms.Panel Panel2;

        private System.Windows.Forms.ListView lvFiles;

        private System.Windows.Forms.ColumnHeader colName;

        private System.Windows.Forms.ColumnHeader colDate;

        private System.Windows.Forms.ColumnHeader colType;

        private System.Windows.Forms.ColumnHeader colSize;

        private System.Windows.Forms.Label lblStatus;

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