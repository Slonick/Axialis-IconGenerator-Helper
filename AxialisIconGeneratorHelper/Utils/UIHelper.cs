#region Usings

using System.Windows;
using System.Windows.Media;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class UIHelper
    {
        #region Public Methods

        public static T GetParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                var parent = VisualTreeHelper.GetParent(child);

                switch (parent)
                {
                    case null:
                        return null;
                    case T tParent:
                        return tParent;
                    default:
                        child = parent;
                        continue;
                }
            }
        }

        #endregion
    }
}