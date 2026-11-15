using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QuanLyMuonTra.Api.Auth;

namespace QuanLyMuonTra.IntegrationTests.Api;

public class HttpContextCurrentUserTests
{
    [Fact]
    public void NFR02_NhanVien_CoMaNguoiDungVaKhongCoMaSv()
    {
        var ma = Guid.CreateVersion7();
        var nguoiDung = Tao(new Claim("sub", ma.ToString()), new Claim("role", "NHAN_VIEN"));

        Assert.Equal(ma, nguoiDung.MaNguoiDung);
        Assert.Null(nguoiDung.MaSv);
    }

    [Fact]
    public void NFR02_QuanTri_CoMaNguoiDung()
    {
        var ma = Guid.CreateVersion7();
        var nguoiDung = Tao(new Claim("sub", ma.ToString()), new Claim("role", "QUAN_TRI"));

        Assert.Equal(ma, nguoiDung.MaNguoiDung);
        Assert.Null(nguoiDung.MaSv);
    }

    [Fact]
    public void NFR02_SinhVien_CoMaSvVaKhongCoMaNguoiDung()
    {
        var nguoiDung = Tao(new Claim("sub", "SV001"), new Claim("role", "SINH_VIEN"));

        Assert.Equal("SV001", nguoiDung.MaSv);
        Assert.Null(nguoiDung.MaNguoiDung);
    }

    [Fact]
    public void NFR02_ClaimTheoClaimTypes_VanNhanRaNguoiDung()
    {
        var ma = Guid.CreateVersion7();
        var nguoiDung = Tao(new Claim(ClaimTypes.NameIdentifier, ma.ToString()), new Claim(ClaimTypes.Role, "NHAN_VIEN"));

        Assert.Equal(ma, nguoiDung.MaNguoiDung);
    }

    [Fact]
    public void NFR02_KhongCoNguoiDung_LaHeThong()
    {
        var nguoiDung = new HttpContextCurrentUser(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        Assert.Null(nguoiDung.MaNguoiDung);
        Assert.Null(nguoiDung.MaSv);
    }

    [Fact]
    public void NFR02_VaiTroLaNhungMaKhongPhaiGuid_KhongCoMaNguoiDung()
    {
        var nguoiDung = Tao(new Claim("sub", "khong-phai-guid"), new Claim("role", "NHAN_VIEN"));

        Assert.Null(nguoiDung.MaNguoiDung);
        Assert.Null(nguoiDung.MaSv);
    }

    private static HttpContextCurrentUser Tao(params Claim[] cacClaim)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(cacClaim, "kiem-thu"))
        };

        return new HttpContextCurrentUser(new HttpContextAccessor { HttpContext = httpContext });
    }
}

