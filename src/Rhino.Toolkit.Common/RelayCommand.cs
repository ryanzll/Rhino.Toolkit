using System;
using System.Windows.Input;

namespace Rhino.Toolkit.Common
{
    public class RelayCommand<T> : ICommand where T : class
    {
        private readonly Predicate<T> _CanExecute;
        private readonly Action<T> _Execute;

        public RelayCommand(Predicate<T> canExecute, Action<T> execute)
        {
            this._CanExecute = canExecute;
            this._Execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute(parameter as T);
        }

        public void Execute(object parameter)
        {
            _Execute(parameter as T);
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<bool> _CanExecute;
        private readonly Action _Execute;

        public RelayCommand(Func<bool> canExecute, Action execute)
        {
            this._CanExecute = canExecute;
            this._Execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute();
        }

        public void Execute(object parameter)
        {
            _Execute();
        }
    }

}
