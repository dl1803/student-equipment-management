using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.BaoMat;

public sealed record GioiHanTanSuatAuth(int SoYeuCau, TimeSpan CuaSo);

public static class CauHinhBaoMat
{
    public const string TenChinhSachCors = "MienChoPhep";

    private static readonly HashSet<string> CacDuongDanGioiHanAuth = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/auth/nhan-vien/dang-nhap",
        "/api/v1/auth/sinh-vien/dang-nhap",
        "/api/v1/auth/lam-moi",
        "/api/v1/auth/dang-xuat"
    };

    public static string[] DocDsMienChoPhep(IConfiguration cauHinh)
    {
        var dsMien = (cauHinh["Cors:AllowedOrigins"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(mien => mien.TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (dsMien.Contains("*"))
        {
            throw new InvalidOperationException("Cors:AllowedOrigins không được chứa ký tự *.");
        }

        return dsMien;
    }

    public static GioiHanTanSuatAuth? DocGioiHanTanSuatAuth(IConfiguration cauHinh)
    {
        var soYeuCauChuoi = cauHinh["RateLimitAuth:SoYeuCau"];
        var cuaSoGiayChuoi = cauHinh["RateLimitAuth:CuaSoGiay"];

        if (string.IsNullOrWhiteSpace(soYeuCauChuoi) && string.IsNullOrWhiteSpace(cuaSoGiayChuoi))
        {
            return null;
        }

        if (!int.TryParse(soYeuCauChuoi, out var soYeuCau) || soYeuCau <= 0
            || !int.TryParse(cuaSoGiayChuoi, out var cuaSoGiay) || cuaSoGiay <= 0)
        {
            throw new InvalidOperationException(
                "RateLimitAuth:SoYeuCau và RateLimitAuth:CuaSoGiay phải cùng là số nguyên dương.");
        }

        return new GioiHanTanSuatAuth(soYeuCau, TimeSpan.FromSeconds(cuaSoGiay));
    }

    public static IServiceCollection AddCorsTheoMien(this IServiceCollection dichVu, string[] dsMien)
    {
        dichVu.AddCors(tuyChon => tuyChon.AddPolicy(TenChinhSachCors, chinhSach => chinhSach
            .WithOrigins(dsMien)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return dichVu;
    }

    public static IServiceCollection AddGioiHanTanSuatAuth(this IServiceCollection dichVu, GioiHanTanSuatAuth gioiHan)
    {
        dichVu.AddRateLimiter(tuyChon =>
        {
            tuyChon.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ngucCanh =>
            {
                if (!ngucCanh.Request.Path.HasValue || !CacDuongDanGioiHanAuth.Contains(ngucCanh.Request.Path.Value))
                {
                    return RateLimitPartition.GetNoLimiter("khac");
                }

                var khoa = ngucCanh.Connection.RemoteIpAddress?.ToString() ?? "khong-ro";

                return RateLimitPartition.GetFixedWindowLimiter(khoa, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = gioiHan.SoYeuCau,
                    Window = gioiHan.CuaSo,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
            tuyChon.OnRejected = (_, _) =>
                throw new LoiNghiepVuException("QUA_NHIEU_YEU_CAU", "Quá nhiều yêu cầu, vui lòng thử lại sau.");
        });

        return dichVu;
    }

    public static IApplicationBuilder UseTieuDeBaoMat(this IApplicationBuilder ungDung)
    {
        ungDung.Use(async (ngucCanh, tiepTheo) =>
        {
            ngucCanh.Response.OnStarting(() =>
            {
                ngucCanh.Response.Headers["X-Content-Type-Options"] = "nosniff";

                return Task.CompletedTask;
            });
            await tiepTheo();
        });

        return ungDung;
    }
}

