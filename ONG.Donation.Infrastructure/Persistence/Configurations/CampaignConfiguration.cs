using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ONG.Donation.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<global::ONG.Donation.Domain.Entities.Campaign>
{
    public void Configure(EntityTypeBuilder<global::ONG.Donation.Domain.Entities.Campaign> builder)
    {
        builder.ToTable("Campaigns");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.StartDate).IsRequired();
        builder.Property(c => c.EndDate).IsRequired();
        builder.Property(c => c.FinancialGoal).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
    }
}
