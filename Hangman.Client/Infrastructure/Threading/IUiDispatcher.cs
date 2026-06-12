using System;
using System.Threading.Tasks;

namespace Hangman.Client.Infrastructure.Threading
{
    public interface IUiDispatcher
    {
        void RunAsync(Func<Task> operation);
    }
}
