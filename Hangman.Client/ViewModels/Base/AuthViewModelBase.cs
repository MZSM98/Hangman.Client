using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Services.Auth;
using Hangman.Client.Validators;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Base
{
    public abstract class AuthViewModelBase : BaseViewModel
    {
        protected const string UnexpectedErrorCode = "UnexpectedError";
        protected const string RuntimeErrorCode = "RuntimeError";

        protected AuthViewModelBase(
            IAuthClient authClient,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
        {
            AuthClient = authClient ??
                throw new ArgumentNullException(nameof(authClient));
            ValidationMessageProvider = validationMessageProvider ??
                throw new ArgumentNullException(nameof(validationMessageProvider));
            ServerMessageProvider = serverMessageProvider ??
                throw new ArgumentNullException(nameof(serverMessageProvider));
            Logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler LoginRequested;

        protected IAuthClient AuthClient { get; private set; }

        protected ClientValidationMessageProvider ValidationMessageProvider { get; private set; }

        protected IServerMessageProvider ServerMessageProvider { get; private set; }

        protected IClientLogger Logger { get; private set; }

        protected bool CanExecuteWhenNotBusy()
        {
            return !IsBusy;
        }

        protected void RequestOpenLogin()
        {
            if (IsBusy)
            {
                return;
            }

            RaiseLoginRequested();
        }

        protected void RaiseLoginRequested()
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        protected void SetValidationError(ClientValidationResult validationResult)
        {
            SetError(ValidationMessageProvider.GetMessage(validationResult.Code));
        }

        protected string GetValidationMessage(ClientValidationCode code)
        {
            return ValidationMessageProvider.GetMessage(code);
        }

        protected string GetAuthServerMessage(string messageCode)
        {
            return ServerMessageProvider.GetMessage(
                ServerMessageModuleName.Auth,
                messageCode);
        }

        protected string GetCommonServerMessage(string messageCode)
        {
            return ServerMessageProvider.GetMessage(
                ServerMessageModuleName.Common,
                messageCode);
        }

        protected void SetCommonUnexpectedError()
        {
            SetError(GetCommonServerMessage(UnexpectedErrorCode));
        }

        protected void SetCommonRuntimeError()
        {
            SetError(GetCommonServerMessage(RuntimeErrorCode));
        }

        protected void SetBusyState(bool value, params RelayCommand[] commands)
        {
            IsBusy = value;
            RaiseCommandsCanExecuteChanged(commands);
        }

        protected static void RaiseCommandsCanExecuteChanged(params RelayCommand[] commands)
        {
            if (commands == null)
            {
                return;
            }

            foreach (RelayCommand command in commands.Where(c => c != null))
            {
                command.RaiseCanExecuteChanged();
            }
        }

        protected async Task ExecuteAuthOperationAsync(
            string operationName,
            Func<Task> operation,
            Action onTechnicalFailure,
            params RelayCommand[] commands)
        {
            SetBusyState(true, commands);

            try
            {
                await operation();
            }
            catch (EndpointNotFoundException exception)
            {
                Logger.Error(operationName + 
                    " failed because the authentication service endpoint was not found.", exception);
                SetCommonRuntimeError();
                onTechnicalFailure?.Invoke();
            }
            catch (TimeoutException exception)
            {
                Logger.Error(operationName + " failed due to timeout.", exception);
                SetCommonRuntimeError();
                onTechnicalFailure?.Invoke();
            }
            catch (CommunicationException exception)
            {
                Logger.Error(operationName + " failed due to communication error.", exception);
                SetCommonRuntimeError();
                onTechnicalFailure?.Invoke();
            }
            catch (Exception exception)
            {
                Logger.Error(operationName + " failed unexpectedly.", exception);
                SetCommonUnexpectedError();
                onTechnicalFailure?.Invoke();
            }
            finally
            {
                SetBusyState(false, commands);
            }
        }
    }
}
