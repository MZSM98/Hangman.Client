using Hangman.Client.Models.Auth;

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

            ClientValidationResult emailValidation = AuthFieldValidator.ValidateEmail(form.Email);
            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ClientValidationResult codeValidation = AuthFieldValidator.ValidateVerificationCode(form.Code);
            if (!codeValidation.IsValid)
            {
                return codeValidation;
            }

            return ClientValidationResult.Success();
        }
    }
}
