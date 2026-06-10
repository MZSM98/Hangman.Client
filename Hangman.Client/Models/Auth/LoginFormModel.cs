namespace Hangman.Client.Models.Auth
{
    public class LoginFormModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public LoginFormModel()
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        public void ClearPassword()
        {
            Password = string.Empty;
        }
    }
}
