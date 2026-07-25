using ONG.Donation.Application.DTOs;

namespace ONG.Donation.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    string GenerateToken(int userId, string email, string role);
}
