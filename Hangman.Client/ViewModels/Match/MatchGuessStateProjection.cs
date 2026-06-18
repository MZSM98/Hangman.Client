using Hangman.Client.Models.Match;
using System;
using System.Collections.Generic;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessStateProjection
    {
        private MatchGuessStateProjection()
        {
            LetterSlots = new List<LetterSlotModel>();
            GuessHistory = new List<GuessHistoryModel>();
            HangmanFigure = new HangmanFigureModel();
        }

        public bool HasState { get; private set; }

        public int FailedAttempts { get; private set; }

        public int MaxAttempts { get; private set; }

        public int RemainingSeconds { get; private set; }

        public bool IsFinished { get; private set; }

        public DateTime? GuessTurnEndsAt { get; private set; }

        public HangmanFigureModel HangmanFigure { get; private set; }

        public IList<LetterSlotModel> LetterSlots { get; private set; }

        public IList<GuessHistoryModel> GuessHistory { get; private set; }

        public static MatchGuessStateProjection Empty()
        {
            return new MatchGuessStateProjection
            {
                HasState = false,
                FailedAttempts = 0,
                MaxAttempts = 0,
                RemainingSeconds = 0,
                IsFinished = true,
                GuessTurnEndsAt = null,
                HangmanFigure = new HangmanFigureModel()
            };
        }

        public static MatchGuessStateProjection From(
            MatchGameStateModel state,
            IList<LetterSlotModel> letterSlots,
            IList<GuessHistoryModel> guessHistory)
        {
            if (state == null)
            {
                return Empty();
            }

            return new MatchGuessStateProjection
            {
                HasState = true,
                FailedAttempts = state.FailedAttempts,
                MaxAttempts = state.MaxAttempts,
                RemainingSeconds = state.RemainingSeconds,
                IsFinished = state.IsFinished,
                GuessTurnEndsAt = state.GuessTurnEndsAt,
                HangmanFigure = state.HangmanFigure ?? new HangmanFigureModel(),
                LetterSlots = letterSlots ?? new List<LetterSlotModel>(),
                GuessHistory = guessHistory ?? new List<GuessHistoryModel>()
            };
        }
    }
}
