using Hangman.Client.Coordinators.Match;
using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Infrastructure.Threading;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using Hangman.Client.Services.Session;
using Hangman.Client.Services.Word;
using Hangman.Client.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public sealed class MatchGameplayViewModel : ServiceViewModelBase, IDisposable
    {
        private const string MatchServiceName = "MatchService";
        private const string MatchGameplayServiceName = "MatchGameplayService";
        private const string LobbyClosedCode = "LobbyClosed";
        private const string InvalidCategoryIdCode = "InvalidCategoryId";
        private const string InvalidWordIdCode = "InvalidWordId";
        private const string LobbySubscriptionFailedCode = "LobbySubscriptionFailed";

        private readonly IMatchGameplayWorkflow matchGameplayWorkflow;
        private readonly IMatchLobbyWorkflow matchLobbyWorkflow;
        private readonly IMatchLobbyNotificationCoordinator notificationCoordinator;
        private readonly IUiDispatcher uiDispatcher;

        private readonly RelayCommand loadCommand;
        private readonly RelayCommand voteCategoryCommand;
        private readonly RelayCommand resolveVotingCommand;
        private readonly RelayCommand loadWordsCommand;
        private readonly RelayCommand selectWordCommand;
        private readonly RelayCommand leaveCommand;

        private readonly ObservableCollection<CategoryOptionModel> categories;
        private readonly ObservableCollection<CategoryVoteModel> votes;
        private readonly ObservableCollection<SelectableWordModel> selectableWords;

        private MatchLobbyModel currentLobby;
        private CategoryVotingStateModel votingState;
        private CategoryOptionModel selectedCategory;
        private SelectableWordModel selectedWord;

        private bool hasCategories;
        private bool hasVotes;
        private bool hasSelectableWords;
        private bool canVote;
        private bool canSelectWord;
        private bool isVotingCategory;
        private bool isWaitingForHostWord;
        private bool isInProgress;
        private bool isWaitingForHostSelection;
        private int remainingVotingSeconds;
        private bool hasNoVotes;
        private bool isRefreshingFromNotification;
        private bool hasRequestedGuessNavigation;

        private CancellationTokenSource countdownCancellationTokenSource;

        public MatchChatViewModel Chat { get; private set; }

        private bool disposed;

        public MatchGameplayViewModel(MatchLobbyModel lobby): this(lobby, null)
        {
        }

        public MatchGameplayViewModel(
            MatchLobbyModel lobby,
            MatchChatViewModel chat)
            : this(
                  lobby,
                  new MatchGameplayClient(),
                  new MatchClient(),
                  new WordClient(),
                  new MatchNotificationClient(),
                  new MatchSessionContext(),
                  new WpfUiDispatcher(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<MatchGameplayViewModel>(),
                  chat)
        {
        }

        internal MatchGameplayViewModel(
            MatchLobbyModel lobby,
            IMatchGameplayClient matchGameplayClient,
            IMatchClient matchClient,
            IWordClient wordClient,
            IMatchNotificationClient matchNotificationClient,
            IMatchSessionContext sessionContext,
            IUiDispatcher uiDispatcher,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger,
            MatchChatViewModel chat)
            : this(
                  lobby,
                  new MatchGameplayWorkflow(
                      matchGameplayClient,
                      wordClient,
                      sessionContext),
                  new MatchLobbyWorkflow(
                      matchClient,
                      sessionContext),
                  new MatchLobbyNotificationCoordinator(
                      matchNotificationClient,
                      sessionContext),
                  uiDispatcher,
                  validationMessageProvider,
                  serverMessageProvider,
                  logger,
                  chat)
        {
        }

        internal MatchGameplayViewModel(
            MatchLobbyModel lobby,
            IMatchGameplayWorkflow matchGameplayWorkflow,
            IMatchLobbyWorkflow matchLobbyWorkflow,
            IMatchLobbyNotificationCoordinator notificationCoordinator,
            IUiDispatcher uiDispatcher,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger,
            MatchChatViewModel chat)
            : base(validationMessageProvider, serverMessageProvider, logger)
        {
            CurrentLobby = lobby ?? throw new ArgumentNullException(nameof(lobby));

            Chat = chat;

            this.matchGameplayWorkflow = matchGameplayWorkflow ??
                throw new ArgumentNullException(nameof(matchGameplayWorkflow));
            this.matchLobbyWorkflow = matchLobbyWorkflow ??
                throw new ArgumentNullException(nameof(matchLobbyWorkflow));
            this.notificationCoordinator = notificationCoordinator ??
                throw new ArgumentNullException(nameof(notificationCoordinator));
            this.uiDispatcher = uiDispatcher ??
                throw new ArgumentNullException(nameof(uiDispatcher));

            categories = new ObservableCollection<CategoryOptionModel>();
            votes = new ObservableCollection<CategoryVoteModel>();
            selectableWords = new ObservableCollection<SelectableWordModel>();

            loadCommand = new RelayCommand(LoadAsync, CanExecuteWhenNotBusy);
            voteCategoryCommand = new RelayCommand(VoteCategoryAsync, CanVoteCategory);
            resolveVotingCommand = new RelayCommand(ResolveCategoryVotingAsync, CanResolveVoting);
            loadWordsCommand = new RelayCommand(LoadSelectableWordsAsync, CanLoadWords);
            selectWordCommand = new RelayCommand(SelectWordAsync, CanSelectWordCommand);
            leaveCommand = new RelayCommand(LeaveAsync, CanLeave);

            SubscribeNotificationEvents();
            UpdatePhaseState();
        }

        public event EventHandler BackRequested;
        public event EventHandler<MatchGuessRequestedEventArgs> GuessRequested;

        public MatchLobbyModel CurrentLobby
        {
            get { return currentLobby; }
            private set
            {
                if (SetProperty(ref currentLobby, value))
                {
                    UpdatePhaseState();
                }
            }
        }

        public CategoryVotingStateModel VotingState
        {
            get { return votingState; }
            private set
            {
                if (SetProperty(ref votingState, value))
                {
                    ApplyVotingState(value);
                }
            }
        }

        public ObservableCollection<CategoryOptionModel> Categories
        {
            get { return categories; }
        }

        public ObservableCollection<CategoryVoteModel> Votes
        {
            get { return votes; }
        }

        public ObservableCollection<SelectableWordModel> SelectableWords
        {
            get { return selectableWords; }
        }

        public CategoryOptionModel SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                if (SetProperty(ref selectedCategory, value))
                {
                    voteCategoryCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public SelectableWordModel SelectedWord
        {
            get { return selectedWord; }
            set
            {
                if (SetProperty(ref selectedWord, value))
                {
                    selectWordCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(SelectedWordDescription));
                }
            }
        }

        public string SelectedWordDescription
        {
            get
            {
                return SelectedWord == null
                    ? string.Empty
                    : SelectedWord.Description;
            }
        }

        public bool HasCategories
        {
            get { return hasCategories; }
            private set { SetProperty(ref hasCategories, value); }
        }

        public bool HasVotes
        {
            get { return hasVotes; }
            private set { SetProperty(ref hasVotes, value); }
        }

        public bool HasSelectableWords
        {
            get { return hasSelectableWords; }
            private set { SetProperty(ref hasSelectableWords, value); }
        }

        public bool CanVote
        {
            get { return canVote; }
            private set
            {
                if (SetProperty(ref canVote, value))
                {
                    voteCategoryCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanSelectWord
        {
            get { return canSelectWord; }
            private set
            {
                if (SetProperty(ref canSelectWord, value))
                {
                    selectWordCommand.RaiseCanExecuteChanged();
                    loadWordsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsVotingCategory
        {
            get { return isVotingCategory; }
            private set { SetProperty(ref isVotingCategory, value); }
        }

        public bool IsWaitingForHostWord
        {
            get { return isWaitingForHostWord; }
            private set { SetProperty(ref isWaitingForHostWord, value); }
        }

        public bool IsInProgress
        {
            get { return isInProgress; }
            private set { SetProperty(ref isInProgress, value); }
        }

        public bool IsWaitingForHostSelection
        {
            get { return isWaitingForHostSelection; }
            private set { SetProperty(ref isWaitingForHostSelection, value); }
        }

        public int RemainingVotingSeconds
        {
            get { return remainingVotingSeconds; }
            private set { SetProperty(ref remainingVotingSeconds, value); }
        }

        public ICommand LoadCommand
        {
            get { return loadCommand; }
        }

        public ICommand VoteCategoryCommand
        {
            get { return voteCategoryCommand; }
        }

        public ICommand ResolveVotingCommand
        {
            get { return resolveVotingCommand; }
        }

        public ICommand LoadWordsCommand
        {
            get { return loadWordsCommand; }
        }

        public ICommand SelectWordCommand
        {
            get { return selectWordCommand; }
        }

        public ICommand LeaveCommand
        {
            get { return leaveCommand; }
        }

        public bool HasNoVotes
        {
            get { return hasNoVotes; }
            private set { SetProperty(ref hasNoVotes, value); }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            CancelCountdown();

            UnsubscribeNotificationEvents();

            notificationCoordinator.Dispose();
        }

        private async Task LoadAsync()
        {
            await ExecuteServiceOperationAsync(
                "LoadMatchGameplayAsync",
                MatchGameplayServiceName,
                async () =>
                {
                    ClearMessages();

                    bool subscribed =
                        await notificationCoordinator.SubscribeToLobbyAsync(
                            CurrentLobby.MatchId);

                    if (!subscribed)
                    {
                        SetError(GetMatchServerMessage(LobbySubscriptionFailedCode));
                        return;
                    }

                    await LoadCategoriesAsync();
                    await RefreshCurrentLobbyAsync();

                    if (TryRequestGuessNavigation())
                    {
                        return;
                    }

                    await LoadVotingStateAsync();

                    if (CanSelectWord)
                    {
                        await LoadSelectableWordsInternalAsync();
                    }
                },
                null,
                GetCommands());
        }

        private async Task LoadCategoriesAsync()
        {
            CategoryOptionsOperationResult result =
                await matchGameplayWorkflow.GetCategoriesAsync();

            if (!result.Success)
            {
                ApplyFailure(result);
                return;
            }

            Categories.Clear();

            foreach (CategoryOptionModel category in result.Categories)
            {
                Categories.Add(category);
            }

            HasCategories = Categories.Any();

            if (SelectedCategory == null && Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        }

        private async Task LoadVotingStateAsync()
        {
            CategoryVotingOperationResult result =
                await matchGameplayWorkflow.GetCategoryVotingStateAsync(
                    CurrentLobby.MatchId);

            ApplyVotingResult(result, false);
        }

        private async Task VoteCategoryAsync()
        {
            await ExecuteServiceOperationAsync(
                "VoteCategoryAsync",
                MatchGameplayServiceName,
                async () =>
                {
                    ClearMessages();

                    if (SelectedCategory == null)
                    {
                        SetError(GetMatchServerMessage(InvalidCategoryIdCode));
                        return;
                    }

                    CategoryVotingOperationResult result =
                        await matchGameplayWorkflow.VoteCategoryAsync(
                            CurrentLobby.MatchId,
                            SelectedCategory.CategoryId);

                    ApplyVotingResult(result, true);
                },
                null,
                GetCommands());
        }

        private async Task ResolveCategoryVotingAsync()
        {
            await ExecuteServiceOperationAsync(
                "ResolveCategoryVotingAsync",
                MatchGameplayServiceName,
                async () =>
                {
                    ClearMessages();

                    CategoryVotingOperationResult result =
                        await matchGameplayWorkflow.ResolveCategoryVotingAsync(
                            CurrentLobby.MatchId);

                    ApplyVotingResult(result, true);

                    if (result != null && result.Success && result.Lobby != null)
                    {
                        CurrentLobby = result.Lobby;
                    }

                    if (CanSelectWord)
                    {
                        await LoadSelectableWordsInternalAsync();
                    }
                },
                null,
                GetCommands());
        }

        private async Task LoadSelectableWordsAsync()
        {
            await ExecuteServiceOperationAsync(
                "GetSelectableWordsAsync",
                MatchGameplayServiceName,
                async () =>
                {
                    ClearMessages();
                    await LoadSelectableWordsInternalAsync();
                },
                null,
                GetCommands());
        }

        private async Task LoadSelectableWordsInternalAsync()
        {
            SelectableWordsOperationResult result =
                await matchGameplayWorkflow.GetSelectableWordsAsync(
                    CurrentLobby.MatchId);

            if (!result.Success)
            {
                ApplyFailure(result);
                return;
            }

            SelectableWords.Clear();

            foreach (SelectableWordModel word in result.Words)
            {
                SelectableWords.Add(word);
            }

            HasSelectableWords = SelectableWords.Any();

            if (SelectedWord == null && SelectableWords.Count > 0)
            {
                SelectedWord = SelectableWords[0];
            }

            SetSuccess(GetMatchServerMessage(result.MessageCode));
        }

        private async Task SelectWordAsync()
        {
            await ExecuteServiceOperationAsync(
                "SelectWordAsync",
                MatchGameplayServiceName,
                async () =>
                {
                    ClearMessages();

                    if (SelectedWord == null)
                    {
                        SetError(GetMatchServerMessage(InvalidWordIdCode));
                        return;
                    }

                    WordSelectionOperationResult result =
                        await matchGameplayWorkflow.SelectWordAsync(
                            CurrentLobby.MatchId,
                            SelectedWord.WordId);

                    if (!result.Success)
                    {
                        ApplyFailure(result);
                        return;
                    }

                    if (result.Lobby != null)
                    {
                        CurrentLobby = result.Lobby;
                    }

                    CancelCountdown();

                    SetSuccess(GetMatchServerMessage(result.MessageCode));

                    TryRequestGuessNavigation();
                },
                null,
                GetCommands());
        }

        private async Task LeaveAsync()
        {
            await ExecuteServiceOperationAsync(
                "LeaveLobbyAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchLobbyOperationResult result =
                        await matchLobbyWorkflow.LeaveLobbyAsync(
                            CurrentLobby.MatchId);

                    if (!result.Success)
                    {
                        ApplyFailure(result);
                        return;
                    }

                    await notificationCoordinator.UnsubscribeFromLobbyAsync(
                        CurrentLobby.MatchId);

                    SetSuccess(GetMatchServerMessage(result.MessageCode));

                    BackRequested?.Invoke(this, EventArgs.Empty);
                },
                null,
                GetCommands());
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

            if (result.Lobby != null)
            {
                CurrentLobby = result.Lobby;
            }
        }

        private void ApplyVotingResult(
            CategoryVotingOperationResult result,
            bool showMessage)
        {
            if (result == null)
            {
                SetCommonUnexpectedError();
                return;
            }

            if (result.VotingState != null)
            {
                VotingState = result.VotingState;
            }

            if (!result.Success)
            {
                ApplyFailure(result);
                return;
            }

            if (result.Lobby != null)
            {
                CurrentLobby = result.Lobby;
            }

            if (showMessage)
            {
                SetSuccess(GetMatchServerMessage(result.MessageCode));
            }
        }

        private void ApplyVotingState(CategoryVotingStateModel state)
        {
            Votes.Clear();

            if (state != null && state.Votes != null)
            {
                foreach (CategoryVoteModel vote in state.Votes)
                {
                    Votes.Add(vote);
                }
            }

            HasVotes = Votes.Any();
            HasNoVotes = !HasVotes;
            RemainingVotingSeconds = state == null ? 0 : state.RemainingVotingSeconds;

            CanVote = state != null && state.CanVote;
            CanSelectWord = state != null && state.CanCurrentPlayerSelectWord;

            if (state != null)
            {
                StartCountdownIfNeeded(state);
            }

            UpdatePhaseState();
        }

        private void UpdatePhaseState()
        {
            IsVotingCategory =
                CurrentLobby != null &&
                CurrentLobby.IsVotingCategory;

            IsWaitingForHostWord =
                CurrentLobby != null &&
                CurrentLobby.IsWaitingForHostWord;

            IsInProgress =
                CurrentLobby != null &&
                CurrentLobby.IsInProgress;

            IsWaitingForHostSelection =
                IsWaitingForHostWord &&
                !CanSelectWord;

            RaiseCommandsCanExecuteChanged(GetCommands());
        }

        private bool TryRequestGuessNavigation()
        {
            if (hasRequestedGuessNavigation ||
                CurrentLobby == null ||
                !CurrentLobby.IsInProgress)
            {
                return false;
            }

            hasRequestedGuessNavigation = true;

            CancelCountdown();

            GuessRequested?.Invoke(
                this,
                new MatchGuessRequestedEventArgs(CurrentLobby));

            return true;
        }

        private void StartCountdownIfNeeded(CategoryVotingStateModel state)
        {
            CancelCountdown();

            if (state == null ||
                !state.CanVote ||
                !state.CategoryVotingEndsAt.HasValue)
            {
                return;
            }

            countdownCancellationTokenSource = new CancellationTokenSource();

            RunVotingCountdownAsync(
                state.CategoryVotingEndsAt.Value,
                countdownCancellationTokenSource.Token);
        }

        private async void RunVotingCountdownAsync(
            DateTime votingEndsAt,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int seconds = CalculateRemainingSeconds(votingEndsAt);

                    RemainingVotingSeconds = seconds;

                    if (seconds <= 0)
                    {
                        await ResolveCategoryVotingAsync();
                        return;
                    }

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
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

        private void CancelCountdown()
        {
            if (countdownCancellationTokenSource == null)
            {
                return;
            }

            countdownCancellationTokenSource.Cancel();
            countdownCancellationTokenSource.Dispose();
            countdownCancellationTokenSource = null;
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

        private void OnLobbyUpdated(object sender, MatchLobbyUpdatedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (CurrentLobby == null ||
                    CurrentLobby.MatchId != e.MatchId)
                {
                    return;
                }

                await RefreshFromNotificationAsync();
            });
        }

        private void OnMatchStatusChanged(object sender, MatchStatusChangedEventArgs e)
        {
            uiDispatcher.RunAsync(async () =>
            {
                if (CurrentLobby == null ||
                    CurrentLobby.MatchId != e.MatchId)
                {
                    return;
                }

                await RefreshFromNotificationAsync();
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
                    CurrentLobby == null ||
                    e.Message.MatchId != CurrentLobby.MatchId)
                {
                    return;
                }

                Chat?.AddIncomingMessage(e.Message);

                await Task.CompletedTask;
            });
        }

        private async Task RefreshFromNotificationAsync()
        {
            if (disposed ||
                isRefreshingFromNotification ||
                CurrentLobby == null)
            {
                return;
            }

            isRefreshingFromNotification = true;

            try
            {
                await RefreshCurrentLobbyAsync();

                if (CurrentLobby == null)
                {
                    return;
                }

                if (TryRequestGuessNavigation())
                {
                    return;
                }

                await LoadVotingStateAsync();

                if (CanSelectWord && !HasSelectableWords)
                {
                    await LoadSelectableWordsInternalAsync();
                }
            }
            catch (EndpointNotFoundException exception)
            {
                Logger.Error(
                    "RefreshFromNotificationAsync failed because the endpoint was not found.",
                    exception);

                SetCommonRuntimeError();
            }
            catch (TimeoutException exception)
            {
                Logger.Error(
                    "RefreshFromNotificationAsync failed due to timeout.",
                    exception);

                SetCommonRuntimeError();
            }
            catch (CommunicationException exception)
            {
                Logger.Error(
                    "RefreshFromNotificationAsync failed due to communication error.",
                    exception);

                SetCommonRuntimeError();
            }
            catch (Exception exception)
            {
                Logger.Error(
                    "RefreshFromNotificationAsync failed unexpectedly.",
                    exception);

                SetCommonUnexpectedError();
            }
            finally
            {
                isRefreshingFromNotification = false;
            }
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

                string messageCode = string.IsNullOrWhiteSpace(e.MessageCode)
                    ? LobbyClosedCode
                    : e.MessageCode;

                SetError(GetMatchServerMessage(messageCode));

                await Task.Delay(800);

                BackRequested?.Invoke(this, EventArgs.Empty);
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

        private bool CanVoteCategory()
        {
            return !IsBusy &&
                   CanVote &&
                   SelectedCategory != null;
        }

        private bool CanResolveVoting()
        {
            return !IsBusy &&
                   IsVotingCategory &&
                   RemainingVotingSeconds <= 0;
        }

        private bool CanLoadWords()
        {
            return !IsBusy && CanSelectWord;
        }

        private bool CanSelectWordCommand()
        {
            return !IsBusy &&
                   CanSelectWord &&
                   SelectedWord != null;
        }

        private bool CanLeave()
        {
            return !IsBusy && CurrentLobby != null;
        }

        private RelayCommand[] GetCommands()
        {
            return new[]
            {
                loadCommand,
                voteCategoryCommand,
                resolveVotingCommand,
                loadWordsCommand,
                selectWordCommand,
                leaveCommand
            };
        }

        private string GetMatchServerMessage(string messageCode)
        {
            return GetServerMessage(ServerMessageModuleName.Match, messageCode);
        }
    }
}
