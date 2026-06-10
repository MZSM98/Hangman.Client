using System.Globalization;

namespace Hangman.Client.Localization.Messages
{
    public interface IMessageProvider<in TCode>
    {
        string GetMessage(TCode code);

        string GetMessage(TCode code, CultureInfo cultureInfo);
    }
}
