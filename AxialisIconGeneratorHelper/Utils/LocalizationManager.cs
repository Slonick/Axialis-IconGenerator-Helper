#region Usings

using System.Windows;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class LocalizationManager
    {
        #region Public Methods

        public static string GetLocalizationString(string key)
            => Application.Current.Resources.Contains(key) ? Application.Current.Resources[key] as string : key;

        #endregion
    }
}