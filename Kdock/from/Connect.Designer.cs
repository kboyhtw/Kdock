namespace Kdock
{
    partial class Connect
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
            this.btnOk = new System.Windows.Forms.Button();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.pConnect = new System.Windows.Forms.Panel();
            this.lblLocalIp = new System.Windows.Forms.Label();
            this.btnChat = new System.Windows.Forms.Button();
            this.BtnRM = new System.Windows.Forms.Button();
            this.tmrFramSender = new System.Windows.Forms.Timer(this.components);
            this.tmrControl = new System.Windows.Forms.Timer(this.components);
            this.tmrFramReceiver = new System.Windows.Forms.Timer(this.components);
            this.bwSender = new System.ComponentModel.BackgroundWorker();
            this.bwReceiver = new System.ComponentModel.BackgroundWorker();
            this.tmrNetworkListener = new System.Windows.Forms.Timer(this.components);
            this.pConnect.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(138, 28);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 26);
            this.btnOk.TabIndex = 3;
            this.btnOk.Text = "Connect";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(7, 29);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(125, 25);
            this.txtIp.TabIndex = 2;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(7, 10);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(97, 17);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "Host Name / IP";
            // 
            // pConnect
            // 
            this.pConnect.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.pConnect.Controls.Add(this.Label1);
            this.pConnect.Controls.Add(this.txtIp);
            this.pConnect.Controls.Add(this.btnOk);
            this.pConnect.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pConnect.Location = new System.Drawing.Point(68, 77);
            this.pConnect.Name = "pConnect";
            this.pConnect.Size = new System.Drawing.Size(232, 65);
            this.pConnect.TabIndex = 5;
            this.pConnect.Visible = false;
            // 
            // lblLocalIp
            // 
            this.lblLocalIp.AutoSize = true;
            this.lblLocalIp.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocalIp.Location = new System.Drawing.Point(65, 57);
            this.lblLocalIp.Name = "lblLocalIp";
            this.lblLocalIp.Size = new System.Drawing.Size(48, 17);
            this.lblLocalIp.TabIndex = 7;
            this.lblLocalIp.Text = "0.0.0.0";
            // 
            // btnChat
            // 
            this.btnChat.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChat.Location = new System.Drawing.Point(62, 12);
            this.btnChat.Name = "btnChat";
            this.btnChat.Size = new System.Drawing.Size(123, 26);
            this.btnChat.TabIndex = 6;
            this.btnChat.Text = "LAN Chat";
            this.btnChat.UseVisualStyleBackColor = true;
            this.btnChat.Click += new System.EventHandler(this.btnChat_Click);
            // 
            // BtnRM
            // 
            this.BtnRM.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnRM.Location = new System.Drawing.Point(186, 12);
            this.BtnRM.Name = "BtnRM";
            this.BtnRM.Size = new System.Drawing.Size(126, 26);
            this.BtnRM.TabIndex = 7;
            this.BtnRM.Text = "Remote Desktop";
            this.BtnRM.UseVisualStyleBackColor = true;
            this.BtnRM.Click += new System.EventHandler(this.BtnRM_Click);
            // 
            // tmrFramSender
            // 
            this.tmrFramSender.Interval = 50;
            this.tmrFramSender.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // tmrControl
            // 
            this.tmrControl.Enabled = true;
            this.tmrControl.Interval = 1;
            this.tmrControl.Tick += new System.EventHandler(this.tmrControl_Tick);
            // 
            // tmrFramReceiver
            // 
            this.tmrFramReceiver.Enabled = true;
            this.tmrFramReceiver.Interval = 1;
            this.tmrFramReceiver.Tick += new System.EventHandler(this.Timer2_Tick);
            // 
            // bwSender
            // 
            this.bwSender.WorkerReportsProgress = true;
            this.bwSender.WorkerSupportsCancellation = true;
            this.bwSender.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwSender_DoWork);
            // 
            // bwReceiver
            // 
            this.bwReceiver.WorkerSupportsCancellation = true;
            this.bwReceiver.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwReceiver_DoWork);
            // 
            // tmrNetworkListener
            // 
            this.tmrNetworkListener.Enabled = true;
            this.tmrNetworkListener.Interval = 50;
            this.tmrNetworkListener.Tick += new System.EventHandler(this.tmrNetworkListener_Tick);
            // 
            // Connect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.ClientSize = new System.Drawing.Size(37, 24);
            this.Controls.Add(this.lblLocalIp);
            this.Controls.Add(this.pConnect);
            this.Controls.Add(this.BtnRM);
            this.Controls.Add(this.btnChat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Connect";
            this.ShowIcon = false;
            this.Text = "Connection";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Connect_Load);
            this.pConnect.ResumeLayout(false);
            this.pConnect.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button btnOk;

        private System.Windows.Forms.TextBox txtIp;

        private System.Windows.Forms.Label Label1;

        private System.Windows.Forms.Panel pConnect;

        private System.Windows.Forms.Button btnChat;

        private System.Windows.Forms.Button BtnRM;

        private System.Windows.Forms.Label lblLocalIp;

        private System.Windows.Forms.Timer tmrFramSender;

        private System.Windows.Forms.Timer tmrControl;

        private System.Windows.Forms.Timer tmrFramReceiver;
        #endregion

        public System.ComponentModel.BackgroundWorker bwSender;
        public System.ComponentModel.BackgroundWorker bwReceiver;
        private System.Windows.Forms.Timer tmrNetworkListener;
    }
}