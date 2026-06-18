using Hangman.Client.Services.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.Coordinators.Match
{
    public interface IMatchLobbyNotificationCoordinator : IDisposable
    {
        event EventHandler AvailableLobbiesChanged;

        event EventHandler<MatchLobbyUpdatedEventArgs> LobbyUpdated;

        event EventHandler<MatchLobbyClosedEventArgs> LobbyClosed;

        event EventHandler<MatchStatusChangedEventArgs> MatchStatusChanged;

        event EventHandler<MatchChatMessageReceivedEventArgs> ChatMessageReceived;

        Task<bool> SubscribeToLobbyAsync(int matchId);

        Task UnsubscribeFromLobbyAsync(int matchId);

        Task<bool> SubscribeToAvailableLobbiesAsync();
    }
}
