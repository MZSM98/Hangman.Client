using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.ServiceReferenceChat;
using Hangman.Contracts.Match;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchChatClient : IMatchChatClient
    {
        private static readonly IClientLogger Log =
            ClientLoggerFactory.Create<MatchChatClient>();

        private MatchChatServiceClient client;

        public async Task<SendMatchChatMessageResponse> SendMessageAsync(
            SendMatchChatMessageRequest request)
        {
            try
            {
                EnsureClient();

                return await client.SendMessageAsync(request);
            }
            catch (TimeoutException exception)
            {
                AbortClient();
                Log.Error("SendMessageAsync failed due to timeout.", exception);
                throw;
            }
            catch (CommunicationException exception)
            {
                AbortClient();
                Log.Error("SendMessageAsync failed due to communication error.", exception);
                throw;
            }
            catch (Exception exception)
            {
                AbortClient();
                Log.Error("SendMessageAsync failed unexpectedly.", exception);
                throw;
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void EnsureClient()
        {
            if (client == null)
            {
                CreateClient();
                return;
            }

            ICommunicationObject communicationObject = client;

            if (communicationObject.State == CommunicationState.Faulted ||
                communicationObject.State == CommunicationState.Closed ||
                communicationObject.State == CommunicationState.Closing)
            {
                AbortClient();
                CreateClient();
            }
        }

        private void CreateClient()
        {
            client = new MatchChatServiceClient();
        }

        private void Close()
        {
            if (client == null)
            {
                return;
            }

            ICommunicationObject communicationObject = client;

            try
            {
                if (communicationObject.State == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                }
                else
                {
                    communicationObject.Close();
                }
            }
            catch (CommunicationException exception)
            {
                Log.Error("Error closing match chat client.", exception);
                communicationObject.Abort();
            }
            catch (TimeoutException exception)
            {
                Log.Error("Timeout closing match chat client.", exception);
                communicationObject.Abort();
            }
            catch (Exception exception)
            {
                Log.Error("Unexpected error closing match chat client.", exception);
                communicationObject.Abort();
            }
            finally
            {
                client = null;
            }
        }

        private void AbortClient()
        {
            if (client == null)
            {
                return;
            }

            ICommunicationObject communicationObject = client;
            communicationObject.Abort();
            client = null;
        }
    }
}
