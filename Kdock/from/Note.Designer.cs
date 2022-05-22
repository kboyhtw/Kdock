namespace Kdock
{
    partial class Note
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
            this.txtnote = new System.Windows.Forms.TextBox();
            this.cmsNote = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.Undo = new System.Windows.Forms.ToolStripMenuItem();
            this.Cut = new System.Windows.Forms.ToolStripMenuItem();
            this.Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.FontSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.Font16ptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SpeakToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.presize = new System.Windows.Forms.Panel();
            this.pmove = new System.Windows.Forms.Panel();
            this.lblAttached = new System.Windows.Forms.Label();
            this.cmsAtt = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RemoveAttachedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoveAttachedFileAndClearContentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Timeropen = new System.Windows.Forms.Timer(this.components);
            this.Timerclose = new System.Windows.Forms.Timer(this.components);
            this.Panel1 = new System.Windows.Forms.Panel();
            this.cmsNote.SuspendLayout();
            this.cmsAtt.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtnote
            // 
            this.txtnote.AllowDrop = true;
            this.txtnote.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtnote.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtnote.ContextMenuStrip = this.cmsNote;
            this.txtnote.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtnote.Location = new System.Drawing.Point(4, 22);
            this.txtnote.Multiline = true;
            this.txtnote.Name = "txtnote";
            this.txtnote.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtnote.Size = new System.Drawing.Size(646, 270);
            this.txtnote.TabIndex = 0;
            this.txtnote.TextChanged += new System.EventHandler(this.txtnote_TextChanged);
            // 
            // cmsNote
            // 
            this.cmsNote.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SSearch,
            this.Undo,
            this.Cut,
            this.Copy,
            this.Paste,
            this.Delete,
            this.SelectAll,
            this.FontSizeToolStripMenuItem,
            this.SpeakToolStripMenuItem});
            this.cmsNote.Name = "cmsNote";
            this.cmsNote.ShowImageMargin = false;
            this.cmsNote.Size = new System.Drawing.Size(140, 202);
            // 
            // SSearch
            // 
            this.SSearch.Name = "SSearch";
            this.SSearch.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.SSearch.Size = new System.Drawing.Size(139, 22);
            this.SSearch.Text = "Search ";
            this.SSearch.Click += new System.EventHandler(this.SSearch_Click);
            // 
            // Undo
            // 
            this.Undo.Name = "Undo";
            this.Undo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.Undo.Size = new System.Drawing.Size(139, 22);
            this.Undo.Text = "Undo";
            this.Undo.Click += new System.EventHandler(this.Undo_Click);
            // 
            // Cut
            // 
            this.Cut.Name = "Cut";
            this.Cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.Cut.Size = new System.Drawing.Size(139, 22);
            this.Cut.Text = "Cut";
            this.Cut.Click += new System.EventHandler(this.Cut_Click);
            // 
            // Copy
            // 
            this.Copy.Name = "Copy";
            this.Copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.Copy.Size = new System.Drawing.Size(139, 22);
            this.Copy.Text = "Copy";
            this.Copy.Click += new System.EventHandler(this.Copy_Click);
            // 
            // Paste
            // 
            this.Paste.Name = "Paste";
            this.Paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.Paste.Size = new System.Drawing.Size(139, 22);
            this.Paste.Text = "Paste";
            this.Paste.Click += new System.EventHandler(this.Paste_Click);
            // 
            // Delete
            // 
            this.Delete.Name = "Delete";
            this.Delete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.Delete.Size = new System.Drawing.Size(139, 22);
            this.Delete.Text = "Delete";
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // SelectAll
            // 
            this.SelectAll.Name = "SelectAll";
            this.SelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.SelectAll.Size = new System.Drawing.Size(139, 22);
            this.SelectAll.Text = "Select All";
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // FontSizeToolStripMenuItem
            // 
            this.FontSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem2,
            this.ToolStripMenuItem4,
            this.ToolStripMenuItem6,
            this.ToolStripMenuItem7,
            this.Font16ptToolStripMenuItem});
            this.FontSizeToolStripMenuItem.Name = "FontSizeToolStripMenuItem";
            this.FontSizeToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.FontSizeToolStripMenuItem.Text = "Font Size";
            // 
            // ToolStripMenuItem2
            // 
            this.ToolStripMenuItem2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolStripMenuItem2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolStripMenuItem2.Name = "ToolStripMenuItem2";
            this.ToolStripMenuItem2.Size = new System.Drawing.Size(168, 34);
            this.ToolStripMenuItem2.Text = "Font 8pt";
            this.ToolStripMenuItem2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolStripMenuItem2.Click += new System.EventHandler(this.ToolStripMenuItem2_Click);
            // 
            // ToolStripMenuItem4
            // 
            this.ToolStripMenuItem4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolStripMenuItem4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            this.ToolStripMenuItem4.Size = new System.Drawing.Size(168, 34);
            this.ToolStripMenuItem4.Text = "Font 10pt";
            this.ToolStripMenuItem4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolStripMenuItem4.Click += new System.EventHandler(this.ToolStripMenuItem4_Click);
            // 
            // ToolStripMenuItem6
            // 
            this.ToolStripMenuItem6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolStripMenuItem6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolStripMenuItem6.Name = "ToolStripMenuItem6";
            this.ToolStripMenuItem6.Size = new System.Drawing.Size(168, 34);
            this.ToolStripMenuItem6.Text = "Font12pt";
            this.ToolStripMenuItem6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolStripMenuItem6.Click += new System.EventHandler(this.ToolStripMenuItem6_Click);
            // 
            // ToolStripMenuItem7
            // 
            this.ToolStripMenuItem7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolStripMenuItem7.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolStripMenuItem7.Name = "ToolStripMenuItem7";
            this.ToolStripMenuItem7.Size = new System.Drawing.Size(168, 34);
            this.ToolStripMenuItem7.Text = "Font 14pt";
            this.ToolStripMenuItem7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolStripMenuItem7.Click += new System.EventHandler(this.ToolStripMenuItem7_Click);
            // 
            // Font16ptToolStripMenuItem
            // 
            this.Font16ptToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Font16ptToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Font16ptToolStripMenuItem.Name = "Font16ptToolStripMenuItem";
            this.Font16ptToolStripMenuItem.Size = new System.Drawing.Size(168, 34);
            this.Font16ptToolStripMenuItem.Text = "Font 16pt";
            this.Font16ptToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Font16ptToolStripMenuItem.Click += new System.EventHandler(this.Font16ptToolStripMenuItem_Click);
            // 
            // SpeakToolStripMenuItem
            // 
            this.SpeakToolStripMenuItem.Name = "SpeakToolStripMenuItem";
            this.SpeakToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.SpeakToolStripMenuItem.Text = "Speak";
            this.SpeakToolStripMenuItem.Click += new System.EventHandler(this.SpeakToolStripMenuItem_Click);
            // 
            // presize
            // 
            this.presize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.presize.BackColor = System.Drawing.Color.Black;
            this.presize.Cursor = System.Windows.Forms.Cursors.Cross;
            this.presize.Location = new System.Drawing.Point(637, 278);
            this.presize.Name = "presize";
            this.presize.Size = new System.Drawing.Size(17, 17);
            this.presize.TabIndex = 1;
            this.presize.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Panel1_MouseMove);
            // 
            // pmove
            // 
            this.pmove.BackColor = System.Drawing.Color.Red;
            this.pmove.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.pmove.Location = new System.Drawing.Point(1, 1);
            this.pmove.Name = "pmove";
            this.pmove.Size = new System.Drawing.Size(17, 17);
            this.pmove.TabIndex = 2;
            this.pmove.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Panel2_MouseMove);
            // 
            // lblAttached
            // 
            this.lblAttached.AutoSize = true;
            this.lblAttached.Location = new System.Drawing.Point(24, 5);
            this.lblAttached.Name = "lblAttached";
            this.lblAttached.Size = new System.Drawing.Size(16, 13);
            this.lblAttached.TabIndex = 3;
            this.lblAttached.Text = "   ";
            // 
            // cmsAtt
            // 
            this.cmsAtt.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RemoveAttachedFileToolStripMenuItem,
            this.RemoveAttachedFileAndClearContentToolStripMenuItem});
            this.cmsAtt.Name = "cmsAtt";
            this.cmsAtt.ShowImageMargin = false;
            this.cmsAtt.Size = new System.Drawing.Size(266, 48);
            // 
            // RemoveAttachedFileToolStripMenuItem
            // 
            this.RemoveAttachedFileToolStripMenuItem.Name = "RemoveAttachedFileToolStripMenuItem";
            this.RemoveAttachedFileToolStripMenuItem.Size = new System.Drawing.Size(265, 22);
            this.RemoveAttachedFileToolStripMenuItem.Text = "Remove Attached File";
            this.RemoveAttachedFileToolStripMenuItem.Click += new System.EventHandler(this.RemoveAttachedFileToolStripMenuItem_Click);
            // 
            // RemoveAttachedFileAndClearContentToolStripMenuItem
            // 
            this.RemoveAttachedFileAndClearContentToolStripMenuItem.Name = "RemoveAttachedFileAndClearContentToolStripMenuItem";
            this.RemoveAttachedFileAndClearContentToolStripMenuItem.Size = new System.Drawing.Size(265, 22);
            this.RemoveAttachedFileAndClearContentToolStripMenuItem.Text = "Remove Attached File And Clear Content";
            this.RemoveAttachedFileAndClearContentToolStripMenuItem.Click += new System.EventHandler(this.RemoveAttachedFileAndClearContentToolStripMenuItem_Click);
            // 
            // Timeropen
            // 
            this.Timeropen.Interval = 1;
            this.Timeropen.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // Timerclose
            // 
            this.Timerclose.Interval = 1;
            this.Timerclose.Tick += new System.EventHandler(this.Timerclose_Tick);
            // 
            // Panel1
            // 
            this.Panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel1.Location = new System.Drawing.Point(633, 12);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(18, 280);
            this.Panel1.TabIndex = 4;
            // 
            // Note
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 295);
            this.ContextMenuStrip = this.cmsAtt;
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.lblAttached);
            this.Controls.Add(this.txtnote);
            this.Controls.Add(this.presize);
            this.Controls.Add(this.pmove);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Note";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Note";
            this.Load += new System.EventHandler(this.Note_Load);
            this.cmsNote.ResumeLayout(false);
            this.cmsAtt.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel presize;

        private System.Windows.Forms.Panel pmove;

        private System.Windows.Forms.ContextMenuStrip cmsAtt;

        private System.Windows.Forms.ToolStripMenuItem RemoveAttachedFileToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem RemoveAttachedFileAndClearContentToolStripMenuItem;

        private System.Windows.Forms.ContextMenuStrip cmsNote;

        private System.Windows.Forms.ToolStripMenuItem SSearch;

        private System.Windows.Forms.ToolStripMenuItem Cut;

        private System.Windows.Forms.ToolStripMenuItem Copy;

        private System.Windows.Forms.ToolStripMenuItem Paste;

        private System.Windows.Forms.ToolStripMenuItem Delete;

        private System.Windows.Forms.ToolStripMenuItem SelectAll;

        private System.Windows.Forms.ToolStripMenuItem FontSizeToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem2;

        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem4;

        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem6;

        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem7;

        private System.Windows.Forms.ToolStripMenuItem Font16ptToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem Undo;

        private System.Windows.Forms.Timer Timeropen;

        private System.Windows.Forms.ToolStripMenuItem SpeakToolStripMenuItem;

        private System.Windows.Forms.Panel Panel1;

        #endregion

        public System.Windows.Forms.TextBox txtnote;
        public System.Windows.Forms.Label lblAttached;
        public System.Windows.Forms.Timer Timerclose;
    }
}