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
    public class RequestPasswordResetViewModel : AuthViewModelBase
    {
        private readonly RelayCommand requestPasswordResetCommand;
        private readonly RelayCommand openLoginCommand;

        private readonly RequestPasswordResetFormModel form;

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
            : base(
                  authClient,
                  validationMessageProvider,
                  serverMessageProvider,
                  logger)
        {
            form = new RequestPasswordResetFormModel
            {
                Email = email ?? string.Empty
            };

            requestPasswordResetCommand = new RelayCommand(
                async () => await RequestPasswordResetAsync(),
                CanExecuteWhenNotBusy);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteWhenNotBusy);
        }

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
                SetValidationError(validationResult);
                return;
            }

            await ExecuteAuthOperationAsync(
                "RequestPasswordResetAsync",
                RequestPasswordResetCoreAsync,
                null,
                requestPasswordResetCommand,
                openLoginCommand);
        }

        private async Task RequestPasswordResetCoreAsync()
        {
            string email = form.Email.Trim();

            RequestPasswordResetRequest request = new RequestPasswordResetRequest
            {
                Email = email
            };

            RequestPasswordResetResponse response =
                await AuthClient.RequestPasswordResetAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("RequestPasswordResetAsync returned a null response.");
                return;
            }

            string translatedMessage = GetAuthServerMessage(response.MessageCode);

            if (!response.Success)
            {
                SetError(translatedMessage);
                return;
            }

            SetSuccess(translatedMessage);
            RaiseResetCodeRequested(email);
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
