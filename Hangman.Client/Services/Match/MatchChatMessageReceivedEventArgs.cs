using Hangman.Client.Models.Match;
using System;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchChatMessageReceivedEventArgs : EventArgs
    {
        public MatchChatMessageReceivedEventArgs(
            MatchChatMessageModel message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public MatchChatMessageModel Message { get; private set; }
    }
}
