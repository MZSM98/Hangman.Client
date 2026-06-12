using Hangman.Client.Models.Auth;
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
            UserSession.Clear();
            RaiseSessionClosed();
        }

        private void RaiseSessionClosed()
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
