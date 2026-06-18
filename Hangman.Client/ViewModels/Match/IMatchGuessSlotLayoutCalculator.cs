using Hangman.Client.Models.Match;

namespace Hangman.Client.ViewModels.Match
{
    internal interface IMatchGuessSlotLayoutCalculator
    {
        MatchGuessSlotLayoutModel Calculate(int slotCount);
    }
}
