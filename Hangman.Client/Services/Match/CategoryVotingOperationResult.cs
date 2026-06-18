using Hangman.Client.Models.Match;

namespace Hangman.Client.Services.Match
{
    public sealed class CategoryVotingOperationResult : MatchWorkflowResultBase
    {
        public CategoryVotingStateModel VotingState { get; private set; }

        public MatchLobbyModel Lobby { get; private set; }

        public static CategoryVotingOperationResult SuccessResult(
            CategoryVotingStateModel votingState,
            MatchLobbyModel lobby,
            string messageCode)
        {
            return new CategoryVotingOperationResult
            {
                Success = true,
                VotingState = votingState,
                Lobby = lobby,
                MessageCode = messageCode
            };
        }

        public static CategoryVotingOperationResult ServerFailure(
            string messageCode,
            CategoryVotingStateModel votingState)
        {
            return new CategoryVotingOperationResult
            {
                Success = false,
                MessageCode = messageCode,
                VotingState = votingState
            };
        }

        public static CategoryVotingOperationResult SessionInvalid()
        {
            return new CategoryVotingOperationResult
            {
                Success = false,
                IsSessionInvalid = true
            };
        }

        public static CategoryVotingOperationResult UnexpectedError()
        {
            return new CategoryVotingOperationResult
            {
                Success = false,
                IsUnexpectedError = true
            };
        }
    }
}
