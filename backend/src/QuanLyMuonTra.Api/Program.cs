using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.Api.Health;
using QuanLyMuonTra.Api.Loi;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Modules.ViPham;
using QuanLyMuonTra.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

var chuoiKetNoi = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(chuoiKetNoi))
{
    throw new InvalidOperationException("Thiếu cấu hình ConnectionStrings:Default.");
}

IReadOnlyCollection<Assembly> assemblyCauHinhEf = [];

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddInfrastructure(chuoiKetNoi, assemblyCauHinhEf);
builder.Services.AddExceptionHandler<XuLyLoiToanCuc>();
builder.Services.AddViPhamModule();
builder.Services.AddHealthChecks()
    .AddCheck<CsdlHealthCheck>("csdl", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler(_ => { });

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();

public partial class Program { }
