namespace ONG.Donation.Application.DTOs;

public record DonorResponse(int Id, string FullName, string Email, string Cpf, string Role, DateTime CreatedAt);
