using ONG.Donation.Application.Interfaces;

namespace ONG.Donation.WebAPI.Endpoints;

public static class TransparencyEndpoints
{
    public static IEndpointRouteBuilder MapTransparencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transparency")
            .RequireAuthorization()
            .WithTags("Transparency");

        group.MapGet("/transparency/campaigns", async (ICampaignService campaignService) =>
        {
            var campaigns = await campaignService.GetActiveAsync();
            return Results.Ok(campaigns);
        })
        .AllowAnonymous()
        .WithName("GetActiveCampaigns")
        .WithSummary("Get active campaigns (public)");

        return app;
    }
}
