using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchChatClient : IDisposable
    {
        Task<SendMatchChatMessageResponse> SendMessageAsync(
            SendMatchChatMessageRequest request);
    }
}
