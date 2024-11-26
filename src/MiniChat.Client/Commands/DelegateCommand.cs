using System;
using System.Windows.Input;

namespace MiniComm.Client.Commands
{
    /// <summary>
    /// 委托命令
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private bool canExecuteCache;

        private Func<bool> canExecuteFunction;
        private Action executeFunction;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action execute) : this(execute, null) { }
        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            executeFunction = execute;
            canExecuteFunction = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteFunction == null)
            {
                return true;
            }
            bool result = canExecuteFunction.Invoke();
            if (result != canExecuteCache)
            {
                canExecuteCache = result;
                //CanExecuteChanged?.Invoke(parameter, EventArgs.Empty);
            }
            return result;
        }
        public void Execute(object parameter)
        {
            executeFunction?.Invoke();
        }
    }

    /// <summary>
    /// 泛型委托命令
    /// </summary>
    public class DelegateCommand<T> : ICommand
    {
        private bool canExecuteCache;

        private Func<T, bool> canExecuteFunction;
        private Action<T> executeFunction;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action<T> execute) : this(execute, null) { }
        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            canExecuteCache = false;
            executeFunction = execute;
            canExecuteFunction = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteFunction == null || !(parameter is T))
            {
                return true;
            }
            bool result = canExecuteFunction.Invoke((T)parameter);
            if (result != canExecuteCache)
            {
                canExecuteCache = result;
                //CanExecuteChanged?.Invoke(parameter, EventArgs.Empty);
            }
            return result;
        }
        public void Execute(object parameter)
        {
            if (parameter is T)
            {
                executeFunction?.Invoke((T)parameter);
            }
        }
    }
}