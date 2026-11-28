namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public class ChiTietPhieuMuon
{
    private ChiTietPhieuMuon()
    {
        MaPhieu = string.Empty;
        MaThietBi = string.Empty;
        TinhTrangBanDau = string.Empty;
    }

    internal ChiTietPhieuMuon(Guid maChiTiet, string maPhieu, string maThietBi, string tinhTrangBanDau, decimal phiTreMoiNgay, KetQuaKiemTra? ketQuaKiemTra = null)
    {
        MaChiTiet = maChiTiet;
        MaPhieu = maPhieu;
        MaThietBi = maThietBi;
        TinhTrangBanDau = tinhTrangBanDau;
        PhiTreMoiNgay = phiTreMoiNgay;
        KetQuaKiemTra = ketQuaKiemTra;
    }

    public Guid MaChiTiet { get; private set; }

    public string MaPhieu { get; private set; }

    public string MaThietBi { get; private set; }

    public string TinhTrangBanDau { get; private set; }

    public decimal PhiTreMoiNgay { get; private set; }

    public string? TinhTrangKhiTra { get; private set; }

    public KetQuaKiemTra? KetQuaKiemTra { get; private set; }

    public DateTimeOffset? NgayGioTraThucTe { get; private set; }

    public Guid? MaNvNhanTra { get; private set; }

    public Guid? MaKhung { get; private set; }

    internal void GhiKetQuaKiemTra(KetQuaKiemTra ketQua, string? moTa, Guid? maKhungPhat, Guid maNvNhanTra, DateTimeOffset bayGio)
    {
        QuyTacNhanTra.KiemTraDauVao(ketQua, moTa, maKhungPhat);
        MaKhung = QuyTacNhanTra.KhungPhatLuuTru(ketQua, maKhungPhat);
        KetQuaKiemTra = ketQua;
        TinhTrangKhiTra = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();
        NgayGioTraThucTe = QuyTacNhanTra.NgayGioTraThucTe(ketQua, bayGio);
        MaNvNhanTra = maNvNhanTra;
    }
}
