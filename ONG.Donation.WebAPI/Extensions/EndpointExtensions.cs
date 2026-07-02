using ONG.Donation.WebAPI.Endpoints;

namespace ONG.Donation.WebAPI.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapDonorEndpoints();
        app.MapCampaignEndpoints();
        app.MapDonationEndpoints();
        app.MapTransparencyEndpoints();
        return app;
    }
}
