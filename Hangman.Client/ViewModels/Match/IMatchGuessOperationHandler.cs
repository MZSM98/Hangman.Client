using Hangman.Client.Services.Match;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Match
{
    internal interface IMatchGuessOperationHandler
    {
        Task<MatchGuessOperationResult> SubscribeAndLoadGameStateAsync(
            int matchId);

        Task<MatchGuessOperationResult> LoadGameStateAsync(
            int matchId);

        Task<MatchGuessOperationResult> GuessLetterAsync(
            int matchId,
            string letter);

        Task<MatchGuessOperationResult> GuessWordAsync(
            int matchId,
            string word);

        Task<MatchGuessOperationResult> ResolveTimeoutAsync(
            int matchId);

        Task<MatchLobbyOperationResult> SurrenderAsync(
            int matchId);
    }
}
