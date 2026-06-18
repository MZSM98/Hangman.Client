using Hangman.Client.Models.Match;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessKeyboardController :
        IMatchGuessKeyboardController
    {
        private const string LetterGuessType = "Letter";

        private static readonly string[] KeyboardLetters =
        {
            "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P",
            "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ñ",
            "Z", "X", "C", "V", "B", "N", "M"
        };

        public MatchGuessKeyboardController(
            Func<string, Task> guessOperation,
            Func<bool> canGuess)
        {
            if (guessOperation == null)
            {
                throw new ArgumentNullException(nameof(guessOperation));
            }

            if (canGuess == null)
            {
                throw new ArgumentNullException(nameof(canGuess));
            }

            LetterKeys = new ObservableCollection<LetterKeyModel>();

            InitializeKeyboard(guessOperation, canGuess);
        }

        public ObservableCollection<LetterKeyModel> LetterKeys { get; private set; }

        public void SynchronizeWithHistory(
            IEnumerable<GuessHistoryModel> guessHistory,
            bool canEnableUnusedKeys)
        {
            IList<GuessHistoryModel> safeHistory = guessHistory == null
                ? new List<GuessHistoryModel>()
                : guessHistory
                    .Where(guess => guess != null)
                    .ToList();

            foreach (LetterKeyModel key in LetterKeys)
            {
                GuessHistoryModel usedGuess = safeHistory
                    .Where(guess =>
                        guess.GuessType == LetterGuessType &&
                        string.Equals(
                            guess.Value,
                            key.Letter,
                            StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(guess => guess.CreatedAt)
                    .FirstOrDefault();

                if (usedGuess == null)
                {
                    key.HasBeenUsed = false;
                    key.IsCorrect = false;
                    key.IsEnabled = canEnableUnusedKeys;
                    continue;
                }

                key.HasBeenUsed = true;
                key.IsCorrect = usedGuess.IsCorrect;
                key.IsEnabled = false;
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            foreach (LetterKeyModel key in LetterKeys)
            {
                key.RaiseCanExecuteChanged();
            }
        }

        private void InitializeKeyboard(
            Func<string, Task> guessOperation,
            Func<bool> canGuess)
        {
            foreach (string letter in KeyboardLetters)
            {
                LetterKeys.Add(
                    new LetterKeyModel(
                        letter,
                        guessOperation,
                        canGuess));
            }
        }
    }
}
