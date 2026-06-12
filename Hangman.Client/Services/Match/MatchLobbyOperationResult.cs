using Hangman.Client.Models.Match;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchLobbyOperationResult : MatchWorkflowResultBase
    {
        public MatchLobbyModel Lobby { get; private set; }

        public static MatchLobbyOperationResult SuccessResult(
            MatchLobbyModel lobby,
            string messageCode)
        {
            return new MatchLobbyOperationResult
            {
                Success = true,
                Lobby = lobby,
                MessageCode = messageCode
            };
        }

        public static MatchLobbyOperationResult ServerFailure(string messageCode)
        {
            return new MatchLobbyOperationResult
            {
                Success = false,
                MessageCode = messageCode
            };
        }

        public static MatchLobbyOperationResult SessionInvalid()
        {
            return new MatchLobbyOperationResult
            {
                Success = false,
                IsSessionInvalid = true
            };
        }

        public static MatchLobbyOperationResult UnexpectedError()
        {
            return new MatchLobbyOperationResult
            {
                Success = false,
                IsUnexpectedError = true
            };
        }
    }
}
