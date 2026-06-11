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
    public class ResetPasswordViewModel : AuthViewModelBase
    {
        private readonly RelayCommand resetPasswordCommand;
        private readonly RelayCommand openLoginCommand;

        private readonly ResetPasswordFormModel form;

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
            : base(
                  authClient,
                  validationMessageProvider,
                  serverMessageProvider,
                  logger)
        {
            form = new ResetPasswordFormModel
            {
                Email = email ?? string.Empty
            };

            resetPasswordCommand = new RelayCommand(
                async () => await ResetPasswordAsync(),
                CanExecuteWhenNotBusy);

            openLoginCommand = new RelayCommand(
                RequestOpenLogin,
                CanExecuteWhenNotBusy);
        }

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
                SetValidationError(validationResult);
                return;
            }

            await ExecuteAuthOperationAsync(
                "ResetPasswordAsync",
                ResetPasswordCoreAsync,
                ClearSensitiveData,
                resetPasswordCommand,
                openLoginCommand);
        }

        private async Task ResetPasswordCoreAsync()
        {
            ResetPasswordRequest request = new ResetPasswordRequest
            {
                Email = form.Email.Trim(),
                Code = form.Code.Trim(),
                NewPassword = form.NewPassword
            };

            ResetPasswordResponse response =
                await AuthClient.ResetPasswordAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("ResetPasswordAsync returned a null response.");
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

            SetSuccess(translatedMessage);
            ClearSensitiveData();
            RaisePasswordResetSucceeded();
        }

        private void ClearSensitiveData()
        {
            form.ClearSensitiveData();
            Code = string.Empty;
            RaiseSensitiveDataClearRequested();
            resetPasswordCommand.RaiseCanExecuteChanged();
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
