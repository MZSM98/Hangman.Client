namespace Hangman.Client.Models.Auth
{
    public class VerifyEmailFormModel
    {
        public string Email { get; set; }

        public string Code { get; set; }

        public VerifyEmailFormModel()
        {
            Email = string.Empty;
            Code = string.Empty;
        }

        public void ClearCode()
        {
            Code = string.Empty;
        }
    }
}
