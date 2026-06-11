namespace Hangman.Client.Validators.Common
{
    public static class PlayerValidationLimits
    {
        public const int FullNameMinimumLength = 3;
        public const int FullNameMaximumLength = 120;

        public const int PhoneMinimumLength = 7;
        public const int PhoneMaximumLength = 20;

        public const int PreferredLanguageMinimumLength = 2;
        public const int PreferredLanguageMaximumLength = 5;

        public const int MaximumAgeInYears = 120;
    }
}
