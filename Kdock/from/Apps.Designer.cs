namespace Kdock
{
    partial class Apps
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
            this.Panel2 = new System.Windows.Forms.Panel();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.papps = new System.Windows.Forms.FlowLayoutPanel();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.TimerRight1 = new System.Windows.Forms.Timer(this.components);
            this.TimerRight2 = new System.Windows.Forms.Timer(this.components);
            this.TimerLeft1 = new System.Windows.Forms.Timer(this.components);
            this.TimerLeft2 = new System.Windows.Forms.Timer(this.components);
            this.BackgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Panel2
            // 
            this.Panel2.BackColor = System.Drawing.Color.White;
            this.Panel2.Controls.Add(this.Label2);
            this.Panel2.Controls.Add(this.Label1);
            this.Panel2.Location = new System.Drawing.Point(453, 0);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(27, 261);
            this.Panel2.TabIndex = 3;
            this.Panel2.Visible = false;
            this.Panel2.MouseLeave += new System.EventHandler(this.Apps_mouseL);
            this.Panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Apps_MouseMove);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(3, 209);
            this.Label2.MaximumSize = new System.Drawing.Size(20, 30);
            this.Label2.MinimumSize = new System.Drawing.Size(20, 30);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(20, 30);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "-";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label2.Click += new System.EventHandler(this.Label2_Click);
            this.Label2.MouseLeave += new System.EventHandler(this.Apps_mouseL);
            this.Label2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Apps_MouseMove);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(4, 15);
            this.Label1.MaximumSize = new System.Drawing.Size(20, 30);
            this.Label1.MinimumSize = new System.Drawing.Size(20, 30);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(20, 30);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "+";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label1.Click += new System.EventHandler(this.Label1_Click);
            this.Label1.MouseLeave += new System.EventHandler(this.Apps_mouseL);
            this.Label1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Apps_MouseMove);
            // 
            // papps
            // 
            this.papps.AutoScroll = true;
            this.papps.BackColor = System.Drawing.Color.White;
            this.papps.Location = new System.Drawing.Point(0, 0);
            this.papps.Name = "papps";
            this.papps.Size = new System.Drawing.Size(480, 261);
            this.papps.TabIndex = 5;
            this.papps.Visible = false;
            this.papps.MouseLeave += new System.EventHandler(this.Apps_mouseL);
            this.papps.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Apps_MouseMove);
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.PictureBox1.Location = new System.Drawing.Point(480, 0);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(1, 271);
            this.PictureBox1.TabIndex = 4;
            this.PictureBox1.TabStop = false;
            this.PictureBox1.Visible = false;
            this.PictureBox1.MouseLeave += new System.EventHandler(this.Apps_mouseL);
            this.PictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Apps_MouseMove);
            // 
            // TimerRight1
            // 
            this.TimerRight1.Interval = 1;
            this.TimerRight1.Tick += new System.EventHandler(this.TimerRight1_Tick);
            // 
            // TimerRight2
            // 
            this.TimerRight2.Interval = 1;
            this.TimerRight2.Tick += new System.EventHandler(this.TimerRight2_Tick);
            // 
            // TimerLeft1
            // 
            this.TimerLeft1.Interval = 1;
            this.TimerLeft1.Tick += new System.EventHandler(this.TimerLeft1_Tick);
            // 
            // TimerLeft2
            // 
            this.TimerLeft2.Interval = 1;
            this.TimerLeft2.Tick += new System.EventHandler(this.TimerLeft2_Tick);
            // 
            // BackgroundWorker1
            // 
            this.BackgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
            // 
            // Apps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MintCream;
            this.ClientSize = new System.Drawing.Size(480, 271);
            this.Controls.Add(this.Panel2);
            this.Controls.Add(this.papps);
            this.Controls.Add(this.PictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Apps";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Apps";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.MintCream;
            this.Load += new System.EventHandler(this.Apps_Load);
            this.MouseLeave += new System.EventHandler(this.Apps_mouseL);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Apps_MouseMove);
            this.Panel2.ResumeLayout(false);
            this.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel Panel2;

        private System.Windows.Forms.PictureBox PictureBox1;

        private System.Windows.Forms.Timer TimerRight1;

        private System.Windows.Forms.Timer TimerRight2;

        private System.Windows.Forms.Timer TimerLeft1;

        private System.Windows.Forms.Timer TimerLeft2;

        private System.Windows.Forms.Label Label2;

        private System.Windows.Forms.Label Label1;

        private System.ComponentModel.BackgroundWorker BackgroundWorker1;
        #endregion

        public System.Windows.Forms.FlowLayoutPanel papps;
    }
}