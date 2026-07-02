namespace ONG.Donation.Application.DTOs;

public record UpdateDonorRequest(string FullName, string Email, string Cpf, string? Password);
