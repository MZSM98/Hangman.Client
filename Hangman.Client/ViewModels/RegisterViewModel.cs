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
    public class RegisterViewModel : AuthViewModelBase
    {
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
            : base(
                  authClient,
                  validationMessageProvider,
                  serverMessageProvider,
                  logger)
        {
            registerForm = new RegisterFormModel();

            registerCommand = new RelayCommand(
                async () => await RegisterAsync(),
                CanExecuteWhenNotBusy);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteWhenNotBusy);
        }

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

            ClientValidationResult validationResult =
                RegisterFormValidator.Validate(registerForm);

            if (!validationResult.IsValid)
            {
                SetValidationError(validationResult);
                return;
            }

            await ExecuteAuthOperationAsync(
                "RegisterAsync",
                RegisterCoreAsync,
                ClearSensitiveData,
                registerCommand,
                openLoginCommand);
        }

        private async Task RegisterCoreAsync()
        {
            RegisterRequest request = new RegisterRequest
            {
                FullName = registerForm.FullName.Trim(),
                DateOfBirth = registerForm.DateOfBirth.Value,
                Phone = registerForm.Phone.Trim(),
                PreferredLanguageCode =
                    registerForm.PreferredLanguageCode.Trim().ToLowerInvariant(),
                Email = registerForm.Email.Trim(),
                Password = registerForm.Password
            };

            RegisterResponse response = await AuthClient.RegisterAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("RegisterAsync returned a null response.");
                ClearSensitiveData();
                return;
            }

            string translatedMessage = GetAuthServerMessage(response.MessageCode);

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

        private void ClearSensitiveData()
        {
            registerForm.ClearSensitiveData();
            RaisePasswordClearRequested();
            registerCommand.RaiseCanExecuteChanged();
        }

        private void RaiseVerificationRequired(string email)
        {
            VerificationRequired?.Invoke(
                this,
                new VerificationRequiredEventArgs(email));
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
