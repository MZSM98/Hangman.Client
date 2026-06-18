using Hangman.Client.Models.Match;
using System;

namespace Hangman.Client.ViewModels
{
    public class MatchGuessRequestedEventArgs : EventArgs
    {
        public MatchGuessRequestedEventArgs(MatchLobbyModel lobby)
        {
            Lobby = lobby;
        }

        public MatchLobbyModel Lobby { get; private set; }
    }
}
