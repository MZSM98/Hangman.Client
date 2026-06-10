using log4net;
using System;

namespace Hangman.Client.Infrastructure.Logging
{
    public static class ClientLoggerFactory
    {
        public static IClientLogger Create<T>()
        {
            return Create(typeof(T));
        }

        public static IClientLogger Create(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            ILog log = LogManager.GetLogger(type);
            return new Log4NetClientLogger(log);
        }
    }
}
