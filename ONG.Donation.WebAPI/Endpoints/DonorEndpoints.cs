using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONG.Donation.Application.DTOs;
using ONG.Donation.Application.Interfaces;

namespace ONG.Donation.WebAPI.Endpoints;

public static class DonorEndpoints
{
    public static IEndpointRouteBuilder MapDonorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/donors")
            .WithTags("Donors");

        group.MapPost("/register", async (
            [FromBody] RegisterDonorRequest request,
            IDonorService donorService) =>
        {
            var donorId = await donorService.RegisterAsync(request);
            return Results.Created($"/donors/{donorId}", new { Id = donorId });
        })
        .WithName("RegisterDonor")
        .WithSummary("Register a new donor");

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateDonorRequest request,
            IDonorService donorService,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = user.FindFirstValue(ClaimTypes.Role);

            if (role != "GestorONG" && userId != id)
                return Results.Forbid();

            var donor = await donorService.UpdateAsync(id, request);
            return Results.Ok(donor);
        })
        .RequireAuthorization()
        .WithName("UpdateDonor")
        .WithSummary("Update donor profile (own profile or Admin only)");

        group.MapDelete("/{id:int}", async (
            int id,
            IDonorService donorService,
            ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = user.FindFirstValue(ClaimTypes.Role);

            if (role != "GestorONG" && userId != id)
                return Results.Forbid();

            await donorService.DeleteAsync(id);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteDonor")
        .WithSummary("Delete donor account (own profile or Admin only)");

        return app;
    }
}
