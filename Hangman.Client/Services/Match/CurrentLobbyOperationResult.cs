using Hangman.Client.Models.Match;

namespace Hangman.Client.Services.Match
{
    public sealed class CurrentLobbyOperationResult : MatchWorkflowResultBase
    {
        public MatchLobbyModel Lobby { get; private set; }

        public static CurrentLobbyOperationResult SuccessResult(
            MatchLobbyModel lobby,
            string messageCode)
        {
            return new CurrentLobbyOperationResult
            {
                Success = true,
                Lobby = lobby,
                MessageCode = messageCode
            };
        }

        public static CurrentLobbyOperationResult ServerFailure(string messageCode)
        {
            return new CurrentLobbyOperationResult
            {
                Success = false,
                MessageCode = messageCode
            };
        }

        public static CurrentLobbyOperationResult SessionInvalid()
        {
            return new CurrentLobbyOperationResult
            {
                Success = false,
                IsSessionInvalid = true
            };
        }

        public static CurrentLobbyOperationResult UnexpectedError()
        {
            return new CurrentLobbyOperationResult
            {
                Success = false,
                IsUnexpectedError = true
            };
        }
    }
}
