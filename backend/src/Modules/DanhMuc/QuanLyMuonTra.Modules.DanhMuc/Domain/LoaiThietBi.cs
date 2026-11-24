using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class LoaiThietBi : IAuditable
{
    private LoaiThietBi()
    {
    }

    internal LoaiThietBi(string tenLoai, string? moTa, string? hangSanXuat, int chuKyBaoTri, decimal phiTreMoiNgay)
    {
        KiemTra(tenLoai, chuKyBaoTri);
        if (phiTreMoiNgay < 0)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Phí trả trễ mỗi ngày không được âm.");
        }

        if (phiTreMoiNgay % 1 != 0)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Phí trả trễ mỗi ngày phải là số nguyên.");
        }

        MaLoai = Guid.CreateVersion7();
        TenLoai = tenLoai;
        MoTa = moTa;
        HangSanXuat = hangSanXuat;
        ChuKyBaoTri = chuKyBaoTri;
        PhiTreMoiNgay = phiTreMoiNgay;
    }

    public Guid MaLoai { get; private set; }

    public string TenLoai { get; private set; } = string.Empty;

    public string? MoTa { get; private set; }

    public string? HangSanXuat { get; private set; }

    public int ChuKyBaoTri { get; private set; }

    public decimal PhiTreMoiNgay { get; private set; }

    public bool NgungSuDung { get; private set; }

    internal void CapNhat(string tenLoai, string? moTa, string? hangSanXuat, int chuKyBaoTri)
    {
        KiemTra(tenLoai, chuKyBaoTri);
        TenLoai = tenLoai;
        MoTa = moTa;
        HangSanXuat = hangSanXuat;
        ChuKyBaoTri = chuKyBaoTri;
    }

    internal void NgungSuDungHoa()
    {
        NgungSuDung = true;
    }

    internal const long PhiTreToiDa = 99_999_999_999_999;

    internal void CapNhatPhiTre(long phiTreMoiNgay)
    {
        if (phiTreMoiNgay < 0)
        {
            throw new LoiNghiepVuException("GIA_TRI_CAU_HINH_KHONG_HOP_LE", "Phí trả trễ mỗi ngày không được âm.");
        }

        if (phiTreMoiNgay > PhiTreToiDa)
        {
            throw new LoiNghiepVuException("GIA_TRI_CAU_HINH_KHONG_HOP_LE", "Phí trả trễ mỗi ngày vượt quá giới hạn lưu trữ.");
        }

        PhiTreMoiNgay = phiTreMoiNgay;
    }

    private static void KiemTra(string tenLoai, int chuKyBaoTri)
    {
        if (string.IsNullOrWhiteSpace(tenLoai))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Tên loại thiết bị không được để trống.");
        }

        if (chuKyBaoTri <= 0)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Chu kỳ bảo trì phải lớn hơn 0.");
        }
    }
}
