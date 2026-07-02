using ONG.Donation.Infrastructure.Persistence.Context;

namespace ONG.Donation.Infrastructure.Persistence.Repositories;

public class UnitOfWork : Application.Interfaces.IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
