using Hangman.Client.Models.Auth;

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

            ClientValidationResult emailValidation = AuthFieldValidator.ValidateEmail(form.Email);
            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ClientValidationResult passwordValidation = AuthFieldValidator.ValidateLoginPassword(form.Password);
            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }

            return ClientValidationResult.Success();
        }
    }
}
