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
    public class VerifyEmailViewModel : AuthViewModelBase
    {
        private readonly RelayCommand verifyEmailCommand;
        private readonly RelayCommand resendCodeCommand;
        private readonly RelayCommand openLoginCommand;

        private readonly VerifyEmailFormModel verifyEmailForm;

        public VerifyEmailViewModel()
            : this(string.Empty)
        {
        }

        public VerifyEmailViewModel(string email)
            : this(
                  email,
                  new AuthClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<VerifyEmailViewModel>())
        {
        }

        public VerifyEmailViewModel(
            string email,
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
            verifyEmailForm = new VerifyEmailFormModel
            {
                Email = email ?? string.Empty
            };

            verifyEmailCommand = new RelayCommand(
                async () => await VerifyEmailAsync(),
                CanExecuteWhenNotBusy);

            resendCodeCommand = new RelayCommand(
                async () => await ResendCodeAsync(),
                CanExecuteWhenNotBusy);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteWhenNotBusy);
        }

        public event EventHandler VerificationSucceeded;

        public event EventHandler CodeClearRequested;

        public string Email
        {
            get { return verifyEmailForm.Email; }
            set
            {
                if (verifyEmailForm.Email == value)
                {
                    return;
                }

                verifyEmailForm.Email = value;
                OnPropertyChanged();
                RaiseCommandsCanExecuteChanged(
                    verifyEmailCommand,
                    resendCodeCommand,
                    openLoginCommand);
            }
        }

        public string Code
        {
            get { return verifyEmailForm.Code; }
            set
            {
                if (verifyEmailForm.Code == value)
                {
                    return;
                }

                verifyEmailForm.Code = value;
                OnPropertyChanged();
                verifyEmailCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand VerifyEmailCommand
        {
            get { return verifyEmailCommand; }
        }

        public ICommand ResendCodeCommand
        {
            get { return resendCodeCommand; }
        }

        public ICommand OpenLoginCommand
        {
            get { return openLoginCommand; }
        }

        private async Task VerifyEmailAsync()
        {
            ClearMessages();

            ClientValidationResult validationResult =
                VerifyEmailFormValidator.Validate(verifyEmailForm);

            if (!validationResult.IsValid)
            {
                SetValidationError(validationResult);
                return;
            }

            await ExecuteAuthOperationAsync(
                "VerifyEmailAsync",
                VerifyEmailCoreAsync,
                ClearCode,
                verifyEmailCommand,
                resendCodeCommand,
                openLoginCommand);
        }

        private async Task VerifyEmailCoreAsync()
        {
            VerifyEmailRequest request = new VerifyEmailRequest
            {
                Email = verifyEmailForm.Email.Trim(),
                Code = verifyEmailForm.Code.Trim()
            };

            VerifyEmailResponse response =
                await AuthClient.VerifyEmailAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("VerifyEmailAsync returned a null response.");
                ClearCode();
                return;
            }

            string translatedMessage = GetAuthServerMessage(response.MessageCode);

            if (!response.Success)
            {
                SetError(translatedMessage);
                ClearCode();
                return;
            }

            SetSuccess(translatedMessage);
            ClearCode();
            RaiseVerificationSucceeded();
        }

        private async Task ResendCodeAsync()
        {
            ClearMessages();

            if (string.IsNullOrWhiteSpace(verifyEmailForm.Email))
            {
                SetError(GetValidationMessage(ClientValidationCode.EmailRequired));
                return;
            }

            await ExecuteAuthOperationAsync(
                "ResendVerificationEmailAsync",
                ResendCodeCoreAsync,
                null,
                verifyEmailCommand,
                resendCodeCommand,
                openLoginCommand);
        }

        private async Task ResendCodeCoreAsync()
        {
            ResendVerificationEmailRequest request =
                new ResendVerificationEmailRequest
                {
                    Email = verifyEmailForm.Email.Trim()
                };

            ResendVerificationEmailResponse response =
                await AuthClient.ResendVerificationEmailAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("ResendVerificationEmailAsync returned a null response.");
                return;
            }

            string translatedMessage = GetAuthServerMessage(response.MessageCode);

            if (!response.Success)
            {
                SetError(translatedMessage);
                return;
            }

            SetSuccess(translatedMessage);
            ClearCode();
        }

        private void ClearCode()
        {
            verifyEmailForm.ClearCode();
            Code = string.Empty;
            RaiseCodeClearRequested();
        }

        private void RaiseVerificationSucceeded()
        {
            VerificationSucceeded?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseCodeClearRequested()
        {
            CodeClearRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
