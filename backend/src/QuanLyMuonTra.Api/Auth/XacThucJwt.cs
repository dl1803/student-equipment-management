using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

namespace QuanLyMuonTra.Api.Auth;

public static class XacThucJwt
{
    public static JwtOptions DocCauHinh(IConfiguration cauHinh)
    {
        var secret = cauHinh["Jwt:Secret"];
        var issuer = cauHinh["Jwt:Issuer"];
        var audience = cauHinh["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Thiếu cấu hình Jwt:Secret, Jwt:Issuer hoặc Jwt:Audience.");
        }

        if (Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException("Jwt:Secret phải có tối thiểu 256 bit (32 byte).");
        }

        return new JwtOptions(secret, issuer, audience);
    }

    public static IServiceCollection AddXacThucJwt(this IServiceCollection dichVu, JwtOptions jwt)
    {
        dichVu.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(tuyChon =>
            {
                tuyChon.MapInboundClaims = false;
                tuyChon.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        return dichVu;
    }
}

