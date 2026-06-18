using System;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Match
{
    internal interface IMatchGuessCountdownController : IDisposable
    {
        void Start(
            DateTime? guessTurnEndsAt,
            bool isFinished,
            Action<int> updateRemainingSeconds,
            Func<Task> timeoutOperation);

        void Cancel();
    }
}
