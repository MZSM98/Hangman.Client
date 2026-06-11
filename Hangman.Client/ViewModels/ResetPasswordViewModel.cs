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
    public class ResetPasswordViewModel : BaseViewModel
    {
        private readonly IAuthClient authClient;
        private readonly ClientValidationMessageProvider validationMessageProvider;
        private readonly IServerMessageProvider serverMessageProvider;
        private readonly IClientLogger logger;

        private readonly RelayCommand resetPasswordCommand;
        private readonly RelayCommand openLoginCommand;

        private readonly ResetPasswordFormModel form;

        private const string UnexpectedErrorCode = "UnexpectedError";
        private const string RuntimeErrorCode = "RuntimeError";

        public ResetPasswordViewModel()
            : this(string.Empty)
        {
        }

        public ResetPasswordViewModel(string email)
            : this(
                  email,
                  new AuthClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<ResetPasswordViewModel>())
        {
        }

        public ResetPasswordViewModel(
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

            form = new ResetPasswordFormModel
            {
                Email = email ?? string.Empty
            };

            resetPasswordCommand = new RelayCommand(
                async () => await ResetPasswordAsync(),
                CanExecuteAction);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteNavigation);
        }

        public event EventHandler LoginRequested;

        public event EventHandler PasswordResetSucceeded;

        public event EventHandler SensitiveDataClearRequested;

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
                resetPasswordCommand.RaiseCanExecuteChanged();
            }
        }

        public string Code
        {
            get { return form.Code; }
            set
            {
                if (form.Code == value)
                {
                    return;
                }

                form.Code = value;
                OnPropertyChanged();
                resetPasswordCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand ResetPasswordCommand
        {
            get { return resetPasswordCommand; }
        }

        public ICommand OpenLoginCommand
        {
            get { return openLoginCommand; }
        }

        public void SetNewPassword(string password)
        {
            form.NewPassword = password ?? string.Empty;
            resetPasswordCommand.RaiseCanExecuteChanged();
        }

        public void SetConfirmPassword(string confirmPassword)
        {
            form.ConfirmPassword = confirmPassword ?? string.Empty;
            resetPasswordCommand.RaiseCanExecuteChanged();
        }

        private async Task ResetPasswordAsync()
        {
            ClearMessages();

            ClientValidationResult validationResult =
                ResetPasswordFormValidator.Validate(form);

            if (!validationResult.IsValid)
            {
                SetError(validationMessageProvider.GetMessage(validationResult.Code));
                return;
            }

            SetBusy(true);

            try
            {
                ResetPasswordRequest request = new ResetPasswordRequest
                {
                    Email = form.Email.Trim(),
                    Code = form.Code.Trim(),
                    NewPassword = form.NewPassword
                };

                ResetPasswordResponse response =
                    await authClient.ResetPasswordAsync(request);

                if (response == null)
                {
                    SetError(serverMessageProvider.GetMessage(
                        ServerMessageModuleName.Common,
                        UnexpectedErrorCode));

                    logger.Warn("ResetPasswordAsync returned a null response.");
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

                SetSuccess(translatedMessage);
                ClearSensitiveData();
                RaisePasswordResetSucceeded();
            }
            catch (EndpointNotFoundException exception)
            {
                logger.Error("ResetPasswordAsync failed because the authentication service endpoint was not found.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));

                ClearSensitiveData();
            }
            catch (TimeoutException exception)
            {
                logger.Error("ResetPasswordAsync failed due to timeout.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));

                ClearSensitiveData();
            }
            catch (CommunicationException exception)
            {
                logger.Error("ResetPasswordAsync failed due to communication error.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    RuntimeErrorCode));

                ClearSensitiveData();
            }
            catch (Exception exception)
            {
                logger.Error("ResetPasswordAsync failed unexpectedly.", exception);

                SetError(serverMessageProvider.GetMessage(
                    ServerMessageModuleName.Common,
                    UnexpectedErrorCode));

                ClearSensitiveData();
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
            resetPasswordCommand.RaiseCanExecuteChanged();
            openLoginCommand.RaiseCanExecuteChanged();
        }

        private void ClearSensitiveData()
        {
            form.ClearSensitiveData();
            Code = string.Empty;
            RaiseSensitiveDataClearRequested();
            resetPasswordCommand.RaiseCanExecuteChanged();
        }

        private void RaiseLoginRequested()
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePasswordResetSucceeded()
        {
            PasswordResetSucceeded?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseSensitiveDataClearRequested()
        {
            SensitiveDataClearRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
