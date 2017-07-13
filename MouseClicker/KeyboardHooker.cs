﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace MouseClicker
{
    /// <summary>
    /// Keyboard and mouse hooks, and send inputs.
    /// Some codes are from other programmers.
    /// </summary>
    class KeyboardAndMouseHooksAndMessages
    {
        public event KeyEventHandler KeyDownEvent;
        public event KeyPressEventHandler KeyPressEvent;
        public event KeyEventHandler KeyUpEvent;

        //public event MouseEventHandler MouseDownEvent;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        static int hKeyboardHook = 0; //initialize keyboard hook handle


        //static int hMouseHook = 0; //initialize mouse hook handle


        //Const defines of low level windows hooks.
        public const int WH_KEYBOARD_LL = 13;
        //public const int WH_MOUSE_LL = 14;

        HookProc KeyboardHookProcedure; // Store Hook Procedure
        
        //HookProc MouseHookProcedure;



        /// <summary>
        /// Keyboard Hook Structure. See:https://msdn.microsoft.com/en-us/library/windows/desktop/ms644967.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;  //virtual key code
            public int scanCode; // scan code
            public int flags;  // flags
            public int time; // timestamp of the message
            public int dwExtraInfo; // Extra info
        }

        /*
         
        [StructLayout(LayoutKind.Sequential)]
        public class Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public Point pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        private static Point CurrentMousePoint;
        */
        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644990.aspx
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="lpfn"></param>
        /// <param name="hInstance"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644993.aspx
        /// </summary>
        /// <param name="idHook"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644974.aspx
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms683183.aspx
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        /// <summary>
        /// Avoid hook failure. see https://msdn.microsoft.com/en-us/library/windows/desktop/ms683199.aspx
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        public void SetHook()
        {
            // Set Hooks
            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                //MouseHookProcedure = new HookProc(MouseHookProc);

                //Keyboard Global Hook (Do not need reflection)
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);


                //hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProcedure, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

                //Keyboard Global Hook (need reflection)
                //hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);

                //If SetWindowsHookEx Failed
                if (hKeyboardHook == 0)
                {
                    ReleaseKeyboardHook();
                    throw new Exception("安装键盘钩子失败");
                }
            }
        }

        /// <summary>
        /// Release Hook
        /// </summary>
        public void ReleaseKeyboardHook()
        {
            bool retKeyboard = true;
            //bool retMouse = true;

            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }/*
            if (hMouseHook != 0)
            {
                retMouse = UnhookWindowsHookEx(hMouseHook);
            }*/

            if (!(retKeyboard)) throw new Exception("Unload Hook Failed!");
            //if (!(retMouse)) throw new Exception("Unload Mouse Hook Failed!");
        }

        /// <summary>
        /// See MS Documentation: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646316.aspx
        /// </summary>
        /// <param name="uVirtKey"></param>
        /// <param name="uScanCode"></param>
        /// <param name="lpbKeyState"></param>
        /// <param name="lpwTransKey"></param>
        /// <param name="fuState"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        /// <summary>
        /// Get Keyboard State. See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646299.aspx
        /// </summary>
        /// <param name="pbKeyState"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        /// <summary>
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646301.aspx
        /// </summary>
        /// <param name="vKey"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        //WM Keyboard Related Consts
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        /// <summary>
        /// Keyboard Hook Listener Callback
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                //Key Down
                if (KeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyDownEvent(this, e);
                }

                //Key Press
                if (KeyPressEvent != null && wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);

                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.vkCode, MyKeyboardHookStruct.scanCode, keyState, inBuffer, MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        KeyPressEvent(this, e);
                    }
                }

                //Key up
                if (KeyUpEvent != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyUpEvent(this, e);
                }

            }
            //return true if you want to cancel the message.
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        /*
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            MouseHookStruct mStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            if (nCode >= 0)
            {
                CurrentMousePoint = mStruct.pt;
            }
            return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }
        */

        ~KeyboardAndMouseHooksAndMessages()
        {
            ReleaseKeyboardHook();
        }

        #region "Send Inputs"
        // all codes below are not referenced.

        /// <summary>
        /// a single INPUT structure. See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646270.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Input
        {
            public Int32 type;
            public InputUnion data;
        }

        /// <summary>
        /// Use this struct to prevent incompatible between x86 and x64 system.
        /// In the INPUT structure, there is a union:
        /// <code>
        /// union {
        ///     MOUSEINPUT mi;
        ///     KEYBDINPUT ki;
        ///     HARDWAREINPUT hi;
        /// };
        /// </code>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            public HardwareInput Hardware;
            [FieldOffset(0)]
            public KeyboardInput Keyboard;
            [FieldOffset(0)]
            public MouseInput Mouse;
        }

        /// <summary>
        /// Hardware input structure. See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646269.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HardwareInput
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        /// <summary>
        /// Keyboard input structure. See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646271.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardInput
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        /// <summary>
        /// Mouse input structure. See:https://msdn.microsoft.com/en-us/library/windows/desktop/ms646273.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }


        /// <summary>
        /// Send Input. See:https://msdn.microsoft.com/en-us/library/windows/desktop/ms646310.aspx
        /// </summary>
        /// <param name="nInputs"></param>
        /// <param name="pInputs"></param>
        /// <param name="cbSize"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        /// <summary>
        /// Format GetLastError data. See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms679351.aspx
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="lpSource"></param>
        /// <param name="dwMessageId"></param>
        /// <param name="dwLanguageId"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nSize"></param>
        /// <param name="Arguments"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer, uint nSize, IntPtr Arguments);

        /// <summary>
        /// enum version of MOUSEEVENTF_* in C
        /// </summary>
        private enum MouseEventF
        {
            ABSOLUTE = 0x8000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            MOVE = 0x0001,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100,
            HWHEEL = 0x01000
        }

        // XButtons are defined in MOUSEINPUT structure's document.
        private const int XBUTTON1 = 0x0001;
        private const int XBUTTON2 = 0x0002;
        // INPUT consts are defined in SendInput functions' document.
        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;
        private const int INPUT_HARDWARE = 2;

        /// <summary>
        /// Mouse Buttons
        /// </summary>
        [Flags]
        public enum MouseButtons
        {
            Left = 1,
            Right = 2,
            Medium = 4,
            XB1 = 8,
            XB2 = 16
        }

        /// <summary>
        /// Process Mouse Move
        /// </summary>
        /// <param name="x">Offset X</param>
        /// <param name="y">Offset Y</param>
        public static void MouseMove(int x, int y, bool absolute = false)
        {
            Input i = new Input { type = INPUT_MOUSE };
            i.data.Mouse = new MouseInput { dx = x, dy = y };
            if (absolute)
                i.data.Mouse.dwFlags = i.data.Mouse.dwFlags | (int)MouseEventF.ABSOLUTE;
            Input[] inputs = new Input[] { i };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        /// <summary>
        /// Process Mouse Click
        /// </summary>
        /// <param name="btns">Mouse Buttons, instance of <c>MouseButtons</param>
        public static void MouseClick(MouseButtons btns)
        {
            List<Input> inpList = new List<Input>();
            if ((btns & MouseButtons.Left) == MouseButtons.Left)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.LEFTDOWN | (int)MouseEventF.LEFTUP };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.Right) == MouseButtons.Right)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.RIGHTDOWN | (int)MouseEventF.RIGHTUP };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.Medium) == MouseButtons.Medium)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.MIDDLEDOWN | (int)MouseEventF.MIDDLEUP };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.XB1) == MouseButtons.XB1)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.XDOWN | (int)MouseEventF.XUP, mouseData = XBUTTON1 };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.XB2) == MouseButtons.XB2)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.XDOWN | (int)MouseEventF.XUP, mouseData = XBUTTON2 };
                inpList.Add(i);
            }
            Input[] iarr = inpList.ToArray();
            uint ret = SendInput((uint)iarr.Length, iarr, Marshal.SizeOf(new Input()));
            if (ret == 0)
            {
                uint Lerr = (uint)Marshal.GetLastWin32Error();
                Debug.WriteLine(Lerr);
                StringBuilder sbuilder = new StringBuilder(2048);
                FormatMessage(0x1000, IntPtr.Zero, Lerr, 0x0C00, sbuilder, 2048, IntPtr.Zero);
                Debug.WriteLine(sbuilder.ToString());
            }
        }

        /// <summary>
        /// Process Mouse Down
        /// </summary>
        /// <param name="btns">Mouse Buttons, instance of <c>MouseButtons</c></param>
        public static void MouseDown(MouseButtons btns)
        {
            List<Input> inpList = new List<Input>();
            if ((btns & MouseButtons.Left) == MouseButtons.Left)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.LEFTDOWN };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.Right) == MouseButtons.Right)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.RIGHTDOWN };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.Medium) == MouseButtons.Medium)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.MIDDLEDOWN };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.XB1) == MouseButtons.XB1)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.XDOWN, mouseData = XBUTTON1 };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.XB2) == MouseButtons.XB2)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.XDOWN, mouseData = XBUTTON2 };
                inpList.Add(i);
            }
            Input[] iarr = inpList.ToArray();
            uint ret = SendInput((uint)iarr.Length, iarr, Marshal.SizeOf(new Input()));
            if (ret == 0)
            {
                uint Lerr = (uint)Marshal.GetLastWin32Error();
                Debug.WriteLine(Lerr);
                StringBuilder sbuilder = new StringBuilder(2048);
                FormatMessage(0x1000, IntPtr.Zero, Lerr, 0x0C00, sbuilder, 2048, IntPtr.Zero);
                Debug.WriteLine(sbuilder.ToString());
            }
        }

        /// <summary>
        /// Process Mouse Up
        /// </summary>
        /// <param name="btns">Mouse Buttons, instance of <c>MouseButtons</c></param>
        public static void MouseUp(MouseButtons btns)
        {
            List<Input> inpList = new List<Input>();
            if ((btns & MouseButtons.Left) == MouseButtons.Left)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.LEFTUP };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.Right) == MouseButtons.Right)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.RIGHTUP };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.Medium) == MouseButtons.Medium)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.MIDDLEUP };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.XB1) == MouseButtons.XB1)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.XUP, mouseData = XBUTTON1 };
                inpList.Add(i);
            }
            if ((btns & MouseButtons.XB2) == MouseButtons.XB2)
            {
                Input i = new Input { type = INPUT_MOUSE };
                i.data.Mouse = new MouseInput { dwFlags = (int)MouseEventF.XUP, mouseData = XBUTTON2 };
                inpList.Add(i);
            }
            Input[] iarr = inpList.ToArray();
            uint ret = SendInput((uint)iarr.Length, iarr, Marshal.SizeOf(new Input()));
            if (ret == 0)
            {
                uint Lerr = (uint)Marshal.GetLastWin32Error();
                Debug.WriteLine(Lerr);
                StringBuilder sbuilder = new StringBuilder(2048);
                FormatMessage(0x1000, IntPtr.Zero, Lerr, 0x0C00, sbuilder, 2048, IntPtr.Zero);
                Debug.WriteLine(sbuilder.ToString());
            }
        }
        #endregion
    }
}