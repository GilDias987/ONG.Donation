using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ONG.Donation.Infrastructure.Persistence.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<global::ONG.Donation.Domain.Entities.Donation>
{
    public void Configure(EntityTypeBuilder<global::ONG.Donation.Domain.Entities.Donation> builder)
    {
        builder.ToTable("Donations");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(d => d.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt);

        builder.HasOne(d => d.Campaign)
            .WithMany(c => c.Donations)
            .HasForeignKey(d => d.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Donor)
            .WithMany(dr => dr.Donations)
            .HasForeignKey(d => d.DonorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
