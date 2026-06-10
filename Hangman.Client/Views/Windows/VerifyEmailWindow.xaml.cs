using Hangman.Client.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Hangman.Client.Views.Windows
{
    public partial class VerifyEmailWindow : Window
    {
        private readonly VerifyEmailViewModel viewModel;

        public VerifyEmailWindow()
            : this(string.Empty)
        {
        }

        public VerifyEmailWindow(string email)
        {
            InitializeComponent();

            viewModel = new VerifyEmailViewModel(email);
            DataContext = viewModel;

            viewModel.LoginRequested += OnLoginRequested;
            viewModel.VerificationSucceeded += OnVerificationSucceeded;
            viewModel.CodeClearRequested += OnCodeClearRequested;
        }

        private void VerificationCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyDigits(e.Text);
        }

        private void VerificationCodeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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

        private void OnLoginRequested(object sender, EventArgs e)
        {
            OpenLoginWindow();
        }

        private void OnVerificationSucceeded(object sender, EventArgs e)
        {
            MessageBox.Show(
                viewModel.SuccessMessage,
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            OpenLoginWindow();
        }

        private void OnCodeClearRequested(object sender, EventArgs e)
        {
            VerificationCodeTextBox.Clear();
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
            viewModel.VerificationSucceeded -= OnVerificationSucceeded;
            viewModel.CodeClearRequested -= OnCodeClearRequested;

            base.OnClosed(e);
        }

        private static bool IsOnlyDigits(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(char.IsDigit);
        }
    }
}
