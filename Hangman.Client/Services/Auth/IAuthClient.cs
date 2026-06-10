using Hangman.Contracts.Auth;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Auth
{
    public interface IAuthClient
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);

        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        Task<VerifyEmailResponse> VerifyEmailAsync(VerifyEmailRequest request);

        Task<ResendVerificationEmailResponse> ResendVerificationEmailAsync(
            ResendVerificationEmailRequest request);

        Task<RequestPasswordResetResponse> RequestPasswordResetAsync(
            RequestPasswordResetRequest request);

        Task<ResetPasswordResponse> ResetPasswordAsync(
            ResetPasswordRequest request);
    }
}
