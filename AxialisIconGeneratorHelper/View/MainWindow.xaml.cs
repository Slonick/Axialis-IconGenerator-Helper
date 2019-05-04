#region Usings

using System.Windows;
using AxialisIconGeneratorHelper.Utils;
using AxialisIconGeneratorHelper.ViewModels;

#endregion

namespace AxialisIconGeneratorHelper.View
{
    public partial class MainWindow
    {
        #region Not Static Constructors

        public MainWindow()
        {
            this.InitializeComponent();

            var vm = new MainViewModel();

            this.DataContext = vm;
            this.ShowInTaskbar = false;
            this.WindowState = WindowState.Minimized;

            this.Loaded += (sender, args) =>
            {
                WindowUtils.HideWindowFromAltTab(this);
                vm.Init();
            };
        }

        #endregion
    }
}