namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed record KhoanLapBienBanYeuCau(string MaThietBi, string LoaiViPham, Guid? MaKhungPhat);

public sealed record LapBienBanLenh(string MaPhieuMuon, IReadOnlyList<KhoanLapBienBanYeuCau> DsKhoan);

