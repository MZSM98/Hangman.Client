namespace Hangman.Client.Services.Match
{
    public abstract class MatchWorkflowResultBase
    {
        public bool Success { get; protected set; }

        public string MessageCode { get; protected set; }

        public bool IsSessionInvalid { get; protected set; }

        public bool IsUnexpectedError { get; protected set; }
    }
}
