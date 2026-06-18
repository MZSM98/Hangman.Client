namespace Hangman.Client.Models.Match
{
    public class LetterSlotModel
    {
        public int Position { get; set; }

        public string Letter { get; set; }

        public bool IsRevealed { get; set; }
    }
}
