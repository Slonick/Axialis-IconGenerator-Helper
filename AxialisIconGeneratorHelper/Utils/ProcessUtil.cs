#region Usings

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class ProcessUtil
    {
        #region Public Methods

        public static string GetProcessNameById(IntPtr hWnd) =>
            GetWindowThreadProcessId(hWnd, out var processId) == 0
                ? string.Empty
                : Process.GetProcessById(processId).ProcessName;

        #endregion

        #region Private Methods

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        #endregion
    }
}