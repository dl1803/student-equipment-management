namespace QuanLyMuonTra.Modules.QuyDinh.Contracts;

public sealed record NgoaiLeTheoLoaiDto(Guid MaLoai, int ThoiGianMuonToiDaGio);

public sealed record QuyDinhNhomDto(
    string MaNhom,
    int SoThietBiToiDa,
    int ThoiGianMuonToiDaGio,
    IReadOnlyList<NgoaiLeTheoLoaiDto> DsNgoaiLeTheoLoai);

public sealed record ThamSoHeThongDto(
    int SoNgayKhoa,
    int ThoiGianGiuChoPhut,
    int AnHanPhut,
    int SoGioGiaHanToiDa,
    int NguongViPham,
    int SoLanDangNhapSaiToiDa,
    TimeOnly? GioMoCua,
    TimeOnly? GioDongCua);

public sealed record KhungPhatDto(Guid MaKhung, string TenKhung, decimal TyLePhanTram, bool DangSuDung);

public interface IQuyDinhReader
{
    Task<QuyDinhNhomDto?> LayQuyDinhNhomAsync(string maNhom, CancellationToken ct);

    Task<ThamSoHeThongDto> LayThamSoAsync(CancellationToken ct);

    Task<KhungPhatDto?> LayKhungPhatAsync(Guid maKhung, CancellationToken ct);

    Task<IReadOnlyList<KhungPhatDto>> DsKhungPhatDangSuDungAsync(CancellationToken ct);
}

