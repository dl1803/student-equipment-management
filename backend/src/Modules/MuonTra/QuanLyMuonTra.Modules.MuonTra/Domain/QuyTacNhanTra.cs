using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Domain;

public static class QuyTacNhanTra
{
    public static void KiemTraDauVao(KetQuaKiemTra ketQua, string? moTa, Guid? maKhungPhat)
    {
        if (ketQua is not (KetQuaKiemTra.HuHong or KetQuaKiemTra.ThieuPhuKien))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(moTa) || maKhungPhat is null || maKhungPhat.Value == Guid.Empty)
        {
            throw new NghiepVuException(
                MaLoi.ThieuMoTaHoacKhungPhat,
                "Hư hỏng hoặc thiếu phụ kiện phải có mô tả và khung phạt.");
        }
    }

    public static DateTimeOffset? NgayGioTraThucTe(KetQuaKiemTra ketQua, DateTimeOffset bayGio)
    {
        return ketQua == KetQuaKiemTra.Mat ? null : bayGio;
    }

    public static void KiemTraThietBiThuocPhieu(string? maPhieuDangHieuLuc, string maPhieu, string maThietBi)
    {
        if (maPhieuDangHieuLuc is null || maPhieuDangHieuLuc != maPhieu)
        {
            throw new NghiepVuException(
                MaLoi.ThietBiKhongThuocPhieu,
                $"Thiết bị {maThietBi} không thuộc phiếu mượn {maPhieu} đang hiệu lực.");
        }
    }
}
