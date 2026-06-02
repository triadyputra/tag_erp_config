using tagApiKonfigurasi.Model.DTO.Mobile;

namespace tagApiKonfigurasi.Services.Mobile
{
    public sealed class MobileLoginResult
    {
        public LoginResponseDto? Response { get; init; }
        public string? ErrorMessage { get; init; }
        public bool IsCredentialFailure { get; init; }

        public bool IsSuccess => Response != null;

        public static MobileLoginResult CredentialFailure() =>
            new() { IsCredentialFailure = true, ErrorMessage = "Username atau password salah" };

        public static MobileLoginResult EligibilityFailure(string message) =>
            new() { ErrorMessage = message };

        public static MobileLoginResult Success(LoginResponseDto response) =>
            new() { Response = response };
    }
}
