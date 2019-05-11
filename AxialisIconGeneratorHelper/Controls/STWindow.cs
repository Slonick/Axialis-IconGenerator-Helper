#region Usings

using System.Windows;
using System.Windows.Input;
using AxialisIconGeneratorHelper.ViewModels.Base;

#endregion

namespace AxialisIconGeneratorHelper.Controls
{
    /// <inheritdoc />
    public class STWindow : Window
    {
        #region Public Fields

        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(nameof(HeaderHeight), typeof(GridLength), typeof(STWindow), new PropertyMetadata(new GridLength(30)));

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(STWindow), new PropertyMetadata(true));

        #endregion

        #region Public Properties

        public ICommand CloseCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand RestoreCommand { get; }

        public GridLength HeaderHeight
        {
            get => (GridLength) this.GetValue(HeaderHeightProperty);
            set => this.SetValue(HeaderHeightProperty, value);
        }

        public bool ShowIcon
        {
            get => (bool) this.GetValue(ShowIconProperty);
            set => this.SetValue(ShowIconProperty, value);
        }

        #endregion

        #region Static Constructors

        static STWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(STWindow),
                                                     new FrameworkPropertyMetadata(typeof(STWindow)));
        }

        #endregion

        #region Not Static Constructors

        public STWindow()
        {
            this.MinimizeCommand = new RelayCommand(() => SystemCommands.MinimizeWindow(this),
                                                    () => this.ResizeMode >= ResizeMode.CanMinimize);

            this.RestoreCommand = new RelayCommand(() => SystemCommands.RestoreWindow(this),
                                                   () => this.ResizeMode >= ResizeMode.CanResize);

            this.MaximizeCommand = new RelayCommand(() => SystemCommands.MaximizeWindow(this),
                                                    () => this.ResizeMode >= ResizeMode.CanResize);

            this.CloseCommand = new RelayCommand(() => SystemCommands.CloseWindow(this));
        }

        #endregion
    }
}