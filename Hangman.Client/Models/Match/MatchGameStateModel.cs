using System;
using System.Collections.Generic;

namespace Hangman.Client.Models.Match
{
    public class MatchGameStateModel
    {
        public int MatchId { get; set; }

        public int HostId { get; set; }

        public string HostFullName { get; set; }

        public int? GuestId { get; set; }

        public string GuestFullName { get; set; }

        public string MatchStatus { get; set; }

        public int FailedAttempts { get; set; }

        public int MaxAttempts { get; set; }

        public DateTime? GuessTurnStartedAt { get; set; }

        public DateTime? GuessTurnEndsAt { get; set; }

        public int RemainingSeconds { get; set; }

        public bool IsFinished { get; set; }

        public int? WinnerId { get; set; }

        public string WinnerFullName { get; set; }

        public string WinnerEmail { get; set; }

        public IList<LetterSlotModel> LetterSlots { get; set; }

        public string WordDescription { get; set; }

        public IList<GuessHistoryModel> GuessHistory { get; set; }

        public HangmanFigureModel HangmanFigure { get; set; }
    }
}
