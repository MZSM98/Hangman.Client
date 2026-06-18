using Hangman.Client.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.Models.Match
{
    public class LetterKeyModel : BaseViewModel
    {
        private readonly RelayCommand guessCommand;

        private bool isEnabled;
        private bool isCorrect;
        private bool hasBeenUsed;

        public LetterKeyModel(
            string letter,
            Func<string, Task> guessOperation,
            Func<bool> canGuess)
        {
            if (string.IsNullOrWhiteSpace(letter))
            {
                throw new ArgumentException(
                    "The letter cannot be null or empty.",
                    nameof(letter));
            }

            if (guessOperation == null)
            {
                throw new ArgumentNullException(nameof(guessOperation));
            }

            if (canGuess == null)
            {
                throw new ArgumentNullException(nameof(canGuess));
            }

            Letter = letter;
            isEnabled = true;

            guessCommand = new RelayCommand(
                async () => await guessOperation(Letter),
                () => IsEnabled && canGuess());
        }

        public string Letter { get; private set; }

        public ICommand GuessCommand
        {
            get { return guessCommand; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (SetProperty(ref isEnabled, value) && guessCommand != null)
                {
                    guessCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCorrect
        {
            get { return isCorrect; }
            set { SetProperty(ref isCorrect, value); }
        }

        public bool HasBeenUsed
        {
            get { return hasBeenUsed; }
            set { SetProperty(ref hasBeenUsed, value); }
        }

        public void RaiseCanExecuteChanged()
        {
            guessCommand?.RaiseCanExecuteChanged();
        }
    }
}
