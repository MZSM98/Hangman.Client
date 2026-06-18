using Hangman.Client.Coordinators.Match;
using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Infrastructure.Threading;
using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessNotificationController :
        IMatchGuessNotificationController
    {
        private readonly IMatchLobbyNotificationCoordinator notificationCoordinator;
        private readonly IUiDispatcher uiDispatcher;
        private readonly IClientLogger logger;

        private Func<int> getCurrentMatchId;
        private Func<Task> refreshGameStateAsync;
        private Func<MatchLobbyClosedEventArgs, Task> handleLobbyClosedAsync;
        
        private Action setRuntimeError;
        private Action setUnexpectedError;
        private Action<MatchChatMessageModel> handleChatMessage;

        private bool isRefreshingFromNotification;
        private bool disposed;

        public MatchGuessNotificationController(
            IMatchLobbyNotificationCoordinator notificationCoordinator,
            IUiDispatcher uiDispatcher,
            IClientLogger logger)
        {
            this.notificationCoordinator = notificationCoordinator ??
                throw new ArgumentNullException(nameof(notificationCoordinator));
            this.uiDispatcher = uiDispatcher ??
                throw new ArgumentNullException(nameof(uiDispatcher));
            this.logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            SubscribeNotificationEvents();
        }

        public void Configure(
            Func<int> getCurrentMatchId,
            Func<Task> refreshGameStateAsync,
            Func<MatchLobbyClosedEventArgs, Task> handleLobbyClosedAsync,
            Action<MatchChatMessageModel> handleChatMessage,
            Action setRuntimeError,
            Action setUnexpectedError)
        {
            this.getCurrentMatchId = getCurrentMatchId ??
                throw new ArgumentNullException(nameof(getCurrentMatchId));
            this.refreshGameStateAsync = refreshGameStateAsync ??
                throw new ArgumentNullException(nameof(refreshGameStateAsync));
            this.handleLobbyClosedAsync = handleLobbyClosedAsync ??
                throw new ArgumentNullException(nameof(handleLobbyClosedAsync));
            this.handleChatMessage = handleChatMessage ??
                throw new ArgumentNullException(nameof(handleChatMessage));
            this.setRuntimeError = setRuntimeError ??
                throw new ArgumentNullException(nameof(setRuntimeError));
            this.setUnexpectedError = setUnexpectedError ??
                throw new ArgumentNullException(nameof(setUnexpectedError));
        }

        public Task<bool> SubscribeToLobbyAsync(int matchId)
        {
            return notificationCoordinator.SubscribeToLobbyAsync(matchId);
        }

        public Task UnsubscribeFromLobbyAsync(int matchId)
        {
            return notificationCoordinator.UnsubscribeFromLobbyAsync(matchId);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            UnsubscribeNotificationEvents();

            notificationCoordinator.Dispose();
        }

        private void SubscribeNotificationEvents()
        {
            notificationCoordinator.LobbyUpdated += OnLobbyUpdated;
            notificationCoordinator.LobbyClosed += OnLobbyClosed;
            notificationCoordinator.MatchStatusChanged += OnMatchStatusChanged;
            notificationCoordinator.ChatMessageReceived += OnChatMessageReceived;
        }

        private void UnsubscribeNotificationEvents()
        {
            notificationCoordinator.LobbyUpdated -= OnLobbyUpdated;
            notificationCoordinator.LobbyClosed -= OnLobbyClosed;
            notificationCoordinator.MatchStatusChanged -= OnMatchStatusChanged;
            notificationCoordinator.ChatMessageReceived -= OnChatMessageReceived;
        }

        private void OnLobbyUpdated(
            object sender,
            MatchLobbyUpdatedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (!IsCurrentMatch(e.MatchId))
                {
                    return;
                }

                await RefreshFromNotificationAsync();
            });
        }

        private void OnMatchStatusChanged(
            object sender,
            MatchStatusChangedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (!IsCurrentMatch(e.MatchId))
                {
                    return;
                }

                await RefreshFromNotificationAsync();
            });
        }

        private void OnLobbyClosed(
            object sender,
            MatchLobbyClosedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (!IsCurrentMatch(e.MatchId))
                {
                    return;
                }

                await handleLobbyClosedAsync(e);
            });
        }

        private void OnChatMessageReceived(
            object sender,
            MatchChatMessageReceivedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (e == null ||
                    e.Message == null ||
                    !IsCurrentMatch(e.Message.MatchId))
                {
                    return;
                }

                handleChatMessage?.Invoke(e.Message);

                await Task.CompletedTask;
            });
        }

        private bool IsCurrentMatch(int matchId)
        {
            return getCurrentMatchId != null &&
                   matchId > 0 &&
                   getCurrentMatchId() == matchId;
        }

        private async Task RefreshFromNotificationAsync()
        {
            if (disposed ||
                isRefreshingFromNotification ||
                refreshGameStateAsync == null)
            {
                return;
            }

            isRefreshingFromNotification = true;

            try
            {
                await refreshGameStateAsync();
            }
            catch (EndpointNotFoundException exception)
            {
                logger.Error("RefreshFromNotificationAsync failed because endpoint was not found.", 
                    exception);

                setRuntimeError();
            }
            catch (TimeoutException exception)
            {
                logger.Error("RefreshFromNotificationAsync failed due to timeout.",
                    exception);

                setRuntimeError();
            }
            catch (CommunicationException exception)
            {
                logger.Error("RefreshFromNotificationAsync failed due to communication error.",
                    exception);

                setRuntimeError();
            }
            catch (Exception exception)
            {
                logger.Error("RefreshFromNotificationAsync failed unexpectedly.",
                    exception);

                setUnexpectedError();
            }
            finally
            {
                isRefreshingFromNotification = false;
            }
        }
    }
}
