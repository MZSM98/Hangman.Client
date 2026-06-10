namespace Hangman.Client.Models.Auth
{
    public class AuthenticatedUserModel
    {
        public int AccountId { get; set; }

        public int PlayerId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string PreferredLanguageCode { get; set; }

        public AuthenticatedUserModel()
        {
            FullName = string.Empty;
            Email = string.Empty;
            PreferredLanguageCode = "es";
        }
    }
}
