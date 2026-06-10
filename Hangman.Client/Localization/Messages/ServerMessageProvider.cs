using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Hangman.Client.Localization.Messages
{
    public class ServerMessageProvider : IServerMessageProvider
    {
        private const string ResourceBaseName = "Hangman.Client.Localization.Resources";

        private readonly ResourceManager resourceManager;

        public ServerMessageProvider()
            : this(new ResourceManager(ResourceBaseName, Assembly.GetExecutingAssembly()))
        {
        }

        internal ServerMessageProvider(ResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new ArgumentNullException(nameof(resourceManager));
            }

            this.resourceManager = resourceManager;
        }

        public string GetMessage(Enum messageCode)
        {
            return GetMessage(messageCode, CultureInfo.CurrentUICulture);
        }

        public string GetMessage(Enum messageCode, CultureInfo cultureInfo)
        {
            if (messageCode == null)
            {
                return string.Empty;
            }

            string moduleName = ServerMessageKeyBuilder.GetModuleName(messageCode.GetType());
            return GetMessage(moduleName, messageCode.ToString(), cultureInfo);
        }

        public string GetMessage(string moduleName, string messageCode)
        {
            return GetMessage(moduleName, messageCode, CultureInfo.CurrentUICulture);
        }

        public string GetMessage(string moduleName, string messageCode, CultureInfo cultureInfo)
        {
            string normalizedModuleName = ServerMessageKeyBuilder.NormalizeModuleName(moduleName);
            string normalizedMessageCode = ServerMessageKeyBuilder.NormalizeMessageCode(messageCode);

            if (string.IsNullOrWhiteSpace(normalizedMessageCode))
            {
                return string.Empty;
            }

            CultureInfo targetCulture = cultureInfo ?? CultureInfo.CurrentUICulture;

            string moduleMessage = GetResourceMessage(normalizedModuleName, normalizedMessageCode, targetCulture);

            if (!string.IsNullOrWhiteSpace(moduleMessage))
            {
                return moduleMessage;
            }

            string commonMessage = GetResourceMessage(
                ServerMessageModuleName.Common,
                normalizedMessageCode,
                targetCulture);

            if (!string.IsNullOrWhiteSpace(commonMessage))
            {
                return commonMessage;
            }

            return normalizedMessageCode;
        }

        private string GetResourceMessage(string moduleName, string messageCode, CultureInfo cultureInfo)
        {
            string resourceKey = ServerMessageKeyBuilder.Build(moduleName, messageCode);

            try
            {
                return resourceManager.GetString(resourceKey, cultureInfo);
            }
            catch (MissingManifestResourceException)
            {
                return string.Empty;
            }
            catch (MissingSatelliteAssemblyException)
            {
                return string.Empty;
            }
        }
    }
}
