using Hangman.Client.Models.Match;
using Hangman.Client.Services.Session;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchChatWorkflow : IMatchChatWorkflow
    {
        private const string InvalidMatchIdCode = "InvalidMatchId";
        private const string InvalidChatMessageCode = "InvalidChatMessage";

        private readonly IMatchChatClient matchChatClient;
        private readonly IMatchSessionContext sessionContext;

        public MatchChatWorkflow(
            IMatchChatClient matchChatClient,
            IMatchSessionContext sessionContext)
        {
            this.matchChatClient = matchChatClient ??
                throw new ArgumentNullException(nameof(matchChatClient));
            this.sessionContext = sessionContext ??
                throw new ArgumentNullException(nameof(sessionContext));
        }

        public async Task<MatchChatOperationResult> SendMessageAsync(
            int matchId,
            string message)
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchChatOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return MatchChatOperationResult.ServerFailure(
                    InvalidMatchIdCode);
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return MatchChatOperationResult.ServerFailure(
                    InvalidChatMessageCode);
            }

            SendMatchChatMessageRequest request =
                new SendMatchChatMessageRequest
                {
                    MatchId = matchId,
                    AccountId = sessionContext.AccountId,
                    Message = message.Trim()
                };

            SendMatchChatMessageResponse response =
                await matchChatClient.SendMessageAsync(request);

            if (response == null)
            {
                return MatchChatOperationResult.UnexpectedError();
            }

            MatchChatMessageModel chatMessage =
                MatchChatMessageModel.FromDto(response.Message);

            if (!response.Success)
            {
                return MatchChatOperationResult.ServerFailure(
                    response.MessageCode);
            }

            return MatchChatOperationResult.SuccessResult(
                chatMessage,
                response.MessageCode);
        }
    }
}
