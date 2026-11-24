using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Domain;

public sealed class LichSuGiaTri : IAuditable
{
    internal const long GiaTriToiDa = 99_999_999_999_999;

    private LichSuGiaTri()
    {
    }

    internal LichSuGiaTri(string maThietBi, long giaTri, DateTimeOffset ngayCapNhat)
    {
        if (giaTri < 0 || giaTri > GiaTriToiDa)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Giá trị thiết bị không hợp lệ.");
        }

        MaLichSu = Guid.CreateVersion7();
        MaThietBi = maThietBi;
        GiaTri = giaTri;
        NgayCapNhat = ngayCapNhat;
    }

    public Guid MaLichSu { get; private set; }

    public string MaThietBi { get; private set; } = string.Empty;

    public decimal GiaTri { get; private set; }

    public DateTimeOffset NgayCapNhat { get; private set; }
}
