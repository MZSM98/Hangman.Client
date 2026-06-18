using Hangman.Client.Models.Match;
using System;

namespace Hangman.Client.ViewModels
{
    public sealed class MatchResultRequestedEventArgs : EventArgs
    {
        public MatchResultRequestedEventArgs(MatchGameStateModel gameState)
        {
            GameState = gameState ??
                throw new ArgumentNullException(nameof(gameState));
        }

        public MatchGameStateModel GameState { get; private set; }
    }
}
