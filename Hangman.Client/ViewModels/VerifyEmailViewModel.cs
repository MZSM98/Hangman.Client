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
    public class VerifyEmailViewModel : BaseViewModel
    {
        private readonly IAuthClient authClient;
        private readonly ClientValidationMessageProvider validationMessageProvider;
        private readonly IServerMessageProvider serverMessageProvider;
        private readonly IClientLogger logger;

        private readonly RelayCommand verifyEmailCommand;
        private readonly RelayCommand resendCodeCommand;
        private readonly RelayCommand openLoginCommand;

        private readonly VerifyEmailFormModel verifyEmailForm;

        private const string UnexpectedErrorCode = "UnexpectedError";
        private const string RuntimeErrorCode = "RuntimeError";

        public VerifyEmailViewModel()
            : this(
                  string.Empty,
                  new AuthClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<VerifyEmailViewModel>())
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
        {
            this.authClient = authClient ?? 
                throw new ArgumentNullException(nameof(authClient));
            this.validationMessageProvider = validationMessageProvider ?? 
                throw new ArgumentNullException(nameof(validationMessageProvider));
            this.serverMessageProvider = serverMessageProvider ?? 
                throw new ArgumentNullException(nameof(serverMessageProvider));
            this.logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));

            verifyEmailForm = new VerifyEmailFormModel
            {
                Email = email ?? string.Empty
            };

            verifyEmailCommand = new RelayCommand(
                async () => await VerifyEmailAsync(),
                CanExecuteAction);

            resendCodeCommand = new RelayCommand(
                async () => await ResendCodeAsync(),
                CanExecuteAction);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteNavigation);
        }

        public event EventHandler LoginRequested;

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
                RaiseCommandsCanExecuteChanged();
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

            ClientValidationResult validationResult = VerifyEmailFormValidator.Validate(verifyEmailForm);

            if (!validationResult.IsValid)
            {
                SetError(validationMessageProvider.GetMessage(validationResult.Code));
                return;
            }

            SetBusy(true);

            try
            {
                VerifyEmailRequest request = new VerifyEmailRequest
                {
                    Email = verifyEmailForm.Email.Trim(),
                    Code = verifyEmailForm.Code.Trim()
                };

                VerifyEmailResponse response = await authClient.VerifyEmailAsync(request);

                if (response == null)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Common,
                        UnexpectedErrorCode));

                    logger.Warn("VerifyEmailAsync returned a null response.");
                    ClearCode();
                    return;
                }

                string translatedMessage = serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Auth,
                    response.MessageCode);

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
            catch (TimeoutException exception)
            {
                logger.Error("VerifyEmailAsync failed due to timeout.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));

                ClearCode();
            }
            catch (CommunicationException exception)
            {
                logger.Error("VerifyEmailAsync failed due to communication error.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));

                ClearCode();
            }
            catch (Exception exception)
            {
                logger.Error("VerifyEmailAsync failed unexpectedly.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    UnexpectedErrorCode));

                ClearCode();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ResendCodeAsync()
        {
            ClearMessages();

            if (string.IsNullOrWhiteSpace(verifyEmailForm.Email))
            {
                SetError(validationMessageProvider.GetMessage(ClientValidationCode.EmailRequired));
                return;
            }

            SetBusy(true);

            try
            {
                ResendVerificationEmailRequest request = new ResendVerificationEmailRequest
                {
                    Email = verifyEmailForm.Email.Trim()
                };

                ResendVerificationEmailResponse response =
                    await authClient.ResendVerificationEmailAsync(request);

                if (response == null)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Common,
                        UnexpectedErrorCode));

                    logger.Warn("ResendVerificationEmailAsync returned a null response.");
                    return;
                }

                string translatedMessage = serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Auth,
                    response.MessageCode);

                if (!response.Success)
                {
                    SetError(translatedMessage);
                    return;
                }

                SetSuccess(translatedMessage);
                ClearCode();
            }
            catch (TimeoutException exception)
            {
                logger.Error("ResendVerificationEmailAsync failed due to timeout.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));
            }
            catch (CommunicationException exception)
            {
                logger.Error("ResendVerificationEmailAsync failed due to communication error.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));
            }
            catch (Exception exception)
            {
                logger.Error("ResendVerificationEmailAsync failed unexpectedly.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    UnexpectedErrorCode));
            }
            finally
            {
                SetBusy(false);
            }
        }

        private bool CanExecuteAction()
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
            RaiseCommandsCanExecuteChanged();
        }

        private void ClearCode()
        {
            verifyEmailForm.ClearCode();
            Code = string.Empty;
            RaiseCodeClearRequested();
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            verifyEmailCommand.RaiseCanExecuteChanged();
            resendCodeCommand.RaiseCanExecuteChanged();
            openLoginCommand.RaiseCanExecuteChanged();
        }

        private void RaiseLoginRequested()
        {
            EventHandler handler = LoginRequested;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void RaiseVerificationSucceeded()
        {
            EventHandler handler = VerificationSucceeded;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void RaiseCodeClearRequested()
        {
            EventHandler handler = CodeClearRequested;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
