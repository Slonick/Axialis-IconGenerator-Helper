#region Usings

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

#endregion

namespace AxialisIconGeneratorHelper.Utils
{
    public static class UIHelper
    {
        #region Public Methods

        public static void Execute(Action action) => Application.Current.Dispatcher.Invoke(action);

        public static async Task ExecuteAsync(Action action) => await Application.Current.Dispatcher.InvokeAsync(action);

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