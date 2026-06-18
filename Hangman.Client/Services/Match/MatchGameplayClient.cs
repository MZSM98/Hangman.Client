using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceMatchGameplay;
using Hangman.Contracts.Match;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public class MatchGameplayClient : IMatchGameplayClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<MatchGameplayClient>();

        public Task<VoteCategoryResponse> VoteCategoryAsync(
            VoteCategoryRequest request)
        {
            return ExecuteAsync(
                client => client.VoteCategoryAsync(request),
                "VoteCategoryAsync");
        }

        public Task<GetCategoryVotingStateResponse> GetCategoryVotingStateAsync(
            GetCategoryVotingStateRequest request)
        {
            return ExecuteAsync(
                client => client.GetCategoryVotingStateAsync(request),
                "GetCategoryVotingStateAsync");
        }

        public Task<ResolveCategoryVotingResponse> ResolveCategoryVotingAsync(
            ResolveCategoryVotingRequest request)
        {
            return ExecuteAsync(
                client => client.ResolveCategoryVotingAsync(request),
                "ResolveCategoryVotingAsync");
        }

        public Task<GetSelectableWordsResponse> GetSelectableWordsAsync(
            GetSelectableWordsRequest request)
        {
            return ExecuteAsync(
                client => client.GetSelectableWordsAsync(request),
                "GetSelectableWordsAsync");
        }

        public Task<SelectWordResponse> SelectWordAsync(
            SelectWordRequest request)
        {
            return ExecuteAsync(
                client => client.SelectWordAsync(request),
                "SelectWordAsync");
        }

        private static async Task<TResponse> ExecuteAsync<TResponse>(
            Func<MatchGameplayServiceClient, Task<TResponse>> operation,
            string operationName)
        {
            MatchGameplayServiceClient client = CreateClient();

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

        private static MatchGameplayServiceClient CreateClient()
        {
            return new MatchGameplayServiceClient();
        }

        private static void CloseClient(MatchGameplayServiceClient client)
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

        private static void AbortClient(MatchGameplayServiceClient client)
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
