#region Usings

using System;
using System.Linq;
using System.Reflection;

#endregion

namespace AxialisIconGeneratorHelper.Extensions
{
    public static class AssemblyExtension
    {
        #region Public Methods

        public static T GetAssemblyAttribute<T>(this Assembly assembly) where T : Attribute =>
            (T) assembly?.GetCustomAttributes(typeof(T), true).FirstOrDefault();

        #endregion
    }
}