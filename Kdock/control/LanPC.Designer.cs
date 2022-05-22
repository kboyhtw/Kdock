namespace Kdock
{
    partial class LanPC : System.Windows .Forms .UserControl 
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
            this.lblPC = new System.Windows.Forms.Label();
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ChatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoteControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.VerifyIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblStatus = new System.Windows.Forms.Button();
            this.lblip = new System.Windows.Forms.Label();
            this.tmrLoad = new System.Windows.Forms.Timer(this.components);
            this.tmrRemove = new System.Windows.Forms.Timer(this.components);
            this.lblChat = new System.Windows.Forms.Label();
            this.lblFileExplorer = new System.Windows.Forms.Label();
            this.lblRD = new System.Windows.Forms.Label();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.tmrOpen = new System.Windows.Forms.Timer(this.components);
            this.ContextMenuStrip1.SuspendLayout();
            this.Panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPC
            // 
            this.lblPC.AutoSize = true;
            this.lblPC.ContextMenuStrip = this.ContextMenuStrip1;
            this.lblPC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPC.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPC.Location = new System.Drawing.Point(3, 1);
            this.lblPC.Name = "lblPC";
            this.lblPC.Size = new System.Drawing.Size(55, 21);
            this.lblPC.TabIndex = 0;
            this.lblPC.Text = "Label1";
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ChatToolStripMenuItem,
            this.RemoteControlToolStripMenuItem,
            this.FileExplorerToolStripMenuItem,
            this.VerifyIPToolStripMenuItem,
            this.RemoveToolStripMenuItem});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(181, 136);
            // 
            // ChatToolStripMenuItem
            // 
            this.ChatToolStripMenuItem.Name = "ChatToolStripMenuItem";
            this.ChatToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ChatToolStripMenuItem.Text = "Chat";
            // 
            // RemoteControlToolStripMenuItem
            // 
            this.RemoteControlToolStripMenuItem.Name = "RemoteControlToolStripMenuItem";
            this.RemoteControlToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.RemoteControlToolStripMenuItem.Text = "Remote Control";
            // 
            // FileExplorerToolStripMenuItem
            // 
            this.FileExplorerToolStripMenuItem.Name = "FileExplorerToolStripMenuItem";
            this.FileExplorerToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.FileExplorerToolStripMenuItem.Text = "File Explorer";
            // 
            // VerifyIPToolStripMenuItem
            // 
            this.VerifyIPToolStripMenuItem.Name = "VerifyIPToolStripMenuItem";
            this.VerifyIPToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.VerifyIPToolStripMenuItem.Text = "Refresh IP";
            // 
            // RemoveToolStripMenuItem
            // 
            this.RemoveToolStripMenuItem.Name = "RemoveToolStripMenuItem";
            this.RemoveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.RemoveToolStripMenuItem.Text = "Remove";
            // 
            // lblStatus
            // 
            this.lblStatus.ContextMenuStrip = this.ContextMenuStrip1;
            this.lblStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblStatus.FlatAppearance.BorderSize = 0;
            this.lblStatus.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.lblStatus.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.lblStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Location = new System.Drawing.Point(113, 1);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(27, 43);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "●";
            // 
            // lblip
            // 
            this.lblip.AutoSize = true;
            this.lblip.ContextMenuStrip = this.ContextMenuStrip1;
            this.lblip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblip.Location = new System.Drawing.Point(5, 27);
            this.lblip.Name = "lblip";
            this.lblip.Size = new System.Drawing.Size(40, 13);
            this.lblip.TabIndex = 2;
            this.lblip.Text = "0.0.0.0";
            // 
            // tmrLoad
            // 
            this.tmrLoad.Enabled = true;
            this.tmrLoad.Interval = 1;
            // 
            // tmrRemove
            // 
            this.tmrRemove.Interval = 1;
            // 
            // lblChat
            // 
            this.lblChat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblChat.ContextMenuStrip = this.ContextMenuStrip1;
            this.lblChat.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblChat.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChat.Location = new System.Drawing.Point(6, 47);
            this.lblChat.Name = "lblChat";
            this.lblChat.Size = new System.Drawing.Size(131, 21);
            this.lblChat.TabIndex = 3;
            this.lblChat.Text = "Chat";
            this.lblChat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFileExplorer
            // 
            this.lblFileExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblFileExplorer.ContextMenuStrip = this.ContextMenuStrip1;
            this.lblFileExplorer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblFileExplorer.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileExplorer.Location = new System.Drawing.Point(6, 66);
            this.lblFileExplorer.Name = "lblFileExplorer";
            this.lblFileExplorer.Size = new System.Drawing.Size(131, 21);
            this.lblFileExplorer.TabIndex = 4;
            this.lblFileExplorer.Text = "File Explorer";
            this.lblFileExplorer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRD
            // 
            this.lblRD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRD.ContextMenuStrip = this.ContextMenuStrip1;
            this.lblRD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRD.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRD.Location = new System.Drawing.Point(6, 86);
            this.lblRD.Name = "lblRD";
            this.lblRD.Size = new System.Drawing.Size(131, 21);
            this.lblRD.TabIndex = 5;
            this.lblRD.Text = "Remote PC";
            this.lblRD.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Panel1
            // 
            this.Panel1.Controls.Add(this.lblStatus);
            this.Panel1.Controls.Add(this.lblPC);
            this.Panel1.Controls.Add(this.lblip);
            this.Panel1.Location = new System.Drawing.Point(0, 0);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(140, 46);
            this.Panel1.TabIndex = 6;
            // 
            // tmrOpen
            // 
            this.tmrOpen.Interval = 1;
            // 
            // LanPC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.ContextMenuStrip1;
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.lblRD);
            this.Controls.Add(this.lblFileExplorer);
            this.Controls.Add(this.lblChat);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
            this.Name = "LanPC";
            this.Size = new System.Drawing.Size(140, 110);
            this.ContextMenuStrip1.ResumeLayout(false);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label lblPC;
        private System.Windows.Forms.Button lblStatus;
        private System.Windows.Forms.Label lblip;
        private System.Windows.Forms.ToolStripMenuItem RemoteControlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileExplorerToolStripMenuItem;
        private System.Windows.Forms.Timer tmrLoad;
        public System.Windows.Forms.Timer tmrRemove;
        private System.Windows.Forms.ToolStripMenuItem RemoveToolStripMenuItem;
        private System.Windows.Forms.Label lblChat;
        private System.Windows.Forms.Label lblFileExplorer;
        private System.Windows.Forms.Label lblRD;
        private System.Windows.Forms.Panel Panel1;
        private System.Windows.Forms.Timer tmrOpen;
        private System.Windows.Forms.ToolStripMenuItem ChatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem VerifyIPToolStripMenuItem;

        #endregion

        public System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;
    }
}
