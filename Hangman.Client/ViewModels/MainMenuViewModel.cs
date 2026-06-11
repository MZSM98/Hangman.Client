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
        private readonly RelayCommand logoutCommand;

        public MainMenuViewModel()
        {
            isHomeVisible = true;

            openProfileCommand = new RelayCommand(OpenProfile, CanExecuteNavigation);
            logoutCommand = new RelayCommand(Logout, CanExecuteNavigation);
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

        public ICommand LogoutCommand
        {
            get { return logoutCommand; }
        }

        private void OpenProfile()
        {
            ProfileViewModel profileViewModel = new ProfileViewModel();

            profileViewModel.BackRequested += OnProfileBackRequested;
            profileViewModel.ProfileDeleted += OnProfileDeleted;

            CurrentViewModel = profileViewModel;
            IsHomeVisible = false;

            profileViewModel.LoadProfileCommand.Execute(null);
        }

        private void OnProfileBackRequested(object sender, EventArgs e)
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
                profileViewModel.BackRequested -= OnProfileBackRequested;
                profileViewModel.ProfileDeleted -= OnProfileDeleted;
            }

            CurrentViewModel = null;
        }

        private void Logout()
        {
            UserSession.Clear();
            RaiseSessionClosed();
        }

        private bool CanExecuteNavigation()
        {
            return true;
        }

        private void RaiseSessionClosed()
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
