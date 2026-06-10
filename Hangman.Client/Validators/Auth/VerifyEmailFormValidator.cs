using Hangman.Client.Models.Auth;
using System;
using System.Linq;
using System.Net.Mail;

namespace Hangman.Client.Validators.Auth
{
    public static class VerifyEmailFormValidator
    {
        public static ClientValidationResult Validate(VerifyEmailFormModel form)
        {
            if (form == null)
            {
                return ClientValidationResult.Fail(ClientValidationCode.VerifyEmailFormRequired);
            }

            ClientValidationResult emailValidation = ValidateEmail(form.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ClientValidationResult codeValidation = ValidateCode(form.Code);

            if (!codeValidation.IsValid)
            {
                return codeValidation;
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

        private static ClientValidationResult ValidateCode(string code)
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
    }
}
