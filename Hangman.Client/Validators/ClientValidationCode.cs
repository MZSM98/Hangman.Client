namespace Hangman.Client.Validators
{
    public enum ClientValidationCode
    {
        None = 0,

        LoginFormRequired,
        RegisterFormRequired,

        FullNameRequired,
        FullNameTooShort,
        FullNameTooLong,

        DateOfBirthRequired,
        DateOfBirthInFuture,
        DateOfBirthInvalid,

        PhoneRequired,
        PhoneTooShort,
        PhoneTooLong,
        PhoneInvalidFormat,

        PreferredLanguageRequired,
        PreferredLanguageTooShort,
        PreferredLanguageTooLong,
        PreferredLanguageNotAvailable,

        EmailRequired,
        EmailTooShort,
        EmailTooLong,
        EmailInvalidFormat,

        PasswordRequired,
        PasswordTooShort,
        PasswordTooLong,
        PasswordContainsWhiteSpace,
        PasswordRequiresUppercase,
        PasswordRequiresLowercase,
        PasswordRequiresDigit,
        PasswordRequiresSpecialCharacter,

        ConfirmPasswordRequired,
        PasswordsDoNotMatch,

        VerifyEmailFormRequired,
        VerificationCodeRequired,
        VerificationCodeInvalidLength,
        VerificationCodeInvalidFormat,

        RequestPasswordResetFormRequired,
        ResetPasswordFormRequired,

        RecoveryCodeRequired,
        RecoveryCodeInvalidLength,
        RecoveryCodeInvalidFormat
    }
}
