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
    public partial class KToggleSwitch : UserControl
    {
        public KToggleSwitch()
        {
            InitializeComponent();
        }

        private Point t;
        private bool bMove;

        private void btnToogle_MouseDown(object sender, MouseEventArgs e)
        {
            t = e.Location;
        }

        private void Button5_MouseLeave(object sender, EventArgs e)
        {
            if (btnToogle.Location.X >= 25)
            {
                btnToogle.Location = new Point(39, btnToogle.Location.Y);
                btnToogle.Text = "Off";
                bToggled = false;
            }
            else if (btnToogle.Location.X < 25)
            {
                btnToogle.Location = new Point(0, btnToogle.Location.Y);
                btnToogle.Text = "On";
                bToggled = true;
            }
        }

        private void Button5_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left & btnToogle.Location.X >= 0 & btnToogle.Location.X <= 39)
                btnToogle.Location = new Point(btnToogle.Location.X - t.X + e.Location.X, btnToogle.Location.Y);
            else if (MouseButtons == MouseButtons.None & btnToogle.Location.X >= 25)
            {
                btnToogle.Location = new Point(39, btnToogle.Location.Y);
                btnToogle.Text = "Off";
                bToggled = false;
            }
            else if (MouseButtons == System.Windows.Forms.MouseButtons.None & btnToogle.Location.X < 25)
            {
                btnToogle.Location = new Point(0, btnToogle.Location.Y);
                btnToogle.Text = "On";
                bToggled = true;
            }
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            if (btnToogle.Location.X < 25)
                // For i As Double = -2 To 39 Step 0.5
                // If (i = i / 1) Then
                // btnToogle.Location = New Point(i, btnToogle.Location.Y)
                // pToogle.Refresh()
                // End If
                // Next
                // btnToogle.Text = "Off"
                Toggled = false;
            else if (btnToogle.Location.X > 25)
                // For i As Double = 39 To -2 Step -0.5
                // If (i = i / 1) Then
                // btnToogle.Location = New Point(i, btnToogle.Location.Y)
                // pToogle.Refresh()
                // End If
                // Next
                // btnToogle.Text = "On"
                Toggled = true;
        }
        private bool bToggled = false;

        public bool Toggled
        {
            get
            {
                return bToggled;
            }
            set
            {
                bToggled = value;
                if (bToggled & btnToogle.Location.X > 25)
                {
                    for (double i = 39; i >= -2; i += -0.5)
                    {
                        if ((i == i / 1))
                        {
                            btnToogle.Location = new Point(Convert.ToInt32(i), btnToogle.Location.Y);
                            pToogle.Refresh();
                        }
                    }
                    btnToogle.Text = "On";
                }
                else if (bToggled == false & btnToogle.Location.X < 25)
                {
                    for (double i = -2; i <= 39; i += 0.5)
                    {
                        if ((i == i / 1))
                        {
                            btnToogle.Location = new Point(Convert .ToInt32 ( i), btnToogle.Location.Y);
                            pToogle.Refresh();
                        }
                    }
                    btnToogle.Text = "Off";
                }
                ToggleChanged?.Invoke();
            }
        }

        public event ToggleChangedEventHandler ToggleChanged;

        public delegate void ToggleChangedEventHandler();

        public Color SwitchBackColor
        {
            get
            {
                return btnToogle.BackColor;
            }
            set
            {
                btnToogle.BackColor = value;
                btnToogle.FlatAppearance.MouseDownBackColor = value;
                btnToogle.FlatAppearance.MouseOverBackColor = value;
            }
        }

        public Color SwitchForeColor
        {
            get
            {
                return btnToogle.ForeColor;
            }
            set
            {
                btnToogle.ForeColor = value;
            }
        }
    }
}
