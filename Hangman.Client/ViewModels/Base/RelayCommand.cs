using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels.Base
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;
        private readonly Func<object, Task> executeAsync;
        private bool isExecuting;

        public RelayCommand(Action execute)
            : this(parameter => execute(), null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(
                  parameter => execute(),
                  canExecute == null ? null : new Predicate<object>(parameter => canExecute()))
        {
        }

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public RelayCommand(Func<Task> executeAsync)
            : this(parameter => executeAsync(), null)
        {
        }

        public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute)
            : this(
                  parameter => executeAsync(),
                  canExecute == null ? null : new Predicate<object>(parameter => canExecute()))
        {
        }

        public RelayCommand(Func<object, Task> executeAsync)
            : this(executeAsync, null)
        {
        }

        public RelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute)
        {
            this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (isExecuting)
            {
                return false;
            }

            return canExecute == null || canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                isExecuting = true;
                RaiseCanExecuteChanged();

                if (executeAsync != null)
                {
                    await executeAsync(parameter);
                    return;
                }

                execute(parameter);
            }
            finally
            {
                isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
