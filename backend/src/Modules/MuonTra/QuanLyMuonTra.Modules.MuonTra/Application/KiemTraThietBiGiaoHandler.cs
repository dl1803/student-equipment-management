using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class KiemTraThietBiGiaoHandler(IDanhMucReader danhMuc, IQuyenPhongService quyenPhong)
{
    private const string TinhTrangSanSang = "SAN_SANG";

    public async Task<ThietBiKiemTraDto> XuLyAsync(
        Guid maNv,
        string maThietBi,
        Guid maPhong,
        Guid maLoai,
        CancellationToken ct)
    {
        await quyenPhong.EnsureAsync(maNv, maPhong, ct);

        var thietBi = await danhMuc.LayThietBiAsync(maThietBi, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, $"Không tìm thấy thiết bị {maThietBi}.");

        if (thietBi.TinhTrang != TinhTrangSanSang || thietBi.NgungSuDung)
        {
            throw new LoiNghiepVuException(
                MaLoi.ThietBiKhongSanSang,
                $"Thiết bị {maThietBi} không ở tình trạng sẵn sàng cho mượn.");
        }

        if (thietBi.MaPhong != maPhong)
        {
            throw new LoiNghiepVuException(
                MaLoi.ThietBiSaiPhong,
                $"Thiết bị {maThietBi} không thuộc phòng đang giao.");
        }

        if (thietBi.MaLoai != maLoai)
        {
            throw new LoiNghiepVuException(
                MaLoi.ThietBiSaiLoai,
                $"Thiết bị {maThietBi} không đúng loại đã duyệt.");
        }

        var phong = await danhMuc.LayPhongAsync(thietBi.MaPhong, ct);
        var loai = await danhMuc.LayLoaiAsync(thietBi.MaLoai, ct);

        return new ThietBiKiemTraDto(
            thietBi.MaThietBi,
            thietBi.TenThietBi,
            thietBi.MaLoai,
            loai?.TenLoai ?? string.Empty,
            thietBi.MaPhong,
            phong?.TenPhong ?? string.Empty,
            thietBi.ViTriKe,
            thietBi.ViTriTu);
    }
}
