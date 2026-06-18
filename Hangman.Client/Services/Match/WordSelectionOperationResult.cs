using Hangman.Client.Models.Match;

namespace Hangman.Client.Services.Match
{
    public sealed class WordSelectionOperationResult : MatchWorkflowResultBase
    {
        public MatchLobbyModel Lobby { get; private set; }

        public static WordSelectionOperationResult SuccessResult(
            MatchLobbyModel lobby,
            string messageCode)
        {
            return new WordSelectionOperationResult
            {
                Success = true,
                Lobby = lobby,
                MessageCode = messageCode
            };
        }

        public static WordSelectionOperationResult ServerFailure(string messageCode)
        {
            return new WordSelectionOperationResult
            {
                Success = false,
                MessageCode = messageCode
            };
        }

        public static WordSelectionOperationResult SessionInvalid()
        {
            return new WordSelectionOperationResult
            {
                Success = false,
                IsSessionInvalid = true
            };
        }

        public static WordSelectionOperationResult UnexpectedError()
        {
            return new WordSelectionOperationResult
            {
                Success = false,
                IsUnexpectedError = true
            };
        }
    }
}
