using Hangman.Client.Validators;
using System.Reflection;
using System.Resources;

namespace Hangman.Client.Localization.Messages
{
    public class ClientValidationMessageProvider : ResourceMessageProvider<ClientValidationCode>
    {
        private const string ResourceBaseName = "Hangman.Client.Localization.Resources";
        private const string ValidationResourcePrefix = "Validation_";

        public ClientValidationMessageProvider()
            : base(
                  new ResourceManager(ResourceBaseName, Assembly.GetExecutingAssembly()),
                  ValidationResourcePrefix)
        {
        }
    }
}
