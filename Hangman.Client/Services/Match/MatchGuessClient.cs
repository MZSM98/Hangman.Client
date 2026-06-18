using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceMatchGuess;
using Hangman.Contracts.Match;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public class MatchGuessClient : IMatchGuessClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<MatchGuessClient>();

        public Task<GetMatchGameStateResponse> GetGameStateAsync(
            GetMatchGameStateRequest request)
        {
            return ExecuteAsync(
                client => client.GetGameStateAsync(request),
                "GetGameStateAsync");
        }

        public Task<GuessLetterResponse> GuessLetterAsync(
            GuessLetterRequest request)
        {
            return ExecuteAsync(
                client => client.GuessLetterAsync(request),
                "GuessLetterAsync");
        }

        public Task<GuessWordResponse> GuessWordAsync(
            GuessWordRequest request)
        {
            return ExecuteAsync(
                client => client.GuessWordAsync(request),
                "GuessWordAsync");
        }

        public Task<ResolveGuessTimeoutResponse> ResolveGuessTimeoutAsync(
            ResolveGuessTimeoutRequest request)
        {
            return ExecuteAsync(
                client => client.ResolveGuessTimeoutAsync(request),
                "ResolveGuessTimeoutAsync");
        }

        private static async Task<TResponse> ExecuteAsync<TResponse>(
            Func<MatchGuessServiceClient, Task<TResponse>> operation,
            string operationName)
        {
            MatchGuessServiceClient client = CreateClient();

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

        private static MatchGuessServiceClient CreateClient()
        {
            return new MatchGuessServiceClient();
        }

        private static void CloseClient(MatchGuessServiceClient client)
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

        private static void AbortClient(MatchGuessServiceClient client)
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
