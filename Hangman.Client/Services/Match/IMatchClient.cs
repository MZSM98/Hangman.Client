using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchClient
    {
        Task<CreateLobbyResponse> CreateLobbyAsync(CreateLobbyRequest request);

        Task<GetAvailableLobbiesResponse> GetAvailableLobbiesAsync(
            GetAvailableLobbiesRequest request);

        Task<JoinLobbyResponse> JoinLobbyAsync(JoinLobbyRequest request);

        Task<GetCurrentLobbyResponse> GetCurrentLobbyAsync(
            GetCurrentLobbyRequest request);

        Task<LeaveLobbyResponse> LeaveLobbyAsync(LeaveLobbyRequest request);
    }
}
