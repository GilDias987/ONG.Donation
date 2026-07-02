namespace ONG.Donation.Application.Commands;

public record RegisterDonorCommand(string FullName, string Email, string Cpf, string Password);
