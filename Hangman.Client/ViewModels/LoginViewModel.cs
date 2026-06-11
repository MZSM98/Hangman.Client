using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Auth;
using Hangman.Client.Services.Auth;
using Hangman.Client.Validators;
using Hangman.Client.Validators.Auth;
using Hangman.Client.ViewModels.Base;
using Hangman.Contracts.Auth;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public class LoginViewModel : AuthViewModelBase
    {
        private readonly RelayCommand loginCommand;
        private readonly RelayCommand openRegisterCommand;
        private readonly RelayCommand openPasswordResetCommand;

        private readonly LoginFormModel loginForm;

        public LoginViewModel()
            : this(
                  new AuthClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<LoginViewModel>())
        {
        }

        public LoginViewModel(
            IAuthClient authClient,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : base(
                  authClient,
                  validationMessageProvider,
                  serverMessageProvider,
                  logger)
        {
            loginForm = new LoginFormModel();

            loginCommand = new RelayCommand(
                async () => await LoginAsync(),
                CanExecuteWhenNotBusy);

            openRegisterCommand = new RelayCommand(
                RequestOpenRegister,
                CanExecuteWhenNotBusy);

            openPasswordResetCommand = new RelayCommand(
                RequestOpenPasswordReset,
                CanExecuteWhenNotBusy);
        }

        public event EventHandler LoginSucceeded;

        public event EventHandler RegisterRequested;

        public event EventHandler<PasswordResetRequestedEventArgs>
            PasswordResetRequested;

        public event EventHandler PasswordClearRequested;

        public string Email
        {
            get { return loginForm.Email; }
            set
            {
                if (loginForm.Email == value)
                {
                    return;
                }

                loginForm.Email = value;
                OnPropertyChanged();

                loginCommand.RaiseCanExecuteChanged();
                openPasswordResetCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoginCommand
        {
            get { return loginCommand; }
        }

        public ICommand OpenRegisterCommand
        {
            get { return openRegisterCommand; }
        }

        public ICommand OpenPasswordResetCommand
        {
            get { return openPasswordResetCommand; }
        }

        public void SetPassword(string password)
        {
            loginForm.Password = password ?? string.Empty;
            loginCommand.RaiseCanExecuteChanged();
        }

        private async Task LoginAsync()
        {
            ClearMessages();

            ClientValidationResult validationResult =
                LoginFormValidator.Validate(loginForm);

            if (!validationResult.IsValid)
            {
                SetValidationError(validationResult);
                return;
            }

            await ExecuteAuthOperationAsync(
                "LoginAsync",
                LoginCoreAsync,
                ClearPassword,
                loginCommand,
                openRegisterCommand,
                openPasswordResetCommand);
        }

        private async Task LoginCoreAsync()
        {
            LoginRequest request = new LoginRequest
            {
                Email = loginForm.Email.Trim(),
                Password = loginForm.Password
            };

            LoginResponse response = await AuthClient.LoginAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("LoginAsync returned a null response.");
                ClearPassword();
                return;
            }

            if (!response.Success)
            {
                SetError(GetAuthServerMessage(response.MessageCode));
                ClearPassword();
                return;
            }

            if (response.Player == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("LoginAsync succeeded but player data was null.");
                ClearPassword();
                return;
            }

            UserSession.Start(new AuthenticatedUserModel
            {
                AccountId = response.Player.AccountId,
                PlayerId = response.Player.PlayerId,
                FullName = response.Player.FullName,
                Email = loginForm.Email.Trim(),
                PreferredLanguageCode = response.Player.PreferredLanguageCode
            });

            SetSuccess(GetAuthServerMessage(response.MessageCode));

            ClearPassword();
            RaiseLoginSucceeded();
        }

        private void RequestOpenRegister()
        {
            if (IsBusy)
            {
                return;
            }

            RaiseRegisterRequested();
        }

        private void RequestOpenPasswordReset()
        {
            if (IsBusy)
            {
                return;
            }

            RaisePasswordResetRequested();
        }

        private void ClearPassword()
        {
            loginForm.ClearPassword();
            RaisePasswordClearRequested();
            loginCommand.RaiseCanExecuteChanged();
        }

        private void RaiseLoginSucceeded()
        {
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseRegisterRequested()
        {
            RegisterRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePasswordResetRequested()
        {
            string email = loginForm.Email ?? string.Empty;

            PasswordResetRequested?.Invoke(
                this,
                new PasswordResetRequestedEventArgs(email));
        }

        private void RaisePasswordClearRequested()
        {
            PasswordClearRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class PasswordResetRequestedEventArgs : EventArgs
    {
        public PasswordResetRequestedEventArgs(string email)
        {
            Email = email ?? string.Empty;
        }

        public string Email { get; private set; }
    }
}
