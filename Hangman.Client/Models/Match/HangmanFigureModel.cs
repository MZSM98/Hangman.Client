namespace Hangman.Client.Models.Match
{
    public class HangmanFigureModel
    {
        public int FailedAttempts { get; set; }

        public int MaxAttempts { get; set; }

        public bool ShowHead { get; set; }

        public bool ShowTorso { get; set; }

        public bool ShowLeftArm { get; set; }

        public bool ShowRightArm { get; set; }

        public bool ShowLeftLeg { get; set; }

        public bool ShowRightLeg { get; set; }
    }
}
