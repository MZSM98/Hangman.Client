using Hangman.Client.Models.Match;
using Hangman.Client.Services.Session;
using Hangman.Contracts.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchLobbyWorkflow : IMatchLobbyWorkflow
    {
        private const string InvalidMatchIdCode = "InvalidMatchId";
        private const string NoActiveLobbyCode = "NoActiveLobby";

        private readonly IMatchClient matchClient;
        private readonly IMatchSessionContext sessionContext;

        public MatchLobbyWorkflow(
            IMatchClient matchClient,
            IMatchSessionContext sessionContext)
        {
            this.matchClient = matchClient ??
                throw new ArgumentNullException(nameof(matchClient));
            this.sessionContext = sessionContext ??
                throw new ArgumentNullException(nameof(sessionContext));
        }

        public async Task<MatchLobbyOperationResult> CreateLobbyAsync()
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchLobbyOperationResult.SessionInvalid();
            }

            CreateLobbyRequest request = new CreateLobbyRequest
            {
                HostAccountId = sessionContext.AccountId,
                HostLanguageCode = sessionContext.PreferredLanguageCode
            };

            CreateLobbyResponse response = await matchClient.CreateLobbyAsync(request);

            if (response == null)
            {
                return MatchLobbyOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return MatchLobbyOperationResult.ServerFailure(response.MessageCode);
            }

            return MatchLobbyOperationResult.SuccessResult(
                MatchLobbyModel.FromDto(response.Lobby),
                response.MessageCode);
        }

        public async Task<AvailableLobbiesOperationResult> GetAvailableLobbiesAsync()
        {
            if (!sessionContext.HasValidSession)
            {
                return AvailableLobbiesOperationResult.SessionInvalid();
            }

            GetAvailableLobbiesRequest request = new GetAvailableLobbiesRequest
            {
                AccountId = sessionContext.AccountId
            };

            GetAvailableLobbiesResponse response =
                await matchClient.GetAvailableLobbiesAsync(request);

            if (response == null)
            {
                return AvailableLobbiesOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return AvailableLobbiesOperationResult.ServerFailure(response.MessageCode);
            }

            IList<AvailableLobbyModel> lobbies = new List<AvailableLobbyModel>();

            if (response.Lobbies != null)
            {
                lobbies = response.Lobbies
                    .Select(AvailableLobbyModel.FromDto)
                    .Where(lobby => lobby != null)
                    .ToList();
            }

            return AvailableLobbiesOperationResult.SuccessResult(
                lobbies,
                response.MessageCode);
        }

        public async Task<MatchLobbyOperationResult> JoinLobbyAsync(int matchId)
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchLobbyOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return MatchLobbyOperationResult.ServerFailure(InvalidMatchIdCode);
            }

            JoinLobbyRequest request = new JoinLobbyRequest
            {
                MatchId = matchId,
                GuestAccountId = sessionContext.AccountId,
                GuestLanguageCode = sessionContext.PreferredLanguageCode
            };

            JoinLobbyResponse response = await matchClient.JoinLobbyAsync(request);

            if (response == null)
            {
                return MatchLobbyOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return MatchLobbyOperationResult.ServerFailure(response.MessageCode);
            }

            return MatchLobbyOperationResult.SuccessResult(
                MatchLobbyModel.FromDto(response.Lobby),
                response.MessageCode);
        }

        public async Task<CurrentLobbyOperationResult> GetCurrentLobbyAsync()
        {
            if (!sessionContext.HasValidSession)
            {
                return CurrentLobbyOperationResult.SessionInvalid();
            }

            GetCurrentLobbyRequest request = new GetCurrentLobbyRequest
            {
                AccountId = sessionContext.AccountId
            };

            GetCurrentLobbyResponse response =
                await matchClient.GetCurrentLobbyAsync(request);

            if (response == null)
            {
                return CurrentLobbyOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return CurrentLobbyOperationResult.ServerFailure(response.MessageCode);
            }

            return CurrentLobbyOperationResult.SuccessResult(
                MatchLobbyModel.FromDto(response.Lobby),
                response.MessageCode);
        }

        public async Task<MatchLobbyOperationResult> LeaveLobbyAsync(int matchId)
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchLobbyOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return MatchLobbyOperationResult.ServerFailure(NoActiveLobbyCode);
            }

            LeaveLobbyRequest request = new LeaveLobbyRequest
            {
                MatchId = matchId,
                AccountId = sessionContext.AccountId
            };

            LeaveLobbyResponse response = await matchClient.LeaveLobbyAsync(request);

            if (response == null)
            {
                return MatchLobbyOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return MatchLobbyOperationResult.ServerFailure(response.MessageCode);
            }

            return MatchLobbyOperationResult.SuccessResult(
                null,
                response.MessageCode);
        }
    }
}
