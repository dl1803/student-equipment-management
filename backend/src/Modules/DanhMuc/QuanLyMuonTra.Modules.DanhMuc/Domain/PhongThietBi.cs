using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class PhongThietBi : IAuditable
{
    private PhongThietBi()
    {
    }

    internal PhongThietBi(string tenPhong, string coSo)
    {
        KiemTra(tenPhong, coSo);
        MaPhong = Guid.CreateVersion7();
        TenPhong = tenPhong;
        CoSo = coSo;
    }

    public Guid MaPhong { get; private set; }

    public string TenPhong { get; private set; } = string.Empty;

    public string CoSo { get; private set; } = string.Empty;

    internal void CapNhat(string tenPhong, string coSo)
    {
        KiemTra(tenPhong, coSo);
        TenPhong = tenPhong;
        CoSo = coSo;
    }

    private static void KiemTra(string tenPhong, string coSo)
    {
        if (string.IsNullOrWhiteSpace(tenPhong))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Tên phòng không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(coSo))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Cơ sở không được để trống.");
        }
    }
}
