namespace ONG.Donation.Application.DTOs;

public record RegisterDonorRequest(string FullName, string Email, string Cpf, string Password);
