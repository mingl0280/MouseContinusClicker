﻿using System;

namespace MouseClicker
{
    /// <summary>
    /// Extended event args for message transmitting.
    /// </summary>
    public class MsgReceivedEventArgs: EventArgs
    {
        public int Msg { get; set; }

        /// <summary>
        /// initializer
        /// </summary>
        /// <param name="i">message</param>
        public MsgReceivedEventArgs(int i)
        {
            Msg = i;
        }
    }
}
