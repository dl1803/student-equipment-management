using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QuanLyMuonTra.Modules.TaiKhoan.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

public sealed class PhatHanhAccessToken(JwtOptions tuyChon, IClock dongHo) : IPhatHanhAccessToken
{
    public const int ThoiHanPhut = 15;

    public string Tao(Guid maNguoiDung, string vaiTro)
    {
        var bayGio = dongHo.UtcNow.UtcDateTime;
        var khoa = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tuyChon.Secret));
        var thongTinKy = new SigningCredentials(khoa, SecurityAlgorithms.HmacSha256);
        var cacClaim = new[]
        {
            new Claim("sub", maNguoiDung.ToString()),
            new Claim("role", vaiTro),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            tuyChon.Issuer,
            tuyChon.Audience,
            cacClaim,
            bayGio,
            bayGio.AddMinutes(ThoiHanPhut),
            thongTinKy);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

