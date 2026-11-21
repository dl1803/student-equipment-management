using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Domain;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;
using AnhRefDto = QuanLyMuonTra.SharedKernel.AnhRefDto;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class GiaoThietBiTests
{
    private const string FileDuLieu = "D09-V-du-lieu-thu.sql";
    private const string YcHopLe = "YC-261121-998001";
    private const string YcHetHan = "YC-261121-998002";
    private const string YcChoDuyet = "YC-261121-998003";
    private const string YcDaNhan = "YC-261121-998004";
    private const string YcTheHetHan = "YC-261121-998005";
    private const string YcTamKhoa = "YC-261121-998006";
    private const string YcVuotThoiGian = "YC-261121-998007";
    private const string DkHopLe = "DK-261121-998001";
    private const string DkDangCho = "DK-261121-998002";
    private const string X1 = "D09-TB-X1";
    private const string X2 = "D09-TB-X2";
    private const string X3 = "D09-TB-X3";
    private const string X4 = "D09-TB-X4";
    private const string X5 = "D09-TB-X5";
    private const string X6 = "D09-TB-X6";
    private const string XM = "D09-TB-XM";
    private const string XN = "D09-TB-XN";
    private const string XP2 = "D09-TB-XP2";
    private const string Y1 = "D09-TB-Y1";
    private const string Z1 = "D09-TB-Z1";
    private static readonly Guid Nv1 = Guid.Parse("019c0000-0000-7000-8000-0000000d9a01");
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d9b01");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d9b02");
    private static readonly Guid LoaiX = Guid.Parse("019c0000-0000-7000-8000-0000000d9c01");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 21, 3, 0, 0, TimeSpan.Zero);
    private static readonly CancellationToken Ct = CancellationToken.None;

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    private sealed class QuyenPhongGia(IReadOnlyCollection<Guid> dsPhong) : IQuyenPhongService
    {
        public Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct)
        {
            return Task.FromResult(dsPhong);
        }

        public Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct)
        {
            if (!dsPhong.Contains(maPhong))
            {
                throw new LoiNghiepVuException("PHONG_CHUA_CAP_QUYEN", "Phòng chưa được cấp quyền.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class NhatKyGia : IAuditLogger
    {
        public List<string> ThaoTacDaGhi { get; } = [];

        public Task GhiAsync(
            string thaoTac,
            string doiTuong,
            string? doiTuongId,
            IReadOnlyDictionary<string, object?>? giaTriTruoc,
            IReadOnlyDictionary<string, object?>? giaTriSau,
            CancellationToken ct)
        {
            ThaoTacDaGhi.Add(thaoTac);
            return Task.CompletedTask;
        }
    }

    private sealed class AnhGia : IAnhService
    {
        public static readonly Guid MaAnhTraVe = Guid.Parse("019c0000-0000-7000-8000-0000000d9f01");

        public List<(IReadOnlyCollection<Guid> DsMaAnh, string DoiTuongLoai, string DoiTuongId, string GiaiDoan)> DaGan { get; } = [];

        public Task<Guid> TaiLenAsync(string giaiDoan, Stream noiDung, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        public Task GanAsync(
            IReadOnlyCollection<Guid> dsMaAnh,
            string doiTuongLoai,
            string doiTuongId,
            string giaiDoan,
            CancellationToken ct)
        {
            DaGan.Add((dsMaAnh, doiTuongLoai, doiTuongId, giaiDoan));
            return Task.CompletedTask;
        }

        public Task GoAnhThietBiAsync(string maThietBi, IReadOnlyCollection<Guid> dsMaAnh, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<AnhRefDto>> LayDsAnhKyAsync(
            string doiTuongLoai,
            string doiTuongId,
            string giaiDoan,
            CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyList<AnhRefDto>>([new AnhRefDto(MaAnhTraVe, "https://anh.test/ban-dau")]);
        }

        public Task<IReadOnlyList<AnhRefDto>> LayDsAnhCongKhaiAsync(string doiTuongLoai, string doiTuongId, CancellationToken ct)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class BoXuLy(
        YeuCauMuonSvTests.YeuCauMuonTestContext db,
        IReadOnlyCollection<Guid> phongDuocCap,
        DateTimeOffset? thoiDiem = null)
    {
        private readonly DongHoCoDinh _dongHo = new(thoiDiem ?? BayGio);

        public NhatKyGia NhatKy { get; } = new();

        public AnhGia Anh { get; } = new();

        public GiaoThietBiHandler Giao => new(
            db,
            _dongHo,
            new DanhMucReader(db),
            new QuyDinhReader(db),
            new MuonTraReader(db),
            new HoSoSinhVienReader(db),
            new AvailabilityService(db, _dongHo),
            new AdvisoryLockService(db),
            new BoTinhHanTra(),
            new ThietBiService(db, NhatKy),
            Anh,
            new QuyenPhongGia(phongDuocCap),
            NhatKy);
    }

    private static async Task<YeuCauMuonSvTests.YeuCauMuonTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync(FileDuLieu);
        var options = new DbContextOptionsBuilder<YeuCauMuonSvTests.YeuCauMuonTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new YeuCauMuonSvTests.YeuCauMuonTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, Ct);
        return db;
    }

    private static IReadOnlyList<ThietBiGiaoYeuCau> Ds(params string[] dsMa)
    {
        return dsMa.Select(ma => new ThietBiGiaoYeuCau(ma, "Tình trạng tốt", null)).ToList();
    }

    private static Task<string> TinhTrangThietBiAsync(DbContext db, string maThietBi)
    {
        return db.Database.SqlQuery<string>($"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {maThietBi}
            """).SingleAsync();
    }

    private static Task<string> TrangThaiYeuCauAsync(DbContext db, string maYeuCau)
    {
        return db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau = {maYeuCau}
            """).SingleAsync();
    }

    private static Task<string> TrangThaiDangKyAsync(DbContext db, string maDangKy)
    {
        return db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.dang_ky_cho WHERE ma_dang_ky = {maDangKy}
            """).SingleAsync();
    }

    private static Task<int> DemPhieuAsync(DbContext db, string maSv)
    {
        return db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_muon WHERE ma_sv = {maSv}
            """).SingleAsync();
    }

    private static Task<long> PhiLuuAsync(DbContext db, string maPhieu, string maThietBi)
    {
        return db.Database.SqlQuery<long>($"""
            SELECT phi_tre_moi_ngay::bigint AS "Value" FROM muon_tra.chi_tiet_phieu_muon
            WHERE ma_phieu = {maPhieu} AND ma_thiet_bi = {maThietBi}
            """).SingleAsync();
    }

    private static Task<string> TinhTrangBanDauAsync(DbContext db, Guid maChiTiet)
    {
        return db.Database.SqlQuery<string>($"""
            SELECT tinh_trang_ban_dau AS "Value" FROM muon_tra.chi_tiet_phieu_muon WHERE ma_chi_tiet = {maChiTiet}
            """).SingleAsync();
    }

    private static async Task DatGioQuayAsync(GiaoDichTestRollback gd, string gioMoCua, string gioDongCua)
    {
        var moCua = gioMoCua == "NULL" ? "NULL" : $"TIME '{gioMoCua}'";
        var dongCua = gioDongCua == "NULL" ? "NULL" : $"TIME '{gioDongCua}'";
        await using var lenh = new NpgsqlCommand(
            $"UPDATE quy_dinh.tham_so_he_thong SET gio_mo_cua = {moCua}, gio_dong_cua = {dongCua} WHERE id = 1",
            gd.KetNoi,
            gd.GiaoDich);
        await lenh.ExecuteNonQueryAsync();
    }

    private static async Task<LoiNghiepVuException> LoiAsync(Task tacVu)
    {
        return await Assert.ThrowsAsync<LoiNghiepVuException>(() => tacVu);
    }

    [CanKetNoiTestFact]
    public async Task UC07_PhieuYeuCau_HopLe_LapPhieuVaTieuThuGiuCho()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var phieu = await bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcHopLe, Ds(X1, X2, Y1), Ct);

        Assert.Matches(@"^PM-\d{6}-\d{6,}$", phieu.MaPhieu);
        Assert.Equal("D09-SV-A", phieu.MaSV);
        Assert.Equal(YcHopLe, phieu.MaYeuCau);
        Assert.Null(phieu.MaDangKy);
        Assert.Equal("DANG_MUON", phieu.TrangThai);
        Assert.Equal(0, phieu.SoLanGiaHan);
        Assert.Equal(3, phieu.SoThietBi);
        Assert.Equal(BayGio, phieu.NgayGioMuon);
        Assert.Equal(BayGio.AddHours(2), phieu.NgayGioHenTra);
        Assert.Equal("DA_NHAN", await TrangThaiYeuCauAsync(db, YcHopLe));
        Assert.All([X1, X2, Y1], ma => Assert.Equal("DANG_DUOC_MUON", TinhTrangThietBiAsync(db, ma).Result));
        Assert.Equal(50000, await PhiLuuAsync(db, phieu.MaPhieu, X1));
        Assert.Equal(30000, await PhiLuuAsync(db, phieu.MaPhieu, Y1));
        Assert.Equal(1, await DemPhieuAsync(db, "D09-SV-A"));
        Assert.Contains("LAP_PHIEU_MUON", bo.NhatKy.ThaoTacDaGhi);
        Assert.Contains("NHAN_YEU_CAU_MUON", bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_PhieuYeuCau_ThoiGianDuKienVuotToiDaNhom_BiCatTheoToiDa()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var phieu = await bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcVuotThoiGian, Ds(Z1), Ct);

        Assert.Equal(BayGio.AddHours(5), phieu.NgayGioHenTra);
    }

    [CanKetNoiTestFact]
    public async Task UC07_DangKyCho_HopLe_DungToiDaNhomVaChuyenDaNhan()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var phieu = await bo.Giao.GiaoTheoDangKyChoAsync(Nv1, DkHopLe, Ds(X3), Ct);

        Assert.Equal("D09-SV-B", phieu.MaSV);
        Assert.Equal(DkHopLe, phieu.MaDangKy);
        Assert.Null(phieu.MaYeuCau);
        Assert.Equal(BayGio.AddHours(5), phieu.NgayGioHenTra);
        Assert.Equal("DA_NHAN", await TrangThaiDangKyAsync(db, DkHopLe));
        Assert.Equal("DANG_DUOC_MUON", await TinhTrangThietBiAsync(db, X3));
        Assert.Contains("NHAN_DANG_KY_CHO", bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_DangKyCho_ChuaDenLuot_Tra409TrangThai()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var loi = await LoiAsync(bo.Giao.GiaoTheoDangKyChoAsync(Nv1, DkDangCho, Ds(X3), Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X3));
    }

    [CanKetNoiTestFact]
    public async Task UC07_TrucTiep_HopLe_LapPhieuKhongNguon()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var phieu = await bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-C",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 3)],
            Ds(X4, X5, X6),
            Ct);

        Assert.Equal("D09-SV-C", phieu.MaSV);
        Assert.Null(phieu.MaYeuCau);
        Assert.Null(phieu.MaDangKy);
        Assert.Equal(3, phieu.SoThietBi);
        Assert.Equal(BayGio.AddHours(5), phieu.NgayGioHenTra);
        Assert.Equal("DANG_DUOC_MUON", await TinhTrangThietBiAsync(db, X6));
        Assert.Contains("LAP_PHIEU_MUON", bo.NhatKy.ThaoTacDaGhi);
        Assert.DoesNotContain("NHAN_YEU_CAU_MUON", bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_TrucTiep_ThieuKhaDung_Tra409KhongDoiThietBi()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var loi = await LoiAsync(bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-C",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 4)],
            Ds(X1, X2, X3, X4),
            Ct));

        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X1));
        Assert.Equal(0, await DemPhieuAsync(db, "D09-SV-C"));
    }

    [CanKetNoiTestFact]
    public async Task UC07_TrucTiep_VuotGioiHanTong_Tra422()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var loi = await LoiAsync(bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-C",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 5)],
            Ds(X1, X2, X3, X4, X5),
            Ct));

        Assert.Equal("VUOT_GIOI_HAN_THIET_BI", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC07_TrucTiep_SinhVienKhongTonTai_Tra404()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var loi = await LoiAsync(bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-KHONG-CO",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 1)],
            Ds(X1),
            Ct));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_GiuChoHetHanHoacSaiTrangThai_Tra409()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var hetHan = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcHetHan, Ds(Z1), Ct));
        var choDuyet = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcChoDuyet, Ds(Z1), Ct));
        var daNhan = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcDaNhan, Ds(Z1), Ct));

        Assert.Equal("GIU_CHO_HET_HAN", hetHan.MaLoi);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", choDuyet.MaLoi);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", daNhan.MaLoi);
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, Z1));
    }

    [CanKetNoiTestFact]
    public async Task UC07_NguonKhongTonTai_Tra404()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var yeuCau = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, "YC-261121-999999", Ds(X1), Ct));
        var dangKy = await LoiAsync(bo.Giao.GiaoTheoDangKyChoAsync(Nv1, "DK-261121-999999", Ds(X1), Ct));

        Assert.Equal("KHONG_TIM_THAY", yeuCau.MaLoi);
        Assert.Equal("KHONG_TIM_THAY", dangKy.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_DieuKienSinhVienKhongDat_Tra422()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var theHetHan = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcTheHetHan, Ds(Z1), Ct));
        var tamKhoa = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcTamKhoa, Ds(Z1), Ct));

        Assert.Equal("SV_THE_HET_HAN", theHetHan.MaLoi);
        Assert.Equal("SV_BI_KHOA_QUYEN_MUON", tamKhoa.MaLoi);
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, Z1));
    }

    [CanKetNoiTestFact]
    public async Task UC07_DanhSachThietBiKhongDat_TraDungMaVaKhongDoiDuLieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);
        var cacCa = new (string[] DsMa, string MaLoiDuKien)[]
        {
            ([X1, Y1], "DU_LIEU_KHONG_HOP_LE"),
            ([X1, X2, X3, Y1], "DU_LIEU_KHONG_HOP_LE"),
            ([X1, X1, Y1], "DU_LIEU_KHONG_HOP_LE"),
            ([X1, X2, Z1], "THIET_BI_SAI_LOAI"),
            ([X1, XP2, Y1], "THIET_BI_SAI_PHONG"),
            ([X1, XM, Y1], "THIET_BI_KHONG_SAN_SANG"),
            ([X1, XN, Y1], "THIET_BI_KHONG_SAN_SANG"),
            ([X1, "D09-TB-KHONG-CO", Y1], "KHONG_TIM_THAY")
        };

        foreach (var (dsMa, maLoiDuKien) in cacCa)
        {
            var loi = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcHopLe, Ds(dsMa), Ct));
            Assert.Equal(maLoiDuKien, loi.MaLoi);
        }

        Assert.Equal("DA_DUYET", await TrangThaiYeuCauAsync(db, YcHopLe));
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X1));
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X2));
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, Y1));
        Assert.Equal(0, await DemPhieuAsync(db, "D09-SV-A"));
        Assert.DoesNotContain("LAP_PHIEU_MUON", bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC07_NhanVienKhongCoQuyenPhong_Tra403KhongDoiDuLieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P2]);

        var theoYeuCau = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcHopLe, Ds(X1, X2, Y1), Ct));
        var trucTiep = await LoiAsync(bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-C",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 1)],
            Ds(X1),
            Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", theoYeuCau.MaLoi);
        Assert.Equal("PHONG_CHUA_CAP_QUYEN", trucTiep.MaLoi);
        Assert.Equal("DA_DUYET", await TrangThaiYeuCauAsync(db, YcHopLe));
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X1));
    }

    [CanKetNoiTestFact]
    public async Task UC07_ChuaCauHinhGioQuay_Tra409()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        await DatGioQuayAsync(gd, "NULL", "NULL");
        var bo = new BoXuLy(db, [P1]);

        var loi = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcHopLe, Ds(X1, X2, Y1), Ct));

        Assert.Equal("CHUA_CAU_HINH_GIO_QUAY", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X1));
    }

    [CanKetNoiTestFact]
    public async Task UC07_GiaoTuGioDongCua_Tra422()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1], new DateTimeOffset(2026, 11, 21, 10, 0, 0, TimeSpan.Zero));

        var loi = await LoiAsync(bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-C",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 1)],
            Ds(X1),
            Ct));

        Assert.Equal("GIAO_SAU_GIO_DONG_CUA", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X1));
    }

    [CanKetNoiTestFact]
    public async Task UC07_GanGioDongCua_HanTraBiCatTaiGioDongCua()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1], new DateTimeOffset(2026, 11, 21, 8, 30, 0, TimeSpan.Zero));

        var phieu = await bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-C",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 1)],
            Ds(X1),
            Ct);

        Assert.Equal(new DateTimeOffset(2026, 11, 21, 10, 0, 0, TimeSpan.Zero), phieu.NgayGioHenTra);
    }

    [CanKetNoiTestFact]
    public async Task UC07_GiaoLaiCungNguon_Tra409VaChiCoMotPhieu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);
        await bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcHopLe, Ds(X1, X2, Y1), Ct);

        var loi = await LoiAsync(bo.Giao.GiaoTheoPhieuYeuCauAsync(Nv1, YcHopLe, Ds(X3, X4, Y1), Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(1, await DemPhieuAsync(db, "D09-SV-A"));
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(db, X3));
    }

    [CanKetNoiTestFact]
    public async Task UC07_CoAnh_GanAnhBanDauTheoChiTietVaTraUrlKy()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);
        var maAnh = Guid.Parse("019c0000-0000-7000-8000-0000000d9f02");
        IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi =
        [
            new ThietBiGiaoYeuCau(X1, "Xước nhẹ vỏ máy", [maAnh]),
            new ThietBiGiaoYeuCau(X2, "Tốt", null)
        ];

        var phieu = await bo.Giao.GiaoTrucTiepAsync(
            Nv1,
            "D09-SV-C",
            P1,
            [new LoaiVaSoLuongYeuCau(LoaiX, 2)],
            dsThietBi,
            Ct);

        var chiTietX1 = phieu.DsChiTiet.Single(c => c.MaThietBi == X1);
        var chiTietX2 = phieu.DsChiTiet.Single(c => c.MaThietBi == X2);
        var lanGan = Assert.Single(bo.Anh.DaGan);
        Assert.Equal([maAnh], lanGan.DsMaAnh);
        Assert.Equal("CHI_TIET_PHIEU_MUON", lanGan.DoiTuongLoai);
        Assert.Equal(chiTietX1.MaChiTiet.ToString(), lanGan.DoiTuongId);
        Assert.Equal("BAN_DAU", lanGan.GiaiDoan);
        Assert.Equal("Xước nhẹ vỏ máy", await TinhTrangBanDauAsync(db, chiTietX1.MaChiTiet));
        Assert.Single(chiTietX1.DsAnhBanDau);
        Assert.Empty(chiTietX2.DsAnhBanDau);
        Assert.Empty(chiTietX1.DsAnhKhiTra);
    }
}
