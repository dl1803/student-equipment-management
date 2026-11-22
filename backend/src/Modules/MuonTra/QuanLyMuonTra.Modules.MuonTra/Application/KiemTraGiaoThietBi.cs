using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

internal static class KiemTraGiaoThietBi
{
    private const string TinhTrangSanSang = "SAN_SANG";

    public static void KiemTraKhongTrungMa(IEnumerable<string> dsMaThietBi)
    {
        var daThay = new HashSet<string>(StringComparer.Ordinal);
        foreach (var maThietBi in dsMaThietBi)
        {
            if (!daThay.Add(maThietBi))
            {
                throw new LoiNghiepVuException(
                    MaLoi.DuLieuKhongHopLe,
                    $"Mã thiết bị {maThietBi} xuất hiện nhiều lần trong danh sách giao.");
            }
        }
    }

    public static ThietBiDto KiemTraThietBi(
        ThietBiDto? thietBi,
        string maThietBi,
        Guid maPhong,
        IReadOnlyCollection<Guid> dsMaLoaiCan)
    {
        if (thietBi is null)
        {
            throw new LoiNghiepVuException(MaLoi.KhongTimThay, $"Không tìm thấy thiết bị {maThietBi}.");
        }

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

        if (!dsMaLoaiCan.Contains(thietBi.MaLoai))
        {
            throw new LoiNghiepVuException(
                MaLoi.ThietBiSaiLoai,
                $"Thiết bị {maThietBi} không đúng loại của nguồn giao.");
        }

        return thietBi;
    }

    public static void KiemTraKhopSoLuong(IEnumerable<Guid> dsMaLoaiTheoThietBi, IReadOnlyList<LoaiVaSoLuongYeuCau> dsCan)
    {
        var soGiao = dsMaLoaiTheoThietBi
            .GroupBy(maLoai => maLoai)
            .ToDictionary(nhom => nhom.Key, nhom => nhom.Count());

        foreach (var dong in dsCan)
        {
            soGiao.TryGetValue(dong.MaLoai, out var daGiao);
            if (daGiao != dong.SoLuong)
            {
                throw new LoiNghiepVuException(
                    MaLoi.DuLieuKhongHopLe,
                    $"Loại {dong.MaLoai} giao {daGiao} thiết bị, nguồn yêu cầu đúng {dong.SoLuong}.");
            }
        }
    }
}
