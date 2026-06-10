using Hangman.Client.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;

namespace Hangman.Client.Views.Windows
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel viewModel;

        public RegisterWindow()
        {
            InitializeComponent();

            viewModel = new RegisterViewModel();
            DataContext = viewModel;

            viewModel.LoginRequested += OnLoginRequested;
            viewModel.PasswordClearRequested += OnPasswordClearRequested;
            viewModel.VerificationRequired += OnVerificationRequired;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.SetPassword(PasswordBox.Password);
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.SetConfirmPassword(ConfirmPasswordBox.Password);
        }

        private void OnLoginRequested(object sender, EventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            Close();
        }

        private void OnPasswordClearRequested(object sender, EventArgs e)
        {
            PasswordBox.Clear();
            ConfirmPasswordBox.Clear();
        }

        private void OnVerificationRequired(object sender, VerificationRequiredEventArgs e)
        {
            VerifyEmailWindow verifyEmailWindow = new VerifyEmailWindow(e.Email);
            verifyEmailWindow.Show();

            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.LoginRequested -= OnLoginRequested;
            viewModel.PasswordClearRequested -= OnPasswordClearRequested;
            viewModel.VerificationRequired -= OnVerificationRequired;

            base.OnClosed(e);
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyDigits(e.Text);
        }

        private void PhoneTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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

        private void OpenDateCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            DateOfBirthPopup.IsOpen = true;
        }

        private void DateOfBirthCalendar_SelectedDatesChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            DateOfBirthPopup.IsOpen = false;
        }

        private static bool IsOnlyDigits(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(char.IsDigit);
        }
    }
}
