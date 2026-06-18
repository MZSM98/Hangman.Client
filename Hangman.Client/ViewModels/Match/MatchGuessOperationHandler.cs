using Hangman.Client.Services.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessOperationHandler :
        IMatchGuessOperationHandler
    {
        private const string LobbySubscriptionFailedCode =
            "LobbySubscriptionFailed";

        private readonly IMatchGuessWorkflow matchGuessWorkflow;
        private readonly IMatchLobbyWorkflow matchLobbyWorkflow;
        private readonly IMatchGuessNotificationController notificationController;

        public MatchGuessOperationHandler(
            IMatchGuessWorkflow matchGuessWorkflow,
            IMatchLobbyWorkflow matchLobbyWorkflow,
            IMatchGuessNotificationController notificationController)
        {
            this.matchGuessWorkflow = matchGuessWorkflow ??
                throw new ArgumentNullException(nameof(matchGuessWorkflow));
            this.matchLobbyWorkflow = matchLobbyWorkflow ??
                throw new ArgumentNullException(nameof(matchLobbyWorkflow));
            this.notificationController = notificationController ??
                throw new ArgumentNullException(nameof(notificationController));
        }

        public async Task<MatchGuessOperationResult> SubscribeAndLoadGameStateAsync(
            int matchId)
        {
            bool subscribed =
                await notificationController.SubscribeToLobbyAsync(matchId);

            if (!subscribed)
            {
                return MatchGuessOperationResult.ServerFailure(
                    LobbySubscriptionFailedCode,
                    null);
            }

            return await LoadGameStateAsync(matchId);
        }

        public Task<MatchGuessOperationResult> LoadGameStateAsync(
            int matchId)
        {
            return matchGuessWorkflow.GetGameStateAsync(matchId);
        }

        public Task<MatchGuessOperationResult> GuessLetterAsync(
            int matchId,
            string letter)
        {
            return matchGuessWorkflow.GuessLetterAsync(
                matchId,
                letter);
        }

        public Task<MatchGuessOperationResult> GuessWordAsync(
            int matchId,
            string word)
        {
            return matchGuessWorkflow.GuessWordAsync(
                matchId,
                word);
        }

        public Task<MatchGuessOperationResult> ResolveTimeoutAsync(
            int matchId)
        {
            return matchGuessWorkflow.ResolveGuessTimeoutAsync(matchId);
        }

        public async Task<MatchLobbyOperationResult> SurrenderAsync(
            int matchId)
        {
            MatchLobbyOperationResult result =
                await matchLobbyWorkflow.LeaveLobbyAsync(matchId);

            if (result != null && result.Success)
            {
                await notificationController.UnsubscribeFromLobbyAsync(matchId);
            }

            return result;
        }
    }
}
