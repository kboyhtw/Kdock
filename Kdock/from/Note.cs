using SpeechLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class Note : Form
    {
        public Note()
        {
            InitializeComponent();
        }

        private bool load = false;

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            int ax = 1;
            int ay = 1;
            if (e.Button == MouseButtons.Left)
            {
                ax = Cursor.Position.X - this.Location.X;
                ay = Cursor.Position.Y - this.Location.Y;
                this.Width = ax;
                this.Height = ay;
                SaveChanges();
            }
        }

        private void Panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = Cursor.Position;
                SaveChanges();
            }
        }

        private void Note_Load(object sender, EventArgs e)
        {
            this.Opacity = KGlobal.kdockmain.Opacity;
            this.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            txtnote.BackColor = KGlobal.kdockmain.PanelMain.BackColor;
            txtnote.ForeColor = KGlobal.kdockmain.Label4.ForeColor;
            lblAttached.ForeColor = KGlobal.kdockmain.Label4.ForeColor;
            LoadMe();
        }

        public void LoadMe()
        {
            this.Location = KGlobal.setting.npoint;
            this.Width = KGlobal.setting.nsize.Width;
            this.txtnote.Font = new System.Drawing.Font("Segoe UI", float.Parse(KGlobal.setting.nFS.ToString()), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            lblAttached.Text = KGlobal.setting.nattached;
            load = true;
            txtnote.Text = KGlobal.setting.ndata;
            load = false;
            Timeropen.Enabled = true;
        }

        private void txtnote_TextChanged(object sender, EventArgs e)
        {
            if (load == false)
                SaveChanges();
        }

        private void SaveChanges()
        {
            if (this.Location.X > Screen.PrimaryScreen.Bounds.Width - this.Width)
                this.Location = new Point(Screen.GetWorkingArea(new Point(0, 0)).Width - this.Width, this.Location.Y);
            else if (this.Location.X < 0)
            {
                this.Location = new Point(0, this.Location.Y);
                this.Size = new Size(this.Width - this.Location.X, this.Height);
            }

            if (this.Location.Y > Screen.PrimaryScreen.Bounds.Height - this.Height)
                this.Location = new Point(this.Location.X, Screen.GetWorkingArea(new Point(0, 0)).Height - this.Height);
            else if (this.Location.Y < 0)
            {
                this.Location = new Point(this.Location.X, 0);
                this.Size = new Size(this.Width, this.Height - this.Location.Y);
            }

            if (this.Width > Screen.GetWorkingArea(new Point(0, 0)).Width - 10)
                this.Width = Screen.GetWorkingArea(new Point(0, 0)).Width - 10;
            else if (this.Width <= 50)
                this.Width = 50;

            if (this.Height > Screen.GetWorkingArea(new Point(0, 0)).Height - 10)
                this.Height = Screen.GetWorkingArea(new Point(0, 0)).Height - 10;
            else if (this.Height <= 50)
                this.Height = 50;

            KGlobal.setting.npoint = this.Location;
            KGlobal.setting.ndata = txtnote.Text;
            KGlobal.setting.nsize = this.Size;
                  
            KGlobal.setting.Save(KGlobal.path);
            // If File.Exists(path + "\note.k") Then
            // File.SetAttributes(path + "\note.k", IO.FileAttributes.Normal)
            // File.WriteAllText(path + "\note.k", Me.Location.X & "ᾧ" & Me.Location.Y & "ᾧ" & Me.Width & "ᾧ" & Me.Height & "ᾧ" & nFS & "ᾧ" & nattached & "ᾧ" & txtnote.Text)
            // File.SetAttributes(path + "\note.k", IO.FileAttributes.System Or IO.FileAttributes.ReadOnly Or IO.FileAttributes.Hidden)
            // Else
            // File.WriteAllText(path + "\note.k", Me.Location.X & "ᾧ" & Me.Location.Y & "ᾧ" & Me.Width & "ᾧ" & Me.Height & "ᾧ" & nFS & "ᾧ" & nattached & "ᾧ" & txtnote.Text)
            // File.SetAttributes(path + "\note.k", IO.FileAttributes.System Or FileAttributes.ReadOnly Or IO.FileAttributes.Hidden)
            // End If
            if (KGlobal.setting.nattached.Length > 0)
                File.WriteAllText(KGlobal.setting.nattached, txtnote.Text);
        }

        private void txtnote_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.Data.GetDataPresent(DataFormats.FileDrop)))
                e.Effect = DragDropEffects.All;
        }

        private void txtnote_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] MyFiles;
                int i;

                // Assign the files to an array.
                MyFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Loop through the array and add the files to the list.
                for (i = 0; i <= MyFiles.Length - 1; i++)
                {
                    if (MyFiles[i].Contains(".txt"))
                    {
                        txtnote.Text += File.ReadAllText(MyFiles[i]);
                        lblAttached.Text = MyFiles[i];
                        KGlobal.setting.nattached = MyFiles[i];
                    }
                }
                SaveChanges();
            }
        }

        private void RemoveAttachedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KGlobal.setting.nattached = "";
            lblAttached.Text = "";
            SaveChanges();
        }

        private void RemoveAttachedFileAndClearContentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KGlobal.setting.nattached = "";
            lblAttached.Text = "";
            txtnote.Text = "";
            SaveChanges();
        }

        private void cmsAtt_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (KGlobal.setting.nattached.Length == 0)
                e.Cancel = true;
        }

        private void cmsNote_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (txtnote.CanUndo)
                cmsNote.Items["Undo"].Enabled = true;
            else
                cmsNote.Items["Undo"].Enabled = false;

            // Disable Cut, Copy and Delete if any text is not selected in TextBox
            if (txtnote.SelectedText.Length == 0)
            {
                cmsNote.Items["Cut"].Enabled = false;
                cmsNote.Items["Copy"].Enabled = false;
                cmsNote.Items["Delete"].Enabled = false;
                cmsNote.Items["SSearch"].Enabled = false;
            }
            else
            {
                cmsNote.Items["Cut"].Enabled = true;
                cmsNote.Items["Copy"].Enabled = true;
                cmsNote.Items["Delete"].Enabled = true;
                cmsNote.Items["SSearch"].Enabled = true;
            }

            // Disable Paste if Clipboard does not contains text
            if (Clipboard.ContainsText())
                cmsNote.Items["Paste"].Enabled = true;
            else
                cmsNote.Items["Paste"].Enabled = false;

            // Disable Select All if TextBox is blank
            if (txtnote.Text.Length == 0)
                cmsNote.Items["SelectAll"].Enabled = false;
            else
                cmsNote.Items["SelectAll"].Enabled = true;
        }

        private void SSearch_Click(object sender, EventArgs e)
        {
            KGlobal.search(txtnote.SelectedText);
        }

        private void Undo_Click(object sender, EventArgs e)
        {
            txtnote.Undo();
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            txtnote.Cut();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            txtnote.Copy();
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            txtnote.Paste();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            int SelectionIndex = txtnote.SelectionStart;
            int SelectionCount = txtnote.SelectionLength;
            txtnote.Text = txtnote.Text.Remove(SelectionIndex, SelectionCount);
            txtnote.SelectionStart = SelectionIndex;
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            txtnote.SelectAll();
        }

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.txtnote.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            KGlobal.setting.nFS = 8.25;
            SaveChanges();
        }

        private void ToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            this.txtnote.Font = new System.Drawing.Font("Segoe UI", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            KGlobal.setting.nFS = 10.0;
            SaveChanges();
        }

        private void ToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            this.txtnote.Font = new System.Drawing.Font("Segoe UI", 12.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            KGlobal.setting.nFS = 12.0;
            SaveChanges();
        }

        private void ToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            this.txtnote.Font = new System.Drawing.Font("Segoe UI", 14.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            KGlobal.setting.nFS = 14.0;
            SaveChanges();
        }

        private void Font16ptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtnote.Font = new System.Drawing.Font("Segoe UI", 16.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
            KGlobal.setting.nFS = 16.0;
            SaveChanges();
        }
        private int i = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            Timerclose.Stop();
            i += 20;
            this.Height = 0 + i;
            if (this.Height >= KGlobal.setting.nsize.Height)
            {
                this.Height = KGlobal.setting.nsize.Height;
                Timeropen.Stop();
                i = 0;
            }
        }

        private void Timerclose_Tick(object sender, EventArgs e)
        {
            Timeropen.Stop();
            if (this.Height > 20)
                this.Height -= 20;
            else
            {
                Timerclose.Stop();
                this.Dispose();
                GC.Collect();
            }
        }

        private void SpeakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpVoice voice = new SpVoice();
            voice.Speak(txtnote.SelectedText);

        }

    }
}
