using Hangman.Client.Models.Match;
using System.Collections.Generic;

namespace Hangman.Client.Services.Match
{
    public sealed class AvailableLobbiesOperationResult : MatchWorkflowResultBase
    {
        public IList<AvailableLobbyModel> Lobbies { get; private set; }

        public static AvailableLobbiesOperationResult SuccessResult(
            IList<AvailableLobbyModel> lobbies,
            string messageCode)
        {
            return new AvailableLobbiesOperationResult
            {
                Success = true,
                Lobbies = lobbies ?? new List<AvailableLobbyModel>(),
                MessageCode = messageCode
            };
        }

        public static AvailableLobbiesOperationResult ServerFailure(string messageCode)
        {
            return new AvailableLobbiesOperationResult
            {
                Success = false,
                MessageCode = messageCode,
                Lobbies = new List<AvailableLobbyModel>()
            };
        }

        public static AvailableLobbiesOperationResult SessionInvalid()
        {
            return new AvailableLobbiesOperationResult
            {
                Success = false,
                IsSessionInvalid = true,
                Lobbies = new List<AvailableLobbyModel>()
            };
        }

        public static AvailableLobbiesOperationResult UnexpectedError()
        {
            return new AvailableLobbiesOperationResult
            {
                Success = false,
                IsUnexpectedError = true,
                Lobbies = new List<AvailableLobbyModel>()
            };
        }
    }
}
