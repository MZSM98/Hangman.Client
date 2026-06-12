using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceMatch;
using Hangman.Contracts.Match;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public class MatchClient : IMatchClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<MatchClient>();

        public Task<CreateLobbyResponse> CreateLobbyAsync(CreateLobbyRequest request)
        {
            return ExecuteAsync(
                client => client.CreateLobbyAsync(request),
                "CreateLobbyAsync");
        }

        public Task<GetAvailableLobbiesResponse> GetAvailableLobbiesAsync(
            GetAvailableLobbiesRequest request)
        {
            return ExecuteAsync(
                client => client.GetAvailableLobbiesAsync(request),
                "GetAvailableLobbiesAsync");
        }

        public Task<JoinLobbyResponse> JoinLobbyAsync(JoinLobbyRequest request)
        {
            return ExecuteAsync(
                client => client.JoinLobbyAsync(request),
                "JoinLobbyAsync");
        }

        public Task<GetCurrentLobbyResponse> GetCurrentLobbyAsync(
            GetCurrentLobbyRequest request)
        {
            return ExecuteAsync(
                client => client.GetCurrentLobbyAsync(request),
                "GetCurrentLobbyAsync");
        }

        public Task<LeaveLobbyResponse> LeaveLobbyAsync(
            LeaveLobbyRequest request)
        {
            return ExecuteAsync(
                client => client.LeaveLobbyAsync(request),
                "LeaveLobbyAsync");
        }

        private static async Task<TResponse> ExecuteAsync<TResponse>(
            Func<MatchServiceClient, Task<TResponse>> operation,
            string operationName)
        {
            MatchServiceClient client = CreateClient();

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

        private static MatchServiceClient CreateClient()
        {
            return new MatchServiceClient();
        }

        private static void CloseClient(MatchServiceClient client)
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

        private static void AbortClient(MatchServiceClient client)
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
