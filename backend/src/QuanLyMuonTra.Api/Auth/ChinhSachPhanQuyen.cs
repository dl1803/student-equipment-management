using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.Auth;

public static class ChinhSach
{
    public const string SinhVien = "SinhVien";
    public const string NhanVien = "NhanVien";
    public const string QuanTri = "QuanTri";
    public const string NhanVienQuanTri = "NhanVienQuanTri";
}

public sealed class VaiTroHoatDongRequirement(params string[] vaiTroChoPhep) : IAuthorizationRequirement
{
    public IReadOnlyCollection<string> VaiTroChoPhep { get; } = vaiTroChoPhep;
}

public sealed class VaiTroHoatDongHandler(ITrangThaiTaiKhoanReader boDoc) : AuthorizationHandler<VaiTroHoatDongRequirement>
{
    public const string LyDoTaiKhoanBiKhoa = "TAI_KHOAN_BI_KHOA";

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VaiTroHoatDongRequirement yeuCau)
    {
        var vaiTro = context.User.FindFirst("role")?.Value;
        if (vaiTro is null || !yeuCau.VaiTroChoPhep.Contains(vaiTro))
        {
            return;
        }

        if (!Guid.TryParse(context.User.FindFirst("sub")?.Value, out var maNguoiDung))
        {
            return;
        }

        var trangThai = await boDoc.LayTrangThaiKhoaAsync(maNguoiDung, CancellationToken.None);

        if (trangThai == MaTrangThaiKhoa.HoatDong)
        {
            context.Succeed(yeuCau);
            return;
        }

        if (trangThai is not null)
        {
            context.Fail(new AuthorizationFailureReason(this, LyDoTaiKhoanBiKhoa));
        }
    }
}

public sealed class KetQuaPhanQuyenHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler macDinh = new();

    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            throw new LoiNghiepVuException("CHUA_XAC_THUC", "Thiếu hoặc không hợp lệ access token.");
        }

        if (authorizeResult.Forbidden)
        {
            var biKhoa = authorizeResult.AuthorizationFailure?.FailureReasons
                .Any(lyDo => lyDo.Message == VaiTroHoatDongHandler.LyDoTaiKhoanBiKhoa) == true;

            throw biKhoa
                ? new LoiNghiepVuException("TAI_KHOAN_BI_KHOA", "Tài khoản đã bị khóa hoặc tạm khóa đăng nhập.")
                : new LoiNghiepVuException("KHONG_DU_QUYEN", "Không đủ quyền thực hiện thao tác này.");
        }

        return macDinh.HandleAsync(next, context, policy, authorizeResult);
    }
}

public static class PhanQuyenExtensions
{
    public static IServiceCollection AddChinhSachPhanQuyen(this IServiceCollection dichVu)
    {
        dichVu.AddAuthorization(tuyChon =>
        {
            tuyChon.AddPolicy(ChinhSach.SinhVien, chinhSach => chinhSach
                .RequireAuthenticatedUser()
                .RequireClaim("role", MaVaiTro.SinhVien));
            tuyChon.AddPolicy(ChinhSach.NhanVien, chinhSach => chinhSach
                .RequireAuthenticatedUser()
                .AddRequirements(new VaiTroHoatDongRequirement(MaVaiTro.NhanVien)));
            tuyChon.AddPolicy(ChinhSach.QuanTri, chinhSach => chinhSach
                .RequireAuthenticatedUser()
                .AddRequirements(new VaiTroHoatDongRequirement(MaVaiTro.QuanTri)));
            tuyChon.AddPolicy(ChinhSach.NhanVienQuanTri, chinhSach => chinhSach
                .RequireAuthenticatedUser()
                .AddRequirements(new VaiTroHoatDongRequirement(MaVaiTro.NhanVien, MaVaiTro.QuanTri)));
        });

        dichVu.AddScoped<IAuthorizationHandler, VaiTroHoatDongHandler>();
        dichVu.AddSingleton<IAuthorizationMiddlewareResultHandler, KetQuaPhanQuyenHandler>();

        return dichVu;
    }
}

