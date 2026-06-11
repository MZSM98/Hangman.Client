using Hangman.Client.ViewModels.Base;
using System;

namespace Hangman.Client.ViewModels
{
    public class LoginMenuViewModel : BaseViewModel
    {
        private BaseViewModel currentViewModel;

        public LoginMenuViewModel()
        {
            ShowLogin();
        }

        public event EventHandler LoginSucceeded;

        public event EventHandler<InformationMessageRequestedEventArgs>
            InformationMessageRequested;

        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            private set { SetProperty(ref currentViewModel, value); }
        }

        private void Navigate(BaseViewModel nextViewModel)
        {
            UnsubscribeFromCurrentViewModel();

            CurrentViewModel = nextViewModel;

            SubscribeToCurrentViewModel();
        }

        private void ShowLogin()
        {
            Navigate(new LoginViewModel());
        }

        private void ShowRegister()
        {
            Navigate(new RegisterViewModel());
        }

        private void ShowVerifyEmail(string email)
        {
            Navigate(new VerifyEmailViewModel(email));
        }

        private void ShowRequestPasswordReset(string email)
        {
            Navigate(new RequestPasswordResetViewModel(email));
        }

        private void ShowResetPassword(string email)
        {
            Navigate(new ResetPasswordViewModel(email));
        }

        private void SubscribeToCurrentViewModel()
        {
            if (CurrentViewModel is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginSucceeded += OnLoginSucceeded;
                loginViewModel.RegisterRequested += OnRegisterRequested;
                loginViewModel.PasswordResetRequested += OnPasswordResetRequested;
                return;
            }

            if (CurrentViewModel is RegisterViewModel registerViewModel)
            {
                registerViewModel.LoginRequested += OnRegisterLoginRequested;
                registerViewModel.VerificationRequired += OnVerificationRequired;
                return;
            }

            if (CurrentViewModel is VerifyEmailViewModel verifyEmailViewModel)
            {
                verifyEmailViewModel.LoginRequested += OnVerifyLoginRequested;
                verifyEmailViewModel.VerificationSucceeded += OnVerificationSucceeded;
                return;
            }


            if (CurrentViewModel is RequestPasswordResetViewModel requestPasswordResetViewModel)
            {
                requestPasswordResetViewModel.LoginRequested += OnRequestResetLoginRequested;
                requestPasswordResetViewModel.ResetCodeRequested += OnResetCodeRequested;
                return;
            }


            if (CurrentViewModel is ResetPasswordViewModel resetPasswordViewModel)
            {
                resetPasswordViewModel.LoginRequested += OnResetPasswordLoginRequested;
                resetPasswordViewModel.PasswordResetSucceeded += OnPasswordResetSucceeded;
            }
        }

        private void UnsubscribeFromCurrentViewModel()
        {
            if (CurrentViewModel is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginSucceeded -= OnLoginSucceeded;
                loginViewModel.RegisterRequested -= OnRegisterRequested;
                loginViewModel.PasswordResetRequested -= OnPasswordResetRequested;
                return;
            }

            if (CurrentViewModel is RegisterViewModel registerViewModel)
            {
                registerViewModel.LoginRequested -= OnRegisterLoginRequested;
                registerViewModel.VerificationRequired -= OnVerificationRequired;
                return;
            }

            if (CurrentViewModel is VerifyEmailViewModel verifyEmailViewModel)
            {
                verifyEmailViewModel.LoginRequested -= OnVerifyLoginRequested;
                verifyEmailViewModel.VerificationSucceeded -= OnVerificationSucceeded;
                return;
            }


            if (CurrentViewModel is RequestPasswordResetViewModel requestPasswordResetViewModel)
            {
                requestPasswordResetViewModel.LoginRequested -= OnRequestResetLoginRequested;
                requestPasswordResetViewModel.ResetCodeRequested -= OnResetCodeRequested;
                return;
            }


            if (CurrentViewModel is ResetPasswordViewModel resetPasswordViewModel)
            {
                resetPasswordViewModel.LoginRequested -= OnResetPasswordLoginRequested;
                resetPasswordViewModel.PasswordResetSucceeded -= OnPasswordResetSucceeded;
            }
        }

        private void OnLoginSucceeded(object sender, EventArgs e)
        {
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }

        private void OnRegisterRequested(object sender, EventArgs e)
        {
            ShowRegister();
        }

        private void OnPasswordResetRequested(
            object sender,
            PasswordResetRequestedEventArgs e)
        {
            ShowRequestPasswordReset(e.Email);
        }

        private void OnRegisterLoginRequested(object sender, EventArgs e)
        {
            ShowLogin();
        }

        private void OnVerificationRequired(
            object sender,
            VerificationRequiredEventArgs e)
        {
            ShowVerifyEmail(e.Email);
        }

        private void OnVerifyLoginRequested(object sender, EventArgs e)
        {
            ShowLogin();
        }

        private void OnVerificationSucceeded(object sender, EventArgs e)
        {
            RaiseInformationMessageRequested(!(sender is VerifyEmailViewModel viewModel) ? 
                string.Empty : viewModel.SuccessMessage);

            ShowLogin();
        }

        private void OnRequestResetLoginRequested(object sender, EventArgs e)
        {
            ShowLogin();
        }

        private void OnResetCodeRequested(
            object sender,
            ResetCodeRequestedEventArgs e)
        {
            ShowResetPassword(e.Email);
        }

        private void OnResetPasswordLoginRequested(object sender, EventArgs e)
        {
            ShowLogin();
        }

        private void OnPasswordResetSucceeded(object sender, EventArgs e)
        {
            RaiseInformationMessageRequested(!(sender is ResetPasswordViewModel viewModel) ? 
                string.Empty : viewModel.SuccessMessage);

            ShowLogin();
        }

        private void RaiseInformationMessageRequested(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            InformationMessageRequested?.Invoke(this, new InformationMessageRequestedEventArgs(message));
        }
    }

    public class InformationMessageRequestedEventArgs : EventArgs
    {
        public InformationMessageRequestedEventArgs(string message)
        {
            Message = message ?? string.Empty;
        }

        public string Message { get; private set; }
    }
}
