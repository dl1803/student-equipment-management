namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public class DangKyCho
{
    private DangKyCho()
    {
        MaDangKy = string.Empty;
        MaSv = string.Empty;
    }

    internal DangKyCho(string maDangKy, string maSv, Guid maPhong, Guid maLoai, DateTimeOffset thoiDiemDangKy, int soThuTuCho)
    {
        MaDangKy = maDangKy;
        MaSv = maSv;
        MaPhong = maPhong;
        MaLoai = maLoai;
        ThoiDiemDangKy = thoiDiemDangKy;
        SoThuTuCho = soThuTuCho;
        TrangThai = TrangThaiDangKyCho.DangCho;
    }

    public string MaDangKy { get; private set; }

    public string MaSv { get; private set; }

    public Guid MaPhong { get; private set; }

    public Guid MaLoai { get; private set; }

    public DateTimeOffset ThoiDiemDangKy { get; private set; }

    public int? SoThuTuCho { get; private set; }

    public TrangThaiDangKyCho TrangThai { get; private set; }

    public DateTimeOffset? HanGiuCho { get; private set; }

    public void ThongBaoDenLuot(DateTimeOffset hanGiuCho)
    {
        YeuCauTrangThai(TrangThaiDangKyCho.DangCho);
        TrangThai = TrangThaiDangKyCho.DaThongBao;
        HanGiuCho = hanGiuCho;
    }

    public void DanhDauDaNhan()
    {
        YeuCauTrangThai(TrangThaiDangKyCho.DaThongBao);
        TrangThai = TrangThaiDangKyCho.DaNhan;
    }

    public void Roi()
    {
        YeuCauTrangThai(TrangThaiDangKyCho.DangCho, TrangThaiDangKyCho.DaThongBao);
        TrangThai = TrangThaiDangKyCho.DaHuy;
    }

    public void HetHan()
    {
        YeuCauTrangThai(TrangThaiDangKyCho.DaThongBao);
        TrangThai = TrangThaiDangKyCho.HetHan;
    }

    private void YeuCauTrangThai(params TrangThaiDangKyCho[] hopLe)
    {
        if (Array.IndexOf(hopLe, TrangThai) < 0)
        {
            throw new NghiepVuException(MaLoi.TrangThaiKhongHopLe, $"Đăng ký chờ {MaDangKy} đang ở trạng thái {TrangThai}, không thể thực hiện thao tác này.");
        }
    }
}
