using Hangman.Client.Models.Profile;
using Hangman.Client.Validators.Common;
using System;
using System.Linq;

namespace Hangman.Client.Validators.Profile
{
    public static class ProfileFormValidator
    {
        public static ClientValidationResult Validate(ProfileFormModel form)
        {
            if (form == null)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.ProfileFormRequired);
            }

            ClientValidationResult accountValidation = ValidateAccount(form.AccountId);

            if (!accountValidation.IsValid)
            {
                return accountValidation;
            }

            ClientValidationResult fullNameValidation = ValidateFullName(form.FullName);

            if (!fullNameValidation.IsValid)
            {
                return fullNameValidation;
            }

            ClientValidationResult dateValidation =
                ValidateDateOfBirth(form.DateOfBirth);

            if (!dateValidation.IsValid)
            {
                return dateValidation;
            }

            ClientValidationResult phoneValidation = ValidatePhone(form.Phone);

            if (!phoneValidation.IsValid)
            {
                return phoneValidation;
            }

            return ValidatePreferredLanguage(form.PreferredLanguageCode);
        }

        private static ClientValidationResult ValidateAccount(int accountId)
        {
            if (accountId <= 0)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.AccountRequired);
            }

            return ClientValidationResult.Success();
        }

        private static ClientValidationResult ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.FullNameRequired);
            }

            string trimmedValue = fullName.Trim();

            if (trimmedValue.Length < PlayerValidationLimits.FullNameMinimumLength)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.FullNameTooShort);
            }

            if (trimmedValue.Length > PlayerValidationLimits.FullNameMaximumLength)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.FullNameTooLong);
            }

            return ClientValidationResult.Success();
        }

        private static ClientValidationResult ValidateDateOfBirth(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.DateOfBirthRequired);
            }

            DateTime selectedDate = dateOfBirth.Value.Date;
            DateTime today = DateTime.Today;

            if (selectedDate > today)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.DateOfBirthInFuture);
            }

            if (selectedDate < today.AddYears(-PlayerValidationLimits.MaximumAgeInYears))
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.DateOfBirthInvalid);
            }

            return ClientValidationResult.Success();
        }

        private static ClientValidationResult ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PhoneRequired);
            }

            string trimmedValue = phone.Trim();

            if (trimmedValue.Length < PlayerValidationLimits.PhoneMinimumLength)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PhoneTooShort);
            }

            if (trimmedValue.Length > PlayerValidationLimits.PhoneMaximumLength)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PhoneTooLong);
            }

            if (!trimmedValue.All(char.IsDigit))
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PhoneInvalidFormat);
            }

            return ClientValidationResult.Success();
        }

        private static ClientValidationResult ValidatePreferredLanguage(
            string preferredLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(preferredLanguageCode))
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PreferredLanguageRequired);
            }

            string normalizedValue = preferredLanguageCode.Trim().ToLowerInvariant();

            if (normalizedValue.Length < PlayerValidationLimits.PreferredLanguageMinimumLength)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PreferredLanguageTooShort);
            }

            if (normalizedValue.Length > PlayerValidationLimits.PreferredLanguageMaximumLength)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PreferredLanguageTooLong);
            }

            if (normalizedValue != "es" && normalizedValue != "en")
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.PreferredLanguageNotAvailable);
            }

            return ClientValidationResult.Success();
        }
    }
}
