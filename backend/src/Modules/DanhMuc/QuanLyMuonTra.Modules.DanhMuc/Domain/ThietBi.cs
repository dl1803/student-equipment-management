using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class ThietBi : IAuditable
{
    private ThietBi()
    {
    }

    internal static ThietBi NhapKho(
        string maThietBi,
        Guid maLoai,
        Guid maPhong,
        string tenThietBi,
        string? moTa,
        string? thongSoKyThuat,
        DateOnly ngayNhapKho,
        DateOnly? thoiHanBaoHanh,
        string? viTriKe,
        string? viTriTu)
    {
        KiemTraMa(maThietBi);
        KiemTraTen(tenThietBi);
        return new ThietBi
        {
            MaThietBi = maThietBi,
            MaLoai = maLoai,
            MaPhong = maPhong,
            TenThietBi = tenThietBi,
            MoTa = moTa,
            ThongSoKyThuat = thongSoKyThuat,
            NgayNhapKho = ngayNhapKho,
            ThoiHanBaoHanh = thoiHanBaoHanh,
            TinhTrang = TinhTrangThietBi.SanSang,
            ViTriKe = viTriKe,
            ViTriTu = viTriTu,
            NgungSuDung = false,
        };
    }

    public string MaThietBi { get; private set; } = string.Empty;

    public Guid MaLoai { get; private set; }

    public Guid MaPhong { get; private set; }

    public string TenThietBi { get; private set; } = string.Empty;

    public string? MoTa { get; private set; }

    public string? ThongSoKyThuat { get; private set; }

    public DateOnly NgayNhapKho { get; private set; }

    public DateOnly? ThoiHanBaoHanh { get; private set; }

    public TinhTrangThietBi TinhTrang { get; private set; } = TinhTrangThietBi.SanSang;

    public string? ViTriKe { get; private set; }

    public string? ViTriTu { get; private set; }

    public bool NgungSuDung { get; private set; }

    internal void CapNhatThongTin(
        Guid maLoai,
        Guid maPhong,
        string tenThietBi,
        string? moTa,
        string? thongSoKyThuat,
        DateOnly ngayNhapKho,
        DateOnly? thoiHanBaoHanh,
        string? viTriKe,
        string? viTriTu)
    {
        KiemTraTen(tenThietBi);
        MaLoai = maLoai;
        MaPhong = maPhong;
        TenThietBi = tenThietBi;
        MoTa = moTa;
        ThongSoKyThuat = thongSoKyThuat;
        NgayNhapKho = ngayNhapKho;
        ThoiHanBaoHanh = thoiHanBaoHanh;
        ViTriKe = viTriKe;
        ViTriTu = viTriTu;
    }

    internal void NgungSuDungHoa()
    {
        NgungSuDung = true;
    }

    private static void KiemTraMa(string maThietBi)
    {
        if (string.IsNullOrWhiteSpace(maThietBi))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Mã thiết bị không được để trống.");
        }
    }

    private static void KiemTraTen(string tenThietBi)
    {
        if (string.IsNullOrWhiteSpace(tenThietBi))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Tên thiết bị không được để trống.");
        }
    }
}
