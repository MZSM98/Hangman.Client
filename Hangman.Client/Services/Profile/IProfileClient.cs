using Hangman.Contracts.Profile;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Profile
{
    public interface IProfileClient
    {
        Task<GetProfileResponse> GetProfileAsync(GetProfileRequest request);

        Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request);

        Task<DeleteProfileResponse> DeleteProfileAsync(DeleteProfileRequest request);
    }
}
