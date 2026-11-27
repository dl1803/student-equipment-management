using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Auditing;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.Modules.ThongBao.Infrastructure;
using QuanLyMuonTra.Modules.ViPham.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

public class LapBienBanTests
{
    private static readonly DateTimeOffset HanTraCoDinh = new(2026, 10, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset BayGio = new(2026, 11, 20, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UC09_LapBienBan_BaLoai_TinhLaiTienVaGhiDuBangLienQuan()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, kb.MaPhong);
        var lenh = TaoLenh(
            kb.MaPhieu,
            new KhoanLapBienBanYeuCau(kb.TraTre, "TRA_TRE", null),
            new KhoanLapBienBanYeuCau(kb.HuHong, "HU_HONG", kb.MaKhung),
            new KhoanLapBienBanYeuCau(kb.DaMat, "MAT", null));

        var ketQua = await boXuLy.XuLyAsync(kb.MaNv, lenh, CancellationToken.None);

        Assert.Matches(@"^BB-\d{6}-\d{6,}$", ketQua.MaBienBan);
        Assert.Equal("CHUA_NOP", ketQua.TrangThai);
        Assert.Equal(kb.MaSv, ketQua.MaSV);
        Assert.Equal(5350000L, ketQua.TongTien);
        Assert.Collection(
            ketQua.DsChiTiet,
            k =>
            {
                Assert.Equal(kb.TraTre, k.MaThietBi);
                Assert.Equal("TRA_TRE", k.LoaiViPham);
                Assert.Equal(2, k.SoNgayTre);
                Assert.Equal(100000L, k.SoTien);
                Assert.Null(k.MaKhungPhat);
            },
            k =>
            {
                Assert.Equal(kb.HuHong, k.MaThietBi);
                Assert.Equal("HU_HONG", k.LoaiViPham);
                Assert.Null(k.SoNgayTre);
                Assert.Equal(250000L, k.SoTien);
                Assert.Equal(kb.MaKhung, k.MaKhungPhat);
            },
            k =>
            {
                Assert.Equal(kb.DaMat, k.MaThietBi);
                Assert.Equal("MAT", k.LoaiViPham);
                Assert.Equal(5000000L, k.SoTien);
            });

        var maBienBan = ketQua.MaBienBan;
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM vi_pham.bien_ban_vi_pham
            WHERE ma_bien_ban = {maBienBan} AND trang_thai = 'CHUA_NOP' AND tong_tien = 5350000 AND ma_nv_lap = {kb.MaNv}
            """));
        Assert.Equal(3L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM vi_pham.chi_tiet_bien_ban WHERE ma_bien_ban = {maBienBan}
            """));
        Assert.Equal(3L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM vi_pham.vi_pham
            WHERE ma_bien_ban = {maBienBan} AND ma_sv = {kb.MaSv} AND hoc_ky = 'HK1'
            """));
        Assert.Equal(2L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM thong_bao.thong_bao
            WHERE loai_thong_bao = 'VI_PHAM_PHI_PHAT' AND doi_tuong_id = {maBienBan}
            """));
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'LAP_BIEN_BAN_VI_PHAM' AND doi_tuong_id = {maBienBan}
            """));
        Assert.Equal("TAM_KHOA", await db.Database.SqlQuery<string>($"""
            SELECT trang_thai_quyen_muon AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {kb.MaSv}
            """).SingleAsync());

        await db.Database.ExecuteSqlRawAsync("SET CONSTRAINTS ALL IMMEDIATE");
    }

    [Theory]
    [InlineData(31)]
    [InlineData(45)]
    public async Task UC09A3_BaoMat_ChuaTraQuaMoc30Ngay_GoiPortVaLapKhoanMat(int soNgayLichSauHan)
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, BayGio.AddDays(-soNgayLichSauHan));
        var port = new PortMatGia(true);
        var boXuLy = TaoHandler(db, BayGio, port, kb.MaPhong);
        var maChiTiet = await db.Database.SqlQuery<Guid>($"""
            SELECT ma_chi_tiet AS "Value" FROM muon_tra.chi_tiet_phieu_muon
            WHERE ma_phieu = {kb.MaPhieu} AND ma_thiet_bi = {kb.ChuaTra}
            """).SingleAsync();

        var ketQua = await boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.ChuaTra, "MAT", null)),
            CancellationToken.None);

        Assert.Equal(1000000L, ketQua.TongTien);
        var khoan = Assert.Single(ketQua.DsChiTiet);
        Assert.Equal("MAT", khoan.LoaiViPham);
        Assert.Equal(kb.ChuaTra, khoan.MaThietBi);
        var loiGoi = Assert.Single(port.DaGoi);
        Assert.Equal(maChiTiet, loiGoi.MaChiTiet);
        Assert.Equal(kb.MaNv, loiGoi.MaNv);
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM vi_pham.vi_pham
            WHERE ma_bien_ban = {ketQua.MaBienBan} AND loai_vi_pham = 'MAT'
            """));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(29)]
    [InlineData(30)]
    public async Task UC09A3_BaoMat_ChuaQuaMoc30Ngay_Tra409VaKhongGhiGi(int soNgayLichSauHan)
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, BayGio.AddDays(-soNgayLichSauHan));
        var port = new PortMatGia(true);
        var boXuLy = TaoHandler(db, BayGio, port, kb.MaPhong);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.ChuaTra, "MAT", null)),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Empty(port.DaGoi);
        Assert.Equal(0L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09A3_BaoMat_ThieuPortThat_ChanAnToanVaKhongGhiGi()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, BayGio.AddDays(-40));
        var boXuLy = TaoHandler(db, BayGio, null, kb.MaPhong);

        await Assert.ThrowsAsync<InvalidOperationException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.ChuaTra, "MAT", null)),
            CancellationToken.None));

        Assert.Equal(0L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09A3_BaoMat_PortTraFalse_Tra409VaKhongGhiGi()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, BayGio.AddDays(-40));
        var boXuLy = TaoHandler(db, BayGio, new PortMatGia(false), kb.MaPhong);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.ChuaTra, "MAT", null)),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(0L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09_LapBienBan_KhoanTrungTrongBienBanChuaHuy_Tra409()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, kb.MaPhong);
        var lenh = TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.DaMat, "MAT", null));
        await boXuLy.XuLyAsync(kb.MaNv, lenh, CancellationToken.None);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(kb.MaNv, lenh, CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(1L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09_LapBienBan_BienBanCuDaHuy_LapLaiDuoc()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, kb.MaPhong);
        var lenh = TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.DaMat, "MAT", null));
        var lanDau = await boXuLy.XuLyAsync(kb.MaNv, lenh, CancellationToken.None);
        var maBienBanCu = lanDau.MaBienBan;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE vi_pham.bien_ban_vi_pham SET trang_thai = 'DA_HUY' WHERE ma_bien_ban = {maBienBanCu}
            """);

        var lanHai = await boXuLy.XuLyAsync(kb.MaNv, lenh, CancellationToken.None);

        Assert.NotEqual(maBienBanCu, lanHai.MaBienBan);
        Assert.Equal(2L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09_LapBienBan_HuHongThieuKhungPhat_Tra422()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, kb.MaPhong);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.HuHong, "HU_HONG", null)),
            CancellationToken.None));

        Assert.Equal("THIEU_MO_TA_HOAC_KHUNG_PHAT", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Equal(0L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09_LapBienBan_ThietBiNgoaiPhieu_Tra422()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, kb.MaPhong);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau("KHONG-CO-TRONG-PHIEU", "MAT", null)),
            CancellationToken.None));

        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public async Task UC09_LapBienBan_NhanVienPhongKhac_Tra403()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, Guid.CreateVersion7());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.DaMat, "MAT", null)),
            CancellationToken.None));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Equal(0L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09_LapBienBan_NhanVienPhongKhacGuiThietBiNgoaiPhieu_Tra403()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, Guid.CreateVersion7());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau("KHONG-CO-TRONG-PHIEU", "MAT", null)),
            CancellationToken.None));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Equal(0L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09_LapBienBan_TraTreChoThietBiKhongTre_Tra409()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var kb = await TaoKichBanAsync(db, HanTraCoDinh);
        var boXuLy = TaoHandler(db, BayGio, null, kb.MaPhong);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            kb.MaNv,
            TaoLenh(kb.MaPhieu, new KhoanLapBienBanYeuCau(kb.HuHong, "TRA_TRE", null)),
            CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(0L, await DemBienBanAsync(db, kb.MaPhieu));
    }

    [Fact]
    public async Task UC09_LapBienBan_MaPhieuKhongTonTai_Tra404()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var boXuLy = TaoHandler(mt.Db, BayGio, null, Guid.CreateVersion7());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(
            Guid.CreateVersion7(),
            TaoLenh($"PM-999999-{ViPhamDuLieuThu.MaHau()}", new KhoanLapBienBanYeuCau("TB", "MAT", null)),
            CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    private static LapBienBanLenh TaoLenh(string maPhieu, params KhoanLapBienBanYeuCau[] dsKhoan) =>
        new(maPhieu, dsKhoan);

    private static LapBienBanHandler TaoHandler(
        AppDbContext db,
        DateTimeOffset bayGio,
        IPortGhiNhanMat? port,
        params Guid[] dsMaPhong)
    {
        var dongHo = new DocPhieuMuonTestSupport.DongHoCoDinh(bayGio);
        var nhatKy = new AuditLogger(db, new TacNhanHeThong(), dongHo);

        return new LapBienBanHandler(
            db,
            dongHo,
            new PhieuMuonReader(db, dongHo),
            new DanhMucReader(db),
            new QuyDinhReader(db),
            new DocPhieuMuonTestSupport.QuyenPhongGia(dsMaPhong),
            new QuyenMuonService(db, nhatKy),
            new ThongBaoOutbox(db, dongHo, new NhanVienEmailReader(db)),
            nhatKy,
            port);
    }

    private static Task<long> DemAsync(AppDbContext db, FormattableString cauLenh) =>
        db.Database.SqlQuery<long>(cauLenh).SingleAsync();

    private static Task<long> DemBienBanAsync(AppDbContext db, string maPhieu) =>
        DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM vi_pham.bien_ban_vi_pham WHERE ma_phieu_muon = {maPhieu}
            """);

    private static async Task<KichBan> TaoKichBanAsync(AppDbContext db, DateTimeOffset hanTra)
    {
        var hau = ViPhamDuLieuThu.MaHau();
        var maSv = $"KT15-SV-{hau}";
        var maNv = await ViPhamDuLieuThu.TaoNhanVienAsync(db, hau);
        var maPhong = await ViPhamDuLieuThu.TaoPhongAsync(db);
        var maLoai = await ViPhamDuLieuThu.TaoLoaiAsync(db, 50000);
        var maKhung = await ViPhamDuLieuThu.TaoKhungPhatAsync(db, 12.5m);
        var maPhieu = $"PM-999995-{hau}";
        var traTre = $"KT15-TB-{hau}-1";
        var huHong = $"KT15-TB-{hau}-2";
        var daMat = $"KT15-TB-{hau}-3";
        var chuaTra = $"KT15-TB-{hau}-4";

        await ViPhamDuLieuThu.TaoSinhVienAsync(db, maSv);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, traTre, maLoai, maPhong, 10000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, huHong, maLoai, maPhong, 2000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, daMat, maLoai, maPhong, 5000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, chuaTra, maLoai, maPhong, 1000000);
        await ViPhamDuLieuThu.TaoPhieuAsync(db, maPhieu, maSv, maNv, hanTra);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(
            db, maPhieu, traTre, 50000, "NGUYEN_VEN", "Nguyen ven", hanTra.AddHours(44), maNv, null);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(
            db, maPhieu, huHong, 50000, "HU_HONG", "Vo man hinh", hanTra.AddHours(-1), maNv, maKhung);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, maPhieu, daMat, 50000, "MAT", null, null, maNv, null);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, maPhieu, chuaTra, 50000, null, null, null, null, null);

        return new KichBan(maNv, maPhong, maKhung, maSv, maPhieu, traTre, huHong, daMat, chuaTra);
    }

    private sealed record KichBan(
        Guid MaNv,
        Guid MaPhong,
        Guid MaKhung,
        string MaSv,
        string MaPhieu,
        string TraTre,
        string HuHong,
        string DaMat,
        string ChuaTra);

    private sealed class PortMatGia(bool ketQua) : IPortGhiNhanMat
    {
        public List<(Guid MaChiTiet, Guid MaNv)> DaGoi { get; } = [];

        public Task<bool> GhiNhanMatAsync(Guid maChiTietPhieu, Guid maNvNhanTra, CancellationToken ct)
        {
            DaGoi.Add((maChiTietPhieu, maNvNhanTra));
            return Task.FromResult(ketQua);
        }
    }

    private sealed class MoiTruong : IAsyncDisposable
    {
        private readonly ServiceProvider dichVu;
        private readonly AsyncServiceScope phamVi;
        private readonly IDbContextTransaction giaoDich;

        private MoiTruong(ServiceProvider dichVu, AsyncServiceScope phamVi, AppDbContext db, IDbContextTransaction giaoDich)
        {
            this.dichVu = dichVu;
            this.phamVi = phamVi;
            this.giaoDich = giaoDich;
            Db = db;
        }

        public AppDbContext Db { get; }

        public static async Task<MoiTruong> TaoAsync()
        {
            var dichVu = ViPhamDuLieuThu.TaoDichVu();
            var phamVi = dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            var giaoDich = await db.Database.BeginTransactionAsync();

            return new MoiTruong(dichVu, phamVi, db, giaoDich);
        }

        public async ValueTask DisposeAsync()
        {
            await giaoDich.DisposeAsync();
            await phamVi.DisposeAsync();
            await dichVu.DisposeAsync();
        }
    }
}

