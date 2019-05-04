#region Usings

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class HotKey
    {
        #region Public Constants

        public const int WmHotKey = 0x0312;

        #endregion

        #region Private Fields

        private static Dictionary<int, Action> dictHotKeyToCalBackProc;

        #endregion

        #region Public Methods

        public static void Register(Key key, KeyModifier keyModifiers, Action action)
        {
            var virtualKeyCode = KeyInterop.VirtualKeyFromKey(key);
            var id = virtualKeyCode + (int) keyModifiers * 0x10000;
            RegisterHotKey(IntPtr.Zero, id, (uint) keyModifiers, (uint) virtualKeyCode);

            if (dictHotKeyToCalBackProc == null)
            {
                dictHotKeyToCalBackProc = new Dictionary<int, Action>();
                ComponentDispatcher.ThreadFilterMessage += ComponentDispatcherThreadFilterMessage;
            }

            dictHotKeyToCalBackProc.Add(id, action);
        }

        #endregion

        #region Private Methods

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (handled ||
                msg.message != WmHotKey ||
                !dictHotKeyToCalBackProc.TryGetValue((int) msg.wParam, out var action))
                return;

            action?.Invoke();
            handled = true;
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        #endregion
    }

    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}