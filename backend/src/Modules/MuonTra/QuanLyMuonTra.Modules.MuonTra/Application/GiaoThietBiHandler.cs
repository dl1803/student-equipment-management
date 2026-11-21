using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class GiaoThietBiHandler(
    DbContext db,
    IClock dongHo,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IMuonTraReader muonTra,
    IHoSoSinhVienReader hoSoReader,
    IAvailabilityService khaDung,
    IAdvisoryLockService khoa,
    IBoTinhHanTra boTinhHanTra,
    IThietBiService thietBiService,
    IAnhService anhService,
    IQuyenPhongService quyenPhong,
    IAuditLogger nhatKy)
{
    private const string NguonPhieuYeuCau = "PHIEU_YEU_CAU";
    private const string NguonDangKyCho = "DANG_KY_CHO";
    private const string NguonTrucTiep = "TRUC_TIEP";
    private const string TrangThaiDangMuon = "DANG_MUON";

    private sealed record NguonGiao(
        string Loai,
        string? MaYeuCau,
        string? MaDangKy,
        string MaSv,
        Guid MaPhong,
        IReadOnlyList<LoaiVaSoLuongYeuCau> DsLoaiCan,
        int? ThoiGianMuonDuKienGio);

    private sealed record DongGiao(
        Guid MaChiTiet,
        ThietBiDto ThietBi,
        string TenLoai,
        decimal PhiTreMoiNgay,
        string MoTa,
        bool CoAnh);

    private sealed record KetQuaGhi(
        string MaPhieu,
        string MaSv,
        string? MaYeuCau,
        string? MaDangKy,
        DateTimeOffset GioGiao,
        DateTimeOffset HanTra,
        IReadOnlyList<DongGiao> DsDong);

    public async Task<PhieuMuonDayDuDto> GiaoTheoPhieuYeuCauAsync(
        Guid maNv,
        string maYeuCau,
        IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi,
        CancellationToken ct)
    {
        KiemTraGiaoThietBi.KiemTraKhongTrungMa(dsThietBi.Select(t => t.MaThietBi));

        var phieu = await PhieuYeuCauMuonQuayTruyCap.LayVaKiemQuyenAsync(db, danhMuc, quyenPhong, maNv, maYeuCau, ct);
        var nguon = new NguonGiao(
            NguonPhieuYeuCau,
            maYeuCau,
            null,
            phieu.MaSV,
            phieu.MaPhong,
            phieu.DsLoaiVaSoLuong.Select(d => new LoaiVaSoLuongYeuCau(d.MaLoai, d.SoLuong)).ToList(),
            phieu.ThoiGianMuonDuKienGio);

        return await ThucHienAsync(maNv, nguon, dsThietBi, ct);
    }

    public async Task<PhieuMuonDayDuDto> GiaoTheoDangKyChoAsync(
        Guid maNv,
        string maDangKy,
        IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi,
        CancellationToken ct)
    {
        KiemTraGiaoThietBi.KiemTraKhongTrungMa(dsThietBi.Select(t => t.MaThietBi));

        var dangKy = await db.Set<DangKyCho>().AsNoTracking().SingleOrDefaultAsync(d => d.MaDangKy == maDangKy, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy đăng ký chờ.");

        await quyenPhong.EnsureAsync(maNv, dangKy.MaPhong, ct);

        var nguon = new NguonGiao(
            NguonDangKyCho,
            null,
            maDangKy,
            dangKy.MaSv,
            dangKy.MaPhong,
            [new LoaiVaSoLuongYeuCau(dangKy.MaLoai, 1)],
            null);

        return await ThucHienAsync(maNv, nguon, dsThietBi, ct);
    }

    public async Task<PhieuMuonDayDuDto> GiaoTrucTiepAsync(
        Guid maNv,
        string maSv,
        Guid maPhong,
        IReadOnlyList<LoaiVaSoLuongYeuCau> dsLoai,
        IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi,
        CancellationToken ct)
    {
        KiemTraGiaoThietBi.KiemTraKhongTrungMa(dsThietBi.Select(t => t.MaThietBi));

        await quyenPhong.EnsureAsync(maNv, maPhong, ct);

        if (await danhMuc.LayPhongAsync(maPhong, ct) is null)
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Phòng thiết bị không tồn tại.");
        }

        foreach (var dong in dsLoai)
        {
            var loai = await danhMuc.LayLoaiAsync(dong.MaLoai, ct);
            if (loai is null || loai.NgungSuDung)
            {
                throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Loại thiết bị không tồn tại hoặc đã ngừng sử dụng.");
            }
        }

        var hoSo = await hoSoReader.LayTheoMaSvAsync(maSv, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy sinh viên.");

        var nguon = new NguonGiao(NguonTrucTiep, null, null, hoSo.MaSv, maPhong, dsLoai, null);

        return await ThucHienAsync(maNv, nguon, dsThietBi, ct);
    }

    private async Task<PhieuMuonDayDuDto> ThucHienAsync(
        Guid maNv,
        NguonGiao nguon,
        IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi,
        CancellationToken ct)
    {
        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        KetQuaGhi ketQua;
        try
        {
            ketQua = await GhiAsync(maNv, nguon, dsThietBi, ct);

            if (giaoDich is not null)
            {
                await giaoDich.CommitAsync(ct);
            }
        }
        catch (Exception loi) when (LoiTrungKhoa(loi) is { } trungKhoa)
        {
            throw DichLoiTrungKhoa(trungKhoa);
        }
        finally
        {
            if (giaoDich is not null)
            {
                await giaoDich.DisposeAsync();
            }
        }

        return await DungKetQuaAsync(ketQua, ct);
    }

    private async Task<KetQuaGhi> GhiAsync(
        Guid maNv,
        NguonGiao nguon,
        IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi,
        CancellationToken ct)
    {
        await HoSoSinhVienTruyVan.KhoaAsync(db, nguon.MaSv, ct);
        await khoa.LockDanhSachPhongLoaiAsync(nguon.DsLoaiCan.Select(d => (nguon.MaPhong, d.MaLoai)), ct);

        var thoiDiem = dongHo.UtcNow.ToUniversalTime();

        PhieuYeuCauMuon? phieuYeuCau = null;
        DangKyCho? dangKy = null;
        if (nguon.Loai == NguonPhieuYeuCau)
        {
            var maYeuCau = nguon.MaYeuCau;
            phieuYeuCau = await db.Set<PhieuYeuCauMuon>().SingleAsync(p => p.MaYeuCau == maYeuCau, ct);
            KiemTraNguon(
                phieuYeuCau.MaYeuCau,
                phieuYeuCau.TrangThai == TrangThaiYeuCauMuon.DaDuyet,
                phieuYeuCau.HanGiuCho,
                thoiDiem);
        }
        else if (nguon.Loai == NguonDangKyCho)
        {
            var maDangKy = nguon.MaDangKy;
            dangKy = await db.Set<DangKyCho>().SingleAsync(d => d.MaDangKy == maDangKy, ct);
            KiemTraNguon(
                dangKy.MaDangKy,
                dangKy.TrangThai == TrangThaiDangKyCho.DaThongBao,
                dangKy.HanGiuCho,
                thoiDiem);
        }

        var hoSo = await hoSoReader.LayTheoMaSvAsync(nguon.MaSv, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy sinh viên.");
        var quyDinhNhom = await quyDinh.LayQuyDinhNhomAsync(hoSo.MaNhom, ct)
            ?? throw new InvalidOperationException($"Thiếu quy định mượn chung của nhóm {hoSo.MaNhom}.");
        var soDangCam = await muonTra.DemThietBiDangCamAsync(hoSo.MaSv, ct);

        var dsLyDo = KiemTraDieuKienGiao.TongHopLyDoSinhVien(
            hoSo,
            VnTime.NgayLich(thoiDiem),
            quyDinhNhom,
            soDangCam,
            nguon.DsLoaiCan.Sum(d => (long)d.SoLuong));
        if (dsLyDo.Count > 0)
        {
            throw new LoiNghiepVuException(dsLyDo[0].MaLoi, dsLyDo[0].ThongDiep);
        }

        var thamSo = await quyDinh.LayThamSoAsync(ct);
        var toiDaGio = quyDinhNhom.ThoiGianToiDaGio(nguon.DsLoaiCan.Select(d => d.MaLoai));
        var thoiGianMuonGio = nguon.ThoiGianMuonDuKienGio is { } duKien ? Math.Min(duKien, toiDaGio) : toiDaGio;
        var hanTra = boTinhHanTra.TinhHanTra(thoiDiem, thoiGianMuonGio, thamSo.GioDongCua);

        if (nguon.Loai == NguonTrucTiep)
        {
            foreach (var dong in nguon.DsLoaiCan)
            {
                var tenLoai = (await danhMuc.LayLoaiAsync(dong.MaLoai, ct))?.TenLoai ?? string.Empty;
                var soKhaDung = await khaDung.TinhKhaDungAsync(nguon.MaPhong, dong.MaLoai, ct);
                var lyDoKhaDung = KiemTraDieuKienGiao.KiemTraKhaDung(tenLoai, soKhaDung, dong.SoLuong);
                if (lyDoKhaDung is not null)
                {
                    throw new LoiNghiepVuException(lyDoKhaDung.MaLoi, lyDoKhaDung.ThongDiep);
                }
            }
        }

        var dsMaLoaiCan = nguon.DsLoaiCan.Select(d => d.MaLoai).ToList();
        var dsDaKiem = new List<(ThietBiGiaoYeuCau YeuCau, ThietBiDto ThietBi, LoaiThietBiDto Loai)>();
        foreach (var yeuCau in dsThietBi.OrderBy(t => t.MaThietBi, StringComparer.Ordinal))
        {
            var thietBi = KiemTraGiaoThietBi.KiemTraThietBi(
                await danhMuc.LayThietBiAsync(yeuCau.MaThietBi, ct),
                yeuCau.MaThietBi,
                nguon.MaPhong,
                dsMaLoaiCan);
            var loai = await danhMuc.LayLoaiAsync(thietBi.MaLoai, ct)
                ?? throw new InvalidOperationException($"Thiếu loại {thietBi.MaLoai} của thiết bị {thietBi.MaThietBi}.");
            dsDaKiem.Add((yeuCau, thietBi, loai));
        }

        KiemTraGiaoThietBi.KiemTraKhopSoLuong(dsDaKiem.Select(d => d.ThietBi.MaLoai), nguon.DsLoaiCan);

        var maPhieu = await ChenPhieuAsync(maNv, nguon, hoSo.MaSv, thoiDiem, hanTra, ct);

        var dsDong = new List<DongGiao>();
        foreach (var (yeuCau, thietBi, loai) in dsDaKiem)
        {
            if (!await thietBiService.DatDangDuocMuonAsync(thietBi.MaThietBi, nguon.MaPhong, thietBi.MaLoai, ct))
            {
                throw new LoiNghiepVuException(
                    MaLoi.ThietBiDaDuocGiao,
                    $"Thiết bị {thietBi.MaThietBi} vừa được giao trong một giao dịch khác.");
            }

            var maChiTiet = Guid.CreateVersion7();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO muon_tra.chi_tiet_phieu_muon
                    (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay)
                VALUES ({maChiTiet}, {maPhieu}, {thietBi.MaThietBi}, {yeuCau.MoTa}, {loai.PhiTreMoiNgay})
                """, ct);

            var dsMaAnh = yeuCau.DsMaAnh ?? [];
            if (dsMaAnh.Count > 0)
            {
                await anhService.GanAsync(
                    dsMaAnh,
                    MaDoiTuongAnh.ChiTietPhieuMuon,
                    maChiTiet.ToString(),
                    MaGiaiDoanAnh.BanDau,
                    ct);
            }

            dsDong.Add(new DongGiao(maChiTiet, thietBi, loai.TenLoai, loai.PhiTreMoiNgay, yeuCau.MoTa, dsMaAnh.Count > 0));
        }

        if (phieuYeuCau is not null)
        {
            ChuyenTrangThai(phieuYeuCau.DanhDauDaNhan);
            await db.SaveChangesAsync(ct);
            await nhatKy.GhiAsync(
                "NHAN_YEU_CAU_MUON",
                "muon_tra.phieu_yeu_cau_muon",
                phieuYeuCau.MaYeuCau,
                new Dictionary<string, object?> { ["TrangThai"] = "DA_DUYET" },
                new Dictionary<string, object?> { ["TrangThai"] = "DA_NHAN", ["MaPhieu"] = maPhieu },
                ct);
        }

        if (dangKy is not null)
        {
            ChuyenTrangThai(dangKy.DanhDauDaNhan);
            await db.SaveChangesAsync(ct);
            await nhatKy.GhiAsync(
                "NHAN_DANG_KY_CHO",
                "muon_tra.dang_ky_cho",
                dangKy.MaDangKy,
                new Dictionary<string, object?> { ["TrangThai"] = "DA_THONG_BAO" },
                new Dictionary<string, object?> { ["TrangThai"] = "DA_NHAN", ["MaPhieu"] = maPhieu },
                ct);
        }

        await nhatKy.GhiAsync(
            "LAP_PHIEU_MUON",
            "muon_tra.phieu_muon",
            maPhieu,
            null,
            new Dictionary<string, object?>
            {
                ["MaSV"] = hoSo.MaSv,
                ["MaNvLap"] = maNv,
                ["Nguon"] = nguon.Loai,
                ["MaYeuCau"] = nguon.MaYeuCau,
                ["MaDangKy"] = nguon.MaDangKy,
                ["NgayGioMuon"] = thoiDiem,
                ["NgayGioHenTra"] = hanTra,
                ["DsThietBi"] = dsDong.Select(d => d.ThietBi.MaThietBi).ToArray()
            },
            ct);

        return new KetQuaGhi(maPhieu, hoSo.MaSv, nguon.MaYeuCau, nguon.MaDangKy, thoiDiem, hanTra, dsDong);
    }

    private async Task<PhieuMuonDayDuDto> DungKetQuaAsync(KetQuaGhi ketQua, CancellationToken ct)
    {
        var dsChiTiet = new List<ChiTietPhieuMuonDayDuDto>();
        foreach (var dong in ketQua.DsDong)
        {
            IReadOnlyList<QuanLyMuonTra.SharedKernel.AnhRefDto> dsAnh = [];
            if (dong.CoAnh)
            {
                dsAnh = await anhService.LayDsAnhKyAsync(
                    MaDoiTuongAnh.ChiTietPhieuMuon,
                    dong.MaChiTiet.ToString(),
                    MaGiaiDoanAnh.BanDau,
                    ct);
            }

            dsChiTiet.Add(new ChiTietPhieuMuonDayDuDto(
                dong.MaChiTiet,
                dong.ThietBi.MaThietBi,
                dong.ThietBi.TenThietBi,
                dong.ThietBi.MaLoai,
                dong.TenLoai,
                dong.MoTa,
                (long)dong.PhiTreMoiNgay,
                null,
                null,
                null,
                null,
                dsAnh,
                []));
        }

        return new PhieuMuonDayDuDto(
            ketQua.MaPhieu,
            ketQua.MaSv,
            ketQua.MaYeuCau,
            ketQua.MaDangKy,
            ketQua.GioGiao,
            ketQua.HanTra,
            0,
            TrangThaiDangMuon,
            dsChiTiet.Count,
            dsChiTiet);
    }

    private async Task<string> ChenPhieuAsync(
        Guid maNv,
        NguonGiao nguon,
        string maSv,
        DateTimeOffset gioGiao,
        DateTimeOffset hanTra,
        CancellationToken ct)
    {
        await using var lenh = db.Database.GetDbConnection().CreateCommand();
        lenh.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        lenh.CommandText = """
            INSERT INTO muon_tra.phieu_muon (ma_sv, ma_nv_lap, ma_yeu_cau, ma_dang_ky, ngay_gio_muon, ngay_gio_hen_tra)
            VALUES (@ma_sv, @ma_nv_lap, @ma_yeu_cau, @ma_dang_ky, @ngay_gio_muon, @ngay_gio_hen_tra)
            RETURNING ma_phieu
            """;
        ThemThamSo(lenh, "ma_sv", maSv);
        ThemThamSo(lenh, "ma_nv_lap", maNv);
        ThemThamSo(lenh, "ma_yeu_cau", (object?)nguon.MaYeuCau ?? DBNull.Value);
        ThemThamSo(lenh, "ma_dang_ky", (object?)nguon.MaDangKy ?? DBNull.Value);
        ThemThamSo(lenh, "ngay_gio_muon", gioGiao);
        ThemThamSo(lenh, "ngay_gio_hen_tra", hanTra);

        return (string)(await lenh.ExecuteScalarAsync(ct))!;
    }

    private static void ThemThamSo(DbCommand lenh, string ten, object giaTri)
    {
        var thamSo = lenh.CreateParameter();
        thamSo.ParameterName = ten;
        thamSo.Value = giaTri;
        lenh.Parameters.Add(thamSo);
    }

    private static void KiemTraNguon(string maNguon, bool dungTrangThai, DateTimeOffset? hanGiuCho, DateTimeOffset thoiDiem)
    {
        var lyDo = KiemTraDieuKienGiao.KiemTraGiuCho(maNguon, dungTrangThai, hanGiuCho, thoiDiem);
        if (lyDo is not null)
        {
            throw new LoiNghiepVuException(lyDo.MaLoi, lyDo.ThongDiep);
        }
    }

    private static void ChuyenTrangThai(Action chuyen)
    {
        try
        {
            chuyen();
        }
        catch (NghiepVuException loi)
        {
            throw DichLoiMienNghiepVu.Chuyen(loi);
        }
    }

    private static PostgresException? LoiTrungKhoa(Exception loi)
    {
        var loiCsdl = loi as PostgresException ?? loi.InnerException as PostgresException;
        return loiCsdl is { SqlState: PostgresErrorCodes.UniqueViolation } ? loiCsdl : null;
    }

    private static LoiNghiepVuException DichLoiTrungKhoa(PostgresException loi)
    {
        return loi.ConstraintName switch
        {
            "ux_ctpm_thiet_bi_dang_muon" => new LoiNghiepVuException(
                MaLoi.ThietBiDaDuocGiao,
                "Thiết bị vừa được giao trong một giao dịch khác."),
            "uq_pm_ma_yeu_cau" or "uq_pm_ma_dang_ky" => new LoiNghiepVuException(
                MaLoi.TrangThaiKhongHopLe,
                "Nguồn giao đã được lập phiếu mượn."),
            _ => new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Danh sách thiết bị giao không hợp lệ.")
        };
    }
}
