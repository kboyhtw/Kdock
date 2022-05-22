namespace Kdock
{
    partial class web
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
            this.pmove = new System.Windows.Forms.Panel();
            this.txtsearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblClose = new System.Windows.Forms.Label();
            this.Timerclose = new System.Windows.Forms.Timer(this.components);
            this.Timeropen = new System.Windows.Forms.Timer(this.components);
            this.ContextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddUrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flpUrls = new System.Windows.Forms.FlowLayoutPanel();
            this.paddurl = new System.Windows.Forms.Panel();
            this.lblCancel = new Kdock.Klabel();
            this.lblSave = new Kdock.Klabel();
            this.txturl = new System.Windows.Forms.TextBox();
            this.txturlname = new System.Windows.Forms.TextBox();
            this.lblUrl = new System.Windows.Forms.Label();
            this.lblname = new System.Windows.Forms.Label();
            this.lblShowurls = new Kdock.Klabel();
            this.ContextMenuStrip1.SuspendLayout();
            this.paddurl.SuspendLayout();
            this.SuspendLayout();
            // 
            // pmove
            // 
            this.pmove.BackColor = System.Drawing.Color.Red;
            this.pmove.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.pmove.Location = new System.Drawing.Point(0, 0);
            this.pmove.Name = "pmove";
            this.pmove.Size = new System.Drawing.Size(10, 10);
            this.pmove.TabIndex = 3;
            this.pmove.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pmove_MouseMove);
            // 
            // txtsearch
            // 
            this.txtsearch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtsearch.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtsearch.Location = new System.Drawing.Point(17, 12);
            this.txtsearch.Name = "txtsearch";
            this.txtsearch.Size = new System.Drawing.Size(388, 29);
            this.txtsearch.TabIndex = 5;
            this.txtsearch.Text = "www.google.com";
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSearch.Location = new System.Drawing.Point(413, 15);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(57, 21);
            this.lblSearch.TabIndex = 6;
            this.lblSearch.Text = "Search";
            this.lblSearch.Click += new System.EventHandler(this.lblSearch_Click);
            this.lblSearch.MouseLeave += new System.EventHandler(this.lblSearch_mouseL);
            this.lblSearch.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblSearch_MouseMove);
            // 
            // lblClose
            // 
            this.lblClose.AutoSize = true;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClose.Location = new System.Drawing.Point(476, -1);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(16, 20);
            this.lblClose.TabIndex = 7;
            this.lblClose.Text = "x";
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            // 
            // Timerclose
            // 
            this.Timerclose.Interval = 1;
            this.Timerclose.Tick += new System.EventHandler(this.Timerclose_Tick);
            // 
            // Timeropen
            // 
            this.Timeropen.Interval = 1;
            this.Timeropen.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // ContextMenuStrip1
            // 
            this.ContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddUrlToolStripMenuItem});
            this.ContextMenuStrip1.Name = "ContextMenuStrip1";
            this.ContextMenuStrip1.Size = new System.Drawing.Size(115, 26);
            // 
            // AddUrlToolStripMenuItem
            // 
            this.AddUrlToolStripMenuItem.Name = "AddUrlToolStripMenuItem";
            this.AddUrlToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.AddUrlToolStripMenuItem.Text = "Add Url";
            this.AddUrlToolStripMenuItem.Click += new System.EventHandler(this.AddUrlToolStripMenuItem_Click);
            // 
            // flpUrls
            // 
            this.flpUrls.ContextMenuStrip = this.ContextMenuStrip1;
            this.flpUrls.Location = new System.Drawing.Point(18, 60);
            this.flpUrls.Name = "flpUrls";
            this.flpUrls.Size = new System.Drawing.Size(456, 228);
            this.flpUrls.TabIndex = 9;
            // 
            // paddurl
            // 
            this.paddurl.Controls.Add(this.lblCancel);
            this.paddurl.Controls.Add(this.lblSave);
            this.paddurl.Controls.Add(this.txturl);
            this.paddurl.Controls.Add(this.txturlname);
            this.paddurl.Controls.Add(this.lblUrl);
            this.paddurl.Controls.Add(this.lblname);
            this.paddurl.Location = new System.Drawing.Point(14, 59);
            this.paddurl.Name = "paddurl";
            this.paddurl.Size = new System.Drawing.Size(464, 233);
            this.paddurl.TabIndex = 0;
            this.paddurl.Visible = false;
            // 
            // lblCancel
            // 
            this.lblCancel.AutoSize = true;
            this.lblCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCancel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCancel.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblCancel.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblCancel.Location = new System.Drawing.Point(210, 147);
            this.lblCancel.Name = "lblCancel";
            this.lblCancel.Size = new System.Drawing.Size(56, 21);
            this.lblCancel.TabIndex = 9;
            this.lblCancel.Text = "Cancel";
            this.lblCancel.Click += new System.EventHandler(this.lblCancel_Click);
            // 
            // lblSave
            // 
            this.lblSave.AutoSize = true;
            this.lblSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSave.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSave.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblSave.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblSave.Location = new System.Drawing.Point(143, 147);
            this.lblSave.Name = "lblSave";
            this.lblSave.Size = new System.Drawing.Size(43, 21);
            this.lblSave.TabIndex = 8;
            this.lblSave.Text = "Save";
            this.lblSave.Click += new System.EventHandler(this.lblSave_Click);
            // 
            // txturl
            // 
            this.txturl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txturl.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txturl.Location = new System.Drawing.Point(135, 106);
            this.txturl.Name = "txturl";
            this.txturl.Size = new System.Drawing.Size(258, 25);
            this.txturl.TabIndex = 7;
            // 
            // txturlname
            // 
            this.txturlname.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txturlname.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txturlname.Location = new System.Drawing.Point(135, 69);
            this.txturlname.Name = "txturlname";
            this.txturlname.Size = new System.Drawing.Size(258, 25);
            this.txturlname.TabIndex = 6;
            // 
            // lblUrl
            // 
            this.lblUrl.AutoSize = true;
            this.lblUrl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUrl.Location = new System.Drawing.Point(72, 110);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(38, 21);
            this.lblUrl.TabIndex = 1;
            this.lblUrl.Text = "Url :";
            // 
            // lblname
            // 
            this.lblname.AutoSize = true;
            this.lblname.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblname.Location = new System.Drawing.Point(72, 72);
            this.lblname.Name = "lblname";
            this.lblname.Size = new System.Drawing.Size(59, 21);
            this.lblname.TabIndex = 0;
            this.lblname.Text = "Name :";
            // 
            // lblShowurls
            // 
            this.lblShowurls.AutoSize = true;
            this.lblShowurls.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblShowurls.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblShowurls.HoverColor = System.Drawing.SystemColors.ControlText;
            this.lblShowurls.LeaveColor = System.Drawing.SystemColors.ControlText;
            this.lblShowurls.Location = new System.Drawing.Point(450, 40);
            this.lblShowurls.Name = "lblShowurls";
            this.lblShowurls.Size = new System.Drawing.Size(31, 13);
            this.lblShowurls.TabIndex = 10;
            this.lblShowurls.Text = "====";
            // 
            // web
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 11);
            this.Controls.Add(this.paddurl);
            this.Controls.Add(this.flpUrls);
            this.Controls.Add(this.lblClose);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtsearch);
            this.Controls.Add(this.pmove);
            this.Controls.Add(this.lblShowurls);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "web";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "web";
            this.Load += new System.EventHandler(this.web_Load);
            this.ContextMenuStrip1.ResumeLayout(false);
            this.paddurl.ResumeLayout(false);
            this.paddurl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel pmove;
        private System.Windows.Forms.TextBox txtsearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblClose;
        public  System.Windows.Forms.Timer Timerclose;
        private System.Windows.Forms.Timer Timeropen;
        private System.Windows.Forms.ContextMenuStrip ContextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem AddUrlToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flpUrls;
        private System.Windows.Forms.Panel paddurl;
        private Klabel lblSave;
        private System.Windows.Forms.TextBox txturl;
        private System.Windows.Forms.TextBox txturlname;
        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.Label lblname;
        private Klabel lblCancel;
        private Klabel lblShowurls;

        #endregion
    }
}