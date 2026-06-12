namespace Hangman.Client.Services.Session
{
    public interface IMatchSessionContext
    {
        bool HasValidSession { get; }

        int AccountId { get; }

        string PreferredLanguageCode { get; }
    }
}
