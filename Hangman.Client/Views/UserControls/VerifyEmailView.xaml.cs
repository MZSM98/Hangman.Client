using Hangman.Client.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hangman.Client.Views.UserControls
{
    public partial class VerifyEmailView : UserControl
    {
        private VerifyEmailViewModel viewModel;

        public VerifyEmailView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            VerifyEmailViewModel oldViewModel = e.OldValue as VerifyEmailViewModel;

            if (oldViewModel != null)
            {
                oldViewModel.CodeClearRequested -= OnCodeClearRequested;
            }

            viewModel = e.NewValue as VerifyEmailViewModel;

            if (viewModel != null)
            {
                viewModel.CodeClearRequested += OnCodeClearRequested;
            }
        }

        private void VerificationCodeTextBox_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyDigits(e.Text);
        }

        private void VerificationCodeTextBox_Pasting(
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

        private void OnCodeClearRequested(object sender, EventArgs e)
        {
            VerificationCodeTextBox.Clear();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.CodeClearRequested -= OnCodeClearRequested;
            }
        }

        private static bool IsOnlyDigits(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(char.IsDigit);
        }
    }
}
