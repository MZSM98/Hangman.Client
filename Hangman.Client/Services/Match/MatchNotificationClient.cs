using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceMatchNotification;
using Hangman.Contracts.Match;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchNotificationClient : IMatchNotificationClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<MatchNotificationClient>();

        private readonly MatchNotificationCallbackHandler callbackHandler;

        private MatchNotificationServiceClient client;

        public MatchNotificationClient()
        {
            callbackHandler = new MatchNotificationCallbackHandler();

            callbackHandler.AvailableLobbiesChanged += OnCallbackAvailableLobbiesChanged;
            callbackHandler.LobbyUpdated += OnCallbackLobbyUpdated;
            callbackHandler.LobbyClosed += OnCallbackLobbyClosed;
            callbackHandler.MatchStatusChanged += OnCallbackMatchStatusChanged;
            callbackHandler.ChatMessageReceived += OnCallbackChatMessageReceived;
        }

        public event EventHandler AvailableLobbiesChanged;

        public event EventHandler<MatchLobbyUpdatedEventArgs> LobbyUpdated;

        public event EventHandler<MatchLobbyClosedEventArgs> LobbyClosed;

        public event EventHandler<MatchStatusChangedEventArgs> MatchStatusChanged;

        public event EventHandler<MatchChatMessageReceivedEventArgs> ChatMessageReceived;

        public async Task<SubscribeMatchResponse> SubscribeAsync(
            SubscribeMatchRequest request)
        {
            try
            {
                EnsureClient();

                return await client.SubscribeAsync(request);
            }
            catch (TimeoutException exception)
            {
                AbortClient();
                Log.Error("SubscribeAsync failed due to timeout.", exception);
                throw;
            }
            catch (CommunicationException exception)
            {
                AbortClient();
                Log.Error("SubscribeAsync failed due to communication error.", exception);
                throw;
            }
            catch (Exception exception)
            {
                AbortClient();
                Log.Error("SubscribeAsync failed unexpectedly.", exception);
                throw;
            }
        }

        public async Task<UnsubscribeMatchResponse> UnsubscribeAsync(
            UnsubscribeMatchRequest request)
        {
            try
            {
                EnsureClient();

                return await client.UnsubscribeAsync(request);
            }
            catch (TimeoutException exception)
            {
                AbortClient();
                Log.Error("UnsubscribeAsync failed due to timeout.", exception);
                throw;
            }
            catch (CommunicationException exception)
            {
                AbortClient();
                Log.Error("UnsubscribeAsync failed due to communication error.", exception);
                throw;
            }
            catch (Exception exception)
            {
                AbortClient();
                Log.Error("UnsubscribeAsync failed unexpectedly.", exception);
                throw;
            }
        }

        public void Close()
        {
            if (client == null)
            {
                return;
            }

            ICommunicationObject communicationObject = client;

            try
            {
                if (communicationObject.State == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                }
                else
                {
                    communicationObject.Close();
                }
            }
            catch (CommunicationException exception)
            {
                Log.Error("Error closing notification client.", exception);
                communicationObject.Abort();
            }
            catch (TimeoutException exception)
            {
                Log.Error("Timeout closing notification client.", exception);
                communicationObject.Abort();
            }
            catch (Exception exception)
            {
                Log.Error("Unexpected error closing notification client.", exception);
                communicationObject.Abort();
            }
            finally
            {
                client = null;
            }
        }

        public async Task<SubscribeAvailableLobbiesResponse> SubscribeAvailableLobbiesAsync(
            SubscribeAvailableLobbiesRequest request)
        {
            try
            {
                EnsureClient();

                return await client.SubscribeAvailableLobbiesAsync(request);
            }
            catch (TimeoutException exception)
            {
                AbortClient();
                Log.Error("SubscribeAvailableLobbiesAsync failed due to timeout.", exception);
                throw;
            }
            catch (CommunicationException exception)
            {
                AbortClient();
                Log.Error("SubscribeAvailableLobbiesAsync failed due to communication error.", exception);
                throw;
            }
            catch (Exception exception)
            {
                AbortClient();
                Log.Error("SubscribeAvailableLobbiesAsync failed unexpectedly.", exception);
                throw;
            }
        }

        public async Task<UnsubscribeAvailableLobbiesResponse> UnsubscribeAvailableLobbiesAsync(
            UnsubscribeAvailableLobbiesRequest request)
        {
            try
            {
                EnsureClient();

                return await client.UnsubscribeAvailableLobbiesAsync(request);
            }
            catch (TimeoutException exception)
            {
                AbortClient();
                Log.Error("UnsubscribeAvailableLobbiesAsync failed due to timeout.", exception);
                throw;
            }
            catch (CommunicationException exception)
            {
                AbortClient();
                Log.Error("UnsubscribeAvailableLobbiesAsync failed due to communication error.", exception);
                throw;
            }
            catch (Exception exception)
            {
                AbortClient();
                Log.Error("UnsubscribeAvailableLobbiesAsync failed unexpectedly.", exception);
                throw;
            }
        }

        private void OnCallbackAvailableLobbiesChanged(object sender, EventArgs e)
        {
            AvailableLobbiesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            callbackHandler.AvailableLobbiesChanged -= OnCallbackAvailableLobbiesChanged;
            callbackHandler.LobbyUpdated -= OnCallbackLobbyUpdated;
            callbackHandler.LobbyClosed -= OnCallbackLobbyClosed;
            callbackHandler.MatchStatusChanged -= OnCallbackMatchStatusChanged;
            callbackHandler.ChatMessageReceived -= OnCallbackChatMessageReceived;

            Close();
        }

        private void EnsureClient()
        {
            if (client == null)
            {
                CreateClient();
                return;
            }

            ICommunicationObject communicationObject = client;

            if (communicationObject.State == CommunicationState.Faulted ||
                communicationObject.State == CommunicationState.Closed ||
                communicationObject.State == CommunicationState.Closing)
            {
                AbortClient();
                CreateClient();
            }
        }

        private void CreateClient()
        {
            InstanceContext context = new InstanceContext(callbackHandler);
            client = new MatchNotificationServiceClient(context);
        }

        private void AbortClient()
        {
            if (client == null)
            {
                return;
            }

            ICommunicationObject communicationObject = client;
            communicationObject.Abort();
            client = null;
        }

        private void OnCallbackLobbyUpdated(
            object sender,
            MatchLobbyUpdatedEventArgs e)
        {
            LobbyUpdated?.Invoke(this, e);
        }

        private void OnCallbackLobbyClosed(
            object sender,
            MatchLobbyClosedEventArgs e)
        {
            LobbyClosed?.Invoke(this, e);
        }

        private void OnCallbackMatchStatusChanged(
            object sender,
            MatchStatusChangedEventArgs e)
        {
            MatchStatusChanged?.Invoke(this, e);
        }

        private void OnCallbackChatMessageReceived(
            object sender,
            MatchChatMessageReceivedEventArgs e)
        {
            ChatMessageReceived?.Invoke(this, e);
        }
    }
}
