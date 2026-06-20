using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceScore;
using Hangman.Contracts.Match;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Score
{
    public class ScoreClient : IScoreClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<ScoreClient>();

        public Task<GetPlayerScoreResponse> GetPlayerScoreAsync(GetPlayerScoreRequest request)
        {
            return ExecuteAsync(
                client => client.GetPlayerScoreAsync(request),
                "GetPlayerScoreAsync");
        }

        private static async Task<TResponse> ExecuteAsync<TResponse>(
            Func<ScoreServiceClient, Task<TResponse>> operation,
            string operationName)
        {
            ScoreServiceClient client = CreateClient();

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

        private static ScoreServiceClient CreateClient()
        {
            return new ScoreServiceClient();
        }

        private static void CloseClient(ScoreServiceClient client)
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

        private static void AbortClient(ScoreServiceClient client)
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
