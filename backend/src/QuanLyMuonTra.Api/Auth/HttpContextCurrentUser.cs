using System.Security.Claims;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Api.Auth;

public sealed class HttpContextCurrentUser(IHttpContextAccessor boTruyCap) : ICurrentUser
{
    private ClaimsPrincipal? NguoiDung => boTruyCap.HttpContext?.User;

    private string? VaiTro => NguoiDung?.FindFirst("role")?.Value ?? NguoiDung?.FindFirst(ClaimTypes.Role)?.Value;

    private string? DinhDanh => NguoiDung?.FindFirst("sub")?.Value ?? NguoiDung?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public Guid? MaNguoiDung =>
        (VaiTro is "NHAN_VIEN" or "QUAN_TRI") && Guid.TryParse(DinhDanh, out var ma) ? ma : null;

    public string? MaSv => VaiTro == "SINH_VIEN" ? DinhDanh : null;
}

