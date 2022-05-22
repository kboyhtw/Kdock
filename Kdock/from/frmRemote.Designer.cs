namespace Kdock
{
    partial class frmRemote
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
            this.btnStop = new System.Windows.Forms.Label();
            this.PicDisplay = new System.Windows.Forms.PictureBox();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.tmrCursor = new System.Windows.Forms.Timer(this.components);
            this.Panel1 = new System.Windows.Forms.Panel();
            this.TrackBar1 = new System.Windows.Forms.TrackBar();
            this.lblMinimize = new System.Windows.Forms.Label();
            this.lblOC = new System.Windows.Forms.Label();
            this.btnFullScreen = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Label();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay)).BeginInit();
            this.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStop
            // 
            this.btnStop.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnStop.AutoSize = true;
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnStop.Location = new System.Drawing.Point(24, 8);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(36, 17);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.Click += new System.EventHandler(this.bntStop_Click);
            // 
            // PicDisplay
            // 
            this.PicDisplay.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PicDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.PicDisplay.Location = new System.Drawing.Point(0, 0);
            this.PicDisplay.Name = "PicDisplay";
            this.PicDisplay.Size = new System.Drawing.Size(497, 267);
            this.PicDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PicDisplay.TabIndex = 3;
            this.PicDisplay.TabStop = false;
            this.PicDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseDown);
            this.PicDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PicDisplay_MouseMove);
            this.PicDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox1_MouseUp);
            // 
            // TextBox1
            // 
            this.TextBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.TextBox1.Enabled = false;
            this.TextBox1.Location = new System.Drawing.Point(117, 8);
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.Size = new System.Drawing.Size(122, 20);
            this.TextBox1.TabIndex = 5;
            // 
            // tmrCursor
            // 
            this.tmrCursor.Interval = 10;
            this.tmrCursor.Tick += new System.EventHandler(this.tmrCursor_Tick);
            // 
            // Panel1
            // 
            this.Panel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.Panel1.Controls.Add(this.TrackBar1);
            this.Panel1.Controls.Add(this.lblMinimize);
            this.Panel1.Controls.Add(this.lblOC);
            this.Panel1.Controls.Add(this.btnFullScreen);
            this.Panel1.Controls.Add(this.btnClose);
            this.Panel1.Controls.Add(this.TextBox1);
            this.Panel1.Controls.Add(this.btnStop);
            this.Panel1.Location = new System.Drawing.Point(81, -1);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(345, 34);
            this.Panel1.TabIndex = 6;
            // 
            // TrackBar1
            // 
            this.TrackBar1.Location = new System.Drawing.Point(81, 0);
            this.TrackBar1.Maximum = 95;
            this.TrackBar1.Minimum = 5;
            this.TrackBar1.Name = "TrackBar1";
            this.TrackBar1.Size = new System.Drawing.Size(104, 45);
            this.TrackBar1.TabIndex = 11;
            this.TrackBar1.TickFrequency = 5;
            this.TrackBar1.Value = 50;
            this.TrackBar1.Scroll += new System.EventHandler(this.TrackBar1_Scroll);
            // 
            // lblMinimize
            // 
            this.lblMinimize.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMinimize.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblMinimize.Location = new System.Drawing.Point(269, 5);
            this.lblMinimize.Name = "lblMinimize";
            this.lblMinimize.Size = new System.Drawing.Size(24, 20);
            this.lblMinimize.TabIndex = 10;
            this.lblMinimize.Text = "__";
            this.lblMinimize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMinimize.Click += new System.EventHandler(this.lblMaxmimize_Click);
            // 
            // lblOC
            // 
            this.lblOC.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblOC.AutoSize = true;
            this.lblOC.BackColor = System.Drawing.Color.Red;
            this.lblOC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblOC.Location = new System.Drawing.Point(244, 21);
            this.lblOC.Name = "lblOC";
            this.lblOC.Size = new System.Drawing.Size(25, 13);
            this.lblOC.TabIndex = 9;
            this.lblOC.Text = "===";
            this.lblOC.Click += new System.EventHandler(this.lblOC_Click);
            // 
            // btnFullScreen
            // 
            this.btnFullScreen.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnFullScreen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFullScreen.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFullScreen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnFullScreen.Location = new System.Drawing.Point(294, 5);
            this.btnFullScreen.Name = "btnFullScreen";
            this.btnFullScreen.Size = new System.Drawing.Size(24, 20);
            this.btnFullScreen.TabIndex = 8;
            this.btnFullScreen.Text = "[[]]";
            this.btnFullScreen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnFullScreen.Click += new System.EventHandler(this.btnFullScreen_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnClose.Location = new System.Drawing.Point(320, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(22, 20);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "X";
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Timer1
            // 
            this.Timer1.Enabled = true;
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // frmRemote
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(501, 269);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.PicDisplay);
            this.KeyPreview = true;
            this.Name = "frmRemote";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmRemote_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay)).EndInit();
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar1)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label btnStop;

        private System.Windows.Forms.Timer tmrCursor;

        private System.Windows.Forms.Panel Panel1;

        private System.Windows.Forms.Label btnFullScreen;

        private System.Windows.Forms.Label btnClose;

        private System.Windows.Forms.Label lblOC;

        private System.Windows.Forms.Label lblMinimize;

        private System.Windows.Forms.TrackBar TrackBar1;

        private System.Windows.Forms.Timer Timer1;

        #endregion

        public System.Windows.Forms.PictureBox PicDisplay;
        public System.Windows.Forms.TextBox TextBox1;
    }
}