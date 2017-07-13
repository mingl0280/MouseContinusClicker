#define KP
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace MouseClicker
{
    public partial class Form1 : Form
    {
        private static KeyboardAndMouseHooksAndMessages hook = new KeyboardAndMouseHooksAndMessages();
        private static List<MouseClickThread> ClickingThreads = new List<MouseClickThread>();
        
        /// <summary>
        /// initializer.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            //hook.KeyPressEvent += new KeyPressEventHandler(GlobalKeyPressHandler);
            hook.KeyDownEvent += new KeyEventHandler(GlobalKeyDownHandler);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hook.SetHook();
        }
#if !KP
        private void GlobalKeyPressHandler(object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine(e.KeyChar);
        }
#endif
        /// <summary>
        /// Keyboard listen for start and stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GlobalKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F10)
            {
                StartClick();
                return;
            }
            if (e.KeyCode == Keys.F12)
            {
                StopClick();
                return;
            }
            
        }

        /// <summary>
        /// Start simulated clicks
        /// </summary>
        private void StartClick()
        {
            KeyboardAndMouseHooksAndMessages.MouseButtons mb = new KeyboardAndMouseHooksAndMessages.MouseButtons();
            if (checkBox1.Checked)
                mb = mb | KeyboardAndMouseHooksAndMessages.MouseButtons.Left;
            if (checkBox2.Checked)
                mb = mb | KeyboardAndMouseHooksAndMessages.MouseButtons.Right;
            if (checkBox3.Checked)
                mb = mb | KeyboardAndMouseHooksAndMessages.MouseButtons.Medium;
            if (checkBox4.Checked)
                mb = mb | KeyboardAndMouseHooksAndMessages.MouseButtons.XB1;
            if (checkBox5.Checked)
                mb = mb | KeyboardAndMouseHooksAndMessages.MouseButtons.XB2;
            MouseClickThread MCT = new MouseClickThread(mb, Convert.ToInt32(numericUpDown1.Value));
            
            ClickingThreads.Add(MCT);
            return;
        }

        /// <summary>
        /// stop simulated clicks
        /// </summary>
        private void StopClick()
        {
            foreach(MouseClickThread MCT in ClickingThreads)
            {
                MCT.RaiseMsgEvent(0);
            }
            ClickingThreads.Clear();
        }
        
        
        ~Form1()
        {
            hook.ReleaseKeyboardHook();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartClick();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopClick();
        }
        
    }
}
