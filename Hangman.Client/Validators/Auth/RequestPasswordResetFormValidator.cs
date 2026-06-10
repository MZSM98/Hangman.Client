using Hangman.Client.Models.Auth;

namespace Hangman.Client.Validators.Auth
{
    public static class RequestPasswordResetFormValidator
    {
        public static ClientValidationResult Validate(RequestPasswordResetFormModel form)
        {
            if (form == null)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.RequestPasswordResetFormRequired);
            }

            ClientValidationResult emailValidation =
                AuthFieldValidator.ValidateEmail(form.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            return ClientValidationResult.Success();
        }
    }
}
