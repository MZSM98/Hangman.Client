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
    public class RequestPasswordResetViewModel : BaseViewModel
    {
        private readonly IAuthClient authClient;
        private readonly ClientValidationMessageProvider validationMessageProvider;
        private readonly IServerMessageProvider serverMessageProvider;
        private readonly IClientLogger logger;

        private readonly RelayCommand requestPasswordResetCommand;
        private readonly RelayCommand openLoginCommand;

        private readonly RequestPasswordResetFormModel form;

        private const string UnexpectedErrorCode = "UnexpectedError";
        private const string RuntimeErrorCode = "RuntimeError";

        public RequestPasswordResetViewModel()
            : this(string.Empty)
        {
        }

        public RequestPasswordResetViewModel(string email)
            : this(
                  email,
                  new AuthClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<RequestPasswordResetViewModel>())
        {
        }

        public RequestPasswordResetViewModel(
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

            form = new RequestPasswordResetFormModel
            {
                Email = email ?? string.Empty
            };

            requestPasswordResetCommand = new RelayCommand(
                async () => await RequestPasswordResetAsync(),
                CanExecuteAction);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteNavigation);
        }

        public event EventHandler LoginRequested;

        public event EventHandler<ResetCodeRequestedEventArgs> ResetCodeRequested;

        public string Email
        {
            get { return form.Email; }
            set
            {
                if (form.Email == value)
                {
                    return;
                }

                form.Email = value;
                OnPropertyChanged();
                requestPasswordResetCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand RequestPasswordResetCommand
        {
            get { return requestPasswordResetCommand; }
        }

        public ICommand OpenLoginCommand
        {
            get { return openLoginCommand; }
        }

        private async Task RequestPasswordResetAsync()
        {
            ClearMessages();

            ClientValidationResult validationResult =
                RequestPasswordResetFormValidator.Validate(form);

            if (!validationResult.IsValid)
            {
                SetError(validationMessageProvider.GetMessage(validationResult.Code));
                return;
            }

            SetBusy(true);

            try
            {
                string email = form.Email.Trim();

                RequestPasswordResetRequest request = new RequestPasswordResetRequest
                {
                    Email = email
                };

                RequestPasswordResetResponse response =
                    await authClient.RequestPasswordResetAsync(request);

                if (response == null)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Common,
                        UnexpectedErrorCode));

                    logger.Warn("RequestPasswordResetAsync returned a null response.");
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
                RaiseResetCodeRequested(email);
            }
            catch (EndpointNotFoundException exception)
            {
                logger.Error("RequestPasswordResetAsync failed because the authentication service endpoint was not found.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));
            }
            catch (TimeoutException exception)
            {
                logger.Error("RequestPasswordResetAsync failed due to timeout.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));
            }
            catch (CommunicationException exception)
            {
                logger.Error("RequestPasswordResetAsync failed due to communication error.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));
            }
            catch (Exception exception)
            {
                logger.Error("RequestPasswordResetAsync failed unexpectedly.", exception);

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
            requestPasswordResetCommand.RaiseCanExecuteChanged();
            openLoginCommand.RaiseCanExecuteChanged();
        }

        private void RaiseLoginRequested()
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseResetCodeRequested(string email)
        {
            ResetCodeRequested?.Invoke(
                this,
                new ResetCodeRequestedEventArgs(email));
        }
    }

    public class ResetCodeRequestedEventArgs : EventArgs
    {
        public ResetCodeRequestedEventArgs(string email)
        {
            Email = email ?? string.Empty;
        }

        public string Email { get; private set; }
    }
}
