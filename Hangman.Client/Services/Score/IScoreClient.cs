using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Score
{
    public interface IScoreClient
    {
        Task<GetPlayerScoreResponse> GetPlayerScoreAsync(GetPlayerScoreRequest request);
    }
}
