using Hangman.Client.Localization;
using Hangman.Client.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Hangman.Client.Views.UserControls
{
    public partial class LoginView : UserControl
    {
        private LoginViewModel viewModel;
        private bool isSynchronizingPassword;

        public LoginView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            LoginViewModel oldViewModel = e.OldValue as LoginViewModel;

            if (oldViewModel != null)
            {
                oldViewModel.PasswordClearRequested -= OnPasswordClearRequested;
            }

            viewModel = e.NewValue as LoginViewModel;

            if (viewModel != null)
            {
                viewModel.PasswordClearRequested += OnPasswordClearRequested;
            }
        }

        private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isSynchronizingPassword || viewModel == null)
            {
                return;
            }

            viewModel.SetPassword(LoginPasswordBox.Password);

            if (ShowPasswordCheckBox.IsChecked ?? false)
            {
                isSynchronizingPassword = true;
                VisiblePasswordTextBox.Text = LoginPasswordBox.Password;
                isSynchronizingPassword = false;
            }
        }

        private void VisiblePasswordTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (isSynchronizingPassword || viewModel == null)
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

        private void OnPasswordClearRequested(object sender, EventArgs e)
        {
            isSynchronizingPassword = true;

            LoginPasswordBox.Clear();
            VisiblePasswordTextBox.Clear();

            isSynchronizingPassword = false;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.PasswordClearRequested -= OnPasswordClearRequested;
            }
        }
    }
}
