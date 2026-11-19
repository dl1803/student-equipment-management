using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DuyetTuChoiYeuCauTests
{
    private const string FileDuLieu = "D07-V-du-lieu-thu.sql";
    private const string YcHopLe = "YC-261118-997001";
    private const string YcTonKho = "YC-261118-997002";
    private const string YcSuatCuoiA = "YC-261118-997003";
    private const string YcSuatCuoiB = "YC-261118-997004";
    private const string YcTheHetHan = "YC-261118-997005";
    private const string YcTamKhoa = "YC-261118-997006";
    private const string YcVuotGioiHan = "YC-261118-997007";
    private const string YcVuotThoiGian = "YC-261118-997008";
    private const string YcPhongKhac = "YC-261118-997009";
    private const string YcDaDuyet = "YC-261118-997010";
    private static readonly Guid Nv1 = Guid.Parse("019c0000-0000-7000-8000-0000000d7a01");
    private static readonly Guid Nv2 = Guid.Parse("019c0000-0000-7000-8000-0000000d7a02");
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d7b01");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d7b02");
    private static readonly Guid X = Guid.Parse("019c0000-0000-7000-8000-0000000d7c01");
    private static readonly Guid Y = Guid.Parse("019c0000-0000-7000-8000-0000000d7c02");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 18, 3, 0, 0, TimeSpan.Zero);
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

    private sealed class ThongBaoGia : IThongBaoDuyetOutbox
    {
        public List<ThongBaoDuyetHoacTuChoi> DaGhi { get; } = [];

        public Task GhiAsync(ThongBaoDuyetHoacTuChoi thongBao, CancellationToken ct)
        {
            DaGhi.Add(thongBao);
            return Task.CompletedTask;
        }
    }

    private sealed class BoXuLy(
        YeuCauMuonSvTests.YeuCauMuonTestContext db,
        IReadOnlyCollection<Guid> phongDuocCap,
        DateTimeOffset? thoiDiem = null)
    {
        private readonly DongHoCoDinh _dongHo = new(thoiDiem ?? BayGio);

        public QuyenPhongGia Quyen { get; } = new(phongDuocCap);

        public NhatKyGia NhatKy { get; } = new();

        public ThongBaoGia ThongBao { get; } = new();

        public DsPhieuYeuCauMuonQuayHandler Ds => new(db, new DanhMucReader(db), Quyen);

        public KiemTraPhieuYeuCauMuonHandler KiemTra => new(
            db,
            _dongHo,
            new DanhMucReader(db),
            new QuyDinhReader(db),
            new MuonTraReader(db),
            new AvailabilityService(db, _dongHo),
            Quyen);

        public DuyetPhieuYeuCauMuonHandler Duyet => new(
            db,
            _dongHo,
            new DanhMucReader(db),
            new QuyDinhReader(db),
            new MuonTraReader(db),
            new AvailabilityService(db, _dongHo),
            new AdvisoryLockService(db),
            Quyen,
            NhatKy,
            ThongBao);

        public TuChoiPhieuYeuCauMuonHandler TuChoi => new(db, new DanhMucReader(db), Quyen, NhatKy, ThongBao);
    }

    private static async Task<YeuCauMuonSvTests.YeuCauMuonTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync(FileDuLieu);
        var options = new DbContextOptionsBuilder<YeuCauMuonSvTests.YeuCauMuonTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new YeuCauMuonSvTests.YeuCauMuonTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, Ct);
        return db;
    }

    private static Task<string> TrangThaiAsync(DbContext db, string maYeuCau)
    {
        return db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau = {maYeuCau}
            """).SingleAsync();
    }

    private static Task<int> DemHanGiuChoAsync(DbContext db, string maYeuCau)
    {
        return db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_yeu_cau_muon
            WHERE ma_yeu_cau = {maYeuCau} AND han_giu_cho IS NOT NULL
            """).SingleAsync();
    }

    private static Task<int> DemViPhamAsync(DbContext db)
    {
        return db.Database.SqlQuery<int>($"""
            SELECT ((SELECT count(*) FROM vi_pham.vi_pham) + (SELECT count(*) FROM vi_pham.bien_ban_vi_pham))::integer AS "Value"
            """).SingleAsync();
    }

    private static async Task DatThoiGianChoKhoaAsync(GiaoDichTestRollback gd, string thoiGian)
    {
        await using var lenh = new NpgsqlCommand($"SET LOCAL lock_timeout = '{thoiGian}'", gd.KetNoi, gd.GiaoDich);
        await lenh.ExecuteNonQueryAsync();
    }

    [CanKetNoiTestFact]
    public async Task UC06_DsQuay_ChiPhongDuocCap_SapXepGuiTangDanKemThongTinSv()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var ketQua = await bo.Ds.XuLyAsync(Nv1, "CHO_DUYET", null, 1, 20, Ct);

        Assert.Equal(8, ketQua.TongSo);
        Assert.Equal(YcHopLe, ketQua.DanhSach[0].MaYeuCau);
        Assert.All(ketQua.DanhSach, p => Assert.Equal(P1, p.MaPhong));
        Assert.Equal(
            ketQua.DanhSach.OrderBy(p => p.ThoiGianGui).ThenBy(p => p.MaYeuCau, StringComparer.Ordinal).Select(p => p.MaYeuCau),
            ketQua.DanhSach.Select(p => p.MaYeuCau));
        var dau = ketQua.DanhSach[0];
        Assert.Equal("D07 Phong P1", dau.TenPhong);
        Assert.Equal("D07-SV-A", dau.SinhVien.MaSV);
        Assert.Equal("Sinh viên D07 A", dau.SinhVien.HoTen);
        Assert.Equal("L1", dau.SinhVien.Lop);
        var dong = Assert.Single(dau.DsLoaiVaSoLuong);
        Assert.Equal("D07 Loai X", dong.TenLoai);
    }

    [CanKetNoiTestFact]
    public async Task UC06_DsQuay_PhanTrang_TrangCuoiCoPhanConLai()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var ketQua = await bo.Ds.XuLyAsync(Nv1, "CHO_DUYET", null, 3, 3, Ct);

        Assert.Equal(8, ketQua.TongSo);
        Assert.Equal(2, ketQua.DanhSach.Count);
        Assert.Equal(YcVuotThoiGian, ketQua.DanhSach[1].MaYeuCau);
    }

    [CanKetNoiTestFact]
    public async Task UC06_DsQuay_LocDaDuyet_ChiTraPhieuDaDuyet()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var ketQua = await bo.Ds.XuLyAsync(Nv1, "DA_DUYET", null, 1, 20, Ct);

        var phieu = Assert.Single(ketQua.DanhSach);
        Assert.Equal(YcDaDuyet, phieu.MaYeuCau);
        Assert.NotNull(phieu.HanGiuCho);
    }

    [CanKetNoiTestFact]
    public async Task UC06_DsQuay_MaPhongNgoaiQuyenHoacNvChuaCapPhong_TraRong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boCoQuyen = new BoXuLy(db, [P1]);
        var boChuaCapPhong = new BoXuLy(db, []);

        var ngoaiQuyen = await boCoQuyen.Ds.XuLyAsync(Nv1, null, P2, 1, 20, Ct);
        var chuaCap = await boChuaCapPhong.Ds.XuLyAsync(Nv1, null, null, 1, 20, Ct);

        Assert.Equal(0, ngoaiQuyen.TongSo);
        Assert.Empty(ngoaiQuyen.DanhSach);
        Assert.Equal(0, chuaCap.TongSo);
        Assert.Empty(chuaCap.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC06_DsQuay_TrangThaiLocSai_Tra400()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Ds.XuLyAsync(Nv1, "KHONG_CO", null, 1, 20, Ct));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC06_KiemTra_PhieuHopLe_DuocDuyetKemKhaDungDaTruGiuCho_KhongTacDungPhu()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var ketQua = await bo.KiemTra.XuLyAsync(Nv1, YcHopLe, Ct);

        Assert.True(ketQua.DuocDuyet);
        Assert.Empty(ketQua.DsLyDo);
        var dong = Assert.Single(ketQua.DsKhaDung);
        Assert.Equal(X, dong.MaLoai);
        Assert.Equal("D07 Loai X", dong.TenLoai);
        Assert.Equal(1, dong.SoLuongYeuCau);
        Assert.Equal(2, dong.SoLuongKhaDung);
        Assert.Equal("CHO_DUYET", await TrangThaiAsync(db, YcHopLe));
        Assert.Equal(0, await DemHanGiuChoAsync(db, YcHopLe));
        Assert.Empty(bo.NhatKy.ThaoTacDaGhi);
        Assert.Empty(bo.ThongBao.DaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_KiemTra_DieuKienKhongDat_TraKhongDuocDuyetKemMaLoi()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);
        (string MaYeuCau, string MaLoi)[] cacCa =
        [
            (YcTheHetHan, "SV_THE_HET_HAN"),
            (YcTamKhoa, "SV_BI_KHOA_QUYEN_MUON"),
            (YcVuotGioiHan, "VUOT_GIOI_HAN_THIET_BI"),
            (YcVuotThoiGian, "VUOT_THOI_GIAN_MUON_TOI_DA")
        ];

        foreach (var (maYeuCau, maLoi) in cacCa)
        {
            var ketQua = await bo.KiemTra.XuLyAsync(Nv1, maYeuCau, Ct);

            Assert.False(ketQua.DuocDuyet);
            Assert.Equal(new[] { maLoi }, ketQua.DsLyDo.Select(d => d.MaLoi));
        }
    }

    [CanKetNoiTestFact]
    public async Task UC06_KiemTra_PhieuDaDuyet_TraKhongDuocDuyetVoiLyDoTrangThai()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var ketQua = await bo.KiemTra.XuLyAsync(Nv1, YcDaDuyet, Ct);

        Assert.False(ketQua.DuocDuyet);
        var lyDo = Assert.Single(ketQua.DsLyDo);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", lyDo.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_KiemTra_PhieuSangNgayHomSau_TraKhongDuocDuyet()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var homSau = BayGio.AddDays(1);
        var bo = new BoXuLy(db, [P1], homSau);

        var ketQua = await bo.KiemTra.XuLyAsync(Nv1, YcHopLe, Ct);

        Assert.False(ketQua.DuocDuyet);
        var lyDo = Assert.Single(ketQua.DsLyDo);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", lyDo.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_TruyCap_MaSaiDinhDang400_KhongTonTai404_PhongChuaCapQuyen403()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.KiemTra.XuLyAsync(Nv1, "khong-phai-ma", Ct));
        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Duyet.XuLyAsync(Nv1, "YC-261118-999999", Ct));
        var phongKhac = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.Duyet.XuLyAsync(Nv1, YcPhongKhac, Ct));

        Assert.Equal(400, saiDinhDang.MaHttp);
        Assert.Equal("KHONG_TIM_THAY", khongTonTai.MaLoi);
        Assert.Equal(404, khongTonTai.MaHttp);
        Assert.Equal("PHONG_CHUA_CAP_QUYEN", phongKhac.MaLoi);
        Assert.Equal(403, phongKhac.MaHttp);
        Assert.Equal("CHO_DUYET", await TrangThaiAsync(db, YcPhongKhac));
    }

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_HopLe_GiuChoTheoThamSoGhiNhatKyVaThongBaoCungLuc()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);
        var thamSo = await new QuyDinhReader(db).LayThamSoAsync(Ct);

        var phieu = await bo.Duyet.XuLyAsync(Nv1, YcHopLe, Ct);

        Assert.Equal("DA_DUYET", phieu.TrangThai);
        Assert.Equal(BayGio, phieu.ThoiDiemDuyet);
        Assert.Equal(BayGio.AddMinutes(thamSo.ThoiGianGiuChoPhut), phieu.HanGiuCho);
        Assert.Equal("Sinh viên D07 A", phieu.SinhVien.HoTen);
        var maNvDuyet = await db.Database.SqlQuery<Guid>($"""
            SELECT ma_nv_duyet AS "Value" FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau = {YcHopLe}
            """).SingleAsync();
        Assert.Equal(Nv1, maNvDuyet);
        var khaDungSau = await new AvailabilityService(db, new DongHoCoDinh(BayGio)).TinhKhaDungAsync(P1, X, Ct);
        Assert.Equal(1, khaDungSau);
        var thongBao = Assert.Single(bo.ThongBao.DaGhi);
        Assert.True(thongBao.DaDuyet);
        Assert.Equal("D07-SV-A", thongBao.MaSv);
        Assert.Equal(phieu.HanGiuCho, thongBao.HanGiuCho);
        Assert.Equal(new[] { "DUYET_YEU_CAU_MUON" }, bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_HaiNvCungDuyetSuatCuoi_NguoiThuHaiNhan409VaPhieuConChoDuyet()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boNv1 = new BoXuLy(db, [P1]);
        var boNv2 = new BoXuLy(db, [P1]);

        var thuNhat = await boNv1.Duyet.XuLyAsync(Nv1, YcSuatCuoiA, Ct);
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boNv2.Duyet.XuLyAsync(Nv2, YcSuatCuoiB, Ct));

        Assert.Equal("DA_DUYET", thuNhat.TrangThai);
        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("CHO_DUYET", await TrangThaiAsync(db, YcSuatCuoiB));
        Assert.Equal(0, await DemHanGiuChoAsync(db, YcSuatCuoiB));
        Assert.Empty(boNv2.ThongBao.DaGhi);
        Assert.Empty(boNv2.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_TonKhoDoiSauKhiKiemTra_Nhan409KhongGiuCho()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var truoc = await bo.KiemTra.XuLyAsync(Nv1, YcTonKho, Ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE danh_muc.thiet_bi SET tinh_trang = 'DANG_DUOC_MUON' WHERE ma_thiet_bi = 'D07-TB-05'
            """, Ct);
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.Duyet.XuLyAsync(Nv1, YcTonKho, Ct));

        Assert.True(truoc.DuocDuyet);
        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal("CHO_DUYET", await TrangThaiAsync(db, YcTonKho));
        Assert.Empty(bo.ThongBao.DaGhi);
        Assert.Empty(bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_DieuKienSvKhongDat_Tra422VaKhongGhiGi()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);
        (string MaYeuCau, string MaLoi)[] cacCa =
        [
            (YcTheHetHan, "SV_THE_HET_HAN"),
            (YcTamKhoa, "SV_BI_KHOA_QUYEN_MUON"),
            (YcVuotGioiHan, "VUOT_GIOI_HAN_THIET_BI"),
            (YcVuotThoiGian, "VUOT_THOI_GIAN_MUON_TOI_DA")
        ];

        foreach (var (maYeuCau, maLoi) in cacCa)
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.Duyet.XuLyAsync(Nv1, maYeuCau, Ct));

            Assert.Equal(maLoi, loi.MaLoi);
            Assert.Equal(422, loi.MaHttp);
            Assert.Equal("CHO_DUYET", await TrangThaiAsync(db, maYeuCau));
            Assert.Equal(0, await DemHanGiuChoAsync(db, maYeuCau));
        }

        Assert.Empty(bo.ThongBao.DaGhi);
        Assert.Empty(bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_PhieuDaDuyetRoi_Tra409TrangThaiKhongHopLe()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.Duyet.XuLyAsync(Nv2, YcDaDuyet, Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Empty(bo.ThongBao.DaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_PhieuSangNgayHomSau_Tra409TrangThaiKhongHopLe()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var homSau = BayGio.AddDays(1);
        var bo = new BoXuLy(db, [P1], homSau);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.Duyet.XuLyAsync(Nv1, YcHopLe, Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Empty(bo.ThongBao.DaGhi);
    }

    [CanKetNoiTestFact]
    public async Task NFR06_Duyet_LayKhoaPhongLoai_GiaoDichKhacDangGiuCungCapThiBiChan()
    {
        await using var a = await GiaoDichTestRollback.BatDauAsync();
        await using var b = await GiaoDichTestRollback.BatDauAsync();
        await new AdvisoryLockService(a.Db).LockPhongLoaiAsync(P1, Y, Ct);
        await using var db = await TaoContextAsync(b);
        await DatThoiGianChoKhoaAsync(b, "300ms");
        var bo = new BoXuLy(db, [P1]);

        var loi = await Assert.ThrowsAsync<PostgresException>(() => bo.Duyet.XuLyAsync(Nv1, YcSuatCuoiA, Ct));

        Assert.Equal(PostgresErrorCodes.LockNotAvailable, loi.SqlState);
    }

    [CanKetNoiTestFact]
    public async Task UC06_TuChoi_HopLe_LuuLyDoDaCatKhongGiuChoKhongGhiViPham()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);
        var viPhamTruoc = await DemViPhamAsync(db);
        var khaDungTruoc = await new AvailabilityService(db, new DongHoCoDinh(BayGio)).TinhKhaDungAsync(P1, X, Ct);

        var phieu = await bo.TuChoi.XuLyAsync(Nv1, YcHopLe, "  Thẻ không còn hiệu lực  ", Ct);

        Assert.Equal("TU_CHOI", phieu.TrangThai);
        Assert.Equal("Thẻ không còn hiệu lực", phieu.LyDoTuChoi);
        Assert.Null(phieu.HanGiuCho);
        Assert.Equal("D07-SV-A", phieu.SinhVien.MaSV);
        var maNvDuyet = await db.Database.SqlQuery<Guid>($"""
            SELECT ma_nv_duyet AS "Value" FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau = {YcHopLe}
            """).SingleAsync();
        Assert.Equal(Nv1, maNvDuyet);
        Assert.Equal(viPhamTruoc, await DemViPhamAsync(db));
        Assert.Equal(khaDungTruoc, await new AvailabilityService(db, new DongHoCoDinh(BayGio)).TinhKhaDungAsync(P1, X, Ct));
        var thongBao = Assert.Single(bo.ThongBao.DaGhi);
        Assert.False(thongBao.DaDuyet);
        Assert.Equal("Thẻ không còn hiệu lực", thongBao.LyDoTuChoi);
        Assert.Equal(new[] { "TU_CHOI_YEU_CAU_MUON" }, bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_TuChoi_LyDoRongHoacChiKhoangTrang_Tra400VaPhieuKhongDoi()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        foreach (var lyDo in new[] { string.Empty, "   " })
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => bo.TuChoi.XuLyAsync(Nv1, YcHopLe, lyDo, Ct));

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(400, loi.MaHttp);
        }

        Assert.Equal("CHO_DUYET", await TrangThaiAsync(db, YcHopLe));
        Assert.Empty(bo.ThongBao.DaGhi);
        Assert.Empty(bo.NhatKy.ThaoTacDaGhi);
    }

    [CanKetNoiTestFact]
    public async Task UC06_TuChoi_SvKhongDuDieuKien_VanTuChoiDuocKhongCanKiemTraDieuKien()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var phieu = await bo.TuChoi.XuLyAsync(Nv1, YcTheHetHan, "Thẻ hết hạn", Ct);

        Assert.Equal("TU_CHOI", phieu.TrangThai);
    }

    [CanKetNoiTestFact]
    public async Task UC06_TuChoi_PhieuDaDuyet_Tra409VaPhongChuaCapQuyen403()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var bo = new BoXuLy(db, [P1]);

        var daDuyet = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.TuChoi.XuLyAsync(Nv1, YcDaDuyet, "Không đủ điều kiện", Ct));
        var phongKhac = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.TuChoi.XuLyAsync(Nv1, YcPhongKhac, "Không đủ điều kiện", Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", daDuyet.MaLoi);
        Assert.Equal(409, daDuyet.MaHttp);
        Assert.Equal("PHONG_CHUA_CAP_QUYEN", phongKhac.MaLoi);
        Assert.Equal("DA_DUYET", await TrangThaiAsync(db, YcDaDuyet));
        Assert.Equal("CHO_DUYET", await TrangThaiAsync(db, YcPhongKhac));
    }
}
