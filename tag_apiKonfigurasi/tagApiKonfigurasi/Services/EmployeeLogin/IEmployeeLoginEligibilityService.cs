namespace tagApiKonfigurasi.Services.EmployeeLogin
{
    public interface IEmployeeLoginEligibilityService
    {
        Task<EmployeeLoginEligibilityResult> ValidateAsync(
            string? noKtp,
            LoginEligibilityMode mode,
            CancellationToken cancellationToken = default);
    }
}
