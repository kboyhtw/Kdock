namespace Kdock
{
    partial class ucFIle
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
            this.CopyToThisPCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Button2 = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ContextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Button1
            // 
            this.Button1.ContextMenuStrip = this.ContextMenuStrip1;
            this.Button1.FlatAppearance.BorderSize = 3;
            this.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 2.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button1.Location = new System.Drawing.Point(20, 3);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(40, 45);
            this.Button1.TabIndex = 0;
            this.Button1.Text = "_________________________________________________________________________________" +
    "___";
            this.Button1.UseVisualStyleBackColor = true;
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DeleteToolStripMenuItem,
            this.CopyToolStripMenuItem,
            this.MoveToolStripMenuItem,
            this.CopyToThisPCToolStripMenuItem});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(181, 114);
            // 
            // DeleteToolStripMenuItem
            // 
            this.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem";
            this.DeleteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.DeleteToolStripMenuItem.Text = "Delete";
            this.DeleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // CopyToolStripMenuItem
            // 
            this.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem";
            this.CopyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.CopyToolStripMenuItem.Text = "Copy";
            this.CopyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // MoveToolStripMenuItem
            // 
            this.MoveToolStripMenuItem.Name = "MoveToolStripMenuItem";
            this.MoveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.MoveToolStripMenuItem.Text = "Move";
            this.MoveToolStripMenuItem.Click += new System.EventHandler(this.MoveToolStripMenuItem_Click);
            // 
            // CopyToThisPCToolStripMenuItem
            // 
            this.CopyToThisPCToolStripMenuItem.Name = "CopyToThisPCToolStripMenuItem";
            this.CopyToThisPCToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.CopyToThisPCToolStripMenuItem.Text = "Copy to this PC";
            this.CopyToThisPCToolStripMenuItem.Click += new System.EventHandler(this.CopyToThisPCToolStripMenuItem_Click);
            // 
            // Button2
            // 
            this.Button2.ContextMenuStrip = this.ContextMenuStrip1;
            this.Button2.FlatAppearance.BorderSize = 3;
            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button2.Location = new System.Drawing.Point(20, 3);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(40, 10);
            this.Button2.TabIndex = 1;
            this.Button2.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(-1, 54);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(81, 26);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "Label1";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label2
            // 
            this.Label2.ContextMenuStrip = this.ContextMenuStrip1;
            this.Label2.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(23, 13);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(33, 31);
            this.Label2.TabIndex = 4;
            this.Label2.Text = "OOOO";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label2.Visible = false;
            // 
            // ucFIle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Button1);
            this.Name = "ucFIle";
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
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.ToolTip ToolTip1;
        private System.Windows.Forms.ToolStripMenuItem CopyToThisPCToolStripMenuItem;

        #endregion
    }
}
