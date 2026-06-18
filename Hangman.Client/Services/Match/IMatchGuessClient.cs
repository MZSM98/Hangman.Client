using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchGuessClient
    {
        Task<GetMatchGameStateResponse> GetGameStateAsync(
            GetMatchGameStateRequest request);

        Task<GuessLetterResponse> GuessLetterAsync(
            GuessLetterRequest request);

        Task<GuessWordResponse> GuessWordAsync(
            GuessWordRequest request);

        Task<ResolveGuessTimeoutResponse> ResolveGuessTimeoutAsync(
            ResolveGuessTimeoutRequest request);
    }
}
