using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Auth;
using Hangman.Client.Models.Profile;
using Hangman.Client.Services.Profile;
using Hangman.Client.Validators;
using Hangman.Client.Validators.Profile;
using Hangman.Client.ViewModels.Base;
using Hangman.Contracts.Profile;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public class ProfileViewModel : ServiceViewModelBase
    {
        private readonly IProfileClient profileClient;

        private readonly RelayCommand loadProfileCommand;
        private readonly RelayCommand editProfileCommand;
        private readonly RelayCommand cancelEditCommand;
        private readonly RelayCommand updateProfileCommand;
        private readonly RelayCommand deleteProfileCommand;
        private readonly RelayCommand backCommand;

        private readonly ProfileFormModel form;
        private ProfileFormModel originalForm;
        private bool isEditing;

        public ProfileViewModel()
            : this(
                  new ProfileClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<ProfileViewModel>())
        {
        }

        public ProfileViewModel(
            IProfileClient profileClient,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : base(validationMessageProvider, serverMessageProvider, logger)
        {
            this.profileClient = profileClient ??
                throw new ArgumentNullException(nameof(profileClient));

            form = new ProfileFormModel();
            originalForm = form.Clone();

            loadProfileCommand = new RelayCommand(
                async () => await LoadProfileAsync(),
                CanExecuteWhenNotBusy);

            editProfileCommand = new RelayCommand(
                StartEdit,
                CanExecuteEdit);

            cancelEditCommand = new RelayCommand(
                CancelEdit,
                CanExecuteCancelEdit);

            updateProfileCommand = new RelayCommand(
                async () => await UpdateProfileAsync(),
                CanExecuteUpdate);

            deleteProfileCommand = new RelayCommand(
                async () => await DeleteProfileAsync(),
                CanExecuteDelete);

            backCommand = new RelayCommand(
                RequestBack,
                CanExecuteBack);
        }

        public event EventHandler BackRequested;

        public event EventHandler ProfileDeleted;

        public bool IsEditing
        {
            get { return isEditing; }
            private set
            {
                if (SetProperty(ref isEditing, value))
                {
                    OnPropertyChanged(nameof(IsReadOnlyMode));
                    RaiseCommandsCanExecuteChanged();
                }
            }
        }

        public bool IsReadOnlyMode
        {
            get { return !IsEditing; }
        }

        public string FullName
        {
            get { return form.FullName; }
            set
            {
                if (form.FullName == value)
                {
                    return;
                }

                form.FullName = value;
                OnPropertyChanged();
                updateProfileCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? DateOfBirth
        {
            get { return form.DateOfBirth; }
            set
            {
                if (form.DateOfBirth == value)
                {
                    return;
                }

                form.DateOfBirth = value;
                OnPropertyChanged();
                updateProfileCommand.RaiseCanExecuteChanged();
            }
        }

        public string Phone
        {
            get { return form.Phone; }
            set
            {
                if (form.Phone == value)
                {
                    return;
                }

                form.Phone = value;
                OnPropertyChanged();
                updateProfileCommand.RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get { return form.Email; }
            private set
            {
                if (form.Email == value)
                {
                    return;
                }

                form.Email = value;
                OnPropertyChanged();
            }
        }

        public string PreferredLanguageCode
        {
            get { return form.PreferredLanguageCode; }
            set
            {
                string normalizedValue = string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : value.Trim().ToLowerInvariant();

                if (form.PreferredLanguageCode == normalizedValue)
                {
                    return;
                }

                form.PreferredLanguageCode = normalizedValue;
                OnPropertyChanged();
                updateProfileCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadProfileCommand
        {
            get { return loadProfileCommand; }
        }

        public ICommand EditProfileCommand
        {
            get { return editProfileCommand; }
        }

        public ICommand CancelEditCommand
        {
            get { return cancelEditCommand; }
        }

        public ICommand UpdateProfileCommand
        {
            get { return updateProfileCommand; }
        }

        public ICommand DeleteProfileCommand
        {
            get { return deleteProfileCommand; }
        }

        public ICommand BackCommand
        {
            get { return backCommand; }
        }

        private async Task LoadProfileAsync()
        {
            ClearMessages();

            if (!EnsureAuthenticatedUser())
            {
                return;
            }

            await ExecuteProfileOperationAsync(
                "GetProfileAsync",
                LoadProfileCoreAsync);
        }

        private async Task LoadProfileCoreAsync()
        {
            GetProfileRequest request = new GetProfileRequest
            {
                AccountId = form.AccountId
            };

            GetProfileResponse response = await profileClient.GetProfileAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("GetProfileAsync returned a null response.");
                return;
            }

            string translatedMessage = GetProfileServerMessage(response.MessageCode);

            if (!response.Success)
            {
                SetError(translatedMessage);
                return;
            }

            if (response.Profile == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("GetProfileAsync succeeded but profile data was null.");
                return;
            }

            LoadProfile(response.Profile);
            IsEditing = false;
            SetSuccess(translatedMessage);
        }

        private void StartEdit()
        {
            if (IsBusy || IsEditing)
            {
                return;
            }

            ClearMessages();
            originalForm = form.Clone();
            IsEditing = true;
        }

        private void CancelEdit()
        {
            if (IsBusy || !IsEditing)
            {
                return;
            }

            ClearMessages();
            form.CopyFrom(originalForm);
            NotifyProfilePropertiesChanged();
            IsEditing = false;
        }

        private async Task UpdateProfileAsync()
        {
            ClearMessages();

            if (!EnsureAuthenticatedUser())
            {
                return;
            }

            ClientValidationResult validationResult =
                ProfileFormValidator.Validate(form);

            if (!validationResult.IsValid)
            {
                SetValidationError(validationResult);
                return;
            }

            await ExecuteProfileOperationAsync(
                "UpdateProfileAsync",
                UpdateProfileCoreAsync);
        }

        private async Task UpdateProfileCoreAsync()
        {
            UpdateProfileRequest request = new UpdateProfileRequest
            {
                AccountId = form.AccountId,
                FullName = form.FullName.Trim(),
                DateOfBirth = form.DateOfBirth.Value,
                Phone = form.Phone.Trim(),
                PreferredLanguageCode =
                    form.PreferredLanguageCode.Trim().ToLowerInvariant()
            };

            UpdateProfileResponse response =
                await profileClient.UpdateProfileAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("UpdateProfileAsync returned a null response.");
                return;
            }

            string translatedMessage = GetProfileServerMessage(response.MessageCode);

            if (!response.Success)
            {
                SetError(translatedMessage);
                return;
            }

            if (response.Profile != null)
            {
                LoadProfile(response.Profile);
                UpdateUserSession(response.Profile);
            }
            else
            {
                originalForm = form.Clone();
                NotifyProfilePropertiesChanged();
            }

            IsEditing = false;
            SetSuccess(translatedMessage);
        }

        private async Task DeleteProfileAsync()
        {
            ClearMessages();

            if (!EnsureAuthenticatedUser())
            {
                return;
            }

            await ExecuteProfileOperationAsync(
                "DeleteProfileAsync",
                DeleteProfileCoreAsync);
        }

        private async Task DeleteProfileCoreAsync()
        {
            DeleteProfileRequest request = new DeleteProfileRequest
            {
                AccountId = form.AccountId
            };

            DeleteProfileResponse response =
                await profileClient.DeleteProfileAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("DeleteProfileAsync returned a null response.");
                return;
            }

            string translatedMessage = GetProfileServerMessage(response.MessageCode);

            if (!response.Success)
            {
                SetError(translatedMessage);
                return;
            }

            SetSuccess(translatedMessage);
            UserSession.Clear();
            RaiseProfileDeleted();
        }

        private bool EnsureAuthenticatedUser()
        {
            if (!UserSession.IsAuthenticated || UserSession.CurrentUser == null)
            {
                SetError(GetValidationMessage(ClientValidationCode.AccountRequired));
                return false;
            }

            form.AccountId = UserSession.CurrentUser.AccountId;
            form.PlayerId = UserSession.CurrentUser.PlayerId;

            return true;
        }

        private void LoadProfile(ProfileDto profile)
        {
            form.LoadFrom(profile);
            originalForm = form.Clone();
            NotifyProfilePropertiesChanged();
        }

        private void NotifyProfilePropertiesChanged()
        {
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(DateOfBirth));
            OnPropertyChanged(nameof(Phone));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(PreferredLanguageCode));

            RaiseCommandsCanExecuteChanged();
        }

        private static void UpdateUserSession(ProfileDto profile)
        {
            if (profile == null || !UserSession.IsAuthenticated)
            {
                return;
            }

            UserSession.Start(new AuthenticatedUserModel
            {
                AccountId = profile.AccountId,
                PlayerId = profile.PlayerId,
                FullName = profile.FullName,
                Email = profile.Email,
                PreferredLanguageCode = profile.PreferredLanguageCode
            });
        }

        private Task ExecuteProfileOperationAsync(
            string operationName,
            Func<Task> operation)
        {
            return ExecuteServiceOperationAsync(
                operationName,
                "profile service",
                operation,
                null,
                loadProfileCommand,
                editProfileCommand,
                cancelEditCommand,
                updateProfileCommand,
                deleteProfileCommand,
                backCommand);
        }

        private bool CanExecuteEdit()
        {
            return !IsBusy && !IsEditing;
        }

        private bool CanExecuteCancelEdit()
        {
            return !IsBusy && IsEditing;
        }

        private bool CanExecuteUpdate()
        {
            return !IsBusy && IsEditing;
        }

        private bool CanExecuteDelete()
        {
            return !IsBusy;
        }

        private bool CanExecuteBack()
        {
            return !IsBusy && !IsEditing;
        }

        private void RequestBack()
        {
            if (IsBusy || IsEditing)
            {
                return;
            }

            RaiseBackRequested();
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            loadProfileCommand.RaiseCanExecuteChanged();
            editProfileCommand.RaiseCanExecuteChanged();
            cancelEditCommand.RaiseCanExecuteChanged();
            updateProfileCommand.RaiseCanExecuteChanged();
            deleteProfileCommand.RaiseCanExecuteChanged();
            backCommand.RaiseCanExecuteChanged();
        }

        private string GetProfileServerMessage(string messageCode)
        {
            return GetServerMessage(ServerMessageModuleName.Profile, messageCode);
        }

        private void RaiseBackRequested()
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseProfileDeleted()
        {
            ProfileDeleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
