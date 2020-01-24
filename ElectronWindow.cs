using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;

namespace electron_host
{
    class ElectronWindow : HwndHost
    {
        IntPtr GetElectronHwnd()
        {
            var rootDir = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), @"..\..\..");

            // prepare to talk to the child electron process
            var hwndFile = Path.Combine(rootDir, "hwnd.txt");
            File.Delete(hwndFile);

            // start electron sub-process
            var electronExecutable = Path.Combine(rootDir, @"electron\electron.exe");
            var jsApp = Path.Combine(rootDir, @"app\index.js");
            using var electronProcess = Process.Start(electronExecutable, '"' + jsApp + '"');

            // get the electron main window hwnd - the index.js writes it.
            while (!File.Exists(hwndFile))
                Thread.Sleep(10);
            while (true)
            {
                var content = File.ReadAllText(hwndFile);
                if (!string.IsNullOrEmpty(content))
                    return new IntPtr(UInt32.Parse(content)); // this works only for 32 WPF process and 32 bit Electron process
            }
        }

        void ConvertToChildWindow(IntPtr hwnd)
        {
            // convert the "normal" window of Electron to a child one
            var style = GetWindowLong(hwnd, GWL_STYLE);
            style |= WS_CHILD;
            SetWindowLong(hwnd, GWL_STYLE, style);

            var styleEx = GetWindowLong(hwnd, GWL_EXSTYLE);
            styleEx &= ~WS_EX_APPWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, styleEx);
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
            ConvertToChildWindow(hwnd);
            SetParent(hwnd, hwndParent.Handle);

            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }
    }
}
