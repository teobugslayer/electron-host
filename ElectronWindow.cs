using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace electron_host
{
    class ElectronWindow : HwndHost
    {
        private Window parentWindow;
        
        public ElectronWindow(Window parentWindow)
        {
            this.parentWindow = parentWindow;
        }

        IntPtr GetElectronHwnd()
        {
            var rootDir = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), @"..\..");

            var hwndFile = Path.Combine(rootDir, "hwnd.txt");
            File.Delete(hwndFile);

            var electronExecutable = Path.Combine(rootDir, @"electron\electron.exe");
            var jsApp = Path.Combine(rootDir, @"app\index.js");
            var electronProcess = Process.Start(electronExecutable, '"' + jsApp + '"');

            while (!File.Exists(hwndFile))
                Thread.Sleep(10);
            while (true)
            {
                var content = File.ReadAllText(hwndFile);
                if (!string.IsNullOrEmpty(content))
                    return new IntPtr(UInt32.Parse(content));
            }
        }

        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;
        const uint WS_CHILD = 0x40000000;
        const uint WS_EX_APPWINDOW = 0x00040000;

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
        public static extern uint SetWindowLong(IntPtr hwnd, int index, uint newLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
        public static extern uint GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hwndChild, IntPtr hwdNewParent);

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var hwnd = GetElectronHwnd();

            // convert the "normal" window of Electron to a child one
            var style = GetWindowLong(hwnd, GWL_STYLE);
            style |= WS_CHILD;
            SetWindowLong(hwnd, GWL_STYLE, style);

            var styleEx = GetWindowLong(hwnd, GWL_EXSTYLE);
            styleEx &= ~WS_EX_APPWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, styleEx);

            // set our window to be a parent to Electron's one
            var helper = new WindowInteropHelper(parentWindow);
            var parentHwnd = helper.Handle;

            SetParent(hwnd, parentHwnd);

            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }
    }
}
