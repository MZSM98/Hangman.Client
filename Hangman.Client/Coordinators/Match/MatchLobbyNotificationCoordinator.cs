using Hangman.Client.Services.Match;
using Hangman.Client.Services.Session;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.Coordinators.Match
{
    public sealed class MatchLobbyNotificationCoordinator :
        IMatchLobbyNotificationCoordinator
    {
        private readonly IMatchNotificationClient notificationClient;
        private readonly IMatchSessionContext sessionContext;

        private bool isAvailableLobbiesSubscriptionActive;
        private bool disposed;

        public MatchLobbyNotificationCoordinator(
            IMatchNotificationClient notificationClient,
            IMatchSessionContext sessionContext)
        {
            this.notificationClient = notificationClient ??
                throw new ArgumentNullException(nameof(notificationClient));
            this.sessionContext = sessionContext ??
                throw new ArgumentNullException(nameof(sessionContext));

            SubscribeNotificationEvents();
        }

        public event EventHandler AvailableLobbiesChanged;

        public event EventHandler<MatchLobbyUpdatedEventArgs> LobbyUpdated;

        public event EventHandler<MatchLobbyClosedEventArgs> LobbyClosed;

        public event EventHandler<MatchStatusChangedEventArgs> MatchStatusChanged;

        public event EventHandler<MatchChatMessageReceivedEventArgs> ChatMessageReceived;

        public async Task<bool> SubscribeToLobbyAsync(int matchId)
        {
            if (matchId <= 0 || !sessionContext.HasValidSession)
            {
                return false;
            }

            try
            {
                SubscribeMatchResponse response =
                    await notificationClient.SubscribeAsync(
                        new SubscribeMatchRequest
                        {
                            MatchId = matchId,
                            AccountId = sessionContext.AccountId
                        });

                return response != null && response.Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task UnsubscribeFromLobbyAsync(int matchId)
        {
            if (matchId <= 0 || !sessionContext.HasValidSession)
            {
                return;
            }

            try
            {
                await notificationClient.UnsubscribeAsync(
                    new UnsubscribeMatchRequest
                    {
                        MatchId = matchId,
                        AccountId = sessionContext.AccountId
                    });
            }
            catch
            {
                notificationClient.Close();
                isAvailableLobbiesSubscriptionActive = false;
            }
        }

        public async Task<bool> SubscribeToAvailableLobbiesAsync()
        {
            if (isAvailableLobbiesSubscriptionActive)
            {
                return true;
            }

            if (!sessionContext.HasValidSession)
            {
                return false;
            }

            try
            {
                SubscribeAvailableLobbiesResponse response =
                    await notificationClient.SubscribeAvailableLobbiesAsync(
                        new SubscribeAvailableLobbiesRequest
                        {
                            AccountId = sessionContext.AccountId
                        });

                isAvailableLobbiesSubscriptionActive =
                    response != null && response.Success;

                return isAvailableLobbiesSubscriptionActive;
            }
            catch
            {
                isAvailableLobbiesSubscriptionActive = false;
                return false;
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            UnsubscribeNotificationEvents();

            notificationClient.Dispose();
        }

        private void SubscribeNotificationEvents()
        {
            notificationClient.AvailableLobbiesChanged += OnAvailableLobbiesChanged;
            notificationClient.LobbyUpdated += OnLobbyUpdated;
            notificationClient.LobbyClosed += OnLobbyClosed;
            notificationClient.MatchStatusChanged += OnMatchStatusChanged;
            notificationClient.ChatMessageReceived += OnChatMessageReceived;
        }

        private void UnsubscribeNotificationEvents()
        {
            notificationClient.AvailableLobbiesChanged -= OnAvailableLobbiesChanged;
            notificationClient.LobbyUpdated -= OnLobbyUpdated;
            notificationClient.LobbyClosed -= OnLobbyClosed;
            notificationClient.MatchStatusChanged -= OnMatchStatusChanged;
            notificationClient.ChatMessageReceived -= OnChatMessageReceived;
        }

        private void OnAvailableLobbiesChanged(object sender, EventArgs e)
        {
            AvailableLobbiesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnLobbyUpdated(object sender, MatchLobbyUpdatedEventArgs e)
        {
            LobbyUpdated?.Invoke(this, e);
        }

        private void OnLobbyClosed(object sender, MatchLobbyClosedEventArgs e)
        {
            LobbyClosed?.Invoke(this, e);
        }

        private void OnMatchStatusChanged(object sender, MatchStatusChangedEventArgs e)
        {
            MatchStatusChanged?.Invoke(this, e);
        }

        private void OnChatMessageReceived(object sender, MatchChatMessageReceivedEventArgs e)
        {
            ChatMessageReceived?.Invoke(this, e);
        }
    }
}
