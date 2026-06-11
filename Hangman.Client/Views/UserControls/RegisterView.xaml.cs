using Hangman.Client.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hangman.Client.Views.UserControls
{
    public partial class RegisterView : UserControl
    {
        private RegisterViewModel viewModel;

        public RegisterView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            RegisterViewModel oldViewModel = e.OldValue as RegisterViewModel;

            if (oldViewModel != null)
            {
                oldViewModel.PasswordClearRequested -= OnPasswordClearRequested;
            }

            viewModel = e.NewValue as RegisterViewModel;

            if (viewModel != null)
            {
                viewModel.PasswordClearRequested += OnPasswordClearRequested;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.SetPassword(PasswordBox.Password);
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.SetConfirmPassword(ConfirmPasswordBox.Password);
        }

        private void OnPasswordClearRequested(object sender, EventArgs e)
        {
            PasswordBox.Clear();
            ConfirmPasswordBox.Clear();
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

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.PasswordClearRequested -= OnPasswordClearRequested;
            }
        }

        private static bool IsOnlyDigits(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(char.IsDigit);
        }
    }
}
