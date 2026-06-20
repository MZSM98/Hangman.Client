using System.Globalization;
using System.Threading;
using System.Windows;

namespace Hangman.Client.Localization
{
    public static class LanguageManager
    {
        private static string currentLanguageCode = "es";

        public static void SetLanguage(string languageCode)
        {
            currentLanguageCode = languageCode;
            CultureInfo culture = new CultureInfo(languageCode);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            });
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            SaveLanguagePreference(languageCode);
        }

        public static void RestoreLanguage()
        {
            string saved = Properties.Settings.Default.LanguageCode;
            string code = string.IsNullOrWhiteSpace(saved) ? "es" : saved;
            SetLanguage(code);
        }

        public static string GetCurrentLanguageCode()
        {
            return currentLanguageCode;
        }

        private static void SaveLanguagePreference(string languageCode)
        {
            Properties.Settings.Default.LanguageCode = languageCode;
            Properties.Settings.Default.Save();
        }
    }
}
