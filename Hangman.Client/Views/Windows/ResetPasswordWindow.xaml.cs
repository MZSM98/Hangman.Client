using Hangman.Client.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Hangman.Client.Views.Windows
{
    public partial class ResetPasswordWindow : Window
    {
        private readonly ResetPasswordViewModel viewModel;

        public ResetPasswordWindow()
            : this(string.Empty)
        {
        }

        public ResetPasswordWindow(string email)
        {
            InitializeComponent();

            viewModel = new ResetPasswordViewModel(email);
            DataContext = viewModel;

            viewModel.LoginRequested += OnLoginRequested;
            viewModel.PasswordResetSucceeded += OnPasswordResetSucceeded;
            viewModel.SensitiveDataClearRequested += OnSensitiveDataClearRequested;
        }

        private void RecoveryCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyDigits(e.Text);
        }

        private void RecoveryCodeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = e.DataObject.GetData(DataFormats.Text) as string;

            if (!IsOnlyDigits(pastedText))
            {
                e.CancelCommand();
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.SetNewPassword(NewPasswordBox.Password);
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.SetConfirmPassword(ConfirmPasswordBox.Password);
        }

        private void OnLoginRequested(object sender, EventArgs e)
        {
            OpenLoginWindow();
        }

        private void OnPasswordResetSucceeded(object sender, EventArgs e)
        {
            MessageBox.Show(
                viewModel.SuccessMessage,
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            OpenLoginWindow();
        }

        private void OnSensitiveDataClearRequested(object sender, EventArgs e)
        {
            RecoveryCodeTextBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
        }

        private void OpenLoginWindow()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.LoginRequested -= OnLoginRequested;
            viewModel.PasswordResetSucceeded -= OnPasswordResetSucceeded;
            viewModel.SensitiveDataClearRequested -= OnSensitiveDataClearRequested;

            base.OnClosed(e);
        }

        private static bool IsOnlyDigits(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(char.IsDigit);
        }
    }
}
