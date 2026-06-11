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

        protected override void OnClosed(EventArgs e)
        {
            viewModel.SessionClosed -= OnSessionClosed;
            base.OnClosed(e);
        }
    }
}
