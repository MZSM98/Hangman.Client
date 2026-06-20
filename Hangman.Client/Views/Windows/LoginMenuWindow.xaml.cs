using Hangman.Client.Localization;
using Hangman.Client.ViewModels;
using System;
using System.Windows;

namespace Hangman.Client.Views.Windows
{
    public partial class LoginMenuWindow : Window
    {
        private readonly LoginMenuViewModel viewModel;

        public LoginMenuWindow()
        {
            InitializeComponent();

            viewModel = new LoginMenuViewModel();
            DataContext = viewModel;

            viewModel.LoginSucceeded += OnLoginSucceeded;
            viewModel.InformationMessageRequested += OnInformationMessageRequested;
        }

        private void OnLoginSucceeded(object sender, EventArgs e)
        {
            MainMenuWindow mainMenuWindow = new MainMenuWindow();
            mainMenuWindow.Show();

            Close();
        }

        private void OnInformationMessageRequested(
            object sender,
            InformationMessageRequestedEventArgs e)
        {
            MessageBox.Show(
                e.Message,
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnSetSpanish(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage("es");
            LoginMenuWindow newWindow = new LoginMenuWindow();
            newWindow.Show();
            Close();
        }

        private void OnSetEnglish(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage("en");
            LoginMenuWindow newWindow = new LoginMenuWindow();
            newWindow.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.LoginSucceeded -= OnLoginSucceeded;
            viewModel.InformationMessageRequested -= OnInformationMessageRequested;
            base.OnClosed(e);
        }
    }
}
