using System;
using System.Globalization;

namespace Hangman.Client.Localization.Messages
{
    public interface IServerMessageProvider
    {
        string GetMessage(Enum messageCode);

        string GetMessage(Enum messageCode, CultureInfo cultureInfo);

        string GetMessage(string moduleName, string messageCode);

        string GetMessage(string moduleName, string messageCode, CultureInfo cultureInfo);
    }
}
