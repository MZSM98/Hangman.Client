namespace Hangman.Client.Validators
{
    public class ClientValidationResult
    {
        public bool IsValid { get; private set; }

        public ClientValidationCode Code { get; private set; }

        private ClientValidationResult(bool isValid, ClientValidationCode code)
        {
            IsValid = isValid;
            Code = code;
        }

        public static ClientValidationResult Success()
        {
            return new ClientValidationResult(true, ClientValidationCode.None);
        }

        public static ClientValidationResult Fail(ClientValidationCode code)
        {
            return new ClientValidationResult(false, code);
        }
    }
}
