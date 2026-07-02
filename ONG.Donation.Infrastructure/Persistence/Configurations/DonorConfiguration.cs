using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ONG.Donation.Infrastructure.Persistence.Configurations;

public class DonorConfiguration : IEntityTypeConfiguration<global::ONG.Donation.Domain.Entities.Donor>
{
    public void Configure(EntityTypeBuilder<global::ONG.Donation.Domain.Entities.Donor> builder)
    {
        builder.ToTable("Donors");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FullName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(d => d.Email).IsUnique();
        builder.Property(d => d.Cpf).IsRequired().HasMaxLength(14);
        builder.Property(d => d.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(d => d.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt);
    }
}
