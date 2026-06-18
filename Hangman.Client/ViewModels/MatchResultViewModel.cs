using Hangman.Client.Coordinators.Match;
using Hangman.Client.Infrastructure.Threading;
using Hangman.Client.Models.Auth;
using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using Hangman.Client.Services.Session;
using Hangman.Client.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public sealed class MatchResultViewModel : BaseViewModel, IDisposable
    {
        private readonly RelayCommand exitCommand;
        private readonly RelayCommand initializeChatCommand;
        private readonly MatchGameStateModel gameState;
        private readonly IMatchLobbyNotificationCoordinator notificationCoordinator;
        private readonly IUiDispatcher uiDispatcher;

        private bool chatInitialized;
        private bool disposed;

        public MatchResultViewModel(MatchGameStateModel gameState)
            : this(gameState, null)
        {
        }

        public MatchResultViewModel(
            MatchGameStateModel gameState,
            MatchChatViewModel chat)
            : this(
                  gameState,
                  chat,
                  new MatchLobbyNotificationCoordinator(
                      new MatchNotificationClient(),
                      new MatchSessionContext()),
                  new WpfUiDispatcher())
        {
        }

        internal MatchResultViewModel(
            MatchGameStateModel gameState,
            MatchChatViewModel chat,
            IMatchLobbyNotificationCoordinator notificationCoordinator,
            IUiDispatcher uiDispatcher)
        {
            this.gameState = gameState ??
                throw new ArgumentNullException(nameof(gameState));
            this.notificationCoordinator = notificationCoordinator ??
                throw new ArgumentNullException(nameof(notificationCoordinator));
            this.uiDispatcher = uiDispatcher ??
                throw new ArgumentNullException(nameof(uiDispatcher));

            Chat = chat;

            exitCommand = new RelayCommand(RequestExit);
            initializeChatCommand = new RelayCommand(InitializeChatAsync);

            SubscribeNotificationEvents();
        }

        public event EventHandler ExitRequested;

        public MatchChatViewModel Chat { get; private set; }

        public ICommand ExitCommand
        {
            get { return exitCommand; }
        }

        public ICommand InitializeChatCommand
        {
            get { return initializeChatCommand; }
        }

        public bool HasWinner
        {
            get { return gameState.WinnerId.HasValue; }
        }

        public bool IsCurrentUserWinner
        {
            get
            {
                return HasWinner &&
                       UserSession.CurrentUser != null &&
                       UserSession.CurrentUser.PlayerId == gameState.WinnerId.Value;
            }
        }

        public bool IsCurrentUserLoser
        {
            get { return HasWinner && !IsCurrentUserWinner; }
        }

        public string WinnerFullName
        {
            get
            {
                return string.IsNullOrWhiteSpace(gameState.WinnerFullName)
                    ? "-"
                    : gameState.WinnerFullName;
            }
        }

        public string WinnerEmail
        {
            get
            {
                return string.IsNullOrWhiteSpace(gameState.WinnerEmail)
                    ? "-"
                    : gameState.WinnerEmail;
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

            if (gameState != null && gameState.MatchId > 0)
            {
                _ = notificationCoordinator.UnsubscribeFromLobbyAsync(
                    gameState.MatchId);
            }

            notificationCoordinator.Dispose();
        }

        private async Task InitializeChatAsync()
        {
            if (chatInitialized ||
                disposed ||
                Chat == null ||
                gameState.MatchId <= 0)
            {
                return;
            }

            chatInitialized = true;

            await notificationCoordinator.SubscribeToLobbyAsync(
                gameState.MatchId);
        }

        private void RequestExit()
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SubscribeNotificationEvents()
        {
            notificationCoordinator.ChatMessageReceived += OnChatMessageReceived;
        }

        private void UnsubscribeNotificationEvents()
        {
            notificationCoordinator.ChatMessageReceived -= OnChatMessageReceived;
        }

        private void OnChatMessageReceived(
            object sender,
            MatchChatMessageReceivedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (e == null ||
                    e.Message == null ||
                    e.Message.MatchId != gameState.MatchId)
                {
                    return;
                }

                Chat?.AddIncomingMessage(e.Message);

                await Task.CompletedTask;
            });
        }
    }
}
