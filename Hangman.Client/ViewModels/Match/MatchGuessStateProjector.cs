using Hangman.Client.Models.Match;
using System.Collections.Generic;
using System.Linq;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessStateProjector :
        IMatchGuessStateProjector
    {
        public MatchGuessStateProjection Project(MatchGameStateModel state)
        {
            if (state == null)
            {
                return MatchGuessStateProjection.Empty();
            }

            IList<LetterSlotModel> letterSlots = BuildLetterSlots(state);
            IList<GuessHistoryModel> guessHistory = BuildGuessHistory(state);

            return MatchGuessStateProjection.From(
                state,
                letterSlots,
                guessHistory);
        }

        private static IList<LetterSlotModel> BuildLetterSlots(
            MatchGameStateModel state)
        {
            if (state.LetterSlots == null)
            {
                return new List<LetterSlotModel>();
            }

            return state.LetterSlots
                .Where(slot => slot != null)
                .GroupBy(slot => slot.Position)
                .Select(group => group.First())
                .OrderBy(slot => slot.Position)
                .ToList();
        }

        private static IList<GuessHistoryModel> BuildGuessHistory(
            MatchGameStateModel state)
        {
            if (state.GuessHistory == null)
            {
                return new List<GuessHistoryModel>();
            }

            return state.GuessHistory
                .Where(guess => guess != null)
                .OrderBy(guess => guess.CreatedAt)
                .ToList();
        }
    }
}
