using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Application.Services;

namespace ONG.Donation.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ICampaignService>();

        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<IDonationService, DonationService>();
        services.AddScoped<IDonorService, DonorService>();

        return services;
    }
}
