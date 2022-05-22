namespace Kdock
{
    partial class KToggleSwitch
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
            this.pToogle = new System.Windows.Forms.Panel();
            this.btnToogle = new System.Windows.Forms.Button();
            this.lblToogle = new System.Windows.Forms.Label();
            this.pToogle.SuspendLayout();
            this.SuspendLayout();
            // 
            // pToogle
            // 
            this.pToogle.BackColor = System.Drawing.Color.Transparent;
            this.pToogle.Controls.Add(this.btnToogle);
            this.pToogle.Controls.Add(this.lblToogle);
            this.pToogle.Location = new System.Drawing.Point(0, 0);
            this.pToogle.MaximumSize = new System.Drawing.Size(84, 26);
            this.pToogle.MinimumSize = new System.Drawing.Size(84, 26);
            this.pToogle.Name = "pToogle";
            this.pToogle.Size = new System.Drawing.Size(84, 26);
            this.pToogle.TabIndex = 22;
            // 
            // btnToogle
            // 
            this.btnToogle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToogle.BackColor = System.Drawing.Color.DarkGreen;
            this.btnToogle.FlatAppearance.BorderSize = 0;
            this.btnToogle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGreen;
            this.btnToogle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGreen;
            this.btnToogle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToogle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToogle.ForeColor = System.Drawing.Color.White;
            this.btnToogle.Location = new System.Drawing.Point(0, -2);
            this.btnToogle.Name = "btnToogle";
            this.btnToogle.Size = new System.Drawing.Size(40, 31);
            this.btnToogle.TabIndex = 22;
            this.btnToogle.Text = "On";
            this.btnToogle.UseVisualStyleBackColor = false;
            this.btnToogle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnToogle_MouseDown);
            this.btnToogle.MouseLeave += new System.EventHandler(this.Button5_MouseLeave);
            this.btnToogle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Button5_MouseMove);
            // 
            // lblToogle
            // 
            this.lblToogle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblToogle.AutoSize = true;
            this.lblToogle.BackColor = System.Drawing.Color.Gray;
            this.lblToogle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblToogle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblToogle.ForeColor = System.Drawing.Color.DimGray;
            this.lblToogle.Location = new System.Drawing.Point(2, 4);
            this.lblToogle.Name = "lblToogle";
            this.lblToogle.Size = new System.Drawing.Size(73, 18);
            this.lblToogle.TabIndex = 22;
            this.lblToogle.Text = "On    Off";
            this.lblToogle.Click += new System.EventHandler(this.Label1_Click);
            // 
            // KToggleSwitch
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.pToogle);
            this.Name = "KToggleSwitch";
            this.Size = new System.Drawing.Size(84, 26);
            this.pToogle.ResumeLayout(false);
            this.pToogle.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pToogle;
        private System.Windows.Forms.Button btnToogle;
        private System.Windows.Forms.Label lblToogle;

        #endregion
    }
}
