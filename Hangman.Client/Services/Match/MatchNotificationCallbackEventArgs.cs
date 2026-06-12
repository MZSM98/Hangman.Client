using System;

namespace Hangman.Client.Services.Match
{
    public class MatchLobbyUpdatedEventArgs : EventArgs
    {
        public MatchLobbyUpdatedEventArgs(int matchId)
        {
            MatchId = matchId;
        }

        public int MatchId { get; private set; }
    }

    public class MatchLobbyClosedEventArgs : EventArgs
    {
        public MatchLobbyClosedEventArgs(int matchId, string messageCode)
        {
            MatchId = matchId;
            MessageCode = messageCode;
        }

        public int MatchId { get; private set; }

        public string MessageCode { get; private set; }
    }

    public class MatchStatusChangedEventArgs : EventArgs
    {
        public MatchStatusChangedEventArgs(int matchId, string matchStatus)
        {
            MatchId = matchId;
            MatchStatus = matchStatus;
        }

        public int MatchId { get; private set; }

        public string MatchStatus { get; private set; }
    }
}
