using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Match
{
    internal interface IMatchGuessNotificationController : IDisposable
    {
        void Configure(
            Func<int> getCurrentMatchId,
            Func<Task> refreshGameStateAsync,
            Func<MatchLobbyClosedEventArgs, Task> handleLobbyClosedAsync,
            Action<MatchChatMessageModel> handleChatMessage,
            Action setRuntimeError,
            Action setUnexpectedError);

        Task<bool> SubscribeToLobbyAsync(int matchId);

        Task UnsubscribeFromLobbyAsync(int matchId);
    }
}
