using Hangman.Client.Models.Match;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessSlotLayoutCalculator :
        IMatchGuessSlotLayoutCalculator
    {
        public MatchGuessSlotLayoutModel Calculate(int slotCount)
        {
            if (slotCount <= 0)
            {
                return CreateDefaultLayout();
            }

            if (slotCount <= 8)
            {
                return new MatchGuessSlotLayoutModel(42, 58, 28);
            }

            if (slotCount <= 10)
            {
                return new MatchGuessSlotLayoutModel(32, 52, 24);
            }

            if (slotCount <= 12)
            {
                return new MatchGuessSlotLayoutModel(27, 48, 21);
            }

            if (slotCount <= 15)
            {
                return new MatchGuessSlotLayoutModel(22, 42, 17);
            }

            return new MatchGuessSlotLayoutModel(18, 38, 14);
        }

        private static MatchGuessSlotLayoutModel CreateDefaultLayout()
        {
            return new MatchGuessSlotLayoutModel(42, 58, 28);
        }
    }
}
