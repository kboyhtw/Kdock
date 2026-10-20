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
            this.lblRemoteTitle = new System.Windows.Forms.Label();
            this.lblSettings = new System.Windows.Forms.Label();
            this.TrackBar1 = new System.Windows.Forms.TrackBar();
            this.lblMinimize = new System.Windows.Forms.Label();
            this.lblOC = new System.Windows.Forms.Label();
            this.btnFullScreen = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Label();
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.lblSession = new System.Windows.Forms.Label();
            this.btnLow = new System.Windows.Forms.Button();
            this.btnBalanced = new System.Windows.Forms.Button();
            this.btnSharp = new System.Windows.Forms.Button();
            this.chkClipboard = new System.Windows.Forms.CheckBox();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay)).BeginInit();
            this.Panel1.SuspendLayout();
            this.pnlSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStop
            // 
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(180, 8);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(88, 28);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Disconnect";
            this.btnStop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStop.Click += new System.EventHandler(this.bntStop_Click);
            // 
            // PicDisplay
            // 
            this.PicDisplay.BackColor = System.Drawing.Color.Black;
            this.PicDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicDisplay.Location = new System.Drawing.Point(0, 0);
            this.PicDisplay.Name = "PicDisplay";
            this.PicDisplay.Size = new System.Drawing.Size(900, 560);
            this.PicDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
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
            this.TextBox1.Visible = false;
            // 
            // tmrCursor
            // 
            this.tmrCursor.Interval = 10;
            this.tmrCursor.Tick += new System.EventHandler(this.tmrCursor_Tick);
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.Panel1.Controls.Add(this.lblRemoteTitle);
            this.Panel1.Controls.Add(this.lblSettings);
            this.Panel1.Controls.Add(this.lblMinimize);
            this.Panel1.Controls.Add(this.lblOC);
            this.Panel1.Controls.Add(this.btnFullScreen);
            this.Panel1.Controls.Add(this.btnClose);
            this.Panel1.Controls.Add(this.TextBox1);
            this.Panel1.Controls.Add(this.btnStop);
            this.Panel1.Location = new System.Drawing.Point(190, 8);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(520, 44);
            this.Panel1.TabIndex = 6;
            // 
            // lblRemoteTitle
            // 
            this.lblRemoteTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblRemoteTitle.ForeColor = System.Drawing.Color.White;
            this.lblRemoteTitle.Location = new System.Drawing.Point(12, 8);
            this.lblRemoteTitle.Name = "lblRemoteTitle";
            this.lblRemoteTitle.Size = new System.Drawing.Size(160, 28);
            this.lblRemoteTitle.TabIndex = 12;
            this.lblRemoteTitle.Text = "Remote desktop";
            this.lblRemoteTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSettings
            // 
            this.lblSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSettings.ForeColor = System.Drawing.Color.White;
            this.lblSettings.Location = new System.Drawing.Point(278, 8);
            this.lblSettings.Name = "lblSettings";
            this.lblSettings.Size = new System.Drawing.Size(70, 28);
            this.lblSettings.TabIndex = 13;
            this.lblSettings.Text = "Settings";
            this.lblSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSettings.Click += new System.EventHandler(this.lblSettings_Click);
            // 
            // TrackBar1
            // 
            this.TrackBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.TrackBar1.Location = new System.Drawing.Point(280, 88);
            this.TrackBar1.Maximum = 95;
            this.TrackBar1.Minimum = 5;
            this.TrackBar1.Name = "TrackBar1";
            this.TrackBar1.Size = new System.Drawing.Size(200, 45);
            this.TrackBar1.TabIndex = 11;
            this.TrackBar1.TickFrequency = 5;
            this.TrackBar1.Value = 50;
            this.TrackBar1.Scroll += new System.EventHandler(this.TrackBar1_Scroll);
            // 
            // lblMinimize
            // 
            this.lblMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMinimize.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblMinimize.ForeColor = System.Drawing.Color.White;
            this.lblMinimize.Location = new System.Drawing.Point(380, 8);
            this.lblMinimize.Name = "lblMinimize";
            this.lblMinimize.Size = new System.Drawing.Size(32, 28);
            this.lblMinimize.TabIndex = 10;
            this.lblMinimize.Text = "—";
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
            this.lblOC.Visible = false;
            this.lblOC.Click += new System.EventHandler(this.lblOC_Click);
            // 
            // btnFullScreen
            // 
            this.btnFullScreen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFullScreen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnFullScreen.ForeColor = System.Drawing.Color.White;
            this.btnFullScreen.Location = new System.Drawing.Point(416, 8);
            this.btnFullScreen.Name = "btnFullScreen";
            this.btnFullScreen.Size = new System.Drawing.Size(32, 28);
            this.btnFullScreen.TabIndex = 8;
            this.btnFullScreen.Text = "▢";
            this.btnFullScreen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnFullScreen.Click += new System.EventHandler(this.btnFullScreen_Click);
            // 
            // btnClose
            // 
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(456, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(32, 28);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "✕";
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlSettings
            // 
            this.pnlSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.pnlSettings.Controls.Add(this.lblSession);
            this.pnlSettings.Controls.Add(this.btnLow);
            this.pnlSettings.Controls.Add(this.btnBalanced);
            this.pnlSettings.Controls.Add(this.btnSharp);
            this.pnlSettings.Controls.Add(this.chkClipboard);
            this.pnlSettings.Controls.Add(this.TrackBar1);
            this.pnlSettings.Location = new System.Drawing.Point(190, 58);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(520, 150);
            this.pnlSettings.TabIndex = 7;
            // 
            // lblSession
            // 
            this.lblSession.AutoSize = true;
            this.lblSession.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblSession.ForeColor = System.Drawing.Color.White;
            this.lblSession.Location = new System.Drawing.Point(16, 12);
            this.lblSession.Name = "lblSession";
            this.lblSession.Size = new System.Drawing.Size(62, 20);
            this.lblSession.TabIndex = 0;
            this.lblSession.Text = "Session";
            // 
            // btnLow
            // 
            this.btnLow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnLow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLow.FlatAppearance.BorderSize = 0;
            this.btnLow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLow.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnLow.ForeColor = System.Drawing.Color.White;
            this.btnLow.Location = new System.Drawing.Point(16, 44);
            this.btnLow.Name = "btnLow";
            this.btnLow.Size = new System.Drawing.Size(116, 28);
            this.btnLow.TabIndex = 1;
            this.btnLow.Text = "Low bandwidth";
            this.btnLow.UseVisualStyleBackColor = false;
            this.btnLow.Click += new System.EventHandler(this.btnLow_Click);
            // 
            // btnBalanced
            // 
            this.btnBalanced.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnBalanced.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBalanced.FlatAppearance.BorderSize = 0;
            this.btnBalanced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBalanced.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnBalanced.ForeColor = System.Drawing.Color.White;
            this.btnBalanced.Location = new System.Drawing.Point(140, 44);
            this.btnBalanced.Name = "btnBalanced";
            this.btnBalanced.Size = new System.Drawing.Size(116, 28);
            this.btnBalanced.TabIndex = 2;
            this.btnBalanced.Text = "Balanced";
            this.btnBalanced.UseVisualStyleBackColor = false;
            this.btnBalanced.Click += new System.EventHandler(this.btnBalanced_Click);
            // 
            // btnSharp
            // 
            this.btnSharp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnSharp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSharp.FlatAppearance.BorderSize = 0;
            this.btnSharp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSharp.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnSharp.ForeColor = System.Drawing.Color.White;
            this.btnSharp.Location = new System.Drawing.Point(264, 44);
            this.btnSharp.Name = "btnSharp";
            this.btnSharp.Size = new System.Drawing.Size(116, 28);
            this.btnSharp.TabIndex = 3;
            this.btnSharp.Text = "Sharp";
            this.btnSharp.UseVisualStyleBackColor = false;
            this.btnSharp.Click += new System.EventHandler(this.btnSharp_Click);
            // 
            // chkClipboard
            // 
            this.chkClipboard.AutoSize = true;
            this.chkClipboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.chkClipboard.Checked = true;
            this.chkClipboard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkClipboard.ForeColor = System.Drawing.Color.White;
            this.chkClipboard.Location = new System.Drawing.Point(16, 100);
            this.chkClipboard.Name = "chkClipboard";
            this.chkClipboard.Size = new System.Drawing.Size(115, 17);
            this.chkClipboard.TabIndex = 4;
            this.chkClipboard.Text = "Clipboard access";
            this.chkClipboard.UseVisualStyleBackColor = false;
            this.chkClipboard.CheckedChanged += new System.EventHandler(this.Clipboard_CheckedChanged);
            // 
            // Timer1
            // 
            this.Timer1.Enabled = true;
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // frmRemote
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(900, 560);
            this.Controls.Add(this.pnlSettings);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.PicDisplay);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "frmRemote";
            this.Text = "Remote desktop";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmRemote_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay)).EndInit();
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label btnStop;

        private System.Windows.Forms.Timer tmrCursor;

        private System.Windows.Forms.Panel Panel1;

        private System.Windows.Forms.Label lblRemoteTitle;

        private System.Windows.Forms.Label lblSettings;

        private System.Windows.Forms.Panel pnlSettings;

        private System.Windows.Forms.Label lblSession;

        private System.Windows.Forms.Button btnLow;

        private System.Windows.Forms.Button btnBalanced;

        private System.Windows.Forms.Button btnSharp;

        private System.Windows.Forms.CheckBox chkClipboard;

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