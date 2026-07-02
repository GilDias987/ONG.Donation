using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONG.Donation.Application.DTOs;
using ONG.Donation.Application.Interfaces;

namespace ONG.Donation.WebAPI.Endpoints;

public static class DonationEndpoints
{
    public static IEndpointRouteBuilder MapDonationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/donations")
            .RequireAuthorization()
            .WithTags("Donations");

        group.MapPost("/", async (
            [FromBody] CreateDonationRequest request,
            IDonationService donationService,
            ClaimsPrincipal user) =>
        {
            var donorId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var donation = await donationService.CreateAsync(donorId, request);
            return Results.Created($"/donations/{donation.Id}", donation);
        })
        .RequireAuthorization("DonorOnly")
        .WithName("CreateDonation")
        .WithSummary("Create a donation (Donor only)");

        group.MapGet("/campaign/{campaignId:int}", async (
            int campaignId,
            IDonationService donationService) =>
        {
            var donations = await donationService.GetByCampaignIdAsync(campaignId);
            return Results.Ok(donations);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetDonationsByCampaign")
        .WithSummary("Get donations by campaign (Admin only)");

        return app;
    }
}
