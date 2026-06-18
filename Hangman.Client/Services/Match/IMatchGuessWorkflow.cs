using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchGuessWorkflow
    {
        Task<MatchGuessOperationResult> GetGameStateAsync(int matchId);

        Task<MatchGuessOperationResult> GuessLetterAsync(
            int matchId,
            string letter);

        Task<MatchGuessOperationResult> GuessWordAsync(
            int matchId,
            string word);

        Task<MatchGuessOperationResult> ResolveGuessTimeoutAsync(int matchId);
    }
}
