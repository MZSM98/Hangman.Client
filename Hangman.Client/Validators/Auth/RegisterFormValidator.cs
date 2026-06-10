using Hangman.Client.Models.Auth;

namespace Hangman.Client.Validators.Auth
{
    public static class RegisterFormValidator
    {
        public static ClientValidationResult Validate(RegisterFormModel form)
        {
            if (form == null)
            {
                return ClientValidationResult.Fail(ClientValidationCode.RegisterFormRequired);
            }

            ClientValidationResult fullNameValidation = AuthFieldValidator.ValidateFullName(form.FullName);
            if (!fullNameValidation.IsValid)
            {
                return fullNameValidation;
            }

            ClientValidationResult dateOfBirthValidation = AuthFieldValidator.ValidateDateOfBirth(form.DateOfBirth);
            if (!dateOfBirthValidation.IsValid)
            {
                return dateOfBirthValidation;
            }

            ClientValidationResult phoneValidation = AuthFieldValidator.ValidatePhone(form.Phone);
            if (!phoneValidation.IsValid)
            {
                return phoneValidation;
            }

            ClientValidationResult languageValidation =
                AuthFieldValidator.ValidatePreferredLanguageCode(form.PreferredLanguageCode);

            if (!languageValidation.IsValid)
            {
                return languageValidation;
            }

            ClientValidationResult emailValidation = AuthFieldValidator.ValidateEmail(form.Email);
            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ClientValidationResult passwordValidation = AuthFieldValidator.ValidateNewPassword(form.Password);
            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }

            ClientValidationResult confirmPasswordValidation =
                AuthFieldValidator.ValidateConfirmPassword(form.Password, form.ConfirmPassword);

            if (!confirmPasswordValidation.IsValid)
            {
                return confirmPasswordValidation;
            }

            return ClientValidationResult.Success();
        }
    }
}
