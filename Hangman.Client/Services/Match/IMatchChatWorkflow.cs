using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchChatWorkflow
    {
        Task<MatchChatOperationResult> SendMessageAsync(
            int matchId,
            string message);
    }
}
