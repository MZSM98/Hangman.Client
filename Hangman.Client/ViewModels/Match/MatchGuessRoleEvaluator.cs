using Hangman.Client.Models.Auth;
using Hangman.Client.Models.Match;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessRoleEvaluator : IMatchGuessRoleEvaluator
    {
        private const string InProgressStatus = "InProgress";

        public bool IsCurrentUserHost(MatchGameStateModel gameState)
        {
            return gameState != null &&
                   UserSession.CurrentUser != null &&
                   UserSession.CurrentUser.PlayerId == gameState.HostId;
        }

        public bool IsCurrentUserGuest(MatchGameStateModel gameState)
        {
            return gameState != null &&
                   UserSession.CurrentUser != null &&
                   gameState.GuestId.HasValue &&
                   UserSession.CurrentUser.PlayerId == gameState.GuestId.Value;
        }

        public bool CanShowGuestControls(
            MatchGameStateModel gameState,
            bool isFinished)
        {
            return IsCurrentUserGuest(gameState) && !isFinished;
        }

        public bool CanShowHostReadOnly(MatchGameStateModel gameState)
        {
            return IsCurrentUserHost(gameState);
        }

        public bool CanShowTimer(
            MatchGameStateModel gameState,
            bool isFinished)
        {
            return IsCurrentUserGuest(gameState) && !isFinished;
        }

        public bool CanEnableUnusedLetterKey(
            MatchGameStateModel gameState,
            int remainingSeconds)
        {
            return gameState != null &&
                   IsCurrentUserGuest(gameState) &&
                   !gameState.IsFinished &&
                   gameState.MatchStatus == InProgressStatus &&
                   remainingSeconds > 0;
        }

        public bool CanResolveTimeout(
            MatchGameStateModel gameState,
            int remainingSeconds)
        {
            return gameState != null &&
                   !gameState.IsFinished &&
                   gameState.MatchStatus == InProgressStatus &&
                   remainingSeconds <= 0;
        }
    }
}
