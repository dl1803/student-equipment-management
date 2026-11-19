using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.Api.BaoMat;
using QuanLyMuonTra.Api.Health;
using QuanLyMuonTra.Api.Loi;
using QuanLyMuonTra.Api.MuonTra;
using QuanLyMuonTra.Api.SinhVien;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.SinhVien;
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
var dsMienChoPhep = CauHinhBaoMat.DocDsMienChoPhep(builder.Configuration);
if (dsMienChoPhep.Length == 0 && builder.Environment.IsDevelopment())
{
    dsMienChoPhep = ["http://localhost:3000", "http://localhost:5173"];
}
var gioiHanTanSuatAuth = CauHinhBaoMat.DocGioiHanTanSuatAuth(builder.Configuration);

IReadOnlyCollection<Assembly> assemblyCauHinhEf = [
    typeof(NguoiDungHeThong).Assembly,
    typeof(DanhMucModelBuilderExtensions).Assembly,
    typeof(QuyDinhModelBuilderExtensions).Assembly,
    typeof(MuonTraModelBuilderExtensions).Assembly
];

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddInfrastructure(chuoiKetNoi, assemblyCauHinhEf);
builder.Services.AddTaiKhoanModule(jwt);
builder.Services.AddSinhVienModule();
builder.Services.AddDanhMucModule();
builder.Services.AddQuyDinhModule();
builder.Services.AddXacThucJwt(jwt);
builder.Services.AddChinhSachPhanQuyen();
builder.Services.AddExceptionHandler<XuLyLoiToanCuc>();
builder.Services.AddViPhamModule();
builder.Services.AddMuonTraModule();
builder.Services.AddScoped<LayHoSoSinhVienHandler>();
if (dsMienChoPhep.Length > 0)
{
    builder.Services.AddCorsTheoMien(dsMienChoPhep);
}

if (gioiHanTanSuatAuth is not null)
{
    builder.Services.AddGioiHanTanSuatAuth(gioiHanTanSuatAuth);
}

builder.Services.AddHealthChecks()
    .AddCheck<CsdlHealthCheck>("csdl", tags: ["ready"]);
builder.Services.AddScoped<FluentValidation.IValidator<DangNhapNhanVienRequest>, DangNhapNhanVienValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<DangNhapSinhVienRequest>, DangNhapSinhVienValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<LamMoiRequest>, LamMoiValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<DangXuatRequest>, DangXuatValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<NhanVienTaoMoiRequest>, NhanVienTaoMoiValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<NhanVienCapNhatRequest>, NhanVienCapNhatValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<DatLaiMatKhauRequest>, DatLaiMatKhauValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<QuyenPhongRequest>, QuyenPhongValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<GuiYeuCauMuonRequest>, GuiYeuCauMuonValidator>();

var app = builder.Build();

var tenQuanTriKhoiTao = builder.Configuration["Seed:AdminUser"];
var matKhauQuanTriKhoiTao = builder.Configuration["Seed:AdminPassword"];
if (!string.IsNullOrWhiteSpace(tenQuanTriKhoiTao) || !string.IsNullOrEmpty(matKhauQuanTriKhoiTao))
{
    await using var phamViKhoiTao = app.Services.CreateAsyncScope();
    var boKhoiTao = phamViKhoiTao.ServiceProvider.GetRequiredService<QuanTriVienSeeder>();
    await boKhoiTao.ChayAsync(tenQuanTriKhoiTao, matKhauQuanTriKhoiTao, app.Lifetime.ApplicationStopping);
}

app.UseTieuDeBaoMat();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler(_ => { });

if (dsMienChoPhep.Length > 0)
{
    app.UseCors(CauHinhBaoMat.TenChinhSachCors);
}

if (gioiHanTanSuatAuth is not null)
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapTaiKhoanEndpoints();
app.MapMuonTraEndpoints();
app.MapSinhVienEndpoints();

app.Run();

public partial class Program { }
