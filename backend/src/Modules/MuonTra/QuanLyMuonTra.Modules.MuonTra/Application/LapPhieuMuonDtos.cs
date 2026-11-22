using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed record ThietBiGiaoYeuCau(string MaThietBi, string MoTa, IReadOnlyList<Guid>? DsMaAnh);

public sealed record ChiTietPhieuMuonDayDuDto(
    Guid MaChiTiet,
    string MaThietBi,
    string TenThietBi,
    Guid MaLoai,
    string TenLoai,
    string TinhTrangBanDau,
    long PhiTreMoiNgay,
    string? TinhTrangKhiTra,
    string? KetQuaKiemTra,
    DateTimeOffset? NgayGioTraThucTe,
    Guid? MaKhungPhat,
    IReadOnlyList<QuanLyMuonTra.SharedKernel.AnhRefDto> DsAnhBanDau,
    IReadOnlyList<QuanLyMuonTra.SharedKernel.AnhRefDto> DsAnhKhiTra);

public sealed record PhieuMuonDayDuDto(
    string MaPhieu,
    string MaSV,
    string? MaYeuCau,
    string? MaDangKy,
    DateTimeOffset NgayGioMuon,
    DateTimeOffset NgayGioHenTra,
    int SoLanGiaHan,
    string TrangThai,
    int SoThietBi,
    IReadOnlyList<ChiTietPhieuMuonDayDuDto> DsChiTiet);
