using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Auth;
using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using Hangman.Client.ViewModels.Base;
using Hangman.Contracts.Match;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public sealed class MatchLobbyViewModel : ServiceViewModelBase, IDisposable
    {
        private const string MatchServiceName = "MatchService";

        private readonly IMatchClient matchClient;
        private readonly IMatchNotificationClient matchNotificationClient;

        private readonly RelayCommand createLobbyCommand;
        private readonly RelayCommand refreshLobbiesCommand;
        private readonly RelayCommand joinLobbyCommand;
        private readonly RelayCommand leaveLobbyCommand;
        private readonly RelayCommand backCommand;

        private ObservableCollection<AvailableLobbyModel> availableLobbies;
        private AvailableLobbyModel selectedLobby;
        private MatchLobbyModel currentLobby;

        private bool isAvailableLobbiesSubscriptionActive;
        private bool hasAvailableLobbies;
        private bool hasNoAvailableLobbies;
        private bool hasCurrentLobby;
        private bool hasNoCurrentLobby;

        public MatchLobbyViewModel()
            : this(
                  new MatchClient(),
                  new MatchNotificationClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<MatchLobbyViewModel>())
        {
        }

        internal MatchLobbyViewModel(
            IMatchClient matchClient,
            IMatchNotificationClient matchNotificationClient,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : base(validationMessageProvider, serverMessageProvider, logger)
        {
            this.matchClient = matchClient ??
                throw new ArgumentNullException(nameof(matchClient));

            this.matchNotificationClient = matchNotificationClient ??
                throw new ArgumentNullException(nameof(matchNotificationClient));

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
            private set { SetProperty(ref availableLobbies, value); }
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

        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            UnsubscribeNotificationEvents();

            matchNotificationClient?.Dispose();
        }

        private async Task CreateLobbyAsync()
        {
            await ExecuteServiceOperationAsync(
                "CreateLobbyAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    if (!HasValidSession())
                    {
                        SetCommonRuntimeError();
                        return;
                    }

                    CreateLobbyRequest request = new CreateLobbyRequest
                    {
                        HostAccountId = UserSession.CurrentUser.AccountId,
                        HostLanguageCode = UserSession.CurrentUser.PreferredLanguageCode
                    };

                    CreateLobbyResponse response = await matchClient.CreateLobbyAsync(request);

                    if (response == null)
                    {
                        SetCommonUnexpectedError();
                        return;
                    }

                    if (!response.Success)
                    {
                        SetError(GetMatchServerMessage(response.MessageCode));
                        return;
                    }

                    CurrentLobby = MatchLobbyModel.FromDto(response.Lobby);
                    SelectedLobby = null;
                    ClearAvailableLobbies();

                    await SubscribeToCurrentLobbyAsync();

                    SetSuccess(GetMatchServerMessage(response.MessageCode));
                },
                null,
                createLobbyCommand,
                refreshLobbiesCommand,
                joinLobbyCommand,
                leaveLobbyCommand,
                backCommand);
        }

        private async Task RefreshLobbiesAsync()
        {
            await ExecuteServiceOperationAsync(
                "GetAvailableLobbiesAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    await SubscribeToAvailableLobbiesAsync();

                    if (HasCurrentLobby)
                    {
                        return;
                    }

                    await LoadAvailableLobbiesAsync(false);
                },
                null,
                createLobbyCommand,
                refreshLobbiesCommand,
                joinLobbyCommand,
                leaveLobbyCommand,
                backCommand);
        }

        private async Task JoinLobbyAsync()
        {
            await ExecuteServiceOperationAsync(
                "JoinLobbyAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    if (!HasValidSession())
                    {
                        SetCommonRuntimeError();
                        return;
                    }

                    if (SelectedLobby == null)
                    {
                        SetError(GetMatchServerMessage("InvalidMatchId"));
                        return;
                    }

                    JoinLobbyRequest request = new JoinLobbyRequest
                    {
                        MatchId = SelectedLobby.MatchId,
                        GuestAccountId = UserSession.CurrentUser.AccountId,
                        GuestLanguageCode = UserSession.CurrentUser.PreferredLanguageCode
                    };

                    JoinLobbyResponse response = await matchClient.JoinLobbyAsync(request);

                    if (response == null)
                    {
                        SetCommonUnexpectedError();
                        return;
                    }

                    if (!response.Success)
                    {
                        SetError(GetMatchServerMessage(response.MessageCode));
                        return;
                    }

                    CurrentLobby = MatchLobbyModel.FromDto(response.Lobby);
                    SelectedLobby = null;
                    ClearAvailableLobbies();

                    await SubscribeToCurrentLobbyAsync();

                    SetSuccess(GetMatchServerMessage(response.MessageCode));
                },
                null,
                createLobbyCommand,
                refreshLobbiesCommand,
                joinLobbyCommand,
                leaveLobbyCommand,
                backCommand);
        }

        private async Task LeaveLobbyAsync()
        {
            await ExecuteServiceOperationAsync(
                "LeaveLobbyAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    if (!HasValidSession())
                    {
                        SetCommonRuntimeError();
                        return;
                    }

                    if (CurrentLobby == null)
                    {
                        SetError(GetMatchServerMessage("NoActiveLobby"));
                        return;
                    }

                    int matchId = CurrentLobby.MatchId;

                    LeaveLobbyRequest request = new LeaveLobbyRequest
                    {
                        MatchId = matchId,
                        AccountId = UserSession.CurrentUser.AccountId
                    };

                    LeaveLobbyResponse response = await matchClient.LeaveLobbyAsync(request);

                    if (response == null)
                    {
                        SetCommonUnexpectedError();
                        return;
                    }

                    if (!response.Success)
                    {
                        SetError(GetMatchServerMessage(response.MessageCode));
                        return;
                    }

                    await UnsubscribeFromLobbyAsync(matchId);

                    CurrentLobby = null;
                    SelectedLobby = null;

                    await SubscribeToAvailableLobbiesAsync();
                    await LoadAvailableLobbiesAsync(false);

                    SetSuccess(GetMatchServerMessage(response.MessageCode));
                },
                null,
                createLobbyCommand,
                refreshLobbiesCommand,
                joinLobbyCommand,
                leaveLobbyCommand,
                backCommand);
        }

        private async Task LoadAvailableLobbiesAsync(bool showSuccessMessage)
        {
            if (!HasValidSession())
            {
                SetCommonRuntimeError();
                return;
            }

            GetAvailableLobbiesRequest request = new GetAvailableLobbiesRequest
            {
                AccountId = UserSession.CurrentUser.AccountId
            };

            GetAvailableLobbiesResponse response =
                await matchClient.GetAvailableLobbiesAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                return;
            }

            if (!response.Success)
            {
                ClearAvailableLobbies();
                SetError(GetMatchServerMessage(response.MessageCode));
                return;
            }

            AvailableLobbies.Clear();

            if (response.Lobbies != null)
            {
                foreach (AvailableLobbyModel lobby in response.Lobbies
                    .Select(AvailableLobbyModel.FromDto)
                    .Where(lobby => lobby != null))
                {
                    AvailableLobbies.Add(lobby);
                }
            }

            UpdateAvailableLobbyState();

            if (showSuccessMessage)
            {
                SetSuccess(GetMatchServerMessage(response.MessageCode));
            }
        }

        private async Task RefreshCurrentLobbyAsync()
        {
            if (!HasValidSession())
            {
                return;
            }

            GetCurrentLobbyRequest request = new GetCurrentLobbyRequest
            {
                AccountId = UserSession.CurrentUser.AccountId
            };

            GetCurrentLobbyResponse response =
                await matchClient.GetCurrentLobbyAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                return;
            }

            if (!response.Success)
            {
                SetError(GetMatchServerMessage(response.MessageCode));
                return;
            }

            if (response.Lobby == null)
            {
                CurrentLobby = null;
                SelectedLobby = null;
                await LoadAvailableLobbiesAsync(false);
                return;
            }

            CurrentLobby = MatchLobbyModel.FromDto(response.Lobby);
            ClearAvailableLobbies();
        }

        private async Task SubscribeToCurrentLobbyAsync()
        {
            if (CurrentLobby == null || !HasValidSession())
            {
                return;
            }

            try
            {
                SubscribeMatchResponse response =
                    await matchNotificationClient.SubscribeAsync(
                        new SubscribeMatchRequest
                        {
                            MatchId = CurrentLobby.MatchId,
                            AccountId = UserSession.CurrentUser.AccountId
                        });

                if (response == null || !response.Success)
                {
                    SetError(GetMatchServerMessage("LobbySubscriptionFailed"));
                }
            }
            catch
            {
                SetError(GetMatchServerMessage("LobbySubscriptionFailed"));
            }
        }

        private async Task UnsubscribeFromLobbyAsync(int matchId)
        {
            if (!HasValidSession())
            {
                return;
            }

            try
            {
                await matchNotificationClient.UnsubscribeAsync(
                    new UnsubscribeMatchRequest
                    {
                        MatchId = matchId,
                        AccountId = UserSession.CurrentUser.AccountId
                    });
            }
            catch
            {
                matchNotificationClient.Close();
                isAvailableLobbiesSubscriptionActive = false;
            }
        }

        private async Task SubscribeToAvailableLobbiesAsync()
        {
            if (isAvailableLobbiesSubscriptionActive || !HasValidSession())
            {
                return;
            }

            try
            {
                SubscribeAvailableLobbiesResponse response =
                    await matchNotificationClient.SubscribeAvailableLobbiesAsync(
                        new SubscribeAvailableLobbiesRequest
                        {
                            AccountId = UserSession.CurrentUser.AccountId
                        });

                isAvailableLobbiesSubscriptionActive =
                    response != null && response.Success;
            }
            catch
            {
                isAvailableLobbiesSubscriptionActive = false;
            }
        }

        private async Task UnsubscribeFromAvailableLobbiesAsync()
        {
            if (!isAvailableLobbiesSubscriptionActive || !HasValidSession())
            {
                return;
            }

            try
            {
                await matchNotificationClient.UnsubscribeAvailableLobbiesAsync(
                    new UnsubscribeAvailableLobbiesRequest
                    {
                        AccountId = UserSession.CurrentUser.AccountId
                    });
            }
            catch
            {
                matchNotificationClient.Close();
            }
            finally
            {
                isAvailableLobbiesSubscriptionActive = false;
            }
        }

        private void SubscribeNotificationEvents()
        {
            matchNotificationClient.AvailableLobbiesChanged += OnAvailableLobbiesChanged;
            matchNotificationClient.LobbyUpdated += OnLobbyUpdated;
            matchNotificationClient.LobbyClosed += OnLobbyClosed;
            matchNotificationClient.MatchStatusChanged += OnMatchStatusChanged;
        }

        private void UnsubscribeNotificationEvents()
        {
            matchNotificationClient.AvailableLobbiesChanged -= OnAvailableLobbiesChanged;
            matchNotificationClient.LobbyUpdated -= OnLobbyUpdated;
            matchNotificationClient.LobbyClosed -= OnLobbyClosed;
            matchNotificationClient.MatchStatusChanged -= OnMatchStatusChanged;
        }

        private bool isLoadingAvailableLobbiesFromNotification;

        private void OnAvailableLobbiesChanged(object sender, EventArgs e)
        {
            RunOnUiThread(async () =>
            {
                await ReloadAvailableLobbiesFromNotificationAsync();
            });
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
            if (CurrentLobby == null || CurrentLobby.MatchId != e.MatchId)
            {
                return;
            }

            RunOnUiThread(async () =>
            {
                if (!IsBusy)
                {
                    await RefreshCurrentLobbyAsync();
                }
            });
        }

        private void OnLobbyClosed(object sender, MatchLobbyClosedEventArgs e)
        {
            if (CurrentLobby == null || CurrentLobby.MatchId != e.MatchId)
            {
                return;
            }

            RunOnUiThread(async () =>
            {
                CurrentLobby = null;
                SelectedLobby = null;

                await SubscribeToAvailableLobbiesAsync();
                await LoadAvailableLobbiesAsync(false);

                SetError(GetMatchServerMessage("LobbyClosed"));
            });
        }

        private void OnMatchStatusChanged(object sender, MatchStatusChangedEventArgs e)
        {
            if (CurrentLobby == null || CurrentLobby.MatchId != e.MatchId)
            {
                return;
            }

            RunOnUiThread(async () =>
            {
                if (!IsBusy)
                {
                    await RefreshCurrentLobbyAsync();
                }
            });
        }

        private void RunOnUiThread(Func<Task> operation)
        {
            if (operation == null)
            {
                return;
            }

            if (Application.Current == null ||
                Application.Current.Dispatcher.CheckAccess())
            {
                operation();
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(
                new Action(async () => await operation()));
        }

        private bool HasValidSession()
        {
            return UserSession.IsAuthenticated &&
                   UserSession.CurrentUser.AccountId > 0 &&
                   !string.IsNullOrWhiteSpace(UserSession.CurrentUser.PreferredLanguageCode);
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

            createLobbyCommand.RaiseCanExecuteChanged();
            refreshLobbiesCommand.RaiseCanExecuteChanged();
            joinLobbyCommand.RaiseCanExecuteChanged();
            leaveLobbyCommand.RaiseCanExecuteChanged();
            backCommand.RaiseCanExecuteChanged();
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
