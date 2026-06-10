using System;
using System.Globalization;
using System.Resources;

namespace Hangman.Client.Localization.Messages
{
    public abstract class ResourceMessageProvider<TCode> : IMessageProvider<TCode>
    {
        private readonly ResourceManager resourceManager;
        private readonly string resourceKeyPrefix;

        protected ResourceMessageProvider(ResourceManager resourceManager, string resourceKeyPrefix)
        {
            if (resourceManager == null)
            {
                throw new ArgumentNullException(nameof(resourceManager));
            }

            if (string.IsNullOrWhiteSpace(resourceKeyPrefix))
            {
                throw new ArgumentException("Resource key prefix is required.", nameof(resourceKeyPrefix));
            }

            this.resourceManager = resourceManager;
            this.resourceKeyPrefix = resourceKeyPrefix;
        }

        public string GetMessage(TCode code)
        {
            return GetMessage(code, CultureInfo.CurrentUICulture);
        }

        public string GetMessage(TCode code, CultureInfo cultureInfo)
        {
            CultureInfo targetCulture = cultureInfo ?? CultureInfo.CurrentUICulture;
            string resourceKey = BuildResourceKey(code);

            try
            {
                string message = resourceManager.GetString(resourceKey, targetCulture);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
            catch (MissingManifestResourceException)
            {
                return GetFallbackMessage(code);
            }
            catch (MissingSatelliteAssemblyException)
            {
                return GetFallbackMessage(code);
            }

            return GetFallbackMessage(code);
        }

        protected virtual string BuildResourceKey(TCode code)
        {
            return resourceKeyPrefix + code;
        }

        protected virtual string GetFallbackMessage(TCode code)
        {
            return object.Equals(code, default(TCode)) ? string.Empty : code.ToString();
        }
    }
}
