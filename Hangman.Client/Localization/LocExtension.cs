using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Markup;

namespace Hangman.Client.Localization
{
    [MarkupExtensionReturnType(typeof(string))]
    public class LocExtension : MarkupExtension
    {
        private const string ResourceBaseName = "Hangman.Client.Localization.Resources";

        private static readonly ResourceManager ResourceManager =
            new ResourceManager(ResourceBaseName, Assembly.GetExecutingAssembly());

        public LocExtension()
        {
        }

        public LocExtension(string key)
        {
            Key = key;
        }

        public string Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Get(Key);
        }

        public static string Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            string value = ResourceManager.GetString(key, CultureInfo.CurrentUICulture);

            if (string.IsNullOrWhiteSpace(value))
            {
                return key;
            }

            return value;
        }
    }
}
