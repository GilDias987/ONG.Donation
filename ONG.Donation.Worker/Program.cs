using ONG.Donation.Infrastructure.DependencyInjection;
using ONG.Donation.Worker.Consumers;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "ONG.Donation.Worker")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki("http://localhost:3100",
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
