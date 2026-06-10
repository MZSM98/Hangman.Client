using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Auth;
using Hangman.Client.Services.Auth;
using Hangman.Client.Validators;
using Hangman.Client.Validators.Auth;
using Hangman.Client.ViewModels.Base;
using Hangman.Contracts.Auth;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthClient authClient;
        private readonly ClientValidationMessageProvider validationMessageProvider;
        private readonly IServerMessageProvider serverMessageProvider;
        private readonly IClientLogger logger;

        private readonly RelayCommand loginCommand;
        private readonly RelayCommand openRegisterCommand;

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
        {
            this.authClient = authClient ?? 
                throw new ArgumentNullException(nameof(authClient));
            this.validationMessageProvider = validationMessageProvider ?? 
                throw new ArgumentNullException(nameof(validationMessageProvider));
            this.serverMessageProvider = serverMessageProvider ?? 
                throw new ArgumentNullException(nameof(serverMessageProvider));
            this.logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));

            loginForm = new LoginFormModel();

            loginCommand = new RelayCommand(
                async () => await LoginAsync(),
                CanExecuteLogin);

            openRegisterCommand = new RelayCommand(
                RequestOpenRegister,
                CanExecuteNavigation);
        }

        public event EventHandler LoginSucceeded;

        public event EventHandler RegisterRequested;

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

        public void SetPassword(string password)
        {
            loginForm.Password = password ?? string.Empty;
            loginCommand.RaiseCanExecuteChanged();
        }

        private async Task LoginAsync()
        {
            ClearMessages();

            ClientValidationResult validationResult = LoginFormValidator.Validate(loginForm);

            if (!validationResult.IsValid)
            {
                SetError(validationMessageProvider.GetMessage(validationResult.Code));
                return;
            }

            SetBusy(true);

            try
            {
                LoginRequest request = new LoginRequest
                {
                    Email = loginForm.Email.Trim(),
                    Password = loginForm.Password
                };

                LoginResponse response = await authClient.LoginAsync(request);

                if (response == null)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Common,
                        "UnexpectedError"));

                    logger.Warn("LoginAsync returned a null response.");
                    return;
                }

                if (!response.Success)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Auth,
                        response.MessageCode));

                    ClearPassword();
                    return;
                }

                if (response.Player == null)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Common, 
                        "UnexpectedError"));

                    logger.Warn("LoginAsync succeeded but player data was null.");
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

                SetSuccess(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Auth,
                    response.MessageCode));

                ClearPassword();
                RaiseLoginSucceeded();
            }
            catch (TimeoutException exception)
            {
                logger.Error("LoginAsync failed due to timeout.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    "RuntimeError"));

                ClearPassword();
            }
            catch (CommunicationException exception)
            {
                logger.Error("LoginAsync failed due to a WCF communication error.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    "RuntimeError"));

                ClearPassword();
            }
            catch (Exception exception)
            {
                logger.Error("LoginAsync failed unexpectedly.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    "UnexpectedError"));

                ClearPassword();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private bool CanExecuteLogin()
        {
            return !IsBusy;
        }

        private bool CanExecuteNavigation()
        {
            return !IsBusy;
        }

        private void RequestOpenRegister()
        {
            if (IsBusy)
            {
                return;
            }

            RaiseRegisterRequested();
        }

        private void SetBusy(bool value)
        {
            IsBusy = value;
            loginCommand.RaiseCanExecuteChanged();
            openRegisterCommand.RaiseCanExecuteChanged();
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

        private void RaisePasswordClearRequested()
        {
            PasswordClearRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
