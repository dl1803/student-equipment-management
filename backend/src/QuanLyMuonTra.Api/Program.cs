using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.Api.Health;
using QuanLyMuonTra.Api.Loi;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.Modules.ViPham;
using QuanLyMuonTra.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

var chuoiKetNoi = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(chuoiKetNoi))
{
    throw new InvalidOperationException("Thiếu cấu hình ConnectionStrings:Default.");
}

var jwt = XacThucJwt.DocCauHinh(builder.Configuration);

IReadOnlyCollection<Assembly> assemblyCauHinhEf = [
    typeof(NguoiDungHeThong).Assembly,
    typeof(DanhMucModelBuilderExtensions).Assembly,
    typeof(QuyDinhModelBuilderExtensions).Assembly
];

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddInfrastructure(chuoiKetNoi, assemblyCauHinhEf);
builder.Services.AddTaiKhoanModule(jwt);
builder.Services.AddDanhMucModule();
builder.Services.AddQuyDinhModule();
builder.Services.AddXacThucJwt(jwt);
builder.Services.AddChinhSachPhanQuyen();
builder.Services.AddExceptionHandler<XuLyLoiToanCuc>();
builder.Services.AddViPhamModule();
builder.Services.AddHealthChecks()
    .AddCheck<CsdlHealthCheck>("csdl", tags: ["ready"]);
builder.Services.AddScoped<FluentValidation.IValidator<DangNhapNhanVienRequest>, DangNhapNhanVienValidator>();

var app = builder.Build();

var tenQuanTriKhoiTao = builder.Configuration["Seed:AdminUser"];
var matKhauQuanTriKhoiTao = builder.Configuration["Seed:AdminPassword"];
if (!string.IsNullOrWhiteSpace(tenQuanTriKhoiTao) || !string.IsNullOrEmpty(matKhauQuanTriKhoiTao))
{
    await using var phamViKhoiTao = app.Services.CreateAsyncScope();
    var boKhoiTao = phamViKhoiTao.ServiceProvider.GetRequiredService<QuanTriVienSeeder>();
    await boKhoiTao.ChayAsync(tenQuanTriKhoiTao, matKhauQuanTriKhoiTao, app.Lifetime.ApplicationStopping);
}

app.UseExceptionHandler(_ => { });

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapTaiKhoanEndpoints();

app.Run();

public partial class Program { }
