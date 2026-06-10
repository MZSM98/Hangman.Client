using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hangman.Client.ViewModels.Base
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool isBusy;
        private string errorMessage;
        private string successMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { SetProperty(ref errorMessage, value); }
        }

        public string SuccessMessage
        {
            get { return successMessage; }
            set { SetProperty(ref successMessage, value); }
        }

        protected bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(ErrorMessage); }
        }

        protected bool HasSuccess
        {
            get { return !string.IsNullOrWhiteSpace(SuccessMessage); }
        }

        protected void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        protected void SetError(string message)
        {
            SuccessMessage = string.Empty;
            ErrorMessage = message;
        }

        protected void SetSuccess(string message)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = message;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetProperty<T>(
            ref T backingField,
            T value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
