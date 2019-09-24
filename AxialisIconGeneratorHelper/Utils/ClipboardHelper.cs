#region Usings

using System.Threading;
using System.Windows;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class ClipboardHelper
    {
        #region Public Methods

        public static bool SetText(string text, int retryTimes = 10, int retryDelay = 10)
        {
            for (var i = 0; i < retryTimes; i++)
            {
                try
                {
                    Clipboard.SetDataObject(text);
                    return true;
                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(retryDelay);
            }

            return false;
        }

        #endregion
    }
}