using System;

namespace Hangman.Client.Infrastructure.Logging
{
    public interface IClientLogger
    {
        void Info(string message);

        void Warn(string message);

        void Error(string message, Exception exception);
    }
}
