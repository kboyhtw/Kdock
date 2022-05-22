using System;

namespace Kdock
{
    partial class ProgressLite
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
            this.Panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Panel1.BackColor = System.Drawing.Color.Red;
            this.Panel1.Location = new System.Drawing.Point(0, 0);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(0, 2);
            this.Panel1.TabIndex = 0;
            // 
            // ProgressLite
            // 
            this.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Controls.Add(this.Panel1);
            this.Size = new System.Drawing.Size(200, 2);
            this.Resize += new System.EventHandler(this.ProgressLite_Resize);
            this.ResumeLayout(false);

        }

        private Int64 iMax = 100;

        public Int64 Maximum
        {
            get
            {
                return iMax;
            }
            set
            {
                iMax = value;
            }
        }

        private Int64 iVal = 0;

        public Int64 Value
        {
            get
            {
                return iVal;
            }
            set
            {
                iVal = value;
                this.Panel1.Size = new System.Drawing.Size(System.Convert.ToInt32((Convert.ToDouble(this.Width )/ Convert.ToDouble(iMax)) * Convert.ToDouble( iVal)), this.Height);
            }
        }

        private System.Windows.Forms.Panel Panel1;

        #endregion
    }
}
