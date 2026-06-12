using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchLobbyWorkflow
    {
        Task<MatchLobbyOperationResult> CreateLobbyAsync();

        Task<AvailableLobbiesOperationResult> GetAvailableLobbiesAsync();

        Task<MatchLobbyOperationResult> JoinLobbyAsync(int matchId);

        Task<CurrentLobbyOperationResult> GetCurrentLobbyAsync();

        Task<MatchLobbyOperationResult> LeaveLobbyAsync(int matchId);
    }
}
