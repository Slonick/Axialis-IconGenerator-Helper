#region Usings

using System;
using Microsoft.Win32;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class AxialisUtils
    {
        #region Private Fields

        private static readonly RegistryKey IconGeneratorKey;

        #endregion

        #region Static Constructors

        static AxialisUtils()
        {
            IconGeneratorKey = Registry.LocalMachine?.OpenSubKey(Environment.Is64BitOperatingSystem
                                                                     ? @"SOFTWARE\WOW6432Node\Axialis\IconGenerator"
                                                                     : @"SOFTWARE\Axialis\IconGenerator");
        }

        #endregion

        #region Public Methods

        public static string GetIconGeneratorPath()
            => IconGeneratorKey?.GetValue("InstallDir") as string;

        public static bool IconGeneratorIsInstalled()
            => IconGeneratorKey != null;

        #endregion
    }
}