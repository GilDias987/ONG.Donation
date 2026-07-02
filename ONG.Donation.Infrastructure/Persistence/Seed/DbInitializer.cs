using ONG.Donation.Domain.Entities;
using ONG.Donation.Domain.Enums;
using ONG.Donation.Infrastructure.Persistence.Context;

namespace ONG.Donation.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, string adminPasswordHash)
    {
        if (context.Donors.Any(d => d.Email == "admin@gmail.com"))
            return;

        var admin = new Donor("admin", "admin@gmail.com", "21181594391", adminPasswordHash, UserRole.GestorONG);
        context.Donors.Add(admin);
        await context.SaveChangesAsync();
    }
}
