using Hangman.Client.Coordinators.Match;
using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Infrastructure.Threading;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using Hangman.Client.Services.Session;
using Hangman.Client.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public sealed class MatchLobbyViewModel : ServiceViewModelBase, IDisposable
    {
        private const string MatchServiceName = "MatchService";
        private const string LobbySubscriptionFailedCode = "LobbySubscriptionFailed";
        private const string InvalidMatchIdCode = "InvalidMatchId";
        private const string NoActiveLobbyCode = "NoActiveLobby";
        private const string LobbyClosedCode = "LobbyClosed";

        private readonly IMatchLobbyWorkflow matchLobbyWorkflow;
        private readonly IMatchLobbyNotificationCoordinator notificationCoordinator;
        private readonly IUiDispatcher uiDispatcher;

        private readonly RelayCommand createLobbyCommand;
        private readonly RelayCommand refreshLobbiesCommand;
        private readonly RelayCommand joinLobbyCommand;
        private readonly RelayCommand leaveLobbyCommand;
        private readonly RelayCommand backCommand;

        private readonly ObservableCollection<AvailableLobbyModel> availableLobbies;
        private AvailableLobbyModel selectedLobby;
        private MatchLobbyModel currentLobby;

        private bool hasAvailableLobbies;
        private bool hasNoAvailableLobbies;
        private bool hasCurrentLobby;
        private bool hasNoCurrentLobby;
        private bool isLoadingAvailableLobbiesFromNotification;
        private bool disposed;

        public MatchLobbyViewModel()
            : this(
                  new MatchClient(),
                  new MatchNotificationClient(),
                  new MatchSessionContext(),
                  new WpfUiDispatcher(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<MatchLobbyViewModel>())
        {
        }

        internal MatchLobbyViewModel(
            IMatchClient matchClient,
            IMatchNotificationClient matchNotificationClient,
            IMatchSessionContext sessionContext,
            IUiDispatcher uiDispatcher,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : this(
                  new MatchLobbyWorkflow(matchClient, sessionContext),
                  new MatchLobbyNotificationCoordinator(
                      matchNotificationClient,
                      sessionContext),
                  uiDispatcher,
                  validationMessageProvider,
                  serverMessageProvider,
                  logger)
        {
        }

        internal MatchLobbyViewModel(
            IMatchLobbyWorkflow matchLobbyWorkflow,
            IMatchLobbyNotificationCoordinator notificationCoordinator,
            IUiDispatcher uiDispatcher,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : base(validationMessageProvider, serverMessageProvider, logger)
        {
            this.matchLobbyWorkflow = matchLobbyWorkflow ??
                throw new ArgumentNullException(nameof(matchLobbyWorkflow));
            this.notificationCoordinator = notificationCoordinator ??
                throw new ArgumentNullException(nameof(notificationCoordinator));
            this.uiDispatcher = uiDispatcher ??
                throw new ArgumentNullException(nameof(uiDispatcher));

            availableLobbies = new ObservableCollection<AvailableLobbyModel>();

            hasNoCurrentLobby = true;
            hasNoAvailableLobbies = true;

            createLobbyCommand = new RelayCommand(CreateLobbyAsync, CanCreateLobby);
            refreshLobbiesCommand = new RelayCommand(RefreshLobbiesAsync, CanRefreshLobbies);
            joinLobbyCommand = new RelayCommand(JoinLobbyAsync, CanJoinLobby);
            leaveLobbyCommand = new RelayCommand(LeaveLobbyAsync, CanLeaveLobby);
            backCommand = new RelayCommand(RequestBack, CanGoBack);

            SubscribeNotificationEvents();
        }

        public event EventHandler BackRequested;

        public ObservableCollection<AvailableLobbyModel> AvailableLobbies
        {
            get { return availableLobbies; }
        }

        public AvailableLobbyModel SelectedLobby
        {
            get { return selectedLobby; }
            set
            {
                if (SetProperty(ref selectedLobby, value))
                {
                    joinLobbyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public MatchLobbyModel CurrentLobby
        {
            get { return currentLobby; }
            private set
            {
                if (SetProperty(ref currentLobby, value))
                {
                    UpdateCurrentLobbyState();
                }
            }
        }

        public bool HasAvailableLobbies
        {
            get { return hasAvailableLobbies; }
            private set { SetProperty(ref hasAvailableLobbies, value); }
        }

        public bool HasNoAvailableLobbies
        {
            get { return hasNoAvailableLobbies; }
            private set { SetProperty(ref hasNoAvailableLobbies, value); }
        }

        public bool HasCurrentLobby
        {
            get { return hasCurrentLobby; }
            private set { SetProperty(ref hasCurrentLobby, value); }
        }

        public bool HasNoCurrentLobby
        {
            get { return hasNoCurrentLobby; }
            private set { SetProperty(ref hasNoCurrentLobby, value); }
        }

        public ICommand CreateLobbyCommand
        {
            get { return createLobbyCommand; }
        }

        public ICommand RefreshLobbiesCommand
        {
            get { return refreshLobbiesCommand; }
        }

        public ICommand JoinLobbyCommand
        {
            get { return joinLobbyCommand; }
        }

        public ICommand LeaveLobbyCommand
        {
            get { return leaveLobbyCommand; }
        }

        public ICommand BackCommand
        {
            get { return backCommand; }
        }

        public Task LoadAsync()
        {
            return RefreshLobbiesAsync();
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

        private async Task CreateLobbyAsync()
        {
            await ExecuteServiceOperationAsync(
                "CreateLobbyAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchLobbyOperationResult result =
                        await matchLobbyWorkflow.CreateLobbyAsync();

                    if (!result.Success)
                    {
                        ApplyFailure(result);
                        return;
                    }

                    if (result.Lobby == null)
                    {
                        SetCommonUnexpectedError();
                        return;
                    }

                    CurrentLobby = result.Lobby;
                    SelectedLobby = null;
                    ClearAvailableLobbies();

                    bool subscribed =
                        await notificationCoordinator.SubscribeToLobbyAsync(
                            CurrentLobby.MatchId);

                    if (!subscribed)
                    {
                        SetError(GetMatchServerMessage(LobbySubscriptionFailedCode));
                        return;
                    }

                    SetSuccess(GetMatchServerMessage(result.MessageCode));
                },
                null,
                GetLobbyCommands());
        }

        private async Task RefreshLobbiesAsync()
        {
            await ExecuteServiceOperationAsync(
                "GetAvailableLobbiesAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    await notificationCoordinator.SubscribeToAvailableLobbiesAsync();

                    if (HasCurrentLobby)
                    {
                        return;
                    }

                    await LoadAvailableLobbiesAsync(false);
                },
                null,
                GetLobbyCommands());
        }

        private async Task JoinLobbyAsync()
        {
            await ExecuteServiceOperationAsync(
                "JoinLobbyAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    if (SelectedLobby == null)
                    {
                        SetError(GetMatchServerMessage(InvalidMatchIdCode));
                        return;
                    }

                    MatchLobbyOperationResult result =
                        await matchLobbyWorkflow.JoinLobbyAsync(SelectedLobby.MatchId);

                    if (!result.Success)
                    {
                        ApplyFailure(result);
                        return;
                    }

                    if (result.Lobby == null)
                    {
                        SetCommonUnexpectedError();
                        return;
                    }

                    CurrentLobby = result.Lobby;
                    SelectedLobby = null;
                    ClearAvailableLobbies();

                    bool subscribed =
                        await notificationCoordinator.SubscribeToLobbyAsync(
                            CurrentLobby.MatchId);

                    if (!subscribed)
                    {
                        SetError(GetMatchServerMessage(LobbySubscriptionFailedCode));
                        return;
                    }

                    SetSuccess(GetMatchServerMessage(result.MessageCode));
                },
                null,
                GetLobbyCommands());
        }

        private async Task LeaveLobbyAsync()
        {
            await ExecuteServiceOperationAsync(
                "LeaveLobbyAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    if (CurrentLobby == null)
                    {
                        SetError(GetMatchServerMessage(NoActiveLobbyCode));
                        return;
                    }

                    int matchId = CurrentLobby.MatchId;

                    MatchLobbyOperationResult result =
                        await matchLobbyWorkflow.LeaveLobbyAsync(matchId);

                    if (!result.Success)
                    {
                        ApplyFailure(result);
                        return;
                    }

                    await notificationCoordinator.UnsubscribeFromLobbyAsync(matchId);

                    CurrentLobby = null;
                    SelectedLobby = null;

                    await notificationCoordinator.SubscribeToAvailableLobbiesAsync();
                    await LoadAvailableLobbiesAsync(false);

                    SetSuccess(GetMatchServerMessage(result.MessageCode));
                },
                null,
                GetLobbyCommands());
        }

        private async Task LoadAvailableLobbiesAsync(bool showSuccessMessage)
        {
            AvailableLobbiesOperationResult result =
                await matchLobbyWorkflow.GetAvailableLobbiesAsync();

            if (!result.Success)
            {
                ClearAvailableLobbies();
                ApplyFailure(result);
                return;
            }

            AvailableLobbies.Clear();

            foreach (AvailableLobbyModel lobby in result.Lobbies)
            {
                AvailableLobbies.Add(lobby);
            }

            UpdateAvailableLobbyState();

            if (showSuccessMessage)
            {
                SetSuccess(GetMatchServerMessage(result.MessageCode));
            }
        }

        private async Task RefreshCurrentLobbyAsync()
        {
            CurrentLobbyOperationResult result =
                await matchLobbyWorkflow.GetCurrentLobbyAsync();

            if (!result.Success)
            {
                ApplyFailure(result);
                return;
            }

            if (result.Lobby == null)
            {
                CurrentLobby = null;
                SelectedLobby = null;

                await notificationCoordinator.SubscribeToAvailableLobbiesAsync();
                await LoadAvailableLobbiesAsync(false);

                return;
            }

            CurrentLobby = result.Lobby;
            ClearAvailableLobbies();
        }

        private void SubscribeNotificationEvents()
        {
            notificationCoordinator.AvailableLobbiesChanged += OnAvailableLobbiesChanged;
            notificationCoordinator.LobbyUpdated += OnLobbyUpdated;
            notificationCoordinator.LobbyClosed += OnLobbyClosed;
            notificationCoordinator.MatchStatusChanged += OnMatchStatusChanged;
        }

        private void UnsubscribeNotificationEvents()
        {
            notificationCoordinator.AvailableLobbiesChanged -= OnAvailableLobbiesChanged;
            notificationCoordinator.LobbyUpdated -= OnLobbyUpdated;
            notificationCoordinator.LobbyClosed -= OnLobbyClosed;
            notificationCoordinator.MatchStatusChanged -= OnMatchStatusChanged;
        }

        private void OnAvailableLobbiesChanged(object sender, EventArgs e)
        {
            uiDispatcher.RunAsync(ReloadAvailableLobbiesFromNotificationAsync);
        }

        private async Task ReloadAvailableLobbiesFromNotificationAsync()
        {
            if (HasCurrentLobby || isLoadingAvailableLobbiesFromNotification)
            {
                return;
            }

            isLoadingAvailableLobbiesFromNotification = true;

            try
            {
                await LoadAvailableLobbiesAsync(false);
            }
            finally
            {
                isLoadingAvailableLobbiesFromNotification = false;
            }
        }

        private void OnLobbyUpdated(object sender, MatchLobbyUpdatedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (CurrentLobby == null ||
                    CurrentLobby.MatchId != e.MatchId ||
                    IsBusy)
                {
                    return;
                }

                await RefreshCurrentLobbyAsync();
            });
        }

        private void OnLobbyClosed(object sender, MatchLobbyClosedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (CurrentLobby == null ||
                    CurrentLobby.MatchId != e.MatchId)
                {
                    return;
                }

                CurrentLobby = null;
                SelectedLobby = null;

                await notificationCoordinator.SubscribeToAvailableLobbiesAsync();
                await LoadAvailableLobbiesAsync(false);

                string messageCode = string.IsNullOrWhiteSpace(e.MessageCode)
                    ? LobbyClosedCode
                    : e.MessageCode;

                SetError(GetMatchServerMessage(messageCode));
            });
        }

        private void OnMatchStatusChanged(object sender, MatchStatusChangedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (CurrentLobby == null ||
                    CurrentLobby.MatchId != e.MatchId ||
                    IsBusy)
                {
                    return;
                }

                await RefreshCurrentLobbyAsync();
            });
        }

        private void ApplyFailure(MatchWorkflowResultBase result)
        {
            if (result == null || result.IsUnexpectedError)
            {
                SetCommonUnexpectedError();
                return; 
            }

            if (result.IsSessionInvalid)
            {
                SetCommonRuntimeError();
                return; 
            }

            SetError(GetMatchServerMessage(result.MessageCode));
        }

        private bool CanCreateLobby()
        {
            return !IsBusy && !HasCurrentLobby;
        }

        private bool CanRefreshLobbies()
        {
            return !IsBusy && !HasCurrentLobby;
        }

        private bool CanJoinLobby()
        {
            return !IsBusy &&
                   SelectedLobby != null &&
                   !HasCurrentLobby;
        }

        private bool CanLeaveLobby()
        {
            return !IsBusy && HasCurrentLobby;
        }

        private bool CanGoBack()
        {
            return !IsBusy && !HasCurrentLobby;
        }

        private void ClearAvailableLobbies()
        {
            AvailableLobbies.Clear();
            UpdateAvailableLobbyState();
        }

        private void UpdateAvailableLobbyState()
        {
            HasAvailableLobbies = AvailableLobbies.Any();
            HasNoAvailableLobbies = !HasAvailableLobbies;

            joinLobbyCommand.RaiseCanExecuteChanged();
        }

        private void UpdateCurrentLobbyState()
        {
            HasCurrentLobby = CurrentLobby != null;
            HasNoCurrentLobby = !HasCurrentLobby;

            RaiseCommandsCanExecuteChanged(GetLobbyCommands());
        }

        private RelayCommand[] GetLobbyCommands()
        {
            return new[]
            {
                createLobbyCommand,
                refreshLobbiesCommand,
                joinLobbyCommand,
                leaveLobbyCommand,
                backCommand
            };
        }

        private string GetMatchServerMessage(string messageCode)
        {
            return GetServerMessage(ServerMessageModuleName.Match, messageCode);
        }

        private void RequestBack()
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
