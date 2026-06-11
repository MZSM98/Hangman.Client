using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Services.Auth;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Base
{
    public abstract class AuthViewModelBase : ServiceViewModelBase
    {
        protected AuthViewModelBase(
            IAuthClient authClient,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : base(validationMessageProvider, serverMessageProvider, logger)
        {
            AuthClient = authClient ??
                throw new ArgumentNullException(nameof(authClient));
        }

        public event EventHandler LoginRequested;

        protected IAuthClient AuthClient { get; private set; }

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

        protected string GetAuthServerMessage(string messageCode)
        {
            return GetServerMessage(ServerMessageModuleName.Auth, messageCode);
        }

        protected Task ExecuteAuthOperationAsync(
            string operationName,
            Func<Task> operation,
            Action onTechnicalFailure,
            params RelayCommand[] commands)
        {
            return ExecuteServiceOperationAsync(
                operationName,
                "authentication service",
                operation,
                onTechnicalFailure,
                commands);
        }
    }
}
