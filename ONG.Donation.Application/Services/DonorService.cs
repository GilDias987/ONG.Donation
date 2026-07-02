using ONG.Donation.Application.DTOs;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Entities;
using ONG.Donation.Domain.Enums;
using ONG.Donation.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ONG.Donation.Application.Services;

public class DonorService : IDonorService
{
    private readonly IDonorRepository _donorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DonorService> _logger;

    public DonorService(
        IDonorRepository donorRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<DonorService> logger)
    {
        _donorRepository = donorRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<int> RegisterAsync(RegisterDonorRequest request)
    {
        _logger.LogInformation("Registering donor with email {Email}", request.Email);

        var existingDonor = await _donorRepository.GetByEmailAsync(request.Email);
        if (existingDonor is not null)
        {
            _logger.LogWarning("Registration failed: email {Email} already registered", request.Email);
            throw new DomainException("Email already registered.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var donor = new Donor(request.FullName, request.Email, request.Cpf, passwordHash, UserRole.Doador);
        await _donorRepository.AddAsync(donor);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Donor {DonorId} registered successfully with email {Email}", donor.Id, donor.Email);
        return donor.Id;
    }

    public async Task<DonorResponse> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching donor {DonorId}", id);

        var donor = await _donorRepository.GetByIdAsync(id);
        if (donor is null)
        {
            _logger.LogWarning("Donor {DonorId} not found", id);
            throw new DomainException("Donor not found.");
        }

        return MapToResponse(donor);
    }

    public async Task<DonorResponse> UpdateAsync(int id, UpdateDonorRequest request)
    {
        _logger.LogInformation("Updating donor {DonorId}", id);

        var donor = await _donorRepository.GetByIdAsync(id);
        if (donor is null)
        {
            _logger.LogWarning("Update failed: donor {DonorId} not found", id);
            throw new DomainException("Donor not found.");
        }

        var emailExists = await _donorRepository.ExistsByEmailAsync(request.Email, id);
        if (emailExists)
        {
            _logger.LogWarning("Update failed: email {Email} already in use by another donor", request.Email);
            throw new DomainException("Email already in use.");
        }

        donor.Update(request.FullName, request.Email, request.Cpf);

        if (!string.IsNullOrWhiteSpace(request.Password))
            donor.SetPasswordHash(_passwordHasher.Hash(request.Password));

        await _donorRepository.UpdateAsync(donor);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Donor {DonorId} updated successfully", id);
        return MapToResponse(donor);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting donor {DonorId}", id);

        var donor = await _donorRepository.GetByIdAsync(id);
        if (donor is null)
        {
            _logger.LogWarning("Delete failed: donor {DonorId} not found", id);
            throw new DomainException("Donor not found.");
        }

        await _donorRepository.DeleteAsync(donor);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Donor {DonorId} deleted successfully", id);
    }

    private static DonorResponse MapToResponse(Donor donor) =>
        new(donor.Id, donor.FullName, donor.Email, donor.Cpf, donor.Role.ToString(), donor.CreatedAt);
}
