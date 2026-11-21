using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Anh;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class ThietBiAnhCsdlTests
{
    private const string TienToUrlCongKhai = "https://luu-tru-gia.test/thiet-bi-anh/thiet-bi/";
    private static readonly byte[] AnhJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];

    [CanKetNoiTestFact]
    public async Task UC12_TaoThietBi_KemAnhDanhMuc_GanDungDoiTuongVaTraUrlCongKhai()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dsAnh = new List<Guid>
        {
            await moi.TaiAnhAsync("THIET_BI"),
            await moi.TaiAnhAsync("THIET_BI"),
            await moi.TaiAnhAsync("THIET_BI"),
        };

        var ketQua = await moi.TaoThietBiAsync("TB-D10-ANH", dsAnh);

        Assert.Equal(dsAnh.ToHashSet(), ketQua.DsAnh.Select(a => a.MaAnh).ToHashSet());
        Assert.All(ketQua.DsAnh, a =>
        {
            Assert.StartsWith(TienToUrlCongKhai, a.Url);
            Assert.DoesNotContain("token", a.Url);
        });
        foreach (var maAnh in dsAnh)
        {
            var dong = await moi.DocAnhAsync(maAnh);
            Assert.Equal("THIET_BI", dong.DoiTuongLoai);
            Assert.Equal("TB-D10-ANH", dong.DoiTuongId);
            Assert.Equal("THIET_BI", dong.GiaiDoan);
            Assert.DoesNotContain("http", dong.KhoaLuuTru);
        }

        Assert.Empty(moi.LuuTru.HieuLucDaKy);
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaoThietBi_QuaNamAnh_Nem400VaKhongGanAnhNao()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dsAnh = new List<Guid>();
        for (var i = 0; i < 6; i++)
        {
            dsAnh.Add(await moi.TaiAnhAsync("THIET_BI"));
        }

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => moi.TaoThietBiAsync("TB-D10-SAU", dsAnh));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
        foreach (var maAnh in dsAnh)
        {
            Assert.Null((await moi.DocAnhAsync(maAnh)).DoiTuongId);
        }
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaoThietBi_AnhGiaiDoanBanDau_Nem400VaGiuNguyenAnh()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var anhTinhTrang = await moi.TaiAnhAsync("BAN_DAU");

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => moi.TaoThietBiAsync("TB-D10-SAIGD", [anhTinhTrang]));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        var dong = await moi.DocAnhAsync(anhTinhTrang);
        Assert.Null(dong.DoiTuongId);
        Assert.Equal("tinh-trang-anh", dong.Bucket);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaThietBi_ThayGiuVaGoDsAnh_KhongDungToiAnhChungTu()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var a1 = await moi.TaiAnhAsync("THIET_BI");
        var a2 = await moi.TaiAnhAsync("THIET_BI");
        var a3 = await moi.TaiAnhAsync("THIET_BI");
        var anhChungTu = Guid.CreateVersion7();
        await ChenAnhDaGanAsync(moi.Db, moi.MaNv, anhChungTu, "tinh-trang-anh", "ban-dau", "CHI_TIET_PHIEU_MUON", "ct-d10-n", "BAN_DAU");
        await moi.TaoThietBiAsync("TB-D10-SUA", [a1, a2]);

        var thay = await moi.SuaThietBiAsync("TB-D10-SUA", [a2, a3]);
        Assert.Equal(new HashSet<Guid> { a2, a3 }, thay.DsAnh.Select(a => a.MaAnh).ToHashSet());
        Assert.Null((await moi.DocAnhAsync(a1)).DoiTuongId);

        var giuNguyen = await moi.SuaThietBiAsync("TB-D10-SUA", null);
        Assert.Equal(2, giuNguyen.DsAnh.Count);

        var goHet = await moi.SuaThietBiAsync("TB-D10-SUA", []);
        Assert.Empty(goHet.DsAnh);

        var dongChungTu = await moi.DocAnhAsync(anhChungTu);
        Assert.Equal("CHI_TIET_PHIEU_MUON", dongChungTu.DoiTuongLoai);
        Assert.Equal("ct-d10-n", dongChungTu.DoiTuongId);
        Assert.Equal("BAN_DAU", dongChungTu.GiaiDoan);
        Assert.Empty(moi.LuuTru.DaXoa);
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_GoLienKetAnhDanhMucNhungGiuAnhChungTu()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var maNv = await TaoNguoiDungAsync(db);
        var (maPhong, maLoai) = await ThietBiGhiCsdlTests.TaoPhongLoaiAsync(phamVi.ServiceProvider, "D10-XOA");
        const string ma = "TB-D10-XOA";
        await phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>().XuLyAsync(
            new ThietBiTaoMoiRequest(ma, maLoai, maPhong, "Thiet bi D10 xoa", null, null, new DateOnly(2026, 1, 5), null, null, null, null, null, 1_000_000),
            CancellationToken.None);
        var anhDanhMuc = Guid.CreateVersion7();
        var anhChungTu = Guid.CreateVersion7();
        await ChenAnhDaGanAsync(db, maNv, anhDanhMuc, "thiet-bi-anh", "thiet-bi", "THIET_BI", ma, "THIET_BI");
        await ChenAnhDaGanAsync(db, maNv, anhChungTu, "tinh-trang-anh", "ban-dau", "CHI_TIET_PHIEU_MUON", "ct-d10-n", "BAN_DAU");

        await phamVi.ServiceProvider.GetRequiredService<XoaThietBiHandler>().XuLyAsync(ma, CancellationToken.None);

        Assert.Equal(0, await ThietBiGhiCsdlTests.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {ma}
            """));
        var dongDanhMuc = await db.Set<AnhTaiLen>().AsNoTracking().SingleAsync(a => a.MaAnh == anhDanhMuc);
        Assert.Null(dongDanhMuc.DoiTuongLoai);
        Assert.Null(dongDanhMuc.DoiTuongId);
        Assert.Null(dongDanhMuc.GiaiDoan);
        var dongChungTu = await db.Set<AnhTaiLen>().AsNoTracking().SingleAsync(a => a.MaAnh == anhChungTu);
        Assert.Equal("ct-d10-n", dongChungTu.DoiTuongId);
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsThietBi_TraDsAnhCongKhaiTheoTungThietBi()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var anh = await moi.TaiAnhAsync("THIET_BI");
        await moi.TaoThietBiAsync("TB-D10-DS-1", [anh]);
        await moi.TaoThietBiAsync("TB-D10-DS-2");

        var ketQua = await moi.Dv.GetRequiredService<DsThietBiHandler>()
            .XuLyAsync("Thiet bi TB-D10-DS", moi.MaLoai, moi.MaPhong, null, 1, 20, CancellationToken.None);

        Assert.Equal(2, ketQua.TongSo);
        var coAnh = ketQua.DanhSach.Single(t => t.MaThietBi == "TB-D10-DS-1");
        Assert.Equal(anh, Assert.Single(coAnh.DsAnh).MaAnh);
        Assert.Empty(ketQua.DanhSach.Single(t => t.MaThietBi == "TB-D10-DS-2").DsAnh);
    }

    [CanKetNoiTestFact]
    public async Task UC12_LichSuGiaTri_LayTheoMocThoiGianVaThemGiaTriMoiGiuBanGhiCu()
    {
        var dongHo = new DongHoDieuChinh(new DateTimeOffset(2026, 3, 1, 1, 0, 0, TimeSpan.Zero));
        await using var moi = await MoiTruong.TaoAsync(dongHo);
        const string ma = "TB-D10-GT";
        await moi.TaoThietBiAsync(ma, null, 1_000_000);
        var capNhat = moi.Dv.GetRequiredService<CapNhatGiaTriThietBiHandler>();
        dongHo.UtcNow = new DateTimeOffset(2026, 4, 1, 1, 0, 0, TimeSpan.Zero);
        await capNhat.XuLyAsync(ma, new GiaTriThietBiRequest(2_000_000), CancellationToken.None);
        dongHo.UtcNow = new DateTimeOffset(2026, 5, 1, 1, 0, 0, TimeSpan.Zero);
        await capNhat.XuLyAsync(ma, new GiaTriThietBiRequest(3_000_000), CancellationToken.None);

        Assert.Null(await moi.LayGiaTriTaiAsync(ma, new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)));
        Assert.Equal(1_000_000L, await moi.LayGiaTriTaiAsync(ma, new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)));
        Assert.Equal(2_000_000L, await moi.LayGiaTriTaiAsync(ma, new DateTimeOffset(2026, 4, 1, 1, 0, 0, TimeSpan.Zero)));
        Assert.Equal(2_000_000L, await moi.LayGiaTriTaiAsync(ma, new DateTimeOffset(2026, 4, 15, 0, 0, 0, TimeSpan.Zero)));
        Assert.Equal(3_000_000L, await moi.LayGiaTriTaiAsync(ma, new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero)));
        var hienHanh = await moi.Dv.GetRequiredService<DsThietBiHandler>()
            .XuLyAsync("Thiet bi TB-D10-GT", null, null, null, 1, 20, CancellationToken.None);
        Assert.Equal(3_000_000L, Assert.Single(hienHanh.DanhSach).GiaTriHienHanh);
        Assert.Equal(3, await ThietBiGhiCsdlTests.DemAsync(moi.Db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi = {ma}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaoThietBi_AnhThuHaiSai_HoanTacThietBiVaLienKetAnhDau()
    {
        var nguoiDung = new NhatKyTests.NguoiDungGia();
        await using var dichVu = ThietBiGhiCsdlTests.TaoDichVu(nguoiDung);
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var maNv = Guid.CreateVersion7();
        var maPhong = Guid.CreateVersion7();
        var maLoai = Guid.CreateVersion7();
        const string ma = "TB-D10-HOANTAC";
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
                VALUES ({maNv}, {$"d10n-{maNv:N}"}, 'khong-dung', 'Nguoi dung kiem thu D10-N')
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so)
                VALUES ({maPhong}, 'Phong D10-N hoan tac', 'Co so D10-N')
                """);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay)
                VALUES ({maLoai}, 'Loai D10-N hoan tac', 30, 1000)
                """);
            nguoiDung.MaNguoiDung = maNv;
            var anhHopLe = await phamVi.ServiceProvider.GetRequiredService<IAnhService>()
                .TaiLenAsync("THIET_BI", new MemoryStream(AnhJpeg), CancellationToken.None);
            var handler = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();

            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(
                new ThietBiTaoMoiRequest(ma, maLoai, maPhong, "Thiet bi hoan tac", null, null, new DateOnly(2026, 1, 5), null, null, null, null, [anhHopLe, Guid.NewGuid()], 1_000_000),
                CancellationToken.None));

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(0, await ThietBiGhiCsdlTests.DemAsync(db, $"""
                SELECT count(*)::int AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {ma}
                """));
            Assert.Equal(0, await ThietBiGhiCsdlTests.DemAsync(db, $"""
                SELECT count(*)::int AS "Value" FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi = {ma}
                """));
            var dongAnh = await db.Set<AnhTaiLen>().AsNoTracking().SingleAsync(a => a.MaAnh == anhHopLe);
            Assert.Null(dongAnh.DoiTuongId);
        }
        finally
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM luu_tru.anh_tai_len WHERE nguoi_tai_len = {maNv}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {ma}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM danh_muc.loai_thiet_bi WHERE ma_loai = {maLoai}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong = {maPhong}");
            await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {maNv}");
        }
    }

    private static async Task<Guid> TaoNguoiDungAsync(AppDbContext db)
    {
        var ma = Guid.CreateVersion7();
        var tenDangNhap = $"d10n-{ma:N}";
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
            VALUES ({ma}, {tenDangNhap}, 'khong-dung', 'Nguoi dung kiem thu D10-N')
            """);
        return ma;
    }

    private static Task ChenAnhDaGanAsync(
        AppDbContext db,
        Guid maNv,
        Guid ma,
        string bucket,
        string tienTo,
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan)
    {
        var khoa = $"{tienTo}/2026/11/{ma}";
        return db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO luu_tru.anh_tai_len (ma_anh, bucket, khoa_luu_tru, loai_noi_dung, kich_thuoc, nguoi_tai_len, doi_tuong_loai, doi_tuong_id, giai_doan)
            VALUES ({ma}, {bucket}, {khoa}, 'image/jpeg', 10, {maNv}, {doiTuongLoai}, {doiTuongId}, {giaiDoan})
            """);
    }

    private sealed class DongHoDieuChinh(DateTimeOffset batDau) : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = batDau;
    }

    private sealed class MoiTruong : IAsyncDisposable
    {
        private readonly ServiceProvider _dichVu;
        private readonly AsyncServiceScope _phamVi;
        private readonly IDbContextTransaction _giaoDich;

        private MoiTruong(
            ServiceProvider dichVu,
            AsyncServiceScope phamVi,
            IDbContextTransaction giaoDich,
            AnhStorageGia luuTru,
            Guid maNv,
            Guid maPhong,
            Guid maLoai)
        {
            _dichVu = dichVu;
            _phamVi = phamVi;
            _giaoDich = giaoDich;
            Db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            LuuTru = luuTru;
            MaNv = maNv;
            MaPhong = maPhong;
            MaLoai = maLoai;
        }

        public AppDbContext Db { get; }

        public IServiceProvider Dv => _phamVi.ServiceProvider;

        public AnhStorageGia LuuTru { get; }

        public Guid MaNv { get; }

        public Guid MaPhong { get; }

        public Guid MaLoai { get; }

        public static async Task<MoiTruong> TaoAsync(IClock? dongHo = null)
        {
            var luuTru = new AnhStorageGia();
            var nguoiDung = new NhatKyTests.NguoiDungGia();
            var dichVu = ThietBiGhiCsdlTests.TaoDichVu(nguoiDung, luuTru, dongHo);
            var phamVi = dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            var giaoDich = await db.Database.BeginTransactionAsync();
            var maNv = await TaoNguoiDungAsync(db);
            nguoiDung.MaNguoiDung = maNv;
            var (maPhong, maLoai) = await ThietBiGhiCsdlTests.TaoPhongLoaiAsync(phamVi.ServiceProvider, "D10");

            return new MoiTruong(dichVu, phamVi, giaoDich, luuTru, maNv, maPhong, maLoai);
        }

        public Task<Guid> TaiAnhAsync(string giaiDoan) =>
            Dv.GetRequiredService<IAnhService>()
                .TaiLenAsync(giaiDoan, new MemoryStream(AnhJpeg), CancellationToken.None);

        public Task<ThietBiQuanTriDto> TaoThietBiAsync(string ma, IReadOnlyList<Guid>? dsAnh = null, long giaTri = 1_000_000) =>
            Dv.GetRequiredService<TaoThietBiHandler>().XuLyAsync(
                new ThietBiTaoMoiRequest(ma, MaLoai, MaPhong, $"Thiet bi {ma}", null, null, new DateOnly(2026, 1, 5), null, null, null, null, dsAnh, giaTri),
                CancellationToken.None);

        public Task<ThietBiQuanTriDto> SuaThietBiAsync(string ma, IReadOnlyList<Guid>? dsAnh) =>
            Dv.GetRequiredService<SuaThietBiHandler>().XuLyAsync(
                ma,
                new ThietBiCapNhatRequest(MaLoai, MaPhong, $"Thiet bi {ma}", null, null, new DateOnly(2026, 1, 5), null, null, null, null, dsAnh),
                CancellationToken.None);

        public Task<AnhTaiLen> DocAnhAsync(Guid maAnh) =>
            Db.Set<AnhTaiLen>().AsNoTracking().SingleAsync(a => a.MaAnh == maAnh);

        public async Task<long?> LayGiaTriTaiAsync(string ma, DateTimeOffset moc)
        {
            var dong = await Db.Database.SqlQuery<long>($"""
                SELECT gia_tri::bigint AS "Value" FROM danh_muc.lich_su_gia_tri
                WHERE ma_thiet_bi = {ma} AND ngay_cap_nhat <= {moc}
                ORDER BY ngay_cap_nhat DESC, ma_lich_su DESC
                LIMIT 1
                """).ToListAsync();

            return dong.Count == 0 ? null : dong[0];
        }

        public async ValueTask DisposeAsync()
        {
            await _giaoDich.DisposeAsync();
            await _phamVi.DisposeAsync();
            await _dichVu.DisposeAsync();
        }
    }
}
