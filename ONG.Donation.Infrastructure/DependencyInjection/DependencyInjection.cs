using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Infrastructure.Authentication;
using ONG.Donation.Infrastructure.Persistence.Context;
using ONG.Donation.Infrastructure.Persistence.Repositories;
using ONG.Donation.Infrastructure.ServiceBus;
using ONG.Donation.Infrastructure.Services;

namespace ONG.Donation.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IDonorRepository, DonorRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthService, JwtService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

        return services;
    }
}
