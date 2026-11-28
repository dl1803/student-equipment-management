using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuanLyMuonTra.Modules.BaoTri.Domain;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.BaoTri.Application;

public sealed class LapQuyetDinhThanhLyHandler(
    DbContext db,
    IDanhMucReader danhMuc,
    IThietBiService thietBiService,
    IAdvisoryLockService khoa,
    IAvailabilityService khaDung,
    IAuditLogger nhatKy)
{
    public async Task<QuyetDinhThanhLyDto> XuLyAsync(
        Guid maQt,
        LapQuyetDinhThanhLyRequest yeuCau,
        CancellationToken ct)
    {
        var dsMa = ChuanHoaDanhSach(yeuCau.DsMaThietBi);

        var truoc = new Dictionary<string, ThietBiDto>(StringComparer.Ordinal);
        foreach (var ma in dsMa)
        {
            truoc[ma] = await danhMuc.LayThietBiAsync(ma, ct) ?? throw KhongTimThay(ma);
        }

        var giaoDichNgoai = db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await db.Database.BeginTransactionAsync(ct) : null;

        await khoa.LockDanhSachPhongLoaiAsync(truoc.Values.Select(t => (t.MaPhong, t.MaLoai)), ct);

        var dsThietBi = new List<ThietBiThanhLyDto>();
        foreach (var ma in dsMa)
        {
            var thietBi = await danhMuc.LayThietBiAsync(ma, ct) ?? throw KhongTimThay(ma);
            var cu = truoc[ma];
            if (thietBi.MaPhong != cu.MaPhong || thietBi.MaLoai != cu.MaLoai)
            {
                throw new LoiNghiepVuException("XUNG_DOT_CAP_NHAT", "Thiết bị vừa được thay đổi bởi giao dịch khác. Vui lòng tải lại.");
            }

            if (thietBi.TinhTrang is not (MaTinhTrangThietBi.SanSang or MaTinhTrangThietBi.DangBaoTriSuaChua))
            {
                throw TrangThaiKhongHopLe($"Thiết bị {ma} không ở trạng thái cho phép thanh lý.");
            }

            if (thietBi.TinhTrang == MaTinhTrangThietBi.SanSang
                && !thietBi.NgungSuDung
                && await khaDung.TinhKhaDungAsync(thietBi.MaPhong, thietBi.MaLoai, ct) <= 0)
            {
                throw new LoiNghiepVuException(
                    "SO_LUONG_KHA_DUNG_THAY_DOI",
                    $"Thiết bị {ma} đang thuộc hạn ngạch giữ chỗ của phiếu mượn hoặc đăng ký chờ, không thể thanh lý.");
            }

            if (!await thietBiService.ThanhLyAsync(ma, ct))
            {
                throw TrangThaiKhongHopLe($"Thiết bị {ma} không còn ở trạng thái cho phép thanh lý.");
            }

            dsThietBi.Add(new ThietBiThanhLyDto(thietBi.MaThietBi, thietBi.TenThietBi));
        }

        var maQuyetDinh = await ChenQuyetDinhAsync(maQt, yeuCau.NgayThanhLy, ct);
        foreach (var thietBi in dsThietBi)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO bao_tri.chi_tiet_thanh_ly (ma_quyet_dinh, ma_thiet_bi)
                VALUES ({maQuyetDinh}, {thietBi.MaThietBi})
                """, ct);
        }

        await nhatKy.GhiAsync(
            "THEM",
            "bao_tri.quyet_dinh_thanh_ly",
            maQuyetDinh,
            null,
            new Dictionary<string, object?>
            {
                ["MaQTLap"] = maQt.ToString(),
                ["NgayThanhLy"] = yeuCau.NgayThanhLy.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["DsMaThietBi"] = string.Join(",", dsThietBi.Select(t => t.MaThietBi)),
            },
            ct);

        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }

        return new QuyetDinhThanhLyDto(maQuyetDinh, maQt, yeuCau.NgayThanhLy, dsThietBi);
    }

    private async Task<string> ChenQuyetDinhAsync(Guid maQt, DateOnly ngayThanhLy, CancellationToken ct)
    {
        await using var lenh = db.Database.GetDbConnection().CreateCommand();
        lenh.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        lenh.CommandText = """
            INSERT INTO bao_tri.quyet_dinh_thanh_ly (ma_qt_lap, ngay_thanh_ly)
            VALUES (@ma_qt_lap, @ngay_thanh_ly::date)
            RETURNING ma_quyet_dinh
            """;
        ThemThamSo(lenh, "ma_qt_lap", maQt);
        ThemThamSo(lenh, "ngay_thanh_ly", ngayThanhLy);

        return (string)(await lenh.ExecuteScalarAsync(ct))!;
    }

    private static void ThemThamSo(DbCommand lenh, string ten, object giaTri)
    {
        var thamSo = lenh.CreateParameter();
        thamSo.ParameterName = ten;
        thamSo.Value = giaTri;
        lenh.Parameters.Add(thamSo);
    }

    private static List<string> ChuanHoaDanhSach(IReadOnlyList<string>? dsMaThietBi)
    {
        if (dsMaThietBi is null || dsMaThietBi.Count == 0)
        {
            throw DuLieuKhongHopLe("Danh sách thiết bị thanh lý không được để trống.");
        }

        var ketQua = new List<string>(dsMaThietBi.Count);
        var daCo = new HashSet<string>(StringComparer.Ordinal);
        foreach (var ma in dsMaThietBi)
        {
            if (string.IsNullOrWhiteSpace(ma))
            {
                throw DuLieuKhongHopLe("Mã thiết bị không được để trống.");
            }

            var chuan = ma.Trim();
            if (!daCo.Add(chuan))
            {
                throw DuLieuKhongHopLe($"Mã thiết bị {chuan} bị lặp trong danh sách.");
            }

            ketQua.Add(chuan);
        }

        return ketQua;
    }

    private static LoiNghiepVuException DuLieuKhongHopLe(string thongDiep) =>
        new("DU_LIEU_KHONG_HOP_LE", thongDiep);

    private static LoiNghiepVuException KhongTimThay(string ma) =>
        new("DU_LIEU_KHONG_HOP_LE", $"Không tìm thấy thiết bị {ma}.");

    private static LoiNghiepVuException TrangThaiKhongHopLe(string thongDiep) =>
        new("TRANG_THAI_KHONG_HOP_LE", thongDiep);
}
