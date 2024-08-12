using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Autoclicker
{
    public partial class Form1 : Form
    {
        [DllImport("User32.dll")]
        public static extern uint SendInput(uint cInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            internal int type;
            internal MOUSEINPUT mi;
        }

        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_VIRTUALDESK = 0x4000;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public Form1()
        {
            InitializeComponent();
        }

        Point cursorpos = new Point();
        int ticks = 0;
        int rx, ry, cx, cy = 0;

        INPUT[] mousedown = new INPUT[1];
        INPUT[] mouseup = new INPUT[1];

        private void initializeclick()
        {
            mousedown[0].mi.dx = 0;
            mousedown[0].mi.dy = 0;
            mousedown[0].mi.mouseData = 0;
            mousedown[0].mi.dwFlags = MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_VIRTUALDESK | MOUSEEVENTF_ABSOLUTE;
            mousedown[0].mi.time = 0;
            mousedown[0].mi.dwExtraInfo = UIntPtr.Zero;

            mouseup[0].type = 0;
            mouseup[0].mi.dx = 0;
            mouseup[0].mi.dy = 0;
            mouseup[0].mi.mouseData = 0;
            mouseup[0].mi.dwFlags = MOUSEEVENTF_LEFTUP | MOUSEEVENTF_VIRTUALDESK | MOUSEEVENTF_ABSOLUTE;
            mouseup[0].mi.time = 0;
            mouseup[0].mi.dwExtraInfo = UIntPtr.Zero;
        }

        private void clickdown()
        {
            SendInput(1, mousedown, Marshal.SizeOf(typeof(INPUT)));
        }

        private void clickup()
        {
            SendInput(1, mouseup, Marshal.SizeOf(typeof(INPUT)));
        }

        private void enablecontrols(bool status)
        {
            customcoords.Enabled = trackptr.Enabled = radiusx.Enabled = drawcircle.Enabled = sendclicks.Enabled = status;

            if (status == false)
            {
                cursorX.Enabled = cursorY.Enabled = false;
                radiusy.Enabled = false;
            }

            if (status == true && trackptr.Checked == true)
            {
                cursorX.Enabled = cursorY.Enabled = false;
            }
            else if (status == true && trackptr.Checked == false)
            {
                cursorX.Enabled = cursorY.Enabled = true;
            }
            
            if (status == true && drawcircle.Checked == true)
            {
                radiusy.Enabled = false;
            }
            else if (status == true && drawcircle.Checked == false)
            {
                radiusy.Enabled = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (customcoords.Checked == true)
            {
                cursorX.Enabled = cursorY.Enabled = true;
                tracktimer.Stop();
                label1.Text = "Enter desired coordinates";
                this.KeyPreview = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (trackptr.Checked == true)
            {
                cursorX.Enabled = cursorY.Enabled = false;
                tracktimer.Start();
                label1.Text = "[Space] to lock coordinates";
                this.KeyPreview = true;
                this.ActiveControl = null;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            cursorX.Text = Cursor.Position.X.ToString();
            cursorY.Text = Cursor.Position.Y.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            customcoords.Checked = true;
            cursorX.Maximum = Screen.PrimaryScreen.Bounds.Width;
            cursorY.Maximum = Screen.PrimaryScreen.Bounds.Height;
            radiusx.Maximum = cursorX.Maximum / 2;
            radiusy.Maximum = cursorY.Maximum / 2;
            initializeclick();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this.ActiveControl = null;
                if (tracktimer.Enabled == true && trackptr.Checked == true)
                {
                    tracktimer.Stop();
                    label1.Text = "[Space] to unlock coordinates";
                }
                else if (tracktimer.Enabled == false && trackptr.Checked == true)
                {
                    tracktimer.Start();
                    label1.Text = "[Space] to lock coordinates";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tracktimer.Enabled == true) //check if tracking is on
            {
                MessageBox.Show("An ellipse cannot be drawn when coordinates are not locked.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.ActiveControl = null;
            }
            else
            {
                Point point = new Point();

                cx = (int)cursorX.Value;
                cy = (int)cursorY.Value;
                rx = (int)radiusx.Value;
                ry = (int)radiusy.Value;

                point.X = (int)Math.Round((double)cx + rx);
                point.Y = cy; //set up cursor position

                DialogResult result = MessageBox.Show("Caution: You are about to draw the most beautiful ellipse that the world has ever seen.\n\nMake sure the window you want to draw on is DIRECTLY underneath the app window and the app window is out of the way of the ellipse's path.\n\nOnce you start drawing, you cannot stop.\n\nDo you wish to continue?", "Caution", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    enablecontrols(false);
                    Cursor.Position = point;
                    clickdown();
                    circletimer.Start();
                    label8.Visible = true;
                }
            }

            this.ActiveControl = null;
        }
        
        private void Form1_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            MessageBox.Show("Ellipse-inator v1 (Build 2024.08)\n\nCheck out the project on GitHub!\n\nDon't know what Length (X) or Length (Y) mean?\nClick on their labels to find out!", "About", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void drawcircle_CheckedChanged(object sender, EventArgs e)
        {
            if (drawcircle.Checked == true)
            {
                radiusy.Enabled = false;
                if (radiusx.Maximum < radiusy.Maximum)
                {
                    radiusy.Maximum = radiusx.Maximum;
                    radiusy.Value = radiusx.Value;
                }
                else
                {
                    if (radiusx.Value > radiusy.Maximum)
                    {
                        radiusx.Value = radiusy.Maximum;
                    }
                    radiusx.Maximum = radiusy.Maximum;
                    radiusy.Value = radiusx.Value;
                }
            }
            else
            {
                radiusy.Enabled = true;
                radiusx.Maximum = cursorX.Maximum / 2;
                radiusy.Maximum = cursorY.Maximum / 2;
            }

            this.ActiveControl = null;
        }

        private void label4_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("The length of the horizontal radius of the ellipse or the radius of the circle if the option to draw a circle is checked.", "Definition", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void label7_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            MessageBox.Show("The length of the vertical radius of the ellipse. This value is forced to be equal to the length of the horizontal radius if the option to draw a circle is checked.", "Definition", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void radiusx_ValueChanged(object sender, EventArgs e)
        {
            if (drawcircle.Checked == true)
            {
                radiusy.Value = radiusx.Value;
            }
        }

        private void radiusx_Leave(object sender, EventArgs e)
        {
            radiusx.Text = radiusx.Value.ToString();
        }

        private void radiusy_Leave(object sender, EventArgs e)
        {
            radiusy.Text = radiusy.Value.ToString();
        }

        private void radiusx_KeyUp(object sender, KeyEventArgs e)
        {
            if (drawcircle.Checked == true)
            {
                if (radiusx.Value < radiusy.Maximum)
                {
                    radiusy.Value = radiusx.Value;
                }
                else
                {
                    radiusx.Value = radiusy.Maximum;
                    radiusy.Value = radiusy.Maximum;
                }
            }
        }

        private void tracktimer_Tick(object sender, EventArgs e)
        {
            cursorX.Value = Cursor.Position.X;
            cursorY.Value = Cursor.Position.Y;
        }

        private void circletimer_Tick(object sender, EventArgs e)
        {
            if (ticks <= 360)
            {
                cursorpos.X = (int) Math.Round(rx * Math.Cos((double)ticks * Math.PI / 180) + cx);
                cursorpos.Y = (int) Math.Round(ry * Math.Sin((double)ticks * Math.PI / 180) + cy);
                Cursor.Position = cursorpos;
                ticks++;
            }
            else
            {
                clickup();
                circletimer.Stop();
                enablecontrols(true);
                label8.Visible = false;
                ticks = 0;
            }
        }
    }
}
