using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

internal sealed class DongKhoanDaLap
{
    public string MaThietBi { get; set; } = string.Empty;

    public string LoaiViPham { get; set; } = string.Empty;
}

public sealed class LapBienBanHandler(
    DbContext db,
    IClock dongHo,
    IPhieuMuonReader phieuMuon,
    IDanhMucReader danhMuc,
    IQuyDinhReader quyDinh,
    IQuyenPhongService quyenPhong,
    IQuyenMuonService quyenMuon,
    IThongBaoOutbox thongBao,
    IAuditLogger nhatKy,
    IPortGhiNhanMat? portGhiNhanMat = null)
{
    private const string KetQuaHuHong = "HU_HONG";
    private const string KetQuaThieuPhuKien = "THIEU_PHU_KIEN";
    private const string KetQuaMat = "MAT";
    private const string TrangThaiChuaNop = "CHUA_NOP";
    private const string DoiTuongLoaiBienBan = "BIEN_BAN_VI_PHAM";

    private sealed record KhoanDaTinh(
        ChiTietPhieuMuonDto ChiTiet,
        string LoaiViPham,
        int? SoNgayTre,
        long SoTien,
        Guid? MaKhung,
        bool CanGhiNhanMat);

    public async Task<BienBanViPhamDto> XuLyAsync(Guid maNv, LapBienBanLenh lenh, CancellationToken ct)
    {
        KiemTraDocViPham.KiemTraMaPhieu(lenh.MaPhieuMuon);
        KiemTraDauVao(lenh.DsKhoan);

        var phieu = await phieuMuon.LayPhieuMuonAsync(lenh.MaPhieuMuon, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu mượn.");

        var dsChiTietBanDau = await phieuMuon.LayChiTietTheoPhieuAsync(phieu.MaPhieu, ct);
        var dsMaPhongBanDau = await quyenPhong.LayDsMaPhongAsync(maNv, ct);
        var coQuyenPhieu = false;
        foreach (var chiTietBanDau in dsChiTietBanDau)
        {
            var tb = await danhMuc.LayThietBiAsync(chiTietBanDau.MaThietBi, ct)
                ?? throw new InvalidOperationException($"Thiếu thiết bị {chiTietBanDau.MaThietBi} của phiếu {phieu.MaPhieu}.");
            if (dsMaPhongBanDau.Contains(tb.MaPhong))
            {
                coQuyenPhieu = true;
                break;
            }
        }

        if (!coQuyenPhieu)
        {
            throw new LoiNghiepVuException(
                MaLoi.PhongChuaCapQuyen,
                "Nhân viên chưa được cấp quyền phòng của phiếu mượn này.");
        }

        var moGiaoDich = db.Database.CurrentTransaction is null;
        var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var ketQua = await GhiAsync(maNv, phieu.MaSv, lenh, ct);

            if (giaoDich is not null)
            {
                await giaoDich.CommitAsync(ct);
            }

            return ketQua;
        }
        catch (Exception loi) when (LaLoiTrungKhoa(loi))
        {
            throw TrangThaiKhongHopLe("Khoản vi phạm đã tồn tại trong biên bản chưa hủy.");
        }
        finally
        {
            if (giaoDich is not null)
            {
                await giaoDich.DisposeAsync();
            }
        }
    }

    private static void KiemTraDauVao(IReadOnlyList<KhoanLapBienBanYeuCau> dsKhoan)
    {
        if (dsKhoan.Count == 0
            || dsKhoan.Select(k => (k.MaThietBi, k.LoaiViPham)).Distinct().Count() != dsKhoan.Count)
        {
            throw new LoiNghiepVuException(
                MaLoi.DuLieuKhongHopLe,
                "Danh sách khoản trống hoặc có khoản trùng thiết bị và loại vi phạm.");
        }

        if (dsKhoan.Any(k => k.LoaiViPham != MaLoaiViPham.HuHong && k.MaKhungPhat is not null))
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Chỉ khoản hư hỏng mới có khung phạt.");
        }
    }

    private async Task<BienBanViPhamDto> GhiAsync(
        Guid maNv,
        string maSv,
        LapBienBanLenh lenh,
        CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            SELECT ma_sv FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv} FOR UPDATE
            """, ct);
        var hocKy = await db.Database.SqlQuery<string>($"""
                SELECT hoc_ky AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv}
                """)
            .SingleOrDefaultAsync(ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy sinh viên.");

        var phieu = await phieuMuon.LayPhieuMuonAsync(lenh.MaPhieuMuon, ct)
            ?? throw new LoiNghiepVuException(MaLoi.KhongTimThay, "Không tìm thấy phiếu mượn.");
        var dsChiTiet = await phieuMuon.LayChiTietTheoPhieuAsync(phieu.MaPhieu, ct);
        var dsMaPhong = await quyenPhong.LayDsMaPhongAsync(maNv, ct);
        var thamSo = await quyDinh.LayThamSoAsync(ct);
        var bayGio = dongHo.UtcNow.ToUniversalTime();

        var dsDaLap = await db.Database.SqlQuery<DongKhoanDaLap>($"""
                SELECT ma_thiet_bi AS "MaThietBi", loai_vi_pham AS "LoaiViPham"
                FROM vi_pham.chi_tiet_bien_ban
                WHERE ma_phieu_muon = {phieu.MaPhieu} AND NOT bien_ban_da_huy
                """)
            .ToListAsync(ct);

        var dsKhoan = new List<KhoanDaTinh>();
        foreach (var yeuCau in lenh.DsKhoan
                     .OrderBy(k => k.MaThietBi, StringComparer.Ordinal)
                     .ThenBy(k => k.LoaiViPham, StringComparer.Ordinal))
        {
            var chiTiet = dsChiTiet.FirstOrDefault(c => c.MaThietBi == yeuCau.MaThietBi)
                ?? throw new LoiNghiepVuException(
                    MaLoi.ThietBiKhongThuocPhieu,
                    $"Thiết bị {yeuCau.MaThietBi} không thuộc phiếu mượn {phieu.MaPhieu}.");
            var thietBi = await danhMuc.LayThietBiAsync(yeuCau.MaThietBi, ct)
                ?? throw new InvalidOperationException($"Thiếu thiết bị {yeuCau.MaThietBi} của phiếu {phieu.MaPhieu}.");

            if (!dsMaPhong.Contains(thietBi.MaPhong))
            {
                throw new LoiNghiepVuException(
                    MaLoi.PhongChuaCapQuyen,
                    "Nhân viên chưa được cấp quyền phòng của thiết bị này.");
            }

            if (dsDaLap.Any(d => d.MaThietBi == yeuCau.MaThietBi && d.LoaiViPham == yeuCau.LoaiViPham))
            {
                throw TrangThaiKhongHopLe(
                    $"Khoản {yeuCau.LoaiViPham} của thiết bị {yeuCau.MaThietBi} đã có trong biên bản chưa hủy.");
            }

            dsKhoan.Add(await TinhKhoanAsync(yeuCau, chiTiet, thietBi, phieu, thamSo.AnHanPhut, bayGio, ct));
        }

        foreach (var khoan in dsKhoan.Where(k => k.CanGhiNhanMat))
        {
            if (portGhiNhanMat is null)
            {
                throw new InvalidOperationException(
                    "Chưa có bản hiện thực IPortGhiNhanMat nên không thể ghi nhận mất thiết bị.");
            }

            if (!await portGhiNhanMat.GhiNhanMatAsync(khoan.ChiTiet.MaChiTiet, maNv, ct))
            {
                throw TrangThaiKhongHopLe($"Không thể ghi nhận mất thiết bị {khoan.ChiTiet.MaThietBi}.");
            }
        }

        var tongTien = dsKhoan.Sum(k => k.SoTien);
        var maBienBan = await ChenBienBanAsync(phieu.MaPhieu, maNv, bayGio, tongTien, ct);

        foreach (var khoan in dsKhoan)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO vi_pham.chi_tiet_bien_ban
                    (ma_bien_ban, ma_phieu_muon, ma_thiet_bi, loai_vi_pham, so_ngay_tre, so_tien, ma_khung)
                VALUES
                    ({maBienBan}, {phieu.MaPhieu}, {khoan.ChiTiet.MaThietBi}, {khoan.LoaiViPham},
                     {khoan.SoNgayTre}::integer, {khoan.SoTien}, {khoan.MaKhung}::uuid)
                """, ct);
        }

        foreach (var loai in dsKhoan.Select(k => k.LoaiViPham).Distinct())
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO vi_pham.vi_pham (ma_vi_pham, ma_sv, loai_vi_pham, thoi_diem_ghi_nhan, hoc_ky, ma_bien_ban)
                VALUES ({Guid.CreateVersion7()}, {maSv}, {loai}, {bayGio}, {hocKy}, {maBienBan})
                """, ct);
        }

        await quyenMuon.KhoaAsync(maSv, ct);

        await thongBao.GhiAsync(
            new YeuCauGhiThongBao(
                MaLoaiThongBao.ViPhamPhiPhat,
                maSv,
                null,
                new Dictionary<string, string>
                {
                    ["maBienBan"] = maBienBan,
                    ["tongTien"] = DinhDangTien(tongTien)
                },
                DoiTuongLoaiBienBan,
                maBienBan,
                $"{MaLoaiThongBao.ViPhamPhiPhat}:{maBienBan}"),
            ct);

        await nhatKy.GhiAsync(
            "LAP_BIEN_BAN_VI_PHAM",
            "vi_pham.bien_ban_vi_pham",
            maBienBan,
            null,
            new Dictionary<string, object?>
            {
                ["MaPhieuMuon"] = phieu.MaPhieu,
                ["MaSV"] = maSv,
                ["MaNvLap"] = maNv,
                ["TrangThai"] = TrangThaiChuaNop,
                ["TongTien"] = tongTien,
                ["DsKhoan"] = dsKhoan.Select(k => $"{k.ChiTiet.MaThietBi}|{k.LoaiViPham}|{k.SoTien}").ToArray(),
                ["DsThietBiGhiNhanMat"] = dsKhoan
                    .Where(k => k.CanGhiNhanMat)
                    .Select(k => k.ChiTiet.MaThietBi)
                    .ToArray()
            },
            ct);

        return new BienBanViPhamDto(
            maBienBan,
            phieu.MaPhieu,
            maSv,
            bayGio,
            tongTien,
            TrangThaiChuaNop,
            null,
            dsKhoan
                .Select(k => new ChiTietBienBanDto(k.ChiTiet.MaThietBi, k.LoaiViPham, k.SoNgayTre, k.SoTien, k.MaKhung))
                .ToList());
    }

    private async Task<KhoanDaTinh> TinhKhoanAsync(
        KhoanLapBienBanYeuCau yeuCau,
        ChiTietPhieuMuonDto chiTiet,
        ThietBiDto thietBi,
        PhieuMuonDto phieu,
        int anHanPhut,
        DateTimeOffset bayGio,
        CancellationToken ct)
    {
        switch (yeuCau.LoaiViPham)
        {
            case MaLoaiViPham.TraTre:
                return TinhTraTre(yeuCau, chiTiet, thietBi, phieu, anHanPhut);
            case MaLoaiViPham.HuHong:
                return await TinhHuHongAsync(yeuCau, chiTiet, thietBi, ct);
            case MaLoaiViPham.Mat:
                return TinhMat(yeuCau, chiTiet, thietBi, phieu, bayGio);
            default:
                throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Loại vi phạm không hợp lệ.");
        }
    }

    private static KhoanDaTinh TinhTraTre(
        KhoanLapBienBanYeuCau yeuCau,
        ChiTietPhieuMuonDto chiTiet,
        ThietBiDto thietBi,
        PhieuMuonDto phieu,
        int anHanPhut)
    {
        if (chiTiet.KetQuaKiemTra is null || chiTiet.NgayGioTraThucTe is not { } ngayTra)
        {
            throw TrangThaiKhongHopLe($"Thiết bị {yeuCau.MaThietBi} chưa có kết quả trả nên chưa lập được khoản trả trễ.");
        }

        var soNgayTre = TinhPhi.SoNgayTre(phieu.NgayGioHenTra, ngayTra, anHanPhut);
        if (soNgayTre <= 0)
        {
            throw TrangThaiKhongHopLe($"Thiết bị {yeuCau.MaThietBi} không trả trễ.");
        }

        var phiTre = TinhPhi.PhiTre(soNgayTre, chiTiet.PhiTreMoiNgay, thietBi.GiaTriHienHanh);

        return new KhoanDaTinh(chiTiet, MaLoaiViPham.TraTre, soNgayTre, (long)phiTre, null, false);
    }

    private async Task<KhoanDaTinh> TinhHuHongAsync(
        KhoanLapBienBanYeuCau yeuCau,
        ChiTietPhieuMuonDto chiTiet,
        ThietBiDto thietBi,
        CancellationToken ct)
    {
        if (chiTiet.KetQuaKiemTra is not (KetQuaHuHong or KetQuaThieuPhuKien))
        {
            throw TrangThaiKhongHopLe($"Thiết bị {yeuCau.MaThietBi} không có kết quả hư hỏng hoặc thiếu phụ kiện.");
        }

        if (yeuCau.MaKhungPhat is not { } maKhung)
        {
            throw new LoiNghiepVuException(
                MaLoi.ThieuMoTaHoacKhungPhat,
                "Khoản hư hỏng hoặc thiếu phụ kiện phải có khung phạt.");
        }

        var khung = await quyDinh.LayKhungPhatAsync(maKhung, ct)
            ?? throw new LoiNghiepVuException(MaLoi.ThieuMoTaHoacKhungPhat, "Khung phạt không tồn tại.");
        var boiThuong = TinhPhi.BoiThuongHuHong(thietBi.GiaTriHienHanh, khung.TyLePhanTram);

        return new KhoanDaTinh(chiTiet, MaLoaiViPham.HuHong, null, (long)boiThuong, maKhung, false);
    }

    private static KhoanDaTinh TinhMat(
        KhoanLapBienBanYeuCau yeuCau,
        ChiTietPhieuMuonDto chiTiet,
        ThietBiDto thietBi,
        PhieuMuonDto phieu,
        DateTimeOffset bayGio)
    {
        bool canGhiNhanMat;
        if (chiTiet.KetQuaKiemTra == KetQuaMat)
        {
            canGhiNhanMat = false;
        }
        else if (chiTiet.KetQuaKiemTra is null && TinhPhi.QuaMocBaoMat(phieu.NgayGioHenTra, bayGio))
        {
            canGhiNhanMat = true;
        }
        else
        {
            throw TrangThaiKhongHopLe(
                $"Thiết bị {yeuCau.MaThietBi} chưa đủ điều kiện lập biên bản mất (chưa trả quá {TinhPhi.MocBaoMatSoNgay} ngày lịch kể từ hạn trả).");
        }

        var boiThuong = TinhPhi.BoiThuongMat(thietBi.GiaTriHienHanh);

        return new KhoanDaTinh(chiTiet, MaLoaiViPham.Mat, null, (long)boiThuong, null, canGhiNhanMat);
    }

    private async Task<string> ChenBienBanAsync(
        string maPhieu,
        Guid maNv,
        DateTimeOffset ngayLap,
        long tongTien,
        CancellationToken ct)
    {
        await using var lenh = db.Database.GetDbConnection().CreateCommand();
        lenh.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        lenh.CommandText = """
            INSERT INTO vi_pham.bien_ban_vi_pham (ma_phieu_muon, ma_nv_lap, ngay_lap, tong_tien)
            VALUES (@ma_phieu_muon, @ma_nv_lap, @ngay_lap, @tong_tien)
            RETURNING ma_bien_ban
            """;
        ThemThamSo(lenh, "ma_phieu_muon", maPhieu);
        ThemThamSo(lenh, "ma_nv_lap", maNv);
        ThemThamSo(lenh, "ngay_lap", ngayLap);
        ThemThamSo(lenh, "tong_tien", (decimal)tongTien);

        return (string)(await lenh.ExecuteScalarAsync(ct))!;
    }

    private static void ThemThamSo(DbCommand lenh, string ten, object giaTri)
    {
        var thamSo = lenh.CreateParameter();
        thamSo.ParameterName = ten;
        thamSo.Value = giaTri;
        lenh.Parameters.Add(thamSo);
    }

    private static string DinhDangTien(long soTien)
    {
        return soTien.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.') + " đồng";
    }

    private static bool LaLoiTrungKhoa(Exception loi)
    {
        return loi is DbException { SqlState: "23505" }
            || loi.InnerException is DbException { SqlState: "23505" };
    }

    private static LoiNghiepVuException TrangThaiKhongHopLe(string thongDiep)
    {
        return new LoiNghiepVuException(MaLoi.TrangThaiKhongHopLe, thongDiep);
    }
}

