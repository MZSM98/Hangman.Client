using Hangman.Client.Models.Match;
using Hangman.Client.Services.Session;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchGuessWorkflow : IMatchGuessWorkflow
    {
        private const string InvalidMatchIdCode = "InvalidMatchId";
        private const string InvalidLetterCode = "InvalidLetter";
        private const string InvalidWordGuessCode = "InvalidWordGuess";

        private readonly IMatchGuessClient matchGuessClient;
        private readonly IMatchSessionContext sessionContext;

        public MatchGuessWorkflow(
            IMatchGuessClient matchGuessClient,
            IMatchSessionContext sessionContext)
        {
            this.matchGuessClient = matchGuessClient ??
                throw new ArgumentNullException(nameof(matchGuessClient));
            this.sessionContext = sessionContext ??
                throw new ArgumentNullException(nameof(sessionContext));
        }

        public async Task<MatchGuessOperationResult> GetGameStateAsync(
            int matchId)
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchGuessOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return MatchGuessOperationResult.ServerFailure(
                    InvalidMatchIdCode,
                    null);
            }

            GetMatchGameStateRequest request =
                new GetMatchGameStateRequest
                {
                    MatchId = matchId,
                    AccountId = sessionContext.AccountId
                };

            GetMatchGameStateResponse response =
                await matchGuessClient.GetGameStateAsync(request);

            if (response == null)
            {
                return MatchGuessOperationResult.UnexpectedError();
            }

            MatchGameStateModel gameState =
                MatchGuessMapper.ToGameStateModel(response.GameState);

            if (!response.Success)
            {
                return MatchGuessOperationResult.ServerFailure(
                    response.MessageCode,
                    gameState);
            }

            return MatchGuessOperationResult.SuccessResult(
                gameState,
                response.MessageCode);
        }

        public async Task<MatchGuessOperationResult> GuessLetterAsync(
            int matchId,
            string letter)
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchGuessOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return MatchGuessOperationResult.ServerFailure(
                    InvalidMatchIdCode,
                    null);
            }

            if (string.IsNullOrWhiteSpace(letter) ||
                letter.Trim().Length != 1)
            {
                return MatchGuessOperationResult.ServerFailure(
                    InvalidLetterCode,
                    null);
            }

            GuessLetterRequest request = new GuessLetterRequest
            {
                MatchId = matchId,
                AccountId = sessionContext.AccountId,
                Letter = letter.Trim()
            };

            GuessLetterResponse response =
                await matchGuessClient.GuessLetterAsync(request);

            if (response == null)
            {
                return MatchGuessOperationResult.UnexpectedError();
            }

            MatchGameStateModel gameState =
                MatchGuessMapper.ToGameStateModel(response.GameState);

            if (!response.Success)
            {
                return MatchGuessOperationResult.ServerFailure(
                    response.MessageCode,
                    gameState,
                    response.IsCorrect,
                    response.MatchFinished);
            }

            return MatchGuessOperationResult.GuessResult(
                gameState,
                response.IsCorrect,
                response.MatchFinished,
                response.MessageCode);
        }

        public async Task<MatchGuessOperationResult> GuessWordAsync(
            int matchId,
            string word)
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchGuessOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return MatchGuessOperationResult.ServerFailure(
                    InvalidMatchIdCode,
                    null);
            }

            if (string.IsNullOrWhiteSpace(word))
            {
                return MatchGuessOperationResult.ServerFailure(
                    InvalidWordGuessCode,
                    null);
            }

            GuessWordRequest request = new GuessWordRequest
            {
                MatchId = matchId,
                AccountId = sessionContext.AccountId,
                Word = word.Trim()
            };

            GuessWordResponse response =
                await matchGuessClient.GuessWordAsync(request);

            if (response == null)
            {
                return MatchGuessOperationResult.UnexpectedError();
            }

            MatchGameStateModel gameState =
                MatchGuessMapper.ToGameStateModel(response.GameState);

            if (!response.Success)
            {
                return MatchGuessOperationResult.ServerFailure(
                    response.MessageCode,
                    gameState,
                    response.IsCorrect,
                    response.MatchFinished);
            }

            return MatchGuessOperationResult.GuessResult(
                gameState,
                response.IsCorrect,
                response.MatchFinished,
                response.MessageCode);
        }

        public async Task<MatchGuessOperationResult> ResolveGuessTimeoutAsync(
            int matchId)
        {
            if (!sessionContext.HasValidSession)
            {
                return MatchGuessOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return MatchGuessOperationResult.ServerFailure(
                    InvalidMatchIdCode,
                    null);
            }

            ResolveGuessTimeoutRequest request =
                new ResolveGuessTimeoutRequest
                {
                    MatchId = matchId,
                    AccountId = sessionContext.AccountId
                };

            ResolveGuessTimeoutResponse response =
                await matchGuessClient.ResolveGuessTimeoutAsync(request);

            if (response == null)
            {
                return MatchGuessOperationResult.UnexpectedError();
            }

            MatchGameStateModel gameState =
                MatchGuessMapper.ToGameStateModel(response.GameState);

            if (!response.Success)
            {
                return MatchGuessOperationResult.ServerFailure(
                    response.MessageCode,
                    gameState,
                    false,
                    response.MatchFinished);
            }

            return MatchGuessOperationResult.GuessResult(
                gameState,
                false,
                response.MatchFinished,
                response.MessageCode);
        }
    }
}
