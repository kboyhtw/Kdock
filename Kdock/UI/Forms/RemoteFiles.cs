using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Kdock.Logic;

namespace Kdock
{
    public partial class RemoteFiles : Form
    {
        public TcpClient client;
        public LanSession Session;
        public string SourceFile = "";
        private bool _listMode = true;
        private ListView _details;
        private Label _status;
        private readonly List<ExplorerEntry> _rows = new List<ExplorerEntry>();
        private int _sortColumn;
        private bool _sortAscending = true;

        public RemoteFiles()
        {
            InitializeComponent();
        }

        public void LoadThis(string ip, string machineName)
        {
            this.Name = "frm" + ip;
            this.Text = machineName + " (" + ip + ")";
            this.Tag = ip;
            lblTitle.Text = this.Text;
        }

        public void LoadFiles(string rawData)
        {
            FlowLayoutPanel1.Controls.Clear();
            _rows.Clear();
            _rows.AddRange(ExplorerListing.Read(rawData));
            foreach (ExplorerEntry row in _rows)
            {
                if (row.Folder)
                {
                    ucFolder item = new ucFolder();
                    item.LoadMe(row.Path, client);
                    FlowLayoutPanel1.Controls.Add(item);
                    item.ApplyView(false, RowWidth());
                }
                else
                {
                    ucFIle item = new ucFIle();
                    item.LoadMe(row.Path);
                    FlowLayoutPanel1.Controls.Add(item);
                    item.ApplyView(false, RowWidth());
                }
            }
            SortRows();
            FillDetails();
            ApplyFileView();
            if (_status != null)
                _status.Text = _rows.Count + " item" + (_rows.Count == 1 ? "" : "s");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string upPath = ExplorerListing.Parent(Label1.Text);
            Label1.Text = upPath;
            SendBrowse(upPath);
        }

        private void SendBrowse(string path)
        {
            FileCommands.Browse(Session, path);
        }

        private void SendTransfer(object tag)
        {
            FileCommands.Transfer(Session, tag, Label1.Text);
        }

        private void MoveHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendTransfer(MoveHereToolStripMenuItem.Tag);
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendTransfer(PasteToolStripMenuItem.Tag);
        }

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshDirectory();
        }

        public void RefreshDirectory()
        {
            SendBrowse(Label1.Text);
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = Cursor.Position;
        }

        private void presize_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int width = Cursor.Position.X - this.Location.X;
                int height = Cursor.Position.Y - this.Location.Y;
                this.Width = width;
                this.Height = height;
            }
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PasteFromThisPCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsFileDropList())
            {
                string source = Clipboard.GetFileDropList()[0];
                if (Session != null)
                    Session.SendFile(source);
            }
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            PasteFromThisPCToolStripMenuItem.Visible = Clipboard.ContainsFileDropList();
        }

        private void RemoteFiles_Load(object sender, EventArgs e)
        {
            StyleExplorer();
        }

        private void StyleExplorer()
        {
            BackColor = Color.FromArgb(32, 32, 32);
            Font = new Font("Segoe UI", 9F);
            Button2.BackColor = Color.FromArgb(32, 32, 32);
            presize.BackColor = Color.FromArgb(0, 103, 192);
            presize.BringToFront();
            EnsureDetails();
            MouseMove += PictureBox1_MouseMove;
            lblTitle.MouseMove += PictureBox1_MouseMove;
            pnlHeader.MouseMove += PictureBox1_MouseMove;
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            _listMode = true;
            btnDetails.BackColor = Color.FromArgb(0, 103, 192);
            btnIcons.BackColor = Color.FromArgb(50, 50, 50);
            ApplyFileView();
        }

        private void btnIcons_Click(object sender, EventArgs e)
        {
            _listMode = false;
            btnDetails.BackColor = Color.FromArgb(50, 50, 50);
            btnIcons.BackColor = Color.FromArgb(0, 103, 192);
            ApplyFileView();
        }

        private int RowWidth()
        {
            return Math.Max(220, FlowLayoutPanel1.ClientSize.Width - 28);
        }

        private void ApplyFileView()
        {
            if (_details != null)
                _details.Visible = _listMode;
            FlowLayoutPanel1.Visible = !_listMode;
            if (_status != null)
                _status.Visible = true;
        }

        private void lblTransfers_Click(object sender, EventArgs e)
        {
            bool any = false;
            foreach (Form frm in Application.OpenForms)
            {
                frmFileStatus status = frm as frmFileStatus;
                if (status == null || status.IsDisposed) continue;
                status.Show();
                status.WindowState = FormWindowState.Normal;
                status.BringToFront();
                any = true;
            }
            if (!any)
                MessageBox.Show("No file transfers are running.");
        }

        private void EnsureDetails()
        {
            if (_details != null) return;
            _status = lblStatus;
            _details = lvFiles;
            ImageList icons = new ImageList();
            icons.ColorDepth = ColorDepth.Depth32Bit;
            icons.ImageSize = new Size(16, 16);
            _details.SmallImageList = icons;
            _details.OwnerDraw = true;
            _details.Font = new Font("Segoe UI", 9.5F);
            _details.ColumnClick += delegate (object sender, ColumnClickEventArgs e)
            {
                if (_sortColumn == e.Column) _sortAscending = !_sortAscending;
                else { _sortColumn = e.Column; _sortAscending = true; }
                SortRows();
                FillDetails();
            };
            _details.DoubleClick += delegate { OpenSelected(); };
            _details.DrawColumnHeader += DrawHeader;
            _details.DrawItem += delegate (object sender, DrawListViewItemEventArgs e) { };
            _details.DrawSubItem += DrawCell;
            _details.Resize += delegate
            {
                if (_details.Columns.Count > 0)
                    _details.Columns[0].Width = Math.Max(160, _details.ClientSize.Width - 390);
            };
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Open", null, delegate { OpenSelected(); });
            menu.Items.Add("Copy to this PC", null, delegate { CopySelected(); });
            menu.Items.Add("Delete", null, delegate { DeleteSelected(); });
            menu.Items.Add("Refresh", null, delegate { RefreshDirectory(); });
            _details.ContextMenuStrip = menu;
            lblStatus.BringToFront();
        }

        private void DrawHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(43, 43, 43)), e.Bounds);
            string text = e.Header.Text;
            if (e.ColumnIndex == _sortColumn)
                text += _sortAscending ? "  ↑" : "  ↓";
            TextRenderer.DrawText(e.Graphics, text, _details.Font, e.Bounds, Color.FromArgb(200, 200, 200), TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void DrawCell(object sender, DrawListViewSubItemEventArgs e)
        {
            Color back = e.Item.Selected ? Color.FromArgb(0, 90, 158) : Color.FromArgb(32, 32, 32);
            using (SolidBrush brush = new SolidBrush(back))
                e.Graphics.FillRectangle(brush, e.Bounds);
            Rectangle text = e.Bounds;
            if (e.ColumnIndex == 0)
            {
                Image image = null;
                if (_details.SmallImageList != null && _details.SmallImageList.Images.ContainsKey(e.Item.ImageKey))
                    image = _details.SmallImageList.Images[e.Item.ImageKey];
                if (image != null)
                    e.Graphics.DrawImage(image, e.Bounds.X + 6, e.Bounds.Y + 4, 16, 16);
                text = new Rectangle(e.Bounds.X + 28, e.Bounds.Y, e.Bounds.Width - 32, e.Bounds.Height);
            }
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.Left;
            if (e.ColumnIndex == 3) flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, _details.Font, text, Color.White, flags);
        }

        private void SortRows()
        {
            ExplorerListing.Sort(_rows, _sortColumn, _sortAscending);
        }

        private void FillDetails()
        {
            if (_details == null) return;
            _details.BeginUpdate();
            _details.Items.Clear();
            foreach (ExplorerEntry row in _rows)
            {
                string key = row.Folder ? "folder" : Path.GetExtension(row.Name);
                if (string.IsNullOrEmpty(key)) key = ".file";
                if (!_details.SmallImageList.Images.ContainsKey(key))
                {
                    Image image = row.Folder ? ShellIcons.ForFolder() : ShellIcons.ForFile(row.Name);
                    if (image != null) _details.SmallImageList.Images.Add(key, image);
                }
                ListViewItem item = new ListViewItem(row.Name);
                item.ImageKey = key;
                item.Tag = row;
                item.SubItems.Add(row.HasDate ? row.Modified.ToString("M/d/yyyy h:mm tt") : "");
                item.SubItems.Add(row.TypeName);
                item.SubItems.Add(row.SizeText);
                _details.Items.Add(item);
            }
            _details.EndUpdate();
            if (_details.Columns.Count > 0)
                _details.Columns[0].Width = Math.Max(160, _details.ClientSize.Width - 390);
        }

        private ExplorerEntry SelectedRow()
        {
            if (_details == null || _details.SelectedItems.Count == 0) return null;
            return _details.SelectedItems[0].Tag as ExplorerEntry;
        }

        private void OpenSelected()
        {
            ExplorerEntry row = SelectedRow();
            if (row == null || !row.Folder) return;
            Label1.Text = row.Path;
            SendBrowse(row.Path);
        }

        private void CopySelected()
        {
            ExplorerEntry row = SelectedRow();
            if (row == null || row.Folder || Session == null) return;
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Copy " + row.Name + " to this PC";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                Session.IncomingFileDirectory = dialog.SelectedPath;
                FileCommands.GetFile(Session, row.Path);
            }
        }

        private void DeleteSelected()
        {
            ExplorerEntry row = SelectedRow();
            if (row == null || Session == null) return;
            if (MessageBox.Show("Delete " + row.Name + "?", "Kdock", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            FileCommands.Delete(Session, row.Folder, row.Path);
            RefreshDirectory();
        }

        // Threaded startup support if needed elsewhere:
        public static void LaunchWindow(object param)
        {
            RemoteFiles rf = param as RemoteFiles;
            if (rf != null) Application.Run(rf);
        }
    }
}