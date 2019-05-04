#region Usings

using System;
using System.Windows.Input;

#endregion

namespace AxialisIconGeneratorHelper.ViewModels.Base
{
    public class RelayCommand : ICommand
    {
        #region Private Fields

        private readonly Func<bool> canExecute;
        private readonly Action execute;
        private bool isExecute;

        #endregion

        #region Not Static Constructors

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        #endregion

        #region Public Methods

        public bool CanExecute(object parameter) => this.canExecute == null || this.canExecute();

        public void Execute(object parameter)
        {
            if (this.isExecute) return;

            this.isExecute = true;
            this.execute?.Invoke();
            this.isExecute = false;
        }

        public void RaiseCanExecute() => CommandManager.InvalidateRequerySuggested();

        #endregion

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class RelayCommand<T> : ICommand
    {
        #region Private Fields

        private readonly Predicate<T> canExecute;
        private readonly Action<T> execute;
        private bool isExecute;

        #endregion

        #region Not Static Constructors

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Public Methods

        public bool CanExecute(object parameter) => this.canExecute == null || this.canExecute((T) parameter);

        public void Execute(object parameter)
        {
            if (this.isExecute) return;

            this.isExecute = true;
            this.execute?.Invoke((T) parameter);
            this.isExecute = false;
        }

        public void RaiseCanExecute() => CommandManager.InvalidateRequerySuggested();

        #endregion

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}