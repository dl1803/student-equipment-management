using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class KhaDungVaKhoaTests
{
    private const string FileDuLieu = "D03-V-du-lieu-thu.sql";
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d3b01");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d3b02");
    private static readonly Guid X = Guid.Parse("019c0000-0000-7000-8000-0000000d3c01");
    private static readonly Guid Y = Guid.Parse("019c0000-0000-7000-8000-0000000d3c02");
    private static readonly DateTimeOffset HanGiuCho = new(2026, 11, 14, 2, 30, 0, TimeSpan.Zero);

    [CanKetNoiTestFact]
    public async Task BR16_TinhKhaDung_QuanhHanGiuCho_ChiTruGiuChoKhiHanConLonHonBayGio()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync(FileDuLieu);

        Assert.Equal(2, await TinhAsync(gd, HanGiuCho.AddSeconds(-1), P1, X));
        Assert.Equal(2, await TinhAsync(gd, HanGiuCho.AddTicks(-10), P1, X));
        Assert.Equal(5, await TinhAsync(gd, HanGiuCho, P1, X));
        Assert.Equal(5, await TinhAsync(gd, HanGiuCho.AddSeconds(1), P1, X));
    }

    [CanKetNoiTestFact]
    public async Task BR20_TinhKhaDung_TungCapPhongLoai_ChiDemThietBiSanSangVaChiTruGiuChoCuaDungCap()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync(FileDuLieu);

        Assert.Equal(2, await TinhAsync(gd, HanGiuCho, P1, Y));
        Assert.Equal(2, await TinhAsync(gd, HanGiuCho, P2, X));
        Assert.Equal(1, await TinhAsync(gd, HanGiuCho, P2, Y));

        var sauHanPhong2 = new DateTimeOffset(2026, 11, 14, 2, 45, 0, TimeSpan.Zero);
        Assert.Equal(3, await TinhAsync(gd, sauHanPhong2, P2, X));
        Assert.Equal(2, await TinhAsync(gd, sauHanPhong2, P2, Y));
    }

    [CanKetNoiTestFact]
    public async Task BR16_TinhKhaDung_CapPhongLoaiKhongCoThietBi_RaKhong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync(FileDuLieu);

        Assert.Equal(0, await TinhAsync(gd, HanGiuCho, Guid.NewGuid(), Guid.NewGuid()));
    }

    [CanKetNoiTestFact]
    public async Task BR16_TinhKhaDung_GiuChoVuotSoLuongSanSang_ChanDuoiBangKhong()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync(FileDuLieu);

        await gd.Db.Database.ExecuteSqlAsync(
            $"UPDATE danh_muc.thiet_bi SET ngung_su_dung = true WHERE ma_phong = {P1} AND ma_loai = {X}");

        Assert.Equal(0, await TinhAsync(gd, HanGiuCho.AddSeconds(-1), P1, X));
    }

    [Fact]
    public async Task NFR06_LockPhongLoai_ChuaCoGiaoDich_NemLoi()
    {
        var options = new DbContextOptionsBuilder<MuonTraTestContext>()
            .UseNpgsql("Host=localhost;Database=khong_ket_noi")
            .Options;
        await using var db = new MuonTraTestContext(options);
        var dichVu = new AdvisoryLockService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dichVu.LockPhongLoaiAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task NFR06_LockDanhSachPhongLoai_DanhSachNull_NemLoi()
    {
        var options = new DbContextOptionsBuilder<MuonTraTestContext>()
            .UseNpgsql("Host=localhost;Database=khong_ket_noi")
            .Options;
        await using var db = new MuonTraTestContext(options);
        var dichVu = new AdvisoryLockService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => dichVu.LockDanhSachPhongLoaiAsync(null!, CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task NFR06_LockPhongLoai_GiaoDichKhacLayCungCap_BiChanDenKhiHetThoiGianCho()
    {
        var maPhong = Guid.NewGuid();
        var maLoai = Guid.NewGuid();
        await using var a = await GiaoDichTestRollback.BatDauAsync();
        await using var b = await GiaoDichTestRollback.BatDauAsync();
        await new AdvisoryLockService(a.Db).LockPhongLoaiAsync(maPhong, maLoai, CancellationToken.None);
        await DatThoiGianChoKhoaAsync(b, "300ms");

        var loi = await Assert.ThrowsAsync<PostgresException>(
            () => new AdvisoryLockService(b.Db).LockPhongLoaiAsync(maPhong, maLoai, CancellationToken.None));

        Assert.Equal(PostgresErrorCodes.LockNotAvailable, loi.SqlState);
    }

    [CanKetNoiTestFact]
    public async Task NFR06_LockPhongLoai_GiaoDichKhacLayCapKhac_KhongBiChan()
    {
        var maPhong = Guid.NewGuid();
        var maLoai = Guid.NewGuid();
        await using var a = await GiaoDichTestRollback.BatDauAsync();
        await using var b = await GiaoDichTestRollback.BatDauAsync();
        await new AdvisoryLockService(a.Db).LockPhongLoaiAsync(maPhong, maLoai, CancellationToken.None);
        await DatThoiGianChoKhoaAsync(b, "300ms");
        var dichVuB = new AdvisoryLockService(b.Db);

        await dichVuB.LockPhongLoaiAsync(maPhong, Guid.NewGuid(), CancellationToken.None);
        await dichVuB.LockPhongLoaiAsync(Guid.NewGuid(), maLoai, CancellationToken.None);
    }

    [CanKetNoiTestFact]
    public async Task NFR06_LockPhongLoai_GiaoDichKetThuc_NhaKhoChoGiaoDichKhac()
    {
        var maPhong = Guid.NewGuid();
        var maLoai = Guid.NewGuid();
        await using var a = await GiaoDichTestRollback.BatDauAsync();
        await using var b = await GiaoDichTestRollback.BatDauAsync();
        await new AdvisoryLockService(a.Db).LockPhongLoaiAsync(maPhong, maLoai, CancellationToken.None);
        await DatThoiGianChoKhoaAsync(b, "300ms");

        await a.GiaoDich.CommitAsync();

        await new AdvisoryLockService(b.Db).LockPhongLoaiAsync(maPhong, maLoai, CancellationToken.None);
    }

    [CanKetNoiTestFact]
    public async Task NFR06_LockDanhSachPhongLoai_DanhSachNguocVaTrung_ChiLayMoiCapMotKhoa()
    {
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var x = Guid.NewGuid();
        await using var gd = await GiaoDichTestRollback.BatDauAsync();

        await new AdvisoryLockService(gd.Db).LockDanhSachPhongLoaiAsync(
            new[] { (p2, x), (p1, x), (p2, x) },
            CancellationToken.None);

        await using var lenh = new NpgsqlCommand(
            "SELECT count(*) FROM pg_locks WHERE locktype = 'advisory' AND pid = pg_backend_pid()",
            gd.KetNoi,
            gd.GiaoDich);
        Assert.Equal(2L, (long)(await lenh.ExecuteScalarAsync())!);
    }

    private static Task<int> TinhAsync(GiaoDichTestRollback gd, DateTimeOffset bayGio, Guid maPhong, Guid maLoai)
    {
        var dichVu = new AvailabilityService(gd.Db, new DongHoCoDinh(bayGio));
        return dichVu.TinhKhaDungAsync(maPhong, maLoai, CancellationToken.None);
    }

    private static async Task DatThoiGianChoKhoaAsync(GiaoDichTestRollback gd, string thoiGian)
    {
        await using var lenh = new NpgsqlCommand($"SET LOCAL lock_timeout = '{thoiGian}'", gd.KetNoi, gd.GiaoDich);
        await lenh.ExecuteNonQueryAsync();
    }

    private sealed class DongHoCoDinh : IClock
    {
        public DongHoCoDinh(DateTimeOffset thoiDiem)
        {
            UtcNow = thoiDiem;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
