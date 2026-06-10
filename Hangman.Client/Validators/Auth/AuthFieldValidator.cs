using Hangman.Client.Validators;
using Hangman.Client.Validators.Auth;
using System;
using System.Linq;
using System.Net.Mail;

namespace Hangman.Client.Validators.Auth
{
    internal static class AuthFieldValidator
    {
        private static readonly string[] AllowedLanguageCodes = { "es", "en" };

        public static ClientValidationResult ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ClientValidationResult.Fail(ClientValidationCode.FullNameRequired);
            }

            string trimmedFullName = fullName.Trim();

            if (trimmedFullName.Length < AuthValidationLimits.FullNameMinimumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.FullNameTooShort);
            }

            if (trimmedFullName.Length > AuthValidationLimits.FullNameMaximumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.FullNameTooLong);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidateDateOfBirth(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
            {
                return ClientValidationResult.Fail(ClientValidationCode.DateOfBirthRequired);
            }

            DateTime selectedDate = dateOfBirth.Value.Date;
            DateTime today = DateTime.Today;
            DateTime oldestValidDate = today.AddYears(-120);

            if (selectedDate > today)
            {
                return ClientValidationResult.Fail(ClientValidationCode.DateOfBirthInFuture);
            }

            if (selectedDate < oldestValidDate)
            {
                return ClientValidationResult.Fail(ClientValidationCode.DateOfBirthInvalid);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PhoneRequired);
            }

            string trimmedPhone = phone.Trim();

            if (trimmedPhone.Length < AuthValidationLimits.PhoneMinimumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.PhoneTooShort);
            }

            if (trimmedPhone.Length > AuthValidationLimits.PhoneMaximumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.PhoneTooLong);
            }

            if (!trimmedPhone.All(char.IsDigit))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PhoneInvalidFormat);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidatePreferredLanguageCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PreferredLanguageRequired);
            }

            string normalizedLanguageCode = languageCode.Trim().ToLowerInvariant();

            if (normalizedLanguageCode.Length < AuthValidationLimits.LanguageCodeMinimumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.PreferredLanguageTooShort);
            }

            if (normalizedLanguageCode.Length > AuthValidationLimits.LanguageCodeMaximumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.PreferredLanguageTooLong);
            }

            if (!AllowedLanguageCodes.Contains(normalizedLanguageCode))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PreferredLanguageNotAvailable);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ClientValidationResult.Fail(ClientValidationCode.EmailRequired);
            }

            string trimmedEmail = email.Trim();

            if (trimmedEmail.Length < AuthValidationLimits.EmailMinimumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.EmailTooShort);
            }

            if (trimmedEmail.Length > AuthValidationLimits.EmailMaximumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.EmailTooLong);
            }

            if (!HasValidEmailFormat(trimmedEmail))
            {
                return ClientValidationResult.Fail(ClientValidationCode.EmailInvalidFormat);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidateLoginPassword(string password)
        {
            ClientValidationResult basicPasswordValidation = ValidateBasicPassword(password);

            if (!basicPasswordValidation.IsValid)
            {
                return basicPasswordValidation;
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidateNewPassword(string password)
        {
            ClientValidationResult basicPasswordValidation = ValidateBasicPassword(password);

            if (!basicPasswordValidation.IsValid)
            {
                return basicPasswordValidation;
            }

            if (!password.Any(char.IsUpper))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordRequiresUppercase);
            }

            if (!password.Any(char.IsLower))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordRequiresLowercase);
            }

            if (!password.Any(char.IsDigit))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordRequiresDigit);
            }

            if (!password.Any(IsSpecialCharacter))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordRequiresSpecialCharacter);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidateConfirmPassword(string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                return ClientValidationResult.Fail(ClientValidationCode.ConfirmPasswordRequired);
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordsDoNotMatch);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidateVerificationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ClientValidationResult.Fail(ClientValidationCode.VerificationCodeRequired);
            }

            string trimmedCode = code.Trim();

            if (trimmedCode.Length != AuthValidationLimits.VerificationCodeLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.VerificationCodeInvalidLength);
            }

            if (!trimmedCode.All(char.IsDigit))
            {
                return ClientValidationResult.Fail(ClientValidationCode.VerificationCodeInvalidFormat);
            }

            return ClientValidationResult.Success();
        }

        private static ClientValidationResult ValidateBasicPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordRequired);
            }

            if (password.Length < AuthValidationLimits.PasswordMinimumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordTooShort);
            }

            if (password.Length > AuthValidationLimits.PasswordMaximumLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordTooLong);
            }

            if (ContainsWhiteSpace(password))
            {
                return ClientValidationResult.Fail(ClientValidationCode.PasswordContainsWhiteSpace);
            }

            return ClientValidationResult.Success();
        }

        public static ClientValidationResult ValidateRecoveryCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ClientValidationResult.Fail(ClientValidationCode.RecoveryCodeRequired);
            }

            string trimmedCode = code.Trim();

            if (trimmedCode.Length != AuthValidationLimits.VerificationCodeLength)
            {
                return ClientValidationResult.Fail(ClientValidationCode.RecoveryCodeInvalidLength);
            }

            if (!trimmedCode.All(char.IsDigit))
            {
                return ClientValidationResult.Fail(ClientValidationCode.RecoveryCodeInvalidFormat);
            }

            return ClientValidationResult.Success();
        }

        private static bool HasValidEmailFormat(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                return string.Equals(mailAddress.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool IsSpecialCharacter(char character)
        {
            return !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character);
        }

        private static bool ContainsWhiteSpace(string value)
        {
            return value.Any(char.IsWhiteSpace);
        }
    }
}
