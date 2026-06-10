using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceAuth;
using Hangman.Contracts.Auth;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Auth
{
    public class AuthClient : IAuthClient
    {
        private static readonly IClientLogger Log = ClientLoggerFactory.Create<AuthClient>();

        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return ExecuteAsync(client => client.LoginAsync(request), "LoginAsync");
        }

        public Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            return ExecuteAsync(client => client.RegisterAsync(request), "RegisterAsync");
        }

        public Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request)
        {
            return ExecuteAsync<VerifyEmailResponse>(
                client => client.VerifyEmailAsync(request),
                "VerifyEmailAsync");
        }

        public Task<ResendVerificationEmailResponse> ResendVerificationEmailAsync(
            ResendVerificationEmailRequest request)
        {
            return ExecuteAsync(client => client.ResendVerificationEmailAsync(request), 
                "ResendVerificationEmailAsync");
        }

        public Task<RequestPasswordResetResponse> RequestPasswordResetAsync(
            RequestPasswordResetRequest request)
        {
            return ExecuteAsync(client => client.RequestPasswordResetAsync(request),
                "RequestPasswordResetAsync");
        }

        public Task<ResetPasswordResponse> ResetPasswordAsync(
            ResetPasswordRequest request)
        {
            return ExecuteAsync(client => client.ResetPasswordAsync(request),
                "ResetPasswordAsync");
        }

        private static async Task<TResponse> ExecuteAsync<TResponse>(
            Func<AuthServiceClient, Task<TResponse>> operation,
            string operationName)
        {
            AuthServiceClient client = CreateClient();

            try
            {
                TResponse response = await operation(client);
                CloseClient(client);
                return response;
            }
            catch (TimeoutException exception)
            {
                AbortClient(client);
                Log.Error(operationName + " failed due to timeout.", exception);
                throw;
            }
            catch (CommunicationException exception)
            {
                AbortClient(client);
                Log.Error(operationName + " failed due to communication error.", exception);
                throw;
            }
            catch (Exception exception)
            {
                AbortClient(client);
                Log.Error(operationName + " failed unexpectedly.", exception);
                throw;
            }
        }

        private static AuthServiceClient CreateClient()
        {
            return new AuthServiceClient();
        }

        private static void CloseClient(AuthServiceClient client)
        {
            if (client == null)
            {
                return;
            }

            ICommunicationObject communicationObject = client;

            if (communicationObject.State == CommunicationState.Faulted)
            {
                communicationObject.Abort();
                return;
            }

            communicationObject.Close();
        }

        private static void AbortClient(AuthServiceClient client)
        {
            if (client == null)
            {
                return;
            }

            ICommunicationObject communicationObject = client;
            communicationObject.Abort();
        }
    }
}
