using System;
using System.Threading.Tasks;
using System.Windows;

namespace Hangman.Client.Infrastructure.Threading
{
    public sealed class WpfUiDispatcher : IUiDispatcher
    {
        public void RunAsync(Func<Task> operation)
        {
            if (operation == null)
            {
                return;
            }

            if (Application.Current == null ||
                Application.Current.Dispatcher.CheckAccess())
            {
                operation();
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(
                new Action(async () => await operation()));
        }
    }
}
