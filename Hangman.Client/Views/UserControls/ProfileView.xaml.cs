using Hangman.Client.Localization;
using Hangman.Client.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hangman.Client.Views.UserControls
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void PhoneTextBox_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled = !IsOnlyDigits(e.Text);
        }

        private void PhoneTextBox_Pasting(
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileViewModel viewModel = DataContext as ProfileViewModel;

            if (viewModel == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                LocExtension.Get("Profile_DeleteConfirmationMessage"),
                LocExtension.Get("Profile_DeleteConfirmationTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            ICommand command = viewModel.DeleteProfileCommand;

            if (command.CanExecute(null))
            {
                command.Execute(null);
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
