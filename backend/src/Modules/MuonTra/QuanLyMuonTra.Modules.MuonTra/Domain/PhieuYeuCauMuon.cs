namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public class PhieuYeuCauMuon
{
    private readonly List<ChiTietYeuCauMuon> _chiTiet = new();

    private PhieuYeuCauMuon()
    {
        MaYeuCau = string.Empty;
        MaSv = string.Empty;
    }

    internal PhieuYeuCauMuon(string maYeuCau, string maSv, Guid maPhong, int thoiGianMuonDuKienGio, DateTimeOffset thoiGianGui)
    {
        MaYeuCau = maYeuCau;
        MaSv = maSv;
        MaPhong = maPhong;
        ThoiGianMuonDuKienGio = thoiGianMuonDuKienGio;
        ThoiGianGui = thoiGianGui;
        TrangThai = TrangThaiYeuCauMuon.ChoDuyet;
    }

    public string MaYeuCau { get; private set; }

    public string MaSv { get; private set; }

    public Guid MaPhong { get; private set; }

    public Guid? MaNvDuyet { get; private set; }

    public DateTimeOffset ThoiGianGui { get; private set; }

    public int ThoiGianMuonDuKienGio { get; private set; }

    public TrangThaiYeuCauMuon TrangThai { get; private set; }

    public string? LyDoTuChoi { get; private set; }

    public DateTimeOffset? ThoiDiemDuyet { get; private set; }

    public DateTimeOffset? HanGiuCho { get; private set; }

    public IReadOnlyCollection<ChiTietYeuCauMuon> ChiTiet => _chiTiet;

    public void Duyet(Guid maNvDuyet, DateTimeOffset thoiDiemDuyet, DateTimeOffset hanGiuCho)
    {
        YeuCauTrangThai(TrangThaiYeuCauMuon.ChoDuyet);
        if (hanGiuCho <= thoiDiemDuyet)
        {
            throw new NghiepVuException(MaLoi.DuLieuKhongHopLe, "Hạn giữ chỗ phải sau thời điểm duyệt.");
        }

        TrangThai = TrangThaiYeuCauMuon.DaDuyet;
        MaNvDuyet = maNvDuyet;
        ThoiDiemDuyet = thoiDiemDuyet;
        HanGiuCho = hanGiuCho;
    }

    public void TuChoi(Guid maNvDuyet, string lyDoTuChoi)
    {
        YeuCauTrangThai(TrangThaiYeuCauMuon.ChoDuyet);
        if (string.IsNullOrWhiteSpace(lyDoTuChoi))
        {
            throw new NghiepVuException(MaLoi.DuLieuKhongHopLe, "Từ chối phải kèm lý do.");
        }

        TrangThai = TrangThaiYeuCauMuon.TuChoi;
        MaNvDuyet = maNvDuyet;
        LyDoTuChoi = lyDoTuChoi;
    }

    public void DanhDauDaNhan()
    {
        YeuCauTrangThai(TrangThaiYeuCauMuon.DaDuyet);
        TrangThai = TrangThaiYeuCauMuon.DaNhan;
    }

    public void HetHan()
    {
        YeuCauTrangThai(TrangThaiYeuCauMuon.ChoDuyet, TrangThaiYeuCauMuon.DaDuyet);
        TrangThai = TrangThaiYeuCauMuon.HetHan;
    }

    public void Huy()
    {
        YeuCauTrangThai(TrangThaiYeuCauMuon.ChoDuyet, TrangThaiYeuCauMuon.DaDuyet);
        TrangThai = TrangThaiYeuCauMuon.DaHuy;
    }

    private void YeuCauTrangThai(params TrangThaiYeuCauMuon[] hopLe)
    {
        if (Array.IndexOf(hopLe, TrangThai) < 0)
        {
            throw new NghiepVuException(MaLoi.TrangThaiKhongHopLe, $"Phiếu yêu cầu {MaYeuCau} đang ở trạng thái {TrangThai}, không thể thực hiện thao tác này.");
        }
    }
}
