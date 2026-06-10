namespace Hangman.Client.Models.Auth
{
    public class ResetPasswordFormModel
    {
        public string Email { get; set; }

        public string Code { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }

        public void ClearSensitiveData()
        {
            Code = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}
