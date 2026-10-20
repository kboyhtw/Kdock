namespace Kdock
{
    partial class frmFileStatus
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
            this.pTransfer = new System.Windows.Forms.Panel();
            this.lblCopyTitle = new System.Windows.Forms.Label();
            this.lblCancelT = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.ProgressBar1 = new Kdock.ProgressLite();
            this.lblSource = new System.Windows.Forms.Label();
            this.timerSpeed = new System.Windows.Forms.Timer(this.components);
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.pTransfer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pTransfer
            // 
            this.pTransfer.BackColor = System.Drawing.Color.White;
            this.pTransfer.Controls.Add(this.lblCopyTitle);
            this.pTransfer.Controls.Add(this.lblCancelT);
            this.pTransfer.Controls.Add(this.Label2);
            this.pTransfer.Controls.Add(this.ProgressBar1);
            this.pTransfer.Controls.Add(this.lblSource);
            this.pTransfer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pTransfer.Location = new System.Drawing.Point(0, 0);
            this.pTransfer.Name = "pTransfer";
            this.pTransfer.Size = new System.Drawing.Size(440, 168);
            this.pTransfer.TabIndex = 6;
            // 
            // lblCopyTitle
            // 
            this.lblCopyTitle.AutoSize = true;
            this.lblCopyTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblCopyTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(16)))));
            this.lblCopyTitle.Location = new System.Drawing.Point(20, 16);
            this.lblCopyTitle.Name = "lblCopyTitle";
            this.lblCopyTitle.Size = new System.Drawing.Size(99, 25);
            this.lblCopyTitle.TabIndex = 6;
            this.lblCopyTitle.Text = "Receiving";
            // 
            // lblCancelT
            // 
            this.lblCancelT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCancelT.AutoSize = true;
            this.lblCancelT.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCancelT.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCancelT.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(103)))), ((int)(((byte)(192)))));
            this.lblCancelT.Location = new System.Drawing.Point(372, 20);
            this.lblCancelT.Name = "lblCancelT";
            this.lblCancelT.Size = new System.Drawing.Size(45, 15);
            this.lblCancelT.TabIndex = 5;
            this.lblCancelT.Text = "Cancel";
            this.lblCancelT.Click += new System.EventHandler(this.lblCancelT_Click);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.Label2.Location = new System.Drawing.Point(20, 100);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(175, 15);
            this.Label2.TabIndex = 4;
            this.Label2.Text = "0%   ·   0 B of 0 B   ·   0.0 MB/s";
            // 
            // ProgressBar1
            // 
            this.ProgressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(229)))), ((int)(((byte)(229)))));
            this.ProgressBar1.Location = new System.Drawing.Point(20, 128);
            this.ProgressBar1.Maximum = 100;
            this.ProgressBar1.Name = "ProgressBar1";
            this.ProgressBar1.Size = new System.Drawing.Size(400, 4);
            this.ProgressBar1.TabIndex = 3;
            this.ProgressBar1.Value = 0;
            // 
            // lblSource
            // 
            this.lblSource.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSource.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
            this.lblSource.Location = new System.Drawing.Point(20, 48);
            this.lblSource.MaximumSize = new System.Drawing.Size(400, 48);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(400, 48);
            this.lblSource.TabIndex = 0;
            this.lblSource.Text = "Source";
            // 
            // timerSpeed
            // 
            this.timerSpeed.Enabled = true;
            this.timerSpeed.Interval = 1000;
            this.timerSpeed.Tick += new System.EventHandler(this.timerSpeed_Tick);
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Blue;
            this.PictureBox1.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.PictureBox1.Location = new System.Drawing.Point(0, 0);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(10, 10);
            this.PictureBox1.TabIndex = 64;
            this.PictureBox1.TabStop = false;
            this.PictureBox1.Visible = false;
            // 
            // frmFileStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(440, 168);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.pTransfer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmFileStatus";
            this.Text = "frmFileStatus";
            this.pTransfer.ResumeLayout(false);
            this.pTransfer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pTransfer;

        private System.Windows.Forms.Label lblCopyTitle;

        private System.Windows.Forms.Label lblCancelT;

        private System.Windows.Forms.Label Label2;

        private ProgressLite ProgressBar1;

        private System.Windows.Forms.Label lblSource;

        private System.Windows.Forms.Timer timerSpeed;

        private System.Windows.Forms.PictureBox PictureBox1;
        #endregion
    }
}