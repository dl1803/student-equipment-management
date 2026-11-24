using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.Anh;

public sealed class DonAnhMoCoJobTests
{
    private static readonly DateTimeOffset BayGio = new(2026, 11, 23, 3, 0, 0, TimeSpan.Zero);

    private static readonly byte[] AnhPng =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01, 0x02, 0x03];

    [Fact]
    public void UC17_J9_DuocDangKyTrongAddAnhLuuTru()
    {
        using var dichVu = new ServiceCollection()
            .AddLogging()
            .AddInfrastructure("Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung")
            .AddAnhLuuTru(new SupabaseStorageOptions(null, null))
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();

        var job = Assert.Single(phamVi.ServiceProvider.GetServices<IJob>(), j => j.MaJob == "J9");

        Assert.Equal("DonAnhMoCo", job.TenJob);
        Assert.Equal(TimeSpan.FromDays(1), job.ChuKy);
    }

    [Fact]
    public async Task UC17_J9_KhongCoGiaoDich_BiTuChoi()
    {
        using var dichVu = new ServiceCollection()
            .AddInfrastructure("Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung")
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();
        var job = new DonAnhMoCoJob(
            phamVi.ServiceProvider.GetRequiredService<AppDbContext>(),
            new LuuTruGia(),
            new DongHoCoDinh(BayGio),
            NullLogger<DonAnhMoCoJob>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => job.ChayAsync(CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task NFR07_J9_ChiXoaAnhChuaGanQuaHan_GiuAnhConHanVaAnhDaGan()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var cu = await moi.ThemAnhAsync(TenBucketAnh.TinhTrang, BayGio.AddHours(-25));
        var dungHan = await moi.ThemAnhAsync(TenBucketAnh.TinhTrang, BayGio.AddHours(-24));
        var chuaQuaHan = await moi.ThemAnhAsync(TenBucketAnh.ThietBi, BayGio.AddHours(-23));
        var daGan = await moi.ThemAnhAsync(
            TenBucketAnh.TinhTrang, BayGio.AddDays(-40), "CHI_TIET_PHIEU_MUON", "ct-d12k", "KHI_TRA");

        var soDaDon = await moi.ChayJobAsync();

        Assert.True(soDaDon >= 1);
        Assert.Contains((cu.Bucket, cu.Khoa), moi.LuuTru.DaXoa);
        Assert.DoesNotContain((dungHan.Bucket, dungHan.Khoa), moi.LuuTru.DaXoa);
        Assert.DoesNotContain((chuaQuaHan.Bucket, chuaQuaHan.Khoa), moi.LuuTru.DaXoa);
        Assert.DoesNotContain((daGan.Bucket, daGan.Khoa), moi.LuuTru.DaXoa);
        Assert.False(await moi.TonTaiAsync(cu.Ma));
        Assert.True(await moi.TonTaiAsync(dungHan.Ma));
        Assert.True(await moi.TonTaiAsync(chuaQuaHan.Ma));
        Assert.True(await moi.TonTaiAsync(daGan.Ma));
    }

    [CanKetNoiTestFact]
    public async Task UC12_J9_LoiXoaStorageCuaMotDong_GiuDongDoVaTiepTucDongKhac()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var hong = await moi.ThemAnhAsync(TenBucketAnh.TinhTrang, BayGio.AddHours(-30));
        var tot = await moi.ThemAnhAsync(TenBucketAnh.TinhTrang, BayGio.AddHours(-31));
        moi.LuuTru.KhoaLoiXoa.Add(hong.Khoa);

        await moi.ChayJobAsync();

        Assert.True(await moi.TonTaiAsync(hong.Ma));
        Assert.False(await moi.TonTaiAsync(tot.Ma));
        Assert.DoesNotContain((hong.Bucket, hong.Khoa), moi.LuuTru.DaXoa);
        Assert.Contains((tot.Bucket, tot.Khoa), moi.LuuTru.DaXoa);
    }

    [CanKetNoiTestFact]
    public async Task NFR06_J9_AnhDangDuocGanChuaCommit_JobBoQuaDongVaAnhKhongBiXoa()
    {
        await using var du = await DuLieuCommit.TaoAsync(BayGio.AddHours(-30));
        await using var phienGan = PhienDb.Tao();
        await using var phienJob = PhienDb.Tao();
        var luuTruJob = new LuuTruGia();

        await using var giaoDichGan = await phienGan.Db.Database.BeginTransactionAsync();
        var dichVuAnh = new AnhService(
            phienGan.Db, new LuuTruGia(), new NguoiDungGia(du.MaNv), new DongHoCoDinh(BayGio),
            NullLogger<AnhService>.Instance);
        await dichVuAnh.GanAsync([du.MaAnh], "CHI_TIET_PHIEU_MUON", "ct-d12k", "KHI_TRA", CancellationToken.None);

        await using var giaoDichJob = await phienJob.Db.Database.BeginTransactionAsync();
        var job = new DonAnhMoCoJob(
            phienJob.Db, luuTruJob, new DongHoCoDinh(BayGio), NullLogger<DonAnhMoCoJob>.Instance);
        await job.ChayAsync(CancellationToken.None);
        await giaoDichJob.RollbackAsync();
        await giaoDichGan.CommitAsync();

        Assert.DoesNotContain((TenBucketAnh.TinhTrang, du.Khoa), luuTruJob.DaXoa);
        Assert.Equal("ct-d12k", await du.DocDoiTuongIdAsync());
    }

    [CanKetNoiTestFact]
    public async Task NFR06_J9_JobDangXoaChuaCommit_GanAnhPhaiDoiRoiBiTuChoi()
    {
        await using var du = await DuLieuCommit.TaoAsync(BayGio.AddHours(-30));
        await using var phienGan = PhienDb.Tao();
        await using var phienJob = PhienDb.Tao();
        var luuTruJob = new LuuTruGia();

        await using var giaoDichJob = await phienJob.Db.Database.BeginTransactionAsync();
        var job = new DonAnhMoCoJob(
            phienJob.Db, luuTruJob, new DongHoCoDinh(BayGio), NullLogger<DonAnhMoCoJob>.Instance);
        await job.ChayAsync(CancellationToken.None);
        Assert.Contains((TenBucketAnh.TinhTrang, du.Khoa), luuTruJob.DaXoa);

        var dichVuAnh = new AnhService(
            phienGan.Db, new LuuTruGia(), new NguoiDungGia(du.MaNv), new DongHoCoDinh(BayGio),
            NullLogger<AnhService>.Instance);
        var gan = dichVuAnh.GanAsync([du.MaAnh], "CHI_TIET_PHIEU_MUON", "ct-d12k", "KHI_TRA", CancellationToken.None);
        var xong = await Task.WhenAny(gan, Task.Delay(TimeSpan.FromSeconds(2)));
        Assert.NotSame(gan, xong);

        await giaoDichJob.CommitAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => gan);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.False(await du.TonTaiAsync());
    }

    [CanStorageThatFact]
    public async Task UC12_J9_XoaObjectThatTrenSupabaseStorage()
    {
        using var http = new HttpClient();
        var luuTru = new SupabaseAnhStorage(http, CauHinhStorageThat.Doc()!);
        await using var moi = await MoiTruong.TaoAsync();
        var anh = await moi.ThemAnhAsync(TenBucketAnh.ThietBi, BayGio.AddHours(-30));
        await luuTru.UploadAsync(anh.Bucket, anh.Khoa, "image/png", AnhPng, CancellationToken.None);
        var url = luuTru.TaoUrlCongKhai(anh.Bucket, anh.Khoa);

        try
        {
            using var truoc = await http.GetAsync(url);
            await moi.ChayJobAsync(luuTru);
            using var sau = await http.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, truoc.StatusCode);
            Assert.False(sau.IsSuccessStatusCode);
        }
        finally
        {
            await luuTru.XoaAsync(anh.Bucket, anh.Khoa, CancellationToken.None);
        }
    }

    private sealed record DongAnh(Guid Ma, string Bucket, string Khoa);

    private sealed class LuuTruGia : IAnhStorage
    {
        public List<(string Bucket, string Khoa)> DaXoa { get; } = [];

        public HashSet<string> KhoaLoiXoa { get; } = [];

        public Task UploadAsync(string bucket, string khoaLuuTru, string loaiNoiDung, byte[] noiDung, CancellationToken ct) =>
            Task.CompletedTask;

        public Task<string> TaoUrlKyAsync(string bucket, string khoaLuuTru, TimeSpan hieuLuc, CancellationToken ct) =>
            Task.FromResult(string.Empty);

        public Task XoaAsync(string bucket, string khoaLuuTru, CancellationToken ct)
        {
            if (KhoaLoiXoa.Contains(khoaLuuTru))
            {
                throw new InvalidOperationException("Lỗi xóa giả lập từ Storage.");
            }

            DaXoa.Add((bucket, khoaLuuTru));
            return Task.CompletedTask;
        }

        public string TaoUrlCongKhai(string bucket, string khoaLuuTru) => string.Empty;
    }

    private sealed class MoiTruong : IAsyncDisposable
    {
        private readonly ServiceProvider _dichVu;
        private readonly AsyncServiceScope _phamVi;
        private readonly IDbContextTransaction _giaoDich;

        private MoiTruong(
            ServiceProvider dichVu,
            AsyncServiceScope phamVi,
            AppDbContext db,
            IDbContextTransaction giaoDich,
            Guid maNv)
        {
            _dichVu = dichVu;
            _phamVi = phamVi;
            Db = db;
            _giaoDich = giaoDich;
            MaNv = maNv;
        }

        public AppDbContext Db { get; }

        public Guid MaNv { get; }

        public LuuTruGia LuuTru { get; } = new();

        public static async Task<MoiTruong> TaoAsync()
        {
            var dichVu = new ServiceCollection()
                .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi())
                .BuildServiceProvider();
            var phamVi = dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            var giaoDich = await db.Database.BeginTransactionAsync();
            var maNv = Guid.CreateVersion7();
            var tenDangNhap = $"d12k-{maNv:N}";

            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
                VALUES ({maNv}, {tenDangNhap}, 'khong-dung', 'Nguoi dung kiem thu D12-K')
                """);

            return new MoiTruong(dichVu, phamVi, db, giaoDich, maNv);
        }

        public async Task<DongAnh> ThemAnhAsync(
            string bucket,
            DateTimeOffset thoiDiem,
            string? doiTuongLoai = null,
            string? doiTuongId = null,
            string? giaiDoan = null)
        {
            var ma = Guid.CreateVersion7();
            var tienTo = bucket == TenBucketAnh.ThietBi ? "thiet-bi" : giaiDoan == "KHI_TRA" ? "khi-tra" : "ban-dau";
            var khoa = $"{tienTo}/2026/11/{ma}";

            await Db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO luu_tru.anh_tai_len
                    (ma_anh, bucket, khoa_luu_tru, loai_noi_dung, kich_thuoc, nguoi_tai_len, thoi_diem,
                     doi_tuong_loai, doi_tuong_id, giai_doan)
                VALUES ({ma}, {bucket}, {khoa}, 'image/png', 10, {MaNv}, {thoiDiem},
                        {doiTuongLoai}, {doiTuongId}, {giaiDoan})
                """);

            return new DongAnh(ma, bucket, khoa);
        }

        public async Task<int> ChayJobAsync(IAnhStorage? luuTru = null)
        {
            var job = new DonAnhMoCoJob(
                Db, luuTru ?? LuuTru, new DongHoCoDinh(BayGio), NullLogger<DonAnhMoCoJob>.Instance);
            return await job.ChayAsync(CancellationToken.None);
        }

        public Task<bool> TonTaiAsync(Guid maAnh) =>
            Db.Set<AnhTaiLen>().AsNoTracking().AnyAsync(a => a.MaAnh == maAnh);

        public async ValueTask DisposeAsync()
        {
            await _giaoDich.RollbackAsync();
            await _giaoDich.DisposeAsync();
            await _phamVi.DisposeAsync();
            await _dichVu.DisposeAsync();
        }
    }

    private sealed class PhienDb : IAsyncDisposable
    {
        private readonly ServiceProvider _dichVu;
        private readonly AsyncServiceScope _phamVi;

        private PhienDb(ServiceProvider dichVu, AsyncServiceScope phamVi, AppDbContext db)
        {
            _dichVu = dichVu;
            _phamVi = phamVi;
            Db = db;
        }

        public AppDbContext Db { get; }

        public static PhienDb Tao()
        {
            var dichVu = new ServiceCollection()
                .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi())
                .BuildServiceProvider();
            var phamVi = dichVu.CreateAsyncScope();
            return new PhienDb(dichVu, phamVi, phamVi.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        public async ValueTask DisposeAsync()
        {
            await _phamVi.DisposeAsync();
            await _dichVu.DisposeAsync();
        }
    }

    private sealed class DuLieuCommit : IAsyncDisposable
    {
        private DuLieuCommit(Guid maNv, Guid maAnh, string khoa)
        {
            MaNv = maNv;
            MaAnh = maAnh;
            Khoa = khoa;
        }

        public Guid MaNv { get; }

        public Guid MaAnh { get; }

        public string Khoa { get; }

        public static async Task<DuLieuCommit> TaoAsync(DateTimeOffset thoiDiem)
        {
            var maNv = Guid.CreateVersion7();
            var maAnh = Guid.CreateVersion7();
            var khoa = $"khi-tra/2026/11/{maAnh}";

            await using var ketNoi = await KetNoiKiemThu.MoAsync();
            await using var lenh = ketNoi.CreateCommand();
            lenh.CommandText = """
                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
                VALUES (@nv, @ten, 'khong-dung', 'Nguoi dung kiem thu D12-K');
                INSERT INTO luu_tru.anh_tai_len
                    (ma_anh, bucket, khoa_luu_tru, loai_noi_dung, kich_thuoc, nguoi_tai_len, thoi_diem)
                VALUES (@anh, 'tinh-trang-anh', @khoa, 'image/png', 10, @nv, @thoi);
                """;
            lenh.Parameters.AddWithValue("nv", maNv);
            lenh.Parameters.AddWithValue("ten", $"d12k-{maNv:N}");
            lenh.Parameters.AddWithValue("anh", maAnh);
            lenh.Parameters.AddWithValue("khoa", khoa);
            lenh.Parameters.AddWithValue("thoi", thoiDiem);
            await lenh.ExecuteNonQueryAsync();

            return new DuLieuCommit(maNv, maAnh, khoa);
        }

        public async Task<string?> DocDoiTuongIdAsync()
        {
            await using var ketNoi = await KetNoiKiemThu.MoAsync();
            await using var lenh = ketNoi.CreateCommand();
            lenh.CommandText = "SELECT doi_tuong_id FROM luu_tru.anh_tai_len WHERE ma_anh = @anh";
            lenh.Parameters.AddWithValue("anh", MaAnh);
            return await lenh.ExecuteScalarAsync() as string;
        }

        public async Task<bool> TonTaiAsync()
        {
            await using var ketNoi = await KetNoiKiemThu.MoAsync();
            await using var lenh = ketNoi.CreateCommand();
            lenh.CommandText = "SELECT count(*) FROM luu_tru.anh_tai_len WHERE ma_anh = @anh";
            lenh.Parameters.AddWithValue("anh", MaAnh);
            return (long)(await lenh.ExecuteScalarAsync())! > 0;
        }

        public async ValueTask DisposeAsync()
        {
            await using var ketNoi = await KetNoiKiemThu.MoAsync();
            await using var lenh = ketNoi.CreateCommand();
            lenh.CommandText = """
                DELETE FROM luu_tru.anh_tai_len WHERE ma_anh = @anh;
                DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = @nv;
                """;
            lenh.Parameters.AddWithValue("anh", MaAnh);
            lenh.Parameters.AddWithValue("nv", MaNv);
            await lenh.ExecuteNonQueryAsync();
        }
    }
}

