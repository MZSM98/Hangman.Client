using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using Hangman.Client.ViewModels.Base;
using Hangman.Client.ViewModels.Match;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public sealed class MatchGuessViewModel : ServiceViewModelBase, IDisposable
    {
        private const string MatchGuessServiceName = "MatchGuessService";
        private const string MatchServiceName = "MatchService";
        private const string LobbyClosedCode = "LobbyClosed";

        private readonly IMatchGuessOperationHandler operationHandler;
        private readonly IMatchGuessSlotLayoutCalculator slotLayoutCalculator;
        private readonly IMatchGuessStateProjector stateProjector;
        private readonly IMatchGuessKeyboardController keyboardController;
        private readonly IMatchGuessCountdownController countdownController;
        private readonly IMatchGuessNotificationController notificationController;
        private readonly IMatchGuessRoleEvaluator roleEvaluator;

        private readonly RelayCommand loadCommand;
        private readonly RelayCommand guessWordCommand;
        private readonly RelayCommand resolveTimeoutCommand;
        private readonly RelayCommand surrenderCommand;
        private readonly RelayCommand openWordGuessPanelCommand;
        private readonly RelayCommand cancelWordGuessCommand;

        private readonly ObservableCollection<LetterSlotModel> letterSlots;
        private readonly ObservableCollection<GuessHistoryModel> guessHistory;

        private MatchGuessSlotLayoutModel slotLayout;
        private MatchLobbyModel currentLobby;
        private MatchGameStateModel gameState;
        private HangmanFigureModel hangmanFigure;
        private string guessWordText;
        private int remainingSeconds;
        private int failedAttempts;
        private int maxAttempts;
        private bool isFinished;
        private bool hasGameState;
        private bool isWordGuessPanelVisible;
        private bool hasRequestedResultNavigation;
        private bool disposed;

        public MatchGuessViewModel(MatchLobbyModel lobby)
            : this(
                  lobby,
                  (MatchChatViewModel)null)
        {
        }

        public MatchGuessViewModel(
            MatchLobbyModel lobby,
            MatchChatViewModel chat)
            : this(
                  lobby,
                  MatchGuessViewModelDependencies.CreateDefault(),
                  chat)
        {
        }

        internal MatchGuessViewModel(
            MatchLobbyModel lobby,
            MatchGuessViewModelDependencies dependencies)
            : this(
                  lobby,
                  dependencies,
                  null)
        {
        }

        internal MatchGuessViewModel(
            MatchLobbyModel lobby,
            MatchGuessViewModelDependencies dependencies,
            MatchChatViewModel chat)
            : base(
                  GetValidationMessageProvider(dependencies),
                  GetServerMessageProvider(dependencies),
                  GetLogger(dependencies))
        {
            dependencies.Validate();

            CurrentLobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
            Chat = chat;

            operationHandler = dependencies.OperationHandler;
            notificationController = dependencies.NotificationController;
            slotLayoutCalculator = dependencies.SlotLayoutCalculator;
            countdownController = dependencies.CountdownController;
            stateProjector = dependencies.StateProjector;
            roleEvaluator = dependencies.RoleEvaluator;

            SlotLayout = slotLayoutCalculator.Calculate(0);

            letterSlots = new ObservableCollection<LetterSlotModel>();
            guessHistory = new ObservableCollection<GuessHistoryModel>();

            loadCommand = new RelayCommand(
                LoadAsync,
                CanExecuteWhenNotBusy);

            guessWordCommand = new RelayCommand(
                GuessWordAsync,
                CanGuessWord);

            resolveTimeoutCommand = new RelayCommand(
                ResolveGuessTimeoutAsync,
                CanResolveTimeout);

            surrenderCommand = new RelayCommand(
                SurrenderAsync,
                CanSurrender);

            openWordGuessPanelCommand = new RelayCommand(
                OpenWordGuessPanel,
                CanOpenWordGuessPanel);

            cancelWordGuessCommand = new RelayCommand(
                CancelWordGuessPanel,
                CanCancelWordGuessPanel);

            keyboardController = new MatchGuessKeyboardController(
                GuessLetterAsync,
                CanGuess);

            notificationController.Configure(
                () => CurrentLobby == null ? 0 : CurrentLobby.MatchId,
                () => LoadGameStateInternalAsync(false),
                HandleLobbyClosedAsync,
                AddIncomingChatMessage,
                SetCommonRuntimeError,
                SetCommonUnexpectedError);
        }

        public event EventHandler BackRequested;

        public event EventHandler<MatchResultRequestedEventArgs> ResultRequested;

        public MatchChatViewModel Chat { get; private set; }

        public MatchLobbyModel CurrentLobby
        {
            get { return currentLobby; }
            private set { SetProperty(ref currentLobby, value); }
        }

        public MatchGameStateModel GameState
        {
            get { return gameState; }
            private set
            {
                if (SetProperty(ref gameState, value))
                {
                    HasGameState = value != null;
                    NotifyRolePropertiesChanged();
                }
            }
        }

        public MatchGuessSlotLayoutModel SlotLayout
        {
            get { return slotLayout; }
            private set { SetProperty(ref slotLayout, value); }
        }

        public bool IsCurrentUserHost
        {
            get { return roleEvaluator.IsCurrentUserHost(GameState); }
        }

        public bool IsCurrentUserGuest
        {
            get { return roleEvaluator.IsCurrentUserGuest(GameState); }
        }

        public bool IsGuestControlsVisible
        {
            get
            {
                return roleEvaluator.CanShowGuestControls(
                    GameState,
                    IsFinished);
            }
        }

        public bool IsHostReadOnlyVisible
        {
            get { return roleEvaluator.CanShowHostReadOnly(GameState); }
        }

        public bool IsTimerVisible
        {
            get
            {
                return roleEvaluator.CanShowTimer(
                    GameState,
                    IsFinished);
            }
        }

        public string HintText
        {
            get
            {
                return GameState == null
                    ? string.Empty
                    : GameState.WordDescription;
            }
        }

        public int RemainingAttempts
        {
            get
            {
                int remaining = MaxAttempts - FailedAttempts;

                return remaining < 0 ? 0 : remaining;
            }
        }

        public bool IsWordGuessPanelVisible
        {
            get { return isWordGuessPanelVisible; }
            private set
            {
                if (SetProperty(ref isWordGuessPanelVisible, value))
                {
                    OnPropertyChanged(nameof(IsWordGuessInitialButtonVisible));

                    openWordGuessPanelCommand?.RaiseCanExecuteChanged();
                    cancelWordGuessCommand?.RaiseCanExecuteChanged();
                    guessWordCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWordGuessInitialButtonVisible
        {
            get { return IsGuestControlsVisible && !IsWordGuessPanelVisible; }
        }

        public ICommand OpenWordGuessPanelCommand
        {
            get { return openWordGuessPanelCommand; }
        }

        public ICommand CancelWordGuessCommand
        {
            get { return cancelWordGuessCommand; }
        }

        public ObservableCollection<LetterKeyModel> LetterKeys
        {
            get { return keyboardController.LetterKeys; }
        }

        public ObservableCollection<LetterSlotModel> LetterSlots
        {
            get { return letterSlots; }
        }

        public ObservableCollection<GuessHistoryModel> GuessHistory
        {
            get { return guessHistory; }
        }

        public HangmanFigureModel HangmanFigure
        {
            get { return hangmanFigure; }
            private set
            {
                if (SetProperty(ref hangmanFigure, value))
                {
                    OnPropertyChanged(nameof(ShowHead));
                    OnPropertyChanged(nameof(ShowTorso));
                    OnPropertyChanged(nameof(ShowLeftArm));
                    OnPropertyChanged(nameof(ShowRightArm));
                    OnPropertyChanged(nameof(ShowLeftLeg));
                    OnPropertyChanged(nameof(ShowRightLeg));
                }
            }
        }

        public string GuessWordText
        {
            get { return guessWordText; }
            set
            {
                if (SetProperty(ref guessWordText, value))
                {
                    guessWordCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public int RemainingSeconds
        {
            get { return remainingSeconds; }
            private set
            {
                if (SetProperty(ref remainingSeconds, value))
                {
                    resolveTimeoutCommand.RaiseCanExecuteChanged();
                    keyboardController.RaiseCanExecuteChanged();
                    guessWordCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public int FailedAttempts
        {
            get { return failedAttempts; }
            private set
            {
                if (SetProperty(ref failedAttempts, value))
                {
                    OnPropertyChanged(nameof(RemainingAttempts));
                }
            }
        }

        public int MaxAttempts
        {
            get { return maxAttempts; }
            private set
            {
                if (SetProperty(ref maxAttempts, value))
                {
                    OnPropertyChanged(nameof(RemainingAttempts));
                }
            }
        }

        public bool IsFinished
        {
            get { return isFinished; }
            private set
            {
                if (SetProperty(ref isFinished, value))
                {
                    RaiseCommandsCanExecuteChanged(GetCommands());
                    keyboardController.RaiseCanExecuteChanged();
                }
            }
        }

        public bool HasGameState
        {
            get { return hasGameState; }
            private set { SetProperty(ref hasGameState, value); }
        }

        public bool ShowHead
        {
            get { return HangmanFigure != null && HangmanFigure.ShowHead; }
        }

        public bool ShowTorso
        {
            get { return HangmanFigure != null && HangmanFigure.ShowTorso; }
        }

        public bool ShowLeftArm
        {
            get { return HangmanFigure != null && HangmanFigure.ShowLeftArm; }
        }

        public bool ShowRightArm
        {
            get { return HangmanFigure != null && HangmanFigure.ShowRightArm; }
        }

        public bool ShowLeftLeg
        {
            get { return HangmanFigure != null && HangmanFigure.ShowLeftLeg; }
        }

        public bool ShowRightLeg
        {
            get { return HangmanFigure != null && HangmanFigure.ShowRightLeg; }
        }

        public ICommand LoadCommand
        {
            get { return loadCommand; }
        }

        public ICommand GuessWordCommand
        {
            get { return guessWordCommand; }
        }

        public ICommand ResolveTimeoutCommand
        {
            get { return resolveTimeoutCommand; }
        }

        public ICommand SurrenderCommand
        {
            get { return surrenderCommand; }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            countdownController.Dispose();
            notificationController.Dispose();
        }

        private static ClientValidationMessageProvider GetValidationMessageProvider(
            MatchGuessViewModelDependencies dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            if (dependencies.ValidationMessageProvider == null)
            {
                throw new InvalidOperationException(
                    "ValidationMessageProvider is required.");
            }

            return dependencies.ValidationMessageProvider;
        }

        private static IServerMessageProvider GetServerMessageProvider(
            MatchGuessViewModelDependencies dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            if (dependencies.ServerMessageProvider == null)
            {
                throw new InvalidOperationException(
                    "ServerMessageProvider is required.");
            }

            return dependencies.ServerMessageProvider;
        }

        private static IClientLogger GetLogger(
            MatchGuessViewModelDependencies dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            if (dependencies.Logger == null)
            {
                throw new InvalidOperationException(
                    "Logger is required.");
            }

            return dependencies.Logger;
        }

        private void NotifyRolePropertiesChanged()
        {
            OnPropertyChanged(nameof(IsCurrentUserHost));
            OnPropertyChanged(nameof(IsCurrentUserGuest));
            OnPropertyChanged(nameof(IsGuestControlsVisible));
            OnPropertyChanged(nameof(IsHostReadOnlyVisible));
            OnPropertyChanged(nameof(IsTimerVisible));
            OnPropertyChanged(nameof(HintText));
            OnPropertyChanged(nameof(IsWordGuessInitialButtonVisible));
        }

        private async Task HandleLobbyClosedAsync(MatchLobbyClosedEventArgs e)
        {
            string messageCode = string.IsNullOrWhiteSpace(e.MessageCode)
                ? LobbyClosedCode
                : e.MessageCode;

            SetError(GetMatchServerMessage(messageCode));

            await Task.Delay(800);

            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void AddIncomingChatMessage(MatchChatMessageModel message)
        {
            Chat?.AddIncomingMessage(message);
        }

        private void ReplaceLetterSlots(IEnumerable<LetterSlotModel> slots)
        {
            LetterSlots.Clear();

            if (slots == null)
            {
                SlotLayout = slotLayoutCalculator.Calculate(0);
                return;
            }

            foreach (LetterSlotModel slot in slots)
            {
                LetterSlots.Add(slot);
            }

            SlotLayout = slotLayoutCalculator.Calculate(LetterSlots.Count);
        }

        private void ReplaceGuessHistory(IEnumerable<GuessHistoryModel> history)
        {
            GuessHistory.Clear();

            if (history == null)
            {
                return;
            }

            foreach (GuessHistoryModel guess in history)
            {
                GuessHistory.Add(guess);
            }
        }

        private void ResetStateValues()
        {
            FailedAttempts = 0;
            MaxAttempts = 0;
            RemainingSeconds = 0;
            IsFinished = true;
            HangmanFigure = new HangmanFigureModel();
        }

        private void ApplyProjectedValues(MatchGuessStateProjection projection)
        {
            FailedAttempts = projection.FailedAttempts;
            MaxAttempts = projection.MaxAttempts;
            RemainingSeconds = projection.RemainingSeconds;
            IsFinished = projection.IsFinished;
            HangmanFigure = projection.HangmanFigure;
        }

        private void ResetWordGuessPanelIfNeeded()
        {
            if (IsCurrentUserGuest)
            {
                return;
            }

            GuessWordText = string.Empty;
            IsWordGuessPanelVisible = false;
        }

        private async Task LoadAsync()
        {
            await ExecuteServiceOperationAsync(
                "LoadMatchGuessAsync",
                MatchGuessServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchGuessOperationResult result =
                        await operationHandler.SubscribeAndLoadGameStateAsync(
                            CurrentLobby.MatchId);

                    ApplyGameStateResult(result, false);
                },
                OnMatchGuessOperationFinished,
                GetCommands());
        }

        private async Task GuessLetterAsync(string letter)
        {
            await ExecuteServiceOperationAsync(
                "GuessLetterAsync",
                MatchGuessServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchGuessOperationResult result =
                        await operationHandler.GuessLetterAsync(
                            CurrentLobby.MatchId,
                            letter);

                    ApplyGuessResult(result, true);
                },
                OnMatchGuessOperationFinished,
                GetCommands());
        }

        private async Task GuessWordAsync()
        {
            await ExecuteServiceOperationAsync(
                "GuessWordAsync",
                MatchGuessServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchGuessOperationResult result =
                        await operationHandler.GuessWordAsync(
                            CurrentLobby.MatchId,
                            GuessWordText);

                    ApplyGuessResult(result, true);

                    if (result != null && result.Success)
                    {
                        GuessWordText = string.Empty;
                        IsWordGuessPanelVisible = false;
                    }
                },
                OnMatchGuessOperationFinished,
                GetCommands());
        }

        private async Task ResolveGuessTimeoutAsync()
        {
            await ExecuteServiceOperationAsync(
                "ResolveGuessTimeoutAsync",
                MatchGuessServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchGuessOperationResult result =
                        await operationHandler.ResolveTimeoutAsync(
                            CurrentLobby.MatchId);

                    ApplyGuessResult(result, true);
                },
                OnMatchGuessOperationFinished,
                GetCommands());
        }

        private async Task SurrenderAsync()
        {
            await ExecuteServiceOperationAsync(
                "SurrenderMatchAsync",
                MatchServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchLobbyOperationResult result =
                        await operationHandler.SurrenderAsync(
                            CurrentLobby.MatchId);

                    if (result == null)
                    {
                        SetCommonUnexpectedError();
                        return;
                    }

                    if (!result.Success)
                    {
                        ApplyFailure(result);
                        return;
                    }

                    SetSuccess(GetMatchServerMessage(result.MessageCode));

                    await Task.Delay(800);

                    BackRequested?.Invoke(this, EventArgs.Empty);
                },
                OnMatchGuessOperationFinished,
                GetCommands());
        }

        private async Task LoadGameStateInternalAsync(bool showSuccessMessage)
        {
            MatchGuessOperationResult result =
                await operationHandler.LoadGameStateAsync(
                    CurrentLobby.MatchId);

            ApplyGameStateResult(result, showSuccessMessage);
        }

        private void ApplyGameStateResult(
            MatchGuessOperationResult result,
            bool showSuccessMessage)
        {
            if (result == null)
            {
                SetCommonUnexpectedError();
                return;
            }

            if (result.GameState != null)
            {
                ApplyGameState(result.GameState);
            }

            if (!result.Success)
            {
                ApplyFailure(result);
                return;
            }

            if (showSuccessMessage)
            {
                SetSuccess(GetMatchServerMessage(result.MessageCode));
            }
        }

        private void ApplyGuessResult(
            MatchGuessOperationResult result,
            bool showMessage)
        {
            if (result == null)
            {
                SetCommonUnexpectedError();
                return;
            }

            if (result.GameState != null)
            {
                ApplyGameState(result.GameState);
            }

            if (!result.Success)
            {
                ApplyFailure(result);
                return;
            }

            if (showMessage && !result.MatchFinished)
            {
                SetSuccess(GetMatchServerMessage(result.MessageCode));
            }
        }

        private void ApplyGameState(MatchGameStateModel state)
        {
            GameState = state;

            MatchGuessStateProjection projection =
                stateProjector.Project(state);

            ReplaceLetterSlots(projection.LetterSlots);
            ReplaceGuessHistory(projection.GuessHistory);

            if (!projection.HasState)
            {
                ResetStateValues();
                NotifyRolePropertiesChanged();
                SynchronizeKeyboard();
                countdownController.Cancel();
                return;
            }

            ApplyProjectedValues(projection);

            NotifyRolePropertiesChanged();

            ResetWordGuessPanelIfNeeded();

            SynchronizeKeyboard();

            if (projection.IsFinished)
            {
                countdownController.Cancel();
                TryRequestResultNavigation(state);
                return;
            }

            StartCountdown(projection);
        }

        private void TryRequestResultNavigation(MatchGameStateModel state)
        {
            if (hasRequestedResultNavigation ||
                state == null ||
                !state.IsFinished)
            {
                return;
            }

            hasRequestedResultNavigation = true;

            ResultRequested?.Invoke(
                this,
                new MatchResultRequestedEventArgs(state));
        }

        private void StartCountdown(MatchGuessStateProjection projection)
        {
            countdownController.Start(
                projection?.GuessTurnEndsAt,
                projection == null || projection.IsFinished,
                seconds => RemainingSeconds = seconds,
                ResolveGuessTimeoutAsync);
        }

        private void SynchronizeKeyboard()
        {
            keyboardController.SynchronizeWithHistory(
                GuessHistory,
                CanEnableUnusedLetterKey());
        }

        private bool CanEnableUnusedLetterKey()
        {
            return roleEvaluator.CanEnableUnusedLetterKey(
                GameState,
                RemainingSeconds);
        }

        private void OnMatchGuessOperationFinished()
        {
            if (GameState != null)
            {
                SynchronizeKeyboard();
            }

            NotifyRolePropertiesChanged();

            RaiseCommandsCanExecuteChanged(GetCommands());
            keyboardController.RaiseCanExecuteChanged();
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

        private bool CanGuess()
        {
            return !IsBusy &&
                   CanEnableUnusedLetterKey();
        }

        private bool CanGuessWord()
        {
            return CanGuess() &&
                   !string.IsNullOrWhiteSpace(GuessWordText);
        }

        private bool CanResolveTimeout()
        {
            return !IsBusy &&
                   roleEvaluator.CanResolveTimeout(
                       GameState,
                       RemainingSeconds);
        }

        private bool CanSurrender()
        {
            return !IsBusy && CurrentLobby != null;
        }

        private void OpenWordGuessPanel()
        {
            IsWordGuessPanelVisible = true;
        }

        private void CancelWordGuessPanel()
        {
            GuessWordText = string.Empty;
            IsWordGuessPanelVisible = false;
        }

        private bool CanOpenWordGuessPanel()
        {
            return CanGuess() && !IsWordGuessPanelVisible;
        }

        private bool CanCancelWordGuessPanel()
        {
            return !IsBusy && IsWordGuessPanelVisible;
        }

        private RelayCommand[] GetCommands()
        {
            return new[]
            {
                loadCommand,
                guessWordCommand,
                resolveTimeoutCommand,
                surrenderCommand,
                openWordGuessPanelCommand,
                cancelWordGuessCommand
            };
        }

        private string GetMatchServerMessage(string messageCode)
        {
            return GetServerMessage(ServerMessageModuleName.Match, messageCode);
        }
    }
}