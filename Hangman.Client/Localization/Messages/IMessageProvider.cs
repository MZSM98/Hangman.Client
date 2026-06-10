using System.Globalization;

namespace Hangman.Client.Localization.Messages
{
    public interface IMessageProvider<TCode>
    {
        string GetMessage(TCode code);

        string GetMessage(TCode code, CultureInfo cultureInfo);
    }
}
