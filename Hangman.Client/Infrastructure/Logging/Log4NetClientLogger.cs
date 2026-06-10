using log4net;
using System;

namespace Hangman.Client.Infrastructure.Logging
{
    public class Log4NetClientLogger : IClientLogger
    {
        private readonly ILog log;

        public Log4NetClientLogger(ILog log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Info(string message)
        {
            log.Info(message);
        }

        public void Warn(string message)
        {
            log.Warn(message);
        }

        public void Error(string message, Exception exception)
        {
            log.Error(message, exception);
        }
    }
}
