using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.TaiKhoan.Domain;

public abstract class NguoiDungHeThong : IAuditable
{
    protected NguoiDungHeThong()
    {
    }

    public Guid MaNguoiDung { get; protected set; }

    public string TenDangNhap { get; protected set; } = string.Empty;

    public string MatKhauBam { get; protected set; } = string.Empty;

    public string HoTen { get; protected set; } = string.Empty;

    public string? Email { get; protected set; }

    public string TrangThaiKhoa { get; protected set; } = MaTrangThaiKhoa.HoatDong;

    public int SoLanDangNhapSai { get; protected set; }

    public abstract string VaiTro { get; }

    public bool DangHoatDong => TrangThaiKhoa == MaTrangThaiKhoa.HoatDong;

    public void GhiNhanDangNhapSai(int soLanToiDa)
    {
        if (!DangHoatDong)
        {
            return;
        }

        SoLanDangNhapSai++;

        if (SoLanDangNhapSai >= soLanToiDa)
        {
            TrangThaiKhoa = MaTrangThaiKhoa.TamKhoaDangNhap;
        }
    }

    public void GhiNhanDangNhapDung()
    {
        SoLanDangNhapSai = 0;
    }
}

