#region Usings

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class WindowUtils
    {
        #region Public Enums

        [Flags]
        public enum ExtendedWindowStyles
        {
            WsExToolWindow = 0x00000080
        }

        public enum GetWindowLongFields
        {
            GwlExStyle = -20
        }

        #endregion

        #region Public Methods

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static void HideWindowFromAltTab(Window window)
        {
            var wndHelper = new WindowInteropHelper(window);

            var exStyle = (int) GetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GwlExStyle);

            exStyle |= (int) ExtendedWindowStyles.WsExToolWindow;
            SetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GwlExStyle, (IntPtr) exStyle);
        }

        public static void RestoreWindowFromAltTab(Window window)
        {
            var wndHelper = new WindowInteropHelper(window);

            var exStyle = (int) GetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GwlExStyle);

            exStyle &= (int) ExtendedWindowStyles.WsExToolWindow;
            SetWindowLong(wndHelper.Handle, (int) GetWindowLongFields.GwlExStyle, (IntPtr) exStyle);
        }

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error;
            IntPtr result;
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                var tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if (result == IntPtr.Zero && error != 0) throw new Win32Exception(error);

            return result;
        }

        #endregion

        #region Private Methods

        private static int IntPtrToInt32(IntPtr intPtr) => unchecked((int) intPtr.ToInt64());

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        private static extern void SetLastError(int dwErrorCode);

        #endregion
    }
}