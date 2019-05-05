#region Usings

using System;
using System.Threading;
using System.Windows;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class ClipboardHelper
    {
        #region Public Methods

        public static void ClipboardSetTextSafely(string text)
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    break;
                }
                catch (Exception)
                {
                    // ignored
                }

                Thread.Sleep(10);
            }
        }

        #endregion
    }
}