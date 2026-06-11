using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceProfile;
using Hangman.Contracts.Profile;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Profile
{
    public class ProfileClient : IProfileClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<ProfileClient>();

        public Task<GetProfileResponse> GetProfileAsync(GetProfileRequest request)
        {
            return ExecuteAsync(
                client => client.GetProfileAsync(request),
                "GetProfileAsync");
        }

        public Task<UpdateProfileResponse> UpdateProfileAsync(
            UpdateProfileRequest request)
        {
            return ExecuteAsync(
                client => client.UpdateProfileAsync(request),
                "UpdateProfileAsync");
        }

        public Task<DeleteProfileResponse> DeleteProfileAsync(
            DeleteProfileRequest request)
        {
            return ExecuteAsync(
                client => client.DeleteProfileAsync(request),
                "DeleteProfileAsync");
        }

        private static async Task<TResponse> ExecuteAsync<TResponse>(
            Func<ProfileServiceClient, Task<TResponse>> operation,
            string operationName)
        {
            ProfileServiceClient client = CreateClient();

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

        private static ProfileServiceClient CreateClient()
        {
            return new ProfileServiceClient();
        }

        private static void CloseClient(ProfileServiceClient client)
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

        private static void AbortClient(ProfileServiceClient client)
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
