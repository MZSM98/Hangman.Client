using Hangman.Client.Models.Auth;

namespace Hangman.Client.Services.Session
{
    public sealed class MatchSessionContext : IMatchSessionContext
    {
        public bool HasValidSession
        {
            get
            {
                return UserSession.IsAuthenticated &&
                       UserSession.CurrentUser.AccountId > 0 &&
                       !string.IsNullOrWhiteSpace(UserSession.CurrentUser.PreferredLanguageCode);
            }
        }

        public int AccountId
        {
            get { return UserSession.CurrentUser.AccountId; }
        }

        public string PreferredLanguageCode
        {
            get { return UserSession.CurrentUser.PreferredLanguageCode; }
        }
    }
}
