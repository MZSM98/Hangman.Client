using Hangman.Client.Models.Auth;
using System;
using System.Net.Mail;

namespace Hangman.Client.Validators.Auth
{
    public static class LoginFormValidator
    {
        public static ClientValidationResult Validate(LoginFormModel form)
        {
            if (form == null)
            {
                return ClientValidationResult.Fail(ClientValidationCode.LoginFormRequired);
            }

            ClientValidationResult emailValidation = ValidateEmail(form.Email);
            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ClientValidationResult passwordValidation = ValidatePassword(form.Password);
            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }

            return ClientValidationResult.Success();
        }

        private static ClientValidationResult ValidateEmail(string email)
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

        private static ClientValidationResult ValidatePassword(string password)
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

        private static bool ContainsWhiteSpace(string value)
        {
            foreach (char character in value)
            {
                if (char.IsWhiteSpace(character))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
