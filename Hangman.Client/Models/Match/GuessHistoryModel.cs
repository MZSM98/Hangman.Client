using System;

namespace Hangman.Client.Models.Match
{
    public class GuessHistoryModel
    {
        public string GuessType { get; set; }

        public string Value { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
