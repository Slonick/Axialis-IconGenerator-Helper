#region Usings

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class InputUtils
    {
        #region Public Methods

        public static IntPtr FocusedControlInActiveWindow()
        {
            var activeWindowHandle = GetForegroundWindow();

            var activeWindowThread = GetWindowThreadProcessId(activeWindowHandle, IntPtr.Zero);
            if (Application.Current.MainWindow == null) return IntPtr.Zero;

            var windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            var thisWindowThread = GetWindowThreadProcessId(windowHandle, IntPtr.Zero);

            AttachThreadInput(activeWindowThread, thisWindowThread, true);
            var focusedControlHandle = GetFocus();
            AttachThreadInput(activeWindowThread, thisWindowThread, false);

            return focusedControlHandle;
        }

        public static string GetText(IntPtr handle)
        {
            const uint wmGetTextLength = 0x000E;
            const uint wmGetText = 0x000D;

            var length = (int) SendMessage(handle, wmGetTextLength, IntPtr.Zero, null);
            var sb = new StringBuilder(length + 1);
            SendMessage(handle, wmGetText, (IntPtr) sb.Capacity, sb);
            return sb.ToString();
        }

        #endregion

        #region Private Methods

        [DllImport("user32.dll")]
        private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, [Out] StringBuilder lParam);

        #endregion
    }
}