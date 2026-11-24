using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

internal static class AnhXaLoaiThietBi
{
    internal static LoaiThietBiQuanTriDto SangDto(LoaiThietBi loai)
    {
        if (loai.PhiTreMoiNgay % 1 != 0)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Phí trả trễ không phải là số nguyên.");
        }

        return new(
            loai.MaLoai,
            loai.TenLoai,
            loai.MoTa,
            loai.HangSanXuat,
            loai.ChuKyBaoTri,
            Convert.ToInt64(loai.PhiTreMoiNgay),
            loai.NgungSuDung);
    }
}
