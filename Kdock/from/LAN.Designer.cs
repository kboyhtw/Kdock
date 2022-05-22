namespace Kdock
{
    partial class LAN
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
            this.pmove = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.Timerclose = new System.Windows.Forms.Timer(this.components);
            this.Timeropen = new System.Windows.Forms.Timer(this.components);
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddUrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SendMessageToAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flpPC = new System.Windows.Forms.FlowLayoutPanel();
            this.paddPC = new System.Windows.Forms.Panel();
            this.lblCancel = new Kdock.Klabel();
            this.lblSave = new Kdock.Klabel();
            this.txtipAdd = new System.Windows.Forms.TextBox();
            this.txturlname = new System.Windows.Forms.TextBox();
            this.lblUrl = new System.Windows.Forms.Label();
            this.lblname = new System.Windows.Forms.Label();
            this.pMsg = new System.Windows.Forms.Panel();
            this.lblCanMsg = new Kdock.Klabel();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.lblRD = new Kdock.Klabel();
            this.lblFileExpl = new Kdock.Klabel();
            this.lblChat = new Kdock.Klabel();
            this.lblShowPC = new Kdock.Klabel();
            this.ContextMenuStrip1.SuspendLayout();
            this.paddPC.SuspendLayout();
            this.pMsg.SuspendLayout();
            this.SuspendLayout();
            // 
            // pmove
            // 
            this.pmove.BackColor = System.Drawing.Color.Red;
            this.pmove.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.pmove.Location = new System.Drawing.Point(0, 0);
            this.pmove.Name = "pmove";
            this.pmove.Size = new System.Drawing.Size(10, 10);
            this.pmove.TabIndex = 3;
            this.pmove.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pmove_MouseMove);
            // 
            // lblClose
            // 
            this.lblClose.AutoSize = true;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClose.Location = new System.Drawing.Point(476, -1);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(16, 20);
            this.lblClose.TabIndex = 7;
            this.lblClose.Text = "x";
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // Timerclose
            // 
            this.Timerclose.Interval = 1;
            this.Timerclose.Tick += new System.EventHandler(this.Timerclose_Tick);
            // 
            // Timeropen
            // 
            this.Timeropen.Interval = 1;
            this.Timeropen.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddUrlToolStripMenuItem,
            this.SendMessageToAllToolStripMenuItem});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(179, 48);
            // 
            // AddUrlToolStripMenuItem
            // 
            this.AddUrlToolStripMenuItem.Name = "AddUrlToolStripMenuItem";
            this.AddUrlToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.AddUrlToolStripMenuItem.Text = "Add PC";
            this.AddUrlToolStripMenuItem.Click += new System.EventHandler(this.AddUrlToolStripMenuItem_Click);
            // 
            // SendMessageToAllToolStripMenuItem
            // 
            this.SendMessageToAllToolStripMenuItem.Name = "SendMessageToAllToolStripMenuItem";
            this.SendMessageToAllToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.SendMessageToAllToolStripMenuItem.Text = "Send message to all";
            this.SendMessageToAllToolStripMenuItem.Click += new System.EventHandler(this.SendMessageToAllToolStripMenuItem_Click);
            // 
            // flpPC
            // 
            this.flpPC.AutoScroll = true;
            this.flpPC.ContextMenuStrip = this.ContextMenuStrip1;
            this.flpPC.Location = new System.Drawing.Point(18, 60);
            this.flpPC.Name = "flpPC";
            this.flpPC.Size = new System.Drawing.Size(456, 228);
            this.flpPC.TabIndex = 9;
            // 
            // paddPC
            // 
            this.paddPC.Controls.Add(this.lblCancel);
            this.paddPC.Controls.Add(this.lblSave);
            this.paddPC.Controls.Add(this.txtipAdd);
            this.paddPC.Controls.Add(this.txturlname);
            this.paddPC.Controls.Add(this.lblUrl);
            this.paddPC.Controls.Add(this.lblname);
            this.paddPC.Location = new System.Drawing.Point(14, 60);
            this.paddPC.Name = "paddPC";
            this.paddPC.Size = new System.Drawing.Size(464, 228);
            this.paddPC.TabIndex = 0;
            this.paddPC.Visible = false;
            // 
            // lblCancel
            // 
            this.lblCancel.AutoSize = true;
            this.lblCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCancel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCancel.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblCancel.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblCancel.Location = new System.Drawing.Point(210, 147);
            this.lblCancel.Name = "lblCancel";
            this.lblCancel.Size = new System.Drawing.Size(56, 21);
            this.lblCancel.TabIndex = 9;
            this.lblCancel.Text = "Cancel";
            this.lblCancel.Click += new System.EventHandler(this.lblCancel_Click);
            // 
            // lblSave
            // 
            this.lblSave.AutoSize = true;
            this.lblSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSave.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSave.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblSave.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblSave.Location = new System.Drawing.Point(143, 147);
            this.lblSave.Name = "lblSave";
            this.lblSave.Size = new System.Drawing.Size(43, 21);
            this.lblSave.TabIndex = 8;
            this.lblSave.Text = "Save";
            this.lblSave.Click += new System.EventHandler(this.lblSave_Click);
            // 
            // txtipAdd
            // 
            this.txtipAdd.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtipAdd.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtipAdd.Location = new System.Drawing.Point(147, 106);
            this.txtipAdd.Name = "txtipAdd";
            this.txtipAdd.Size = new System.Drawing.Size(224, 25);
            this.txtipAdd.TabIndex = 7;
            // 
            // txturlname
            // 
            this.txturlname.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txturlname.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txturlname.Location = new System.Drawing.Point(147, 69);
            this.txturlname.Name = "txturlname";
            this.txturlname.Size = new System.Drawing.Size(224, 25);
            this.txturlname.TabIndex = 6;
            // 
            // lblUrl
            // 
            this.lblUrl.AutoSize = true;
            this.lblUrl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUrl.Location = new System.Drawing.Point(68, 110);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(30, 21);
            this.lblUrl.TabIndex = 1;
            this.lblUrl.Text = "IP :";
            // 
            // lblname
            // 
            this.lblname.AutoSize = true;
            this.lblname.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblname.Location = new System.Drawing.Point(68, 72);
            this.lblname.Name = "lblname";
            this.lblname.Size = new System.Drawing.Size(82, 21);
            this.lblname.TabIndex = 0;
            this.lblname.Text = "PC Name :";
            // 
            // pMsg
            // 
            this.pMsg.Controls.Add(this.lblCanMsg);
            this.pMsg.Controls.Add(this.btnSend);
            this.pMsg.Controls.Add(this.txtMsg);
            this.pMsg.Location = new System.Drawing.Point(14, 64);
            this.pMsg.Name = "pMsg";
            this.pMsg.Size = new System.Drawing.Size(464, 221);
            this.pMsg.TabIndex = 10;
            this.pMsg.Visible = false;
            // 
            // lblCanMsg
            // 
            this.lblCanMsg.AutoSize = true;
            this.lblCanMsg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCanMsg.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCanMsg.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCanMsg.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblCanMsg.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblCanMsg.Location = new System.Drawing.Point(322, 144);
            this.lblCanMsg.Name = "lblCanMsg";
            this.lblCanMsg.Size = new System.Drawing.Size(56, 21);
            this.lblCanMsg.TabIndex = 10;
            this.lblCanMsg.Text = "Cancel";
            this.lblCanMsg.Click += new System.EventHandler(this.lblCanMsg_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(318, 64);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(68, 72);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = ">>>";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMsg.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMsg.Location = new System.Drawing.Point(72, 62);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.Size = new System.Drawing.Size(240, 76);
            this.txtMsg.TabIndex = 5;
            // 
            // txtIP
            // 
            this.txtIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtIP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIP.Location = new System.Drawing.Point(17, 12);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(128, 29);
            this.txtIP.TabIndex = 5;
            this.txtIP.Text = "0.0.0.0";
            // 
            // lblRD
            // 
            this.lblRD.AutoSize = true;
            this.lblRD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRD.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRD.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblRD.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblRD.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblRD.Location = new System.Drawing.Point(336, 15);
            this.lblRD.Name = "lblRD";
            this.lblRD.Size = new System.Drawing.Size(125, 21);
            this.lblRD.TabIndex = 13;
            this.lblRD.Text = "Remote Desktop";
            this.lblRD.Click += new System.EventHandler(this.lblRD_Click);
            // 
            // lblFileExpl
            // 
            this.lblFileExpl.AutoSize = true;
            this.lblFileExpl.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblFileExpl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileExpl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblFileExpl.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblFileExpl.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblFileExpl.Location = new System.Drawing.Point(224, 15);
            this.lblFileExpl.Name = "lblFileExpl";
            this.lblFileExpl.Size = new System.Drawing.Size(95, 21);
            this.lblFileExpl.TabIndex = 12;
            this.lblFileExpl.Text = "File Explorer";
            this.lblFileExpl.Click += new System.EventHandler(this.lblFileExpl_Click);
            // 
            // lblChat
            // 
            this.lblChat.AutoSize = true;
            this.lblChat.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblChat.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChat.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblChat.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblChat.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblChat.Location = new System.Drawing.Point(157, 15);
            this.lblChat.Name = "lblChat";
            this.lblChat.Size = new System.Drawing.Size(42, 21);
            this.lblChat.TabIndex = 11;
            this.lblChat.Text = "Chat";
            this.lblChat.Click += new System.EventHandler(this.lblChat_Click);
            // 
            // lblShowPC
            // 
            this.lblShowPC.AutoSize = true;
            this.lblShowPC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblShowPC.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblShowPC.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblShowPC.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblShowPC.Location = new System.Drawing.Point(450, 40);
            this.lblShowPC.Name = "lblShowPC";
            this.lblShowPC.Size = new System.Drawing.Size(31, 13);
            this.lblShowPC.TabIndex = 10;
            this.lblShowPC.Text = "====";
            this.lblShowPC.Click += new System.EventHandler(this.lblShowPC_Click);
            // 
            // LAN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 299);
            this.Controls.Add(this.pMsg);
            this.Controls.Add(this.lblRD);
            this.Controls.Add(this.lblFileExpl);
            this.Controls.Add(this.lblChat);
            this.Controls.Add(this.paddPC);
            this.Controls.Add(this.flpPC);
            this.Controls.Add(this.lblClose);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.pmove);
            this.Controls.Add(this.lblShowPC);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LAN";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "web";
            this.Load += new System.EventHandler(this.web_Load);
            this.ContextMenuStrip1.ResumeLayout(false);
            this.paddPC.ResumeLayout(false);
            this.paddPC.PerformLayout();
            this.pMsg.ResumeLayout(false);
            this.pMsg.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel pmove;
        private System.Windows.Forms.Label lblClose;
        public System.Windows.Forms.Timer Timerclose;
        private System.Windows.Forms.Timer Timeropen;
        private System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem AddUrlToolStripMenuItem;
        private System.Windows.Forms.Panel paddPC;
        private Klabel lblSave;
        private System.Windows.Forms.TextBox txtipAdd;
        private System.Windows.Forms.TextBox txturlname;
        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.Label lblname;
        private Klabel lblCancel;
        private Klabel lblShowPC;
        private System.Windows.Forms.TextBox txtIP;
        private Klabel lblChat;
        private Klabel lblFileExpl;
        private Klabel lblRD;
        public System.Windows.Forms.FlowLayoutPanel flpPC;
        private System.Windows.Forms.ToolStripMenuItem SendMessageToAllToolStripMenuItem;
        private System.Windows.Forms.Panel pMsg;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtMsg;
        private Klabel lblCanMsg;

        #endregion
    }
}