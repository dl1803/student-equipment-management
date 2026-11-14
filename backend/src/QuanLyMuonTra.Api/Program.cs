using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuanLyMuonTra.Api.Health;
using QuanLyMuonTra.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var chuoiKetNoi = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(chuoiKetNoi))
{
    throw new InvalidOperationException("Thiếu cấu hình ConnectionStrings:Default.");
}

IReadOnlyCollection<Assembly> assemblyCauHinhEf = [];

builder.Services.AddInfrastructure(chuoiKetNoi, assemblyCauHinhEf);
builder.Services.AddHealthChecks()
    .AddCheck<CsdlHealthCheck>("csdl", tags: ["ready"]);

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();

public partial class Program { }
