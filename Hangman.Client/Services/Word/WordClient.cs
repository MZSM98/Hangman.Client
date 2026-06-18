using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceWord;
using Hangman.Contracts.Word;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Word
{
    public class WordClient : IWordClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<WordClient>();

        public Task<GetCategoriesByLanguageResponse> GetCategoriesByLanguageAsync(
            GetCategoriesByLanguageRequest request)
        {
            return ExecuteAsync(
                client => client.GetCategoriesByLanguageAsync(request),
                "GetCategoriesByLanguageAsync");
        }

        private static async Task<TResponse> ExecuteAsync<TResponse>(
            Func<WordServiceClient, Task<TResponse>> operation,
            string operationName)
        {
            WordServiceClient client = CreateClient();

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

        private static WordServiceClient CreateClient()
        {
            return new WordServiceClient();
        }

        private static void CloseClient(WordServiceClient client)
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

        private static void AbortClient(WordServiceClient client)
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
