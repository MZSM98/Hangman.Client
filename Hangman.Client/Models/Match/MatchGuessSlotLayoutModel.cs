namespace Hangman.Client.Models.Match
{
    public sealed class MatchGuessSlotLayoutModel
    {
        public MatchGuessSlotLayoutModel(
            double width,
            double height,
            double fontSize)
        {
            Width = width;
            Height = height;
            FontSize = fontSize;
        }

        public double Width { get; private set; }

        public double Height { get; private set; }

        public double FontSize { get; private set; }
    }
}
