using ONG.Donation.Domain.Common;
using ONG.Donation.Domain.Enums;

namespace ONG.Donation.Domain.Entities;

public class Donor : BaseEntity
{
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string Cpf { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public ICollection<Donation> Donations { get; private set; } = [];

    private Donor() { }

    public Donor(string fullName, string email, string cpf, string passwordHash, UserRole role)
    {
        FullName = fullName;
        Email = email;
        Cpf = cpf;
        PasswordHash = passwordHash;
        Role = role;
        Validate();
    }

    public void Update(string fullName, string email, string cpf)
    {
        FullName = fullName;
        Email = email;
        Cpf = cpf;
        Validate();
        SetUpdatedAt();
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(FullName))
            throw new Exceptions.DomainException("Full name is required.");

        if (string.IsNullOrWhiteSpace(Email))
            throw new Exceptions.DomainException("Email is required.");

        if (string.IsNullOrWhiteSpace(Cpf))
            throw new Exceptions.DomainException("CPF is required.");

        if (!IsCpfValid(Cpf))
            throw new Exceptions.DomainException("Invalid CPF format.");
    }

    private static bool IsCpfValid(string cpf)
    {
        var digits = cpf.Where(char.IsDigit).ToArray();
        if (digits.Length != 11)
            return false;

        if (digits.Distinct().Count() == 1)
            return false;

        var numbers = digits.Select(d => int.Parse(d.ToString())).ToArray();

        int sum1 = 0;
        for (int i = 0; i < 9; i++)
            sum1 += numbers[i] * (10 - i);
        int rest1 = (sum1 * 10) % 11;
        if (rest1 == 10) rest1 = 0;
        if (rest1 != numbers[9]) return false;

        int sum2 = 0;
        for (int i = 0; i < 10; i++)
            sum2 += numbers[i] * (11 - i);
        int rest2 = (sum2 * 10) % 11;
        if (rest2 == 10) rest2 = 0;
        if (rest2 != numbers[10]) return false;

        return true;
    }
}
