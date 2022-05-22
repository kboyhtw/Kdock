namespace Kdock
{
    partial class ucFolder
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
            this.components = new System.ComponentModel.Container();
            this.Button1 = new System.Windows.Forms.Button();
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.DeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Button2 = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ContextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Button1
            // 
            this.Button1.ContextMenuStrip = this.ContextMenuStrip1;
            this.Button1.FlatAppearance.BorderSize = 3;
            this.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button1.Location = new System.Drawing.Point(15, 15);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(51, 29);
            this.Button1.TabIndex = 0;
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DeleteToolStripMenuItem,
            this.CopyToolStripMenuItem,
            this.MoveToolStripMenuItem});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(108, 70);
            // 
            // DeleteToolStripMenuItem
            // 
            this.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem";
            this.DeleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.DeleteToolStripMenuItem.Text = "Delete";
            // 
            // CopyToolStripMenuItem
            // 
            this.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem";
            this.CopyToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.CopyToolStripMenuItem.Text = "Copy";
            // 
            // MoveToolStripMenuItem
            // 
            this.MoveToolStripMenuItem.Name = "MoveToolStripMenuItem";
            this.MoveToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.MoveToolStripMenuItem.Text = "Move";
            // 
            // Button2
            // 
            this.Button2.FlatAppearance.BorderSize = 3;
            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button2.Location = new System.Drawing.Point(47, 10);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(19, 10);
            this.Button2.TabIndex = 1;
            this.Button2.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(-1, 54);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(81, 26);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "Label1";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Button1);
            this.Name = "ucFolder";
            this.Size = new System.Drawing.Size(80, 80);
            this.ContextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Button Button1;

        private System.Windows.Forms.Button Button2;

        private System.Windows.Forms.Label Label1;

        private System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;

        private System.Windows.Forms.ToolStripMenuItem DeleteToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem CopyToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem MoveToolStripMenuItem;

        private System.Windows.Forms.ToolTip ToolTip1;

        #endregion
    }
}
