using Hangman.Client.Models.Match;

namespace Hangman.Client.ViewModels.Match
{
    internal interface IMatchGuessRoleEvaluator
    {
        bool IsCurrentUserHost(MatchGameStateModel gameState);

        bool IsCurrentUserGuest(MatchGameStateModel gameState);

        bool CanShowGuestControls(
            MatchGameStateModel gameState,
            bool isFinished);

        bool CanShowHostReadOnly(MatchGameStateModel gameState);

        bool CanShowTimer(
            MatchGameStateModel gameState,
            bool isFinished);

        bool CanEnableUnusedLetterKey(
            MatchGameStateModel gameState,
            int remainingSeconds);

        bool CanResolveTimeout(
            MatchGameStateModel gameState,
            int remainingSeconds);
    }
}
