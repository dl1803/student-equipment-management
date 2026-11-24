using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public class PhieuMuon
{
    private readonly List<ChiTietPhieuMuon> _chiTiet = new();

    private PhieuMuon()
    {
        MaPhieu = string.Empty;
        MaSv = string.Empty;
    }

    internal PhieuMuon(string maPhieu, string maSv, Guid maNvLap, DateTimeOffset ngayGioMuon, DateTimeOffset ngayGioHenTra, IEnumerable<ChiTietPhieuMuon> chiTiet)
    {
        if (ngayGioHenTra <= ngayGioMuon)
        {
            throw new NghiepVuException(MaLoi.DuLieuKhongHopLe, "Ngày giờ hẹn trả phải sau ngày giờ mượn.");
        }

        MaPhieu = maPhieu;
        MaSv = maSv;
        MaNvLap = maNvLap;
        NgayGioMuon = ngayGioMuon;
        NgayGioHenTra = ngayGioHenTra;
        TrangThai = TrangThaiPhieuMuon.DangMuon;
        _chiTiet.AddRange(chiTiet);
    }

    public string MaPhieu { get; private set; }

    public string MaSv { get; private set; }

    public Guid MaNvLap { get; private set; }

    public string? MaYeuCau { get; private set; }

    public string? MaDangKy { get; private set; }

    public DateTimeOffset NgayGioMuon { get; private set; }

    public DateTimeOffset NgayGioHenTra { get; private set; }

    public int SoLanGiaHan { get; private set; }

    public TrangThaiPhieuMuon TrangThai { get; private set; }

    public IReadOnlyCollection<ChiTietPhieuMuon> ChiTiet => _chiTiet;

    public void DanhDauQuaHan()
    {
        YeuCauTrangThai(TrangThaiPhieuMuon.DangMuon);
        TrangThai = TrangThaiPhieuMuon.QuaHan;
    }

    public void XetDaTra()
    {
        YeuCauTrangThai(TrangThaiPhieuMuon.DangMuon, TrangThaiPhieuMuon.QuaHan);
        if (_chiTiet.Count == 0)
        {
            throw new NghiepVuException(MaLoi.DuLieuKhongHopLe, "Phiếu mượn không có chi tiết hoặc chưa nạp chi tiết thiết bị.");
        }

        if (_chiTiet.All(c => c.KetQuaKiemTra.HasValue))
        {
            TrangThai = TrangThaiPhieuMuon.DaTra;
        }
    }

    private void YeuCauTrangThai(params TrangThaiPhieuMuon[] hopLe)
    {
        if (Array.IndexOf(hopLe, TrangThai) < 0)
        {
            throw new NghiepVuException(MaLoi.TrangThaiKhongHopLe, $"Phiếu mượn {MaPhieu} đang ở trạng thái {TrangThai}, không thể thực hiện thao tác này.");
        }
    }
}
