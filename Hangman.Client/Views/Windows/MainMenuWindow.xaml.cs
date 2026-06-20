using Hangman.Client.Localization;
using Hangman.Client.ViewModels;
using System;
using System.Windows;

namespace Hangman.Client.Views.Windows
{
    public partial class MainMenuWindow : Window
    {
        private readonly MainMenuViewModel viewModel;

        public MainMenuWindow()
        {
            InitializeComponent();

            viewModel = new MainMenuViewModel();
            DataContext = viewModel;

            viewModel.SessionClosed += OnSessionClosed;
        }

        private void OnSessionClosed(object sender, EventArgs e)
        {
            LoginMenuWindow loginMenuWindow = new LoginMenuWindow();
            loginMenuWindow.Show();

            Close();
        }

        private void OnSetSpanish(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage("es");
            MainMenuWindow newWindow = new MainMenuWindow();
            newWindow.Show();
            Close();
        }

        private void OnSetEnglish(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage("en");
            MainMenuWindow newWindow = new MainMenuWindow();
            newWindow.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.SessionClosed -= OnSessionClosed;
            base.OnClosed(e);
        }
    }
}
