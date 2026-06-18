using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessCountdownController :
        IMatchGuessCountdownController
    {
        private CancellationTokenSource cancellationTokenSource;
        private bool disposed;

        public void Start(
            DateTime? guessTurnEndsAt,
            bool isFinished,
            Action<int> updateRemainingSeconds,
            Func<Task> timeoutOperation)
        {
            Cancel();

            if (disposed ||
                isFinished ||
                !guessTurnEndsAt.HasValue ||
                updateRemainingSeconds == null ||
                timeoutOperation == null)
            {
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            _ = RunCountdownAsync(
                guessTurnEndsAt.Value,
                updateRemainingSeconds,
                timeoutOperation,
                cancellationTokenSource.Token);
        }

        public void Cancel()
        {
            if (cancellationTokenSource == null)
            {
                return;
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            Cancel();
        }

        private static async Task RunCountdownAsync(
            DateTime guessTurnEndsAt,
            Action<int> updateRemainingSeconds,
            Func<Task> timeoutOperation,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int seconds = CalculateRemainingSeconds(guessTurnEndsAt);

                    updateRemainingSeconds(seconds);

                    if (seconds <= 0)
                    {
                        await timeoutOperation();
                        return;
                    }

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the countdown is cancelled.
            }
            catch (ObjectDisposedException)
            {
                // Expected when the countdown is disposed while the delay is active.
            }
        }

        private static int CalculateRemainingSeconds(DateTime endsAt)
        {
            double seconds = (endsAt - DateTime.UtcNow).TotalSeconds;

            if (seconds <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(seconds);
        }
    }
}
