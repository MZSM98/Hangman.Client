using Hangman.Client.Localization;
using Hangman.Client.ViewModels;
using System;
using System.Windows;

namespace Hangman.Client.Views.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel viewModel;
        private bool isSynchronizingPassword;

        public LoginWindow()
        {
            InitializeComponent();

            viewModel = new LoginViewModel();
            DataContext = viewModel;

            viewModel.LoginSucceeded += OnLoginSucceeded;
            viewModel.RegisterRequested += OnRegisterRequested;
            viewModel.PasswordClearRequested += OnPasswordClearRequested;
        }

        private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isSynchronizingPassword)
            {
                return;
            }

            viewModel.SetPassword(LoginPasswordBox.Password);

            if (ShowPasswordCheckBox.IsChecked == true)
            {
                isSynchronizingPassword = true;
                VisiblePasswordTextBox.Text = LoginPasswordBox.Password;
                isSynchronizingPassword = false;
            }
        }

        private void VisiblePasswordTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isSynchronizingPassword)
            {
                return;
            }

            isSynchronizingPassword = true;
            LoginPasswordBox.Password = VisiblePasswordTextBox.Text;
            viewModel.SetPassword(VisiblePasswordTextBox.Text);
            isSynchronizingPassword = false;
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isSynchronizingPassword = true;

            VisiblePasswordTextBox.Text = LoginPasswordBox.Password;
            VisiblePasswordTextBox.Visibility = Visibility.Visible;
            LoginPasswordBox.Visibility = Visibility.Collapsed;

            isSynchronizingPassword = false;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isSynchronizingPassword = true;

            LoginPasswordBox.Password = VisiblePasswordTextBox.Text;
            LoginPasswordBox.Visibility = Visibility.Visible;
            VisiblePasswordTextBox.Visibility = Visibility.Collapsed;

            isSynchronizingPassword = false;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                LocExtension.Get("Login_SettingsUnavailableMessage"),
                LocExtension.Get("Login_SettingsUnavailableTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnLoginSucceeded(object sender, EventArgs e)
        {
            MainMenuWindow mainMenuWindow = new MainMenuWindow();
            mainMenuWindow.Show();

            Close();
        }

        private void OnRegisterRequested(object sender, EventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();

            Close();
        }

        private void OnPasswordClearRequested(object sender, EventArgs e)
        {
            isSynchronizingPassword = true;

            LoginPasswordBox.Clear();
            VisiblePasswordTextBox.Clear();

            isSynchronizingPassword = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.LoginSucceeded -= OnLoginSucceeded;
            viewModel.RegisterRequested -= OnRegisterRequested;
            viewModel.PasswordClearRequested -= OnPasswordClearRequested;

            base.OnClosed(e);
        }
    }
}
