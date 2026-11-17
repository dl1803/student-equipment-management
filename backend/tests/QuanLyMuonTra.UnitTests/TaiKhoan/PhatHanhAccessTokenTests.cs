using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.TaiKhoan;

public sealed class PhatHanhAccessTokenTests
{
    private const string Secret = "khoa-kiem-thu-khong-phai-bi-mat-0123456789";
    private static readonly JwtOptions TuyChon = new(Secret, "iss-kiem-thu", "aud-kiem-thu");

    [Fact]
    public void UC19_Tao_ClaimSubRoleJtiVaHanMuoiLamPhut()
    {
        var ma = Guid.CreateVersion7();
        var phatHanh = new PhatHanhAccessToken(TuyChon, new DongHoCoDinh(new DateTimeOffset(2026, 11, 16, 3, 0, 0, TimeSpan.Zero)));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(phatHanh.Tao(ma, "NHAN_VIEN"));

        Assert.Equal(ma.ToString(), token.Claims.Single(c => c.Type == "sub").Value);
        Assert.Equal("NHAN_VIEN", token.Claims.Single(c => c.Type == "role").Value);
        Assert.False(string.IsNullOrWhiteSpace(token.Id));
        Assert.Equal(TimeSpan.FromMinutes(15), token.ValidTo - token.ValidFrom);
        Assert.Equal("HS256", token.Header.Alg);
        Assert.Equal("iss-kiem-thu", token.Issuer);
        Assert.Contains("aud-kiem-thu", token.Audiences);
    }

    [Fact]
    public void UC19_Tao_ChoSinhVien_SubLaMssvVaRoleSinhVien()
    {
        var phatHanh = new PhatHanhAccessToken(TuyChon, new DongHoCoDinh(new DateTimeOffset(2026, 11, 16, 3, 0, 0, TimeSpan.Zero)));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(phatHanh.Tao("SV-001", "SINH_VIEN"));

        Assert.Equal("SV-001", token.Claims.Single(c => c.Type == "sub").Value);
        Assert.Equal("SINH_VIEN", token.Claims.Single(c => c.Type == "role").Value);
        Assert.Equal(TimeSpan.FromMinutes(15), token.ValidTo - token.ValidFrom);
    }

    [Fact]
    public void UC19_Tao_ChuKyHopLeVoiKhoaDungVaSaiVoiKhoaKhac()
    {
        var phatHanh = new PhatHanhAccessToken(TuyChon, new DongHoCoDinh(new DateTimeOffset(2026, 11, 16, 3, 0, 0, TimeSpan.Zero)));
        var chuoiToken = phatHanh.Tao(Guid.CreateVersion7(), "QUAN_TRI");
        var boKiemTra = new JwtSecurityTokenHandler { MapInboundClaims = false };

        var thamSoDung = TaoThamSo(Secret);
        var thamSoSai = TaoThamSo("mot-khoa-khac-hoan-toan-0123456789-abcdef");

        boKiemTra.ValidateToken(chuoiToken, thamSoDung, out _);
        Assert.ThrowsAny<SecurityTokenException>(() => boKiemTra.ValidateToken(chuoiToken, thamSoSai, out _));
    }

    private static TokenValidationParameters TaoThamSo(string khoa) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = "iss-kiem-thu",
        ValidateAudience = true,
        ValidAudience = "aud-kiem-thu",
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(khoa))
    };

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }
}

