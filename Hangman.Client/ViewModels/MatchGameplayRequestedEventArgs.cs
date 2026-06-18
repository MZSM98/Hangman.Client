using Hangman.Client.Models.Match;
using System;

namespace Hangman.Client.ViewModels
{
    public class MatchGameplayRequestedEventArgs : EventArgs
    {
        public MatchGameplayRequestedEventArgs(MatchLobbyModel lobby)
        {
            Lobby = lobby;
        }

        public MatchLobbyModel Lobby { get; private set; }
    }
}
