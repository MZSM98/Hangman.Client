using Hangman.Client.Models.Auth;
using Hangman.Client.Models.Match;
using Hangman.Client.ViewModels.Base;
using System;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        private BaseViewModel currentViewModel;
        private bool isHomeVisible;

        private readonly RelayCommand openProfileCommand;
        private readonly RelayCommand openMatchLobbyCommand;
        private readonly RelayCommand logoutCommand;

        private MatchChatViewModel activeMatchChat;

        public MainMenuViewModel()
        {
            isHomeVisible = true;

            openProfileCommand = new RelayCommand(OpenProfile);
            openMatchLobbyCommand = new RelayCommand(OpenMatchLobby);
            logoutCommand = new RelayCommand(Logout);
        }

        public event EventHandler SessionClosed;

        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            private set { SetProperty(ref currentViewModel, value); }
        }

        public bool IsHomeVisible
        {
            get { return isHomeVisible; }
            private set { SetProperty(ref isHomeVisible, value); }
        }

        public ICommand OpenProfileCommand
        {
            get { return openProfileCommand; }
        }

        public ICommand OpenMatchLobbyCommand
        {
            get { return openMatchLobbyCommand; }
        }

        public ICommand LogoutCommand
        {
            get { return logoutCommand; }
        }

        private void OpenProfile()
        {
            CloseCurrentViewModel();

            ProfileViewModel profileViewModel = new ProfileViewModel();

            profileViewModel.BackRequested += OnChildBackRequested;
            profileViewModel.ProfileDeleted += OnProfileDeleted;

            CurrentViewModel = profileViewModel;
            IsHomeVisible = false;

            profileViewModel.LoadProfileCommand.Execute(null);
        }

        private void OpenMatchLobby()
        {
            CloseCurrentViewModel();

            MatchLobbyViewModel matchLobbyViewModel = new MatchLobbyViewModel();

            matchLobbyViewModel.BackRequested += OnChildBackRequested;
            matchLobbyViewModel.GameplayRequested += OnGameplayRequested;

            CurrentViewModel = matchLobbyViewModel;
            IsHomeVisible = false;

            matchLobbyViewModel.RefreshLobbiesCommand.Execute(null);
        }

        private void OnChildBackRequested(object sender, EventArgs e)
        {
            CloseCurrentViewModel();
            IsHomeVisible = true;
        }

        private void OnProfileDeleted(object sender, EventArgs e)
        {
            CloseCurrentViewModel();
            RaiseSessionClosed();
        }

        private void OnGameplayRequested(object sender, MatchGameplayRequestedEventArgs e)
        {
            if (e == null || e.Lobby == null)
            {
                return;
            }

            OpenMatchGameplay(e.Lobby);
        }

        private void OnGuessRequested(object sender, MatchGuessRequestedEventArgs e)
        {
            if (e == null || e.Lobby == null)
            {
                return;
            }

            OpenMatchGuess(e.Lobby);
        }

        private void OpenMatchGameplay(MatchLobbyModel lobby)
        {
            CloseCurrentViewModel();

            MatchChatViewModel chatViewModel =
                GetOrCreateMatchChat(lobby.MatchId);

            MatchGameplayViewModel matchGameplayViewModel =
                new MatchGameplayViewModel(lobby, chatViewModel);

            matchGameplayViewModel.BackRequested += OnChildBackRequested;
            matchGameplayViewModel.GuessRequested += OnGuessRequested;

            CurrentViewModel = matchGameplayViewModel;
            IsHomeVisible = false;

            matchGameplayViewModel.LoadCommand.Execute(null);
        }

        private void OpenMatchGuess(MatchLobbyModel lobby)
        {
            CloseCurrentViewModel();

            MatchChatViewModel chatViewModel =
                GetOrCreateMatchChat(lobby.MatchId);

            MatchGuessViewModel matchGuessViewModel =
                new MatchGuessViewModel(lobby, chatViewModel);

            matchGuessViewModel.BackRequested += OnChildBackRequested;
            matchGuessViewModel.ResultRequested += OnMatchResultRequested;

            CurrentViewModel = matchGuessViewModel;
            IsHomeVisible = false;

            matchGuessViewModel.LoadCommand.Execute(null);
        }

        private void OnMatchResultRequested(
            object sender,
            MatchResultRequestedEventArgs e)
        {
            if (e == null || e.GameState == null)
            {
                return;
            }

            OpenMatchResult(e.GameState);
        }

        private void OpenMatchResult(MatchGameStateModel gameState)
        {
            CloseCurrentViewModel();

            MatchChatViewModel chatViewModel =
                GetOrCreateMatchChat(gameState.MatchId);

            MatchResultViewModel matchResultViewModel =
                new MatchResultViewModel(gameState, chatViewModel);

            matchResultViewModel.ExitRequested += OnMatchResultExitRequested;

            CurrentViewModel = matchResultViewModel;
            IsHomeVisible = false;

            matchResultViewModel.InitializeChatCommand.Execute(null);
        }

        private void OnMatchResultExitRequested(object sender, EventArgs e)
        {
            ClearActiveMatchChat();
            OpenMatchLobby();
        }

        private MatchChatViewModel GetOrCreateMatchChat(int matchId)
        {
            if (matchId <= 0)
            {
                return null;
            }

            if (activeMatchChat != null &&
                activeMatchChat.MatchId == matchId)
            {
                return activeMatchChat;
            }

            ClearActiveMatchChat();

            activeMatchChat = new MatchChatViewModel(matchId);

            return activeMatchChat;
        }

        private void ClearActiveMatchChat()
        {
            if (activeMatchChat == null)
            {
                return;
            }

            activeMatchChat.Dispose();
            activeMatchChat = null;
        }

        private void CloseCurrentViewModel()
        {
            if (CurrentViewModel is ProfileViewModel profileViewModel)
            {
                profileViewModel.BackRequested -= OnChildBackRequested;
                profileViewModel.ProfileDeleted -= OnProfileDeleted;
            }

            if (CurrentViewModel is MatchLobbyViewModel matchLobbyViewModel)
            {
                matchLobbyViewModel.BackRequested -= OnChildBackRequested;
                matchLobbyViewModel.GameplayRequested -= OnGameplayRequested;
            }

            if (CurrentViewModel is MatchGameplayViewModel matchGameplayViewModel)
            {
                matchGameplayViewModel.BackRequested -= OnChildBackRequested;
                matchGameplayViewModel.GuessRequested -= OnGuessRequested;
            }

            if (CurrentViewModel is MatchGuessViewModel matchGuessViewModel)
            {
                matchGuessViewModel.BackRequested -= OnChildBackRequested;
                matchGuessViewModel.ResultRequested -= OnMatchResultRequested;
            }

            if (CurrentViewModel is MatchResultViewModel matchResultViewModel)
            {
                matchResultViewModel.ExitRequested -= OnMatchResultExitRequested;
            }

            if (CurrentViewModel is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
            }

            CurrentViewModel = null;
        }

        private void Logout()
        {
            CloseCurrentViewModel();
            ClearActiveMatchChat();
            UserSession.Clear();
            RaiseSessionClosed();
        }

        private void RaiseSessionClosed()
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
