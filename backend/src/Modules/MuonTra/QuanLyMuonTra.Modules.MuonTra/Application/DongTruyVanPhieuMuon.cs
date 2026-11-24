namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DongPhieuMuon
{
    public string MaPhieu { get; set; } = string.Empty;

    public string MaSv { get; set; } = string.Empty;

    public Guid MaNvLap { get; set; }

    public string? MaYeuCau { get; set; }

    public string? MaDangKy { get; set; }

    public DateTimeOffset NgayGioMuon { get; set; }

    public DateTimeOffset NgayGioHenTra { get; set; }

    public int SoLanGiaHan { get; set; }

    public string TrangThai { get; set; } = string.Empty;

    public int SoThietBi { get; set; }
}

public sealed class DongChiTietPhieuMuon
{
    public Guid MaChiTiet { get; set; }

    public string MaPhieu { get; set; } = string.Empty;

    public string MaThietBi { get; set; } = string.Empty;

    public string TinhTrangBanDau { get; set; } = string.Empty;

    public long PhiTreMoiNgay { get; set; }

    public string? TinhTrangKhiTra { get; set; }

    public string? KetQuaKiemTra { get; set; }

    public DateTimeOffset? NgayGioTraThucTe { get; set; }

    public Guid? MaKhung { get; set; }
}

public sealed class DongBienBanPhieu
{
    public string MaBienBan { get; set; } = string.Empty;

    public string MaPhieuMuon { get; set; } = string.Empty;

    public DateTimeOffset NgayLap { get; set; }

    public long TongTien { get; set; }

    public string TrangThai { get; set; } = string.Empty;

    public DateTimeOffset? ThoiDiemNop { get; set; }
}

public sealed class DongChiTietBienBan
{
    public string MaBienBan { get; set; } = string.Empty;

    public string MaThietBi { get; set; } = string.Empty;

    public string LoaiViPham { get; set; } = string.Empty;

    public int? SoNgayTre { get; set; }

    public long SoTien { get; set; }

    public Guid? MaKhung { get; set; }
}
