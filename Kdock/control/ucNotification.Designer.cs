namespace Kdock
{
    partial class ucNotification
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
            this.lblPC = new System.Windows.Forms.Label();
            this.lblip = new System.Windows.Forms.Label();
            this.lblMsg = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.FlowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPC
            // 
            this.lblPC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPC.Font = new System.Drawing.Font("Segoe UI", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPC.Location = new System.Drawing.Point(-2, 0);
            this.lblPC.Margin = new System.Windows.Forms.Padding(3);
            this.lblPC.Name = "lblPC";
            this.lblPC.Size = new System.Drawing.Size(82, 18);
            this.lblPC.TabIndex = 0;
            this.lblPC.Text = "Label1";
            // 
            // lblip
            // 
            this.lblip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblip.Font = new System.Drawing.Font("Segoe UI", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblip.Location = new System.Drawing.Point(187, 1);
            this.lblip.Margin = new System.Windows.Forms.Padding(3);
            this.lblip.Name = "lblip";
            this.lblip.Size = new System.Drawing.Size(82, 13);
            this.lblip.TabIndex = 1;
            this.lblip.Text = "0.0.0.0";
            this.lblip.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMsg.Location = new System.Drawing.Point(0, 0);
            this.lblMsg.Margin = new System.Windows.Forms.Padding(0);
            this.lblMsg.MaximumSize = new System.Drawing.Size(250, 25);
            this.lblMsg.MinimumSize = new System.Drawing.Size(250, 25);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(250, 25);
            this.lblMsg.TabIndex = 2;
            this.lblMsg.Click += new System.EventHandler(this.lblMsg_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(0, 37);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(20, 7);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "More";
            this.Label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(0, 18);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(13, 13);
            this.lblTotal.TabIndex = 4;
            this.lblTotal.Text = "0";
            // 
            // FlowLayoutPanel1
            // 
            this.FlowLayoutPanel1.AutoSize = true;
            this.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FlowLayoutPanel1.Controls.Add(this.lblMsg);
            this.FlowLayoutPanel1.Controls.Add(this.PictureBox1);
            this.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowLayoutPanel1.Location = new System.Drawing.Point(22, 20);
            this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
            this.FlowLayoutPanel1.Size = new System.Drawing.Size(250, 34);
            this.FlowLayoutPanel1.TabIndex = 5;
            // 
            // PictureBox1
            // 
            this.PictureBox1.Location = new System.Drawing.Point(0, 31);
            this.PictureBox1.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(250, 3);
            this.PictureBox1.TabIndex = 3;
            this.PictureBox1.TabStop = false;
            // 
            // ucNotification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Controls.Add(this.FlowLayoutPanel1);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.lblip);
            this.Controls.Add(this.lblPC);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 3, 3);
            this.Name = "ucNotification";
            this.Size = new System.Drawing.Size(275, 57);
            this.FlowLayoutPanel1.ResumeLayout(false);
            this.FlowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label Label1;

        private System.Windows.Forms.Label lblTotal;

        private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;

        private System.Windows.Forms.PictureBox PictureBox1;

        #endregion

        public System.Windows.Forms.Label lblPC;
        public System.Windows.Forms.Label lblip;
        public System.Windows.Forms.Label lblMsg;
    }
}
