using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class GhiDongKetQuaNhanTra(
    IThietBiService thietBiService,
    IAnhService anhService,
    IAuditLogger nhatKy)
{
    public async Task GhiAsync(
        PhieuMuon phieu,
        KetQuaKiemTraYeuCau dong,
        Guid maNv,
        DateTimeOffset bayGio,
        CancellationToken ct)
    {
        ChiTietPhieuMuon chiTiet;
        try
        {
            chiTiet = phieu.GhiKetQuaKiemTra(dong.MaThietBi, dong.KetQua, dong.MoTa, dong.MaKhungPhat, maNv, bayGio);
        }
        catch (NghiepVuException loi)
        {
            throw DichLoiMienNghiepVu.Chuyen(loi);
        }

        var daChuyen = dong.KetQua switch
        {
            KetQuaKiemTra.NguyenVen => await thietBiService.NhanTraNguyenVenAsync(dong.MaThietBi, ct),
            KetQuaKiemTra.Mat => await thietBiService.DanhDauMatAsync(dong.MaThietBi, ct),
            _ => await thietBiService.NhanTraHuHongAsync(dong.MaThietBi, ct)
        };
        if (!daChuyen)
        {
            throw new LoiNghiepVuException(
                MaLoi.TrangThaiKhongHopLe,
                $"Thiết bị {dong.MaThietBi} không còn ở trạng thái đang được mượn.");
        }

        if (dong.DsMaAnh.Count > 0)
        {
            await anhService.GanAsync(
                dong.DsMaAnh,
                MaDoiTuongAnh.ChiTietPhieuMuon,
                chiTiet.MaChiTiet.ToString(),
                MaGiaiDoanAnh.KhiTra,
                ct);
        }

        await nhatKy.GhiAsync(
            "GHI_KET_QUA_KIEM_TRA",
            "muon_tra.chi_tiet_phieu_muon",
            chiTiet.MaChiTiet.ToString(),
            new Dictionary<string, object?> { ["KetQuaKiemTra"] = null },
            new Dictionary<string, object?>
            {
                ["MaPhieu"] = phieu.MaPhieu,
                ["MaThietBi"] = dong.MaThietBi,
                ["KetQuaKiemTra"] = QuyTacNhanTra.MaKetQua(dong.KetQua),
                ["MaNvNhanTra"] = maNv,
                ["NgayGioTraThucTe"] = chiTiet.NgayGioTraThucTe,
                ["MaKhungPhat"] = chiTiet.MaKhung
            },
            ct);
    }
}
