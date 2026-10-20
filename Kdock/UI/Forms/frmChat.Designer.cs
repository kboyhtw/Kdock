namespace Kdock
{
    partial class frmChat
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
            this.tmrOpen = new System.Windows.Forms.Timer(this.components);
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.flpChat = new System.Windows.Forms.FlowLayoutPanel();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblClose = new System.Windows.Forms.Label();
            this.lblHost = new System.Windows.Forms.Label();
            this.Panel2 = new System.Windows.Forms.Panel();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.tmrClose = new System.Windows.Forms.Timer(this.components);
            this.lblMin = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.Panel2.SuspendLayout();
            this.Panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmrOpen
            // 
            this.tmrOpen.Interval = 1;
            this.tmrOpen.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.txtMsg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMsg.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMsg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.txtMsg.Location = new System.Drawing.Point(60, 458);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.Size = new System.Drawing.Size(244, 34);
            this.txtMsg.TabIndex = 3;
            this.txtMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox1_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(46)))), ((int)(((byte)(145)))));
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.Location = new System.Drawing.Point(316, 454);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(72, 40);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // flpChat
            // 
            this.flpChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpChat.AutoScroll = true;
            this.flpChat.AutoScrollMargin = new System.Drawing.Size(0, 100);
            this.flpChat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(242)))), ((int)(((byte)(241)))));
            this.flpChat.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpChat.Location = new System.Drawing.Point(0, 0);
            this.flpChat.Name = "flpChat";
            this.flpChat.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.flpChat.Size = new System.Drawing.Size(400, 440);
            this.flpChat.WrapContents = false;
            this.flpChat.TabIndex = 4;
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Blue;
            this.PictureBox1.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.PictureBox1.Location = new System.Drawing.Point(0, 0);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(10, 10);
            this.PictureBox1.TabIndex = 15;
            this.PictureBox1.TabStop = false;
            this.PictureBox1.Visible = false;
            this.PictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseMove);
            // 
            // lblClose
            // 
            this.lblClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(46)))), ((int)(((byte)(145)))));
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblClose.ForeColor = System.Drawing.Color.White;
            this.lblClose.Location = new System.Drawing.Point(358, 10);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(32, 28);
            this.lblClose.TabIndex = 16;
            this.lblClose.Text = "✕";
            this.lblClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // lblHost
            // 
            this.lblHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(46)))), ((int)(((byte)(145)))));
            this.lblHost.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblHost.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.lblHost.ForeColor = System.Drawing.Color.White;
            this.lblHost.Location = new System.Drawing.Point(0, 0);
            this.lblHost.Name = "lblHost";
            this.lblHost.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblHost.Size = new System.Drawing.Size(400, 48);
            this.lblHost.TabIndex = 17;
            this.lblHost.Text = "Host";
            this.lblHost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Panel2
            // 
            this.Panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel2.Controls.Add(this.Label2);
            this.Panel2.Controls.Add(this.Label1);
            this.Panel2.Location = new System.Drawing.Point(240, -3);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(17, 329);
            this.Panel2.TabIndex = 4;
            this.Panel2.Visible = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(-1, 291);
            this.Label2.MaximumSize = new System.Drawing.Size(20, 30);
            this.Label2.MinimumSize = new System.Drawing.Size(20, 30);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(20, 30);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "-";
            this.Label2.Visible = false;
            this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label2.Click += new System.EventHandler(this.Label2_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(-1, 15);
            this.Label1.MaximumSize = new System.Drawing.Size(20, 30);
            this.Label1.MinimumSize = new System.Drawing.Size(20, 30);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(20, 30);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "+";
            this.Label1.Visible = false;
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // Panel1
            // 
            this.Panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(242)))), ((int)(((byte)(241)))));
            this.Panel1.Controls.Add(this.button1);
            this.Panel1.Controls.Add(this.Panel2);
            this.Panel1.Controls.Add(this.flpChat);
            this.Panel1.Controls.Add(this.btnSend);
            this.Panel1.Controls.Add(this.txtMsg);
            this.Panel1.Location = new System.Drawing.Point(0, 48);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(400, 512);
            this.Panel1.TabIndex = 18;
            // 
            // tmrClose
            // 
            this.tmrClose.Interval = 1;
            this.tmrClose.Tick += new System.EventHandler(this.Timer2_Tick);
            // 
            // lblMin
            // 
            this.lblMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(46)))), ((int)(((byte)(145)))));
            this.lblMin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMin.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblMin.ForeColor = System.Drawing.Color.White;
            this.lblMin.Location = new System.Drawing.Point(320, 10);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(32, 28);
            this.lblMin.TabIndex = 19;
            this.lblMin.Text = "—";
            this.lblMin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMin.Click += new System.EventHandler(this.lblMin_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.button1.Location = new System.Drawing.Point(12, 454);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(40, 40);
            this.button1.TabIndex = 5;
            this.button1.Text = "+";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(242)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(400, 560);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.lblHost);
            this.Controls.Add(this.lblMin);
            this.Controls.Add(this.lblClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(380, 520);
            this.Name = "frmChat";
            this.ShowIcon = false;
            this.Text = "LAN Chat";
            this.Load += new System.EventHandler(this.frmLanChat_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.Panel2.ResumeLayout(false);
            this.Panel2.PerformLayout();
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Timer tmrOpen;

        private System.Windows.Forms.TextBox txtMsg;

        private System.Windows.Forms.Button btnSend;

        private System.Windows.Forms.FlowLayoutPanel flpChat;

        private System.Windows.Forms.PictureBox PictureBox1;

        private System.Windows.Forms.Label lblClose;

        private System.Windows.Forms.Label lblHost;

        private System.Windows.Forms.Panel Panel2;

        private System.Windows.Forms.Label Label2;

        private System.Windows.Forms.Label Label1;

        private System.Windows.Forms.Panel Panel1;

        private System.Windows.Forms.Timer tmrClose;

        private System.Windows.Forms.Label lblMin;

        #endregion

        private System.Windows.Forms.Button button1;
    }
}