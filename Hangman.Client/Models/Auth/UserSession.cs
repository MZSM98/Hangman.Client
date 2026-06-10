namespace Hangman.Client.Models.Auth
{
    public static class UserSession
    {
        public static AuthenticatedUserModel CurrentUser { get; private set; }

        public static bool IsAuthenticated
        {
            get { return CurrentUser != null; }
        }

        public static void Start(AuthenticatedUserModel user)
        {
            CurrentUser = user;
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}
