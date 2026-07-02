using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ONG.Donation.Application.Interfaces;

namespace ONG.Donation.Infrastructure.Authentication;

public class JwtService : IAuthService
{
    private readonly IDonorRepository _donorRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(
        IDonorRepository donorRepository,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<JwtService> logger)
    {
        _donorRepository = donorRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Application.DTOs.LoginResponse> LoginAsync(Application.DTOs.LoginRequest request)
    {
        _logger.LogInformation("Login attempt with email {Email}", request.Email);

        var donor = await _donorRepository.GetByEmailAsync(request.Email);
        if (donor is null || !_passwordHasher.Verify(request.Password, donor.PasswordHash))
        {
            _logger.LogWarning("Login failed for email {Email}: invalid credentials", request.Email);
            throw new Domain.Exceptions.DomainException("Invalid credentials.");
        }

        var token = GenerateToken(donor.Id, donor.Email, donor.Role.ToString());
        _logger.LogInformation("Login successful for donor {DonorId} with role {Role}", donor.Id, donor.Role);
        return new Application.DTOs.LoginResponse(token, donor.Role.ToString());
    }

    public string GenerateToken(int userId, string email, string role)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryInHours = int.Parse(_configuration["Jwt:ExpiryInHours"] ?? "8");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryInHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
