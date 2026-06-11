using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Validators;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Base
{
    public abstract class ServiceViewModelBase : BaseViewModel
    {
        protected const string UnexpectedErrorCode = "UnexpectedError";
        protected const string RuntimeErrorCode = "RuntimeError";

        protected ServiceViewModelBase(
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
        {
            ValidationMessageProvider = validationMessageProvider ??
                throw new ArgumentNullException(nameof(validationMessageProvider));
            ServerMessageProvider = serverMessageProvider ??
                throw new ArgumentNullException(nameof(serverMessageProvider));
            Logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        protected ClientValidationMessageProvider ValidationMessageProvider { get; private set; }

        protected IServerMessageProvider ServerMessageProvider { get; private set; }

        protected IClientLogger Logger { get; private set; }

        protected bool CanExecuteWhenNotBusy()
        {
            return !IsBusy;
        }

        protected void SetValidationError(ClientValidationResult validationResult)
        {
            SetError(ValidationMessageProvider.GetMessage(validationResult.Code));
        }

        protected string GetValidationMessage(ClientValidationCode code)
        {
            return ValidationMessageProvider.GetMessage(code);
        }

        protected string GetServerMessage(string moduleName, string messageCode)
        {
            return ServerMessageProvider.GetMessage(moduleName, messageCode);
        }

        protected string GetCommonServerMessage(string messageCode)
        {
            return GetServerMessage(ServerMessageModuleName.Common, messageCode);
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

        protected async Task ExecuteServiceOperationAsync(
            string operationName,
            string serviceName,
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
                Logger.Error(
                    operationName + " failed because the " + serviceName + " endpoint was not found.",
                    exception);

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
