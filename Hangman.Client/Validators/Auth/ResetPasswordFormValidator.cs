using Hangman.Client.Models.Auth;

namespace Hangman.Client.Validators.Auth
{
    public static class ResetPasswordFormValidator
    {
        public static ClientValidationResult Validate(ResetPasswordFormModel form)
        {
            if (form == null)
            {
                return ClientValidationResult.Fail(
                    ClientValidationCode.ResetPasswordFormRequired);
            }

            ClientValidationResult emailValidation =
                AuthFieldValidator.ValidateEmail(form.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ClientValidationResult codeValidation =
                AuthFieldValidator.ValidateRecoveryCode(form.Code);

            if (!codeValidation.IsValid)
            {
                return codeValidation;
            }

            ClientValidationResult passwordValidation =
                AuthFieldValidator.ValidateNewPassword(form.NewPassword);

            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }

            ClientValidationResult confirmPasswordValidation =
                AuthFieldValidator.ValidateConfirmPassword(
                    form.NewPassword,
                    form.ConfirmPassword);

            if (!confirmPasswordValidation.IsValid)
            {
                return confirmPasswordValidation;
            }

            return ClientValidationResult.Success();
        }
    }
}
