using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kdock
{
    public partial class ucChatFile : UserControl
    {
        public ucChatFile()
        {
            InitializeComponent();
        }
        FileObject _File;
        public FileObject File
        {
            get
            {
                return _File;
            }
            set
            {
                _File = value;
                textBox1.Text = _File.FileName;
                progressLite1.Maximum = File.TotalLength;
                progressLite1.Value = File.RecivedData;
            }
        }

        private void ucChatFile_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            if (File != null)
            {

                progressLite1.Value = File.RecivedData;
                lblStatus.Text = (Convert.ToDouble(File.RecivedData / 1024) / 1024).ToString("0.00") + "/" + (Convert.ToDouble(File.TotalLength / 1024) / 1024).ToString("0.00") + "Mb  " + ((File.Speed / 1024) / 1024).ToString("00.00") + "Mb/s";
                this.Refresh();
            }
        }
    }
}
