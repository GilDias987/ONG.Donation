using Microsoft.AspNetCore.Mvc;
using ONG.Donation.Application.DTOs;
using ONG.Donation.Application.Interfaces;

namespace ONG.Donation.WebAPI.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            return Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Authenticate donor and return JWT token");

        return app;
    }
}
