using Hangman.Client.Models.Match;
using Hangman.Client.ServiceReferenceMatchNotification;
using Hangman.Contracts.Match;
using System;
using System.ServiceModel;

namespace Hangman.Client.Services.Match
{
    [CallbackBehavior(
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        UseSynchronizationContext = false)]
    public class MatchNotificationCallbackHandler : IMatchNotificationServiceCallback
    {
        public event EventHandler<MatchLobbyUpdatedEventArgs> LobbyUpdated;

        public event EventHandler<MatchLobbyClosedEventArgs> LobbyClosed;

        public event EventHandler<MatchStatusChangedEventArgs> MatchStatusChanged;

        public event EventHandler AvailableLobbiesChanged;

        public event EventHandler<MatchChatMessageReceivedEventArgs> ChatMessageReceived;

        public void OnLobbyUpdated(int matchId)
        {
            LobbyUpdated?.Invoke(this, new MatchLobbyUpdatedEventArgs(matchId));
        }

        public void OnLobbyClosed(int matchId, string messageCode)
        {
            LobbyClosed?.Invoke(this, new MatchLobbyClosedEventArgs(matchId, messageCode));
        }

        public void OnMatchStatusChanged(int matchId, string matchStatus)
        {
            MatchStatusChanged?.Invoke(this, new MatchStatusChangedEventArgs(matchId, matchStatus));
        }

        public void OnAvailableLobbiesChanged()
        {
            AvailableLobbiesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void OnMatchChatMessageReceived(MatchChatMessageDto message)
        {
            MatchChatMessageModel model =
                MatchChatMessageModel.FromDto(message);

            if (model == null)
            {
                return;
            }

            ChatMessageReceived?.Invoke(
                this,
                new MatchChatMessageReceivedEventArgs(model));
        }
    }
}
