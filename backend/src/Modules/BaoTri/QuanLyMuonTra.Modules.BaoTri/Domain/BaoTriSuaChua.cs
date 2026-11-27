using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.BaoTri.Domain;

public sealed class BaoTriSuaChua : IAuditable
{
    private BaoTriSuaChua()
    {
    }

    public Guid MaBaoTri { get; private set; }

    public string MaThietBi { get; private set; } = string.Empty;

    public Guid MaNvThucHien { get; private set; }

    public string? MaPhieuMuon { get; private set; }

    public LoaiThucHienBaoTri LoaiThucHien { get; private set; }

    public DateTimeOffset NgayLap { get; private set; }

    public string? NoiDungThucHien { get; private set; }

    public string? KetQua { get; private set; }

    public DateTimeOffset? NgayHoanThanh { get; private set; }

    internal static void KiemTraYeuCau(string maThietBi, LoaiThucHienBaoTri loaiThucHien, string? maPhieuMuon)
    {
        if (string.IsNullOrWhiteSpace(maThietBi))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Mã thiết bị không được để trống.");
        }

        if (maPhieuMuon is not null && string.IsNullOrWhiteSpace(maPhieuMuon))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Mã phiếu mượn không được để trống khi có gửi.");
        }

        if (loaiThucHien == LoaiThucHienBaoTri.BaoTriDinhKy && maPhieuMuon is not null)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Chỉ tiếp nhận sửa chữa mới liên kết phiếu mượn.");
        }
    }

    internal static BaoTriSuaChua Lap(
        string maThietBi,
        Guid maNvThucHien,
        LoaiThucHienBaoTri loaiThucHien,
        string? maPhieuMuon,
        DateTimeOffset ngayLap)
    {
        KiemTraYeuCau(maThietBi, loaiThucHien, maPhieuMuon);
        if (maNvThucHien == Guid.Empty)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Thiếu nhân viên thực hiện.");
        }

        return new BaoTriSuaChua
        {
            MaBaoTri = Guid.CreateVersion7(),
            MaThietBi = maThietBi,
            MaNvThucHien = maNvThucHien,
            MaPhieuMuon = maPhieuMuon,
            LoaiThucHien = loaiThucHien,
            NgayLap = ngayLap.ToUniversalTime(),
        };
    }

    internal static void KiemTraKetQua(string? noiDungThucHien, string? ketQua)
    {
        if (string.IsNullOrWhiteSpace(noiDungThucHien))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Nội dung thực hiện không được để trống.");
        }

        if (!KetQuaBaoTriMa.HopLe(ketQua))
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Kết quả bảo trì/sửa chữa không hợp lệ.");
        }
    }

    internal void HoanThanh(string? noiDungThucHien, string? ketQua, DateTimeOffset ngayHoanThanh)
    {
        if (KetQua is not null)
        {
            throw new LoiNghiepVuException("TRANG_THAI_KHONG_HOP_LE", "Hồ sơ bảo trì/sửa chữa đã hoàn thành.");
        }

        KiemTraKetQua(noiDungThucHien, ketQua);

        var ngayHoanThanhUtc = ngayHoanThanh.ToUniversalTime();
        if (ngayHoanThanhUtc < NgayLap)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Ngày hoàn thành không được sớm hơn ngày lập.");
        }

        NoiDungThucHien = noiDungThucHien!.Trim();
        KetQua = ketQua;
        NgayHoanThanh = ngayHoanThanhUtc;
    }
}
