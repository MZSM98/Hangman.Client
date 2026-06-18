using Hangman.Client.Models.Match;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchChatOperationResult : MatchWorkflowResultBase
    {
        public MatchChatMessageModel Message { get; private set; }

        public static MatchChatOperationResult SuccessResult(
            MatchChatMessageModel message,
            string messageCode)
        {
            return new MatchChatOperationResult
            {
                Success = true,
                Message = message,
                MessageCode = messageCode
            };
        }

        public static MatchChatOperationResult ServerFailure(
            string messageCode)
        {
            return new MatchChatOperationResult
            {
                Success = false,
                MessageCode = messageCode
            };
        }

        public static MatchChatOperationResult SessionInvalid()
        {
            return new MatchChatOperationResult
            {
                Success = false,
                IsSessionInvalid = true
            };
        }

        public static MatchChatOperationResult UnexpectedError()
        {
            return new MatchChatOperationResult
            {
                Success = false,
                IsUnexpectedError = true
            };
        }
    }
}
