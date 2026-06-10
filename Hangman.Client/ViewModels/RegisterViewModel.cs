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
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthClient authClient;
        private readonly ClientValidationMessageProvider validationMessageProvider;
        private readonly IServerMessageProvider serverMessageProvider;
        private readonly IClientLogger logger;

        private readonly RelayCommand registerCommand;
        private readonly RelayCommand openLoginCommand;

        private readonly RegisterFormModel registerForm;

        public RegisterViewModel()
            : this(
                  new AuthClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<RegisterViewModel>())
        {
        }

        public RegisterViewModel(
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

            registerForm = new RegisterFormModel();

            registerCommand = new RelayCommand(
                async () => await RegisterAsync(),
                CanExecuteRegister);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteNavigation);
        }

        public event EventHandler LoginRequested;

        public event EventHandler PasswordClearRequested;

        public event EventHandler<VerificationRequiredEventArgs> VerificationRequired;

        public string FullName
        {
            get { return registerForm.FullName; }
            set
            {
                if (registerForm.FullName == value)
                {
                    return;
                }

                registerForm.FullName = value;
                OnPropertyChanged();
                registerCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? DateOfBirth
        {
            get { return registerForm.DateOfBirth; }
            set
            {
                if (registerForm.DateOfBirth == value)
                {
                    return;
                }

                registerForm.DateOfBirth = value;
                OnPropertyChanged();
                registerCommand.RaiseCanExecuteChanged();
            }
        }

        public string Phone
        {
            get { return registerForm.Phone; }
            set
            {
                if (registerForm.Phone == value)
                {
                    return;
                }

                registerForm.Phone = value;
                OnPropertyChanged();
                registerCommand.RaiseCanExecuteChanged();
            }
        }

        public string PreferredLanguageCode
        {
            get { return registerForm.PreferredLanguageCode; }
            set
            {
                string normalizedValue = string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : value.Trim().ToLowerInvariant();

                if (registerForm.PreferredLanguageCode == normalizedValue)
                {
                    return;
                }

                registerForm.PreferredLanguageCode = normalizedValue;
                OnPropertyChanged();
                registerCommand.RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get { return registerForm.Email; }
            set
            {
                if (registerForm.Email == value)
                {
                    return;
                }

                registerForm.Email = value;
                OnPropertyChanged();
                registerCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand RegisterCommand
        {
            get { return registerCommand; }
        }

        public ICommand OpenLoginCommand
        {
            get { return openLoginCommand; }
        }

        public void SetPassword(string password)
        {
            registerForm.Password = password ?? string.Empty;
            registerCommand.RaiseCanExecuteChanged();
        }

        public void SetConfirmPassword(string confirmPassword)
        {
            registerForm.ConfirmPassword = confirmPassword ?? string.Empty;
            registerCommand.RaiseCanExecuteChanged();
        }

        private async Task RegisterAsync()
        {
            ClearMessages();

            ClientValidationResult validationResult = RegisterFormValidator.Validate(registerForm);

            if (!validationResult.IsValid)
            {
                SetError(validationMessageProvider.GetMessage(validationResult.Code));
                return;
            }

            SetBusy(true);

            try
            {
                RegisterRequest request = new RegisterRequest
                {
                    FullName = registerForm.FullName.Trim(),
                    DateOfBirth = registerForm.DateOfBirth.Value,
                    Phone = registerForm.Phone.Trim(),
                    PreferredLanguageCode = registerForm.PreferredLanguageCode.Trim().ToLowerInvariant(),
                    Email = registerForm.Email.Trim(),
                    Password = registerForm.Password
                };

                RegisterResponse response = await authClient.RegisterAsync(request);

                if (response == null)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Common,
                        "UnexpectedError"));

                    logger.Warn("RegisterAsync returned a null response.");
                    ClearSensitiveData();
                    return;
                }

                string translatedMessage = serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Auth,
                    response.MessageCode);

                if (!response.Success)
                {
                    SetError(translatedMessage);
                    ClearSensitiveData();
                    return;
                }

                string registeredEmail = registerForm.Email.Trim();

                SetSuccess(translatedMessage);
                ClearSensitiveData();

                if (response.RequiresEmailVerification)
                {
                    RaiseVerificationRequired(registeredEmail);
                }
            }
            catch (TimeoutException exception)
            {
                logger.Error("RegisterAsync failed due to timeout.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    "RuntimeError"));

                ClearSensitiveData();
            }
            catch (CommunicationException exception)
            {
                logger.Error("RegisterAsync failed due to communication error.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    "RuntimeError"));

                ClearSensitiveData();
            }
            catch (Exception exception)
            {
                logger.Error("RegisterAsync failed unexpectedly.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    "UnexpectedError"));

                ClearSensitiveData();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void RaiseVerificationRequired(string email)
        {
            EventHandler<VerificationRequiredEventArgs> handler = VerificationRequired;

            if (handler != null)
            {
                handler(this, new VerificationRequiredEventArgs(email));
            }
        }

        private bool CanExecuteRegister()
        {
            return !IsBusy;
        }

        private bool CanExecuteNavigation()
        {
            return !IsBusy;
        }

        private void RequestOpenLogin()
        {
            if (IsBusy)
            {
                return;
            }

            RaiseLoginRequested();
        }

        private void SetBusy(bool value)
        {
            IsBusy = value;
            registerCommand.RaiseCanExecuteChanged();
            openLoginCommand.RaiseCanExecuteChanged();
        }

        private void ClearSensitiveData()
        {
            registerForm.ClearSensitiveData();
            RaisePasswordClearRequested();
            registerCommand.RaiseCanExecuteChanged();
        }

        private void RaiseLoginRequested()
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePasswordClearRequested()
        {
            PasswordClearRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class VerificationRequiredEventArgs : EventArgs
    {
        public VerificationRequiredEventArgs(string email)
        {
            Email = email ?? string.Empty;
        }

        public string Email { get; private set; }
    }
}
