using Hangman.Client.ViewModels;
using System;
using System.Windows;

namespace Hangman.Client.Views.Windows
{
    public partial class RequestPasswordResetWindow : Window
    {
        private readonly RequestPasswordResetViewModel viewModel;

        public RequestPasswordResetWindow()
            : this(string.Empty)
        {
        }

        public RequestPasswordResetWindow(string email)
        {
            InitializeComponent();

            viewModel = new RequestPasswordResetViewModel(email);
            DataContext = viewModel;

            viewModel.LoginRequested += OnLoginRequested;
            viewModel.ResetCodeRequested += OnResetCodeRequested;
        }

        private void OnLoginRequested(object sender, EventArgs e)
        {
            OpenLoginWindow();
        }

        private void OnResetCodeRequested(object sender, ResetCodeRequestedEventArgs e)
        {
            ResetPasswordWindow resetPasswordWindow = new ResetPasswordWindow(e.Email);
            resetPasswordWindow.Show();

            Close();
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
            viewModel.ResetCodeRequested -= OnResetCodeRequested;

            base.OnClosed(e);
        }
    }
}
