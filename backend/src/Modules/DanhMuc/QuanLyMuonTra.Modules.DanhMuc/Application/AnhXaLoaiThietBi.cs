using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal static class AnhXaLoaiThietBi
{
    internal static LoaiThietBiQuanTriDto SangDto(LoaiThietBi loai) => new(
        loai.MaLoai,
        loai.TenLoai,
        loai.MoTa,
        loai.HangSanXuat,
        loai.ChuKyBaoTri,
        Convert.ToInt64(Math.Round(loai.PhiTreMoiNgay)),
        loai.NgungSuDung);
}
