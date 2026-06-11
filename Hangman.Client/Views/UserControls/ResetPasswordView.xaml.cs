using Hangman.Client.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hangman.Client.Views.UserControls
{
    public partial class ResetPasswordView : UserControl
    {
        private ResetPasswordViewModel viewModel;

        public ResetPasswordView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            ResetPasswordViewModel oldViewModel = e.OldValue as ResetPasswordViewModel;

            if (oldViewModel != null)
            {
                oldViewModel.SensitiveDataClearRequested -= OnSensitiveDataClearRequested;
            }

            viewModel = e.NewValue as ResetPasswordViewModel;

            if (viewModel != null)
            {
                viewModel.SensitiveDataClearRequested += OnSensitiveDataClearRequested;
            }
        }

        private void RecoveryCodeTextBox_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyDigits(e.Text);
        }

        private void RecoveryCodeTextBox_Pasting(
            object sender,
            DataObjectPastingEventArgs e)
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
            if (viewModel == null)
            {
                return;
            }

            viewModel.SetNewPassword(NewPasswordBox.Password);
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.SetConfirmPassword(ConfirmPasswordBox.Password);
        }

        private void OnSensitiveDataClearRequested(object sender, EventArgs e)
        {
            RecoveryCodeTextBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.SensitiveDataClearRequested -= OnSensitiveDataClearRequested;
            }
        }

        private static bool IsOnlyDigits(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(char.IsDigit);
        }
    }
}
