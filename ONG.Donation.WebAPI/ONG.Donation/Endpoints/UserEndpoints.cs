namespace Owong.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", () =>
        {
            return Results.Ok(new[]
            {
                new { Id = 1, Name = "Carlos" }
            });
        })
        .WithName("Users");

        return app;
    }
}