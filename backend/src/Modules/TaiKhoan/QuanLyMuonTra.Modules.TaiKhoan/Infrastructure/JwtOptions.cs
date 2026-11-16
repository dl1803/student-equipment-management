namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

public sealed record JwtOptions(string Secret, string Issuer, string Audience);

