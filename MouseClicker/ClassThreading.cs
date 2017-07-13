using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MouseClicker
{
    public delegate void MsgHandlerDelegate(Object sender, MsgReceivedEventArgs e);
    class MouseClickThread
    {
        public event MsgHandlerDelegate OnMsgRevceivedEvent;

        private static int RunningStatus;
        private static int Delay;
        private static KeyboardAndMouseHooksAndMessages.MouseButtons btns;
        private Thread ClickThread = new Thread(new ThreadStart(DoClickThread));

        /// <summary>
        /// initializer, set buttons and delay, and start the thread.
        /// </summary>
        /// <param name="mbs"></param>
        /// <param name="delay"></param>
        public MouseClickThread(KeyboardAndMouseHooksAndMessages.MouseButtons mbs, int delay)
        {
            btns = mbs;
            Delay = delay;
            OnMsgRevceivedEvent += OnMsgRevceived;
            RunningStatus = 1;
            ClickThread.Start();
        }

        /// <summary>
        /// Message Received event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMsgRevceived(Object sender, MsgReceivedEventArgs e)
        {
            RunningStatus = e.msg;
        }

        /// <summary>
        /// Worker thread
        /// </summary>
        private static void DoClickThread()
        {
            while(true)
            {
                if (RunningStatus == 0) break;

                if (RunningStatus == 1)
                {
                    KeyboardAndMouseHooksAndMessages.MouseClick(btns);
                }
                Thread.Sleep(Delay);
            }
        }

        /// <summary>
        /// Trigger Event Handler
        /// </summary>
        /// <param name="e"></param>
        protected virtual void MsgReceivedEvent(MsgReceivedEventArgs e)
        {
            if (OnMsgRevceivedEvent != null)
            {
                OnMsgRevceived(this, e);
            }
        }

        /// <summary>
        /// Raise Event
        /// </summary>
        /// <param name="i"></param>
        public void RaiseMsgEvent(int i)
        {
            MsgReceivedEventArgs e = new MsgReceivedEventArgs(i);
            OnMsgRevceivedEvent.Invoke(this, e);
        }
        
    }

    /// <summary>
    /// Extended event args for message transmitting.
    /// </summary>
    public class MsgReceivedEventArgs: EventArgs
    {
        public int msg { get; set; }

        /// <summary>
        /// initializer
        /// </summary>
        /// <param name="i">message</param>
        public MsgReceivedEventArgs(int i)
        {
            msg = i;
        }
    }
}
