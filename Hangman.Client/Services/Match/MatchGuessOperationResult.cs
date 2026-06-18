using Hangman.Client.Models.Match;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchGuessOperationResult : MatchWorkflowResultBase
    {
        public MatchGameStateModel GameState { get; private set; }

        public bool IsCorrect { get; private set; }

        public bool MatchFinished { get; private set; }

        public static MatchGuessOperationResult SuccessResult(
            MatchGameStateModel gameState,
            string messageCode)
        {
            return new MatchGuessOperationResult
            {
                Success = true,
                GameState = gameState,
                MessageCode = messageCode
            };
        }

        public static MatchGuessOperationResult GuessResult(
            MatchGameStateModel gameState,
            bool isCorrect,
            bool matchFinished,
            string messageCode)
        {
            return new MatchGuessOperationResult
            {
                Success = true,
                GameState = gameState,
                IsCorrect = isCorrect,
                MatchFinished = matchFinished,
                MessageCode = messageCode
            };
        }

        public static MatchGuessOperationResult ServerFailure(
            string messageCode,
            MatchGameStateModel gameState)
        {
            return new MatchGuessOperationResult
            {
                Success = false,
                MessageCode = messageCode,
                GameState = gameState
            };
        }

        public static MatchGuessOperationResult ServerFailure(
            string messageCode,
            MatchGameStateModel gameState,
            bool isCorrect,
            bool matchFinished)
        {
            return new MatchGuessOperationResult
            {
                Success = false,
                MessageCode = messageCode,
                GameState = gameState,
                IsCorrect = isCorrect,
                MatchFinished = matchFinished
            };
        }

        public static MatchGuessOperationResult SessionInvalid()
        {
            return new MatchGuessOperationResult
            {
                Success = false,
                IsSessionInvalid = true
            };
        }

        public static MatchGuessOperationResult UnexpectedError()
        {
            return new MatchGuessOperationResult
            {
                Success = false,
                IsUnexpectedError = true
            };
        }
    }
}
