using Hangman.Client.Models.Match;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hangman.Client.ViewModels.Match
{
    internal interface IMatchGuessKeyboardController
    {
        ObservableCollection<LetterKeyModel> LetterKeys { get; }

        void SynchronizeWithHistory(
            IEnumerable<GuessHistoryModel> guessHistory,
            bool canEnableUnusedKeys);

        void RaiseCanExecuteChanged();
    }
}
