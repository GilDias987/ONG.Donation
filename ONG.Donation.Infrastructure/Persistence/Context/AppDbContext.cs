using Microsoft.EntityFrameworkCore;
using DomainCampaign = ONG.Donation.Domain.Entities.Campaign;
using DomainDonation = ONG.Donation.Domain.Entities.Donation;
using DomainDonor = ONG.Donation.Domain.Entities.Donor;

namespace ONG.Donation.Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DomainCampaign> Campaigns { get; set; }
    public DbSet<DomainDonation> Donations { get; set; }
    public DbSet<DomainDonor> Donors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
