using Microsoft.Extensions.Configuration;
using ONG.Donation.Infrastructure.DependencyInjection;
using ONG.Donation.Worker.Consumers;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var lokiUrl = config["Loki:Url"] ?? "http://localhost:3100";

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "ONG.Donation.Worker")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(lokiUrl,
        new[]
        {
            new LokiLabel { Key = "app", Value = "ong-donation-worker" },
        })
    .CreateLogger();

var builder = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddInfrastructure(context.Configuration);
        services.AddHostedService<DonationConsumer>();
    });

var host = builder.Build();
host.Run();
