using Hangman.Contracts.Profile;
using System;

namespace Hangman.Client.Models.Profile
{
    public class ProfileFormModel
    {
        public int AccountId { get; set; }

        public int PlayerId { get; set; }

        public string FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string PreferredLanguageCode { get; set; }

        public ProfileFormModel()
        {
            FullName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            PreferredLanguageCode = "es";
        }

        public void LoadFrom(ProfileDto profile)
        {
            if (profile == null)
            {
                return;
            }

            AccountId = profile.AccountId;
            PlayerId = profile.PlayerId;
            FullName = profile.FullName ?? string.Empty;
            DateOfBirth = profile.DateOfBirth;
            Phone = profile.Phone ?? string.Empty;
            Email = profile.Email ?? string.Empty;
            PreferredLanguageCode = profile.PreferredLanguageCode ?? "es";
        }

        public void CopyFrom(ProfileFormModel source)
        {
            if (source == null)
            {
                return;
            }

            AccountId = source.AccountId;
            PlayerId = source.PlayerId;
            FullName = source.FullName ?? string.Empty;
            DateOfBirth = source.DateOfBirth;
            Phone = source.Phone ?? string.Empty;
            Email = source.Email ?? string.Empty;
            PreferredLanguageCode = source.PreferredLanguageCode ?? "es";
        }

        public ProfileFormModel Clone()
        {
            return new ProfileFormModel
            {
                AccountId = AccountId,
                PlayerId = PlayerId,
                FullName = FullName,
                DateOfBirth = DateOfBirth,
                Phone = Phone,
                Email = Email,
                PreferredLanguageCode = PreferredLanguageCode
            };
        }
    }
}
