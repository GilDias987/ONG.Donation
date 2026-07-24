FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ONG.Donation.WebAPI/ONG.Donation.WebAPI.csproj", "ONG.Donation.WebAPI/"]
COPY ["ONG.Donation.Application/ONG.Donation.Application.csproj", "ONG.Donation.Application/"]
COPY ["ONG.Donation.Domain/ONG.Donation.Domain.csproj", "ONG.Donation.Domain/"]
COPY ["ONG.Donation.Infrastructure/ONG.Donation.Infrastructure.csproj", "ONG.Donation.Infrastructure/"]
RUN dotnet restore "./ONG.Donation.WebAPI/ONG.Donation.WebAPI.csproj"
COPY . .
WORKDIR "/src/ONG.Donation.WebAPI"
RUN dotnet build "./ONG.Donation.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ONG.Donation.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ONG.Donation.WebAPI.dll"]
