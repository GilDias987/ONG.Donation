using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONG.Donation.Application.DTOs;
using ONG.Donation.Application.Interfaces;

namespace ONG.Donation.WebAPI.Endpoints;

public static class CampaignEndpoints
{
    public static IEndpointRouteBuilder MapCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/campaigns")
            .RequireAuthorization()
            .WithTags("Campaigns");

        group.MapGet("/", async (ICampaignService campaignService) =>
        {
            var campaigns = await campaignService.GetAllAsync();
            return Results.Ok(campaigns);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetAllCampaigns")
        .WithSummary("Get all campaigns");

        group.MapGet("/{id:int}", async (int id, ICampaignService campaignService) =>
        {
            var campaign = await campaignService.GetByIdAsync(id);
            return Results.Ok(campaign);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("GetCampaignById")
        .WithSummary("Get campaign by ID");

        group.MapPost("/", async (
            [FromBody] CreateCampaignRequest request,
            ICampaignService campaignService) =>
        {
            var campaign = await campaignService.CreateAsync(request);
            return Results.Created($"/campaigns/{campaign.Id}", campaign);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("CreateCampaign")
        .WithSummary("Create a new campaign");

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateCampaignRequest request,
            ICampaignService campaignService) =>
        {
            var campaign = await campaignService.UpdateAsync(id, request);
            return Results.Ok(campaign);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("UpdateCampaign")
        .WithSummary("Update an existing campaign");

        group.MapPatch("/{id:int}/status", async (
            int id,
            [FromBody] SetCampaignStatusRequest request,
            ICampaignService campaignService) =>
        {
            var campaign = await campaignService.SetStatusAsync(id, request);
            return Results.Ok(campaign);
        })
        .RequireAuthorization("AdminOnly")
        .WithName("SetCampaignStatus")
        .WithSummary("Activate or inactivate a campaign (Admin only)");

        return app;
    }
}
