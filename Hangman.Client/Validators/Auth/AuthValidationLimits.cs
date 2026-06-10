namespace Hangman.Client.Validators.Auth
{
    public static class AuthValidationLimits
    {
        public const int FullNameMinimumLength = 3;
        public const int FullNameMaximumLength = 120;

        public const int PhoneMinimumLength = 7;
        public const int PhoneMaximumLength = 20;

        public const int EmailMinimumLength = 6;
        public const int EmailMaximumLength = 200;

        public const int PasswordMinimumLength = 8;
        public const int PasswordMaximumLength = 100;

        public const int LanguageCodeMinimumLength = 2;
        public const int LanguageCodeMaximumLength = 5;

        public const int VerificationCodeLength = 6;
    }
}
