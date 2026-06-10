using System;

namespace Hangman.Client.Models.Auth
{
    public class RegisterFormModel
    {
        public string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Phone { get; set; }

        public string PreferredLanguageCode { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public RegisterFormModel()
        {
            FullName = string.Empty;
            Phone = string.Empty;
            PreferredLanguageCode = "es";
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        public void ClearSensitiveData()
        {
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}
