using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.Anh;

public sealed class AnhServiceTests
{
    private static readonly DateTimeOffset BayGio = new(2026, 11, 20, 3, 0, 0, TimeSpan.Zero);
    private static readonly byte[] AnhJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];

    [CanKetNoiTestFact]
    public async Task UC07_TaiLen_AnhHopLe_GhiStorageVaDongChuaGan()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();

        var maAnh = await TaiAnhAsync(dichVuAnh, "BAN_DAU");

        var dongStorage = Assert.Single(moi.LuuTru.DaTai);
        var dong = await moi.DocAnhAsync(maAnh);
        Assert.Equal("tinh-trang-anh", dongStorage.Bucket);
        Assert.StartsWith("ban-dau/2026/11/", dongStorage.Khoa);
        Assert.Equal("image/jpeg", dongStorage.LoaiNoiDung);
        Assert.Equal(dongStorage.Khoa, dong.KhoaLuuTru);
        Assert.Equal(AnhJpeg.Length, dong.KichThuoc);
        Assert.Equal(moi.MaNv, dong.NguoiTaiLen);
        Assert.Null(dong.DoiTuongLoai);
        Assert.Null(dong.DoiTuongId);
        Assert.Null(dong.GiaiDoan);
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaiLen_GiaiDoanThietBi_VaoBucketCongKhai()
    {
        await using var moi = await MoiTruong.TaoAsync();

        await TaiAnhAsync(moi.TaoDichVuAnh(), "THIET_BI");

        var dongStorage = Assert.Single(moi.LuuTru.DaTai);
        Assert.Equal("thiet-bi-anh", dongStorage.Bucket);
        Assert.StartsWith("thiet-bi/2026/11/", dongStorage.Khoa);
    }

    [CanKetNoiTestFact]
    public async Task NFR07_TaiLen_TepGiaMao_TuChoiVaKhongGhiGi()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var giaMao = "Day khong phai anh"u8.ToArray();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.TaiLenAsync("BAN_DAU", new MemoryStream(giaMao), CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Empty(moi.LuuTru.DaTai);
        Assert.Equal(0, await moi.DemAnhCuaMinhAsync());
    }

    [CanKetNoiTestFact]
    public async Task NFR07_TaiLen_VuotHaiMB_TuChoi()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var lon = TaoAnhKichThuoc(AnhService.KichThuocToiDa + 1);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.TaiLenAsync("BAN_DAU", new MemoryStream(lon), CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Empty(moi.LuuTru.DaTai);
    }

    [CanKetNoiTestFact]
    public async Task NFR07_TaiLen_DungHaiMB_ChapNhan()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var dung = TaoAnhKichThuoc(AnhService.KichThuocToiDa);

        var maAnh = await dichVuAnh.TaiLenAsync("KHI_TRA", new MemoryStream(dung), CancellationToken.None);

        Assert.Equal(AnhService.KichThuocToiDa, (await moi.DocAnhAsync(maAnh)).KichThuoc);
    }

    [CanKetNoiTestFact]
    public async Task UC07_TaiLen_GiaiDoanSai_TuChoi()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.TaiLenAsync("KHAC", new MemoryStream(AnhJpeg), CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Empty(moi.LuuTru.DaTai);
    }

    [CanKetNoiTestFact]
    public async Task UC07_TaiLen_GhiDongLoi_XoaObjectDaTai()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh(Guid.CreateVersion7());

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dichVuAnh.TaiLenAsync("BAN_DAU", new MemoryStream(AnhJpeg), CancellationToken.None));

        var daTai = Assert.Single(moi.LuuTru.DaTai);
        var daXoa = Assert.Single(moi.LuuTru.DaXoa);
        Assert.Equal((daTai.Bucket, daTai.Khoa), daXoa);
    }

    [CanKetNoiTestFact]
    public async Task UC07_Gan_DungNguoiTaiVaGiaiDoan_GanThanhCong()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var a1 = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        var a2 = await TaiAnhAsync(dichVuAnh, "BAN_DAU");

        await dichVuAnh.GanAsync([a1, a2], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);

        foreach (var ma in new[] { a1, a2 })
        {
            var dong = await moi.DocAnhAsync(ma);
            Assert.Equal("CHI_TIET_PHIEU_MUON", dong.DoiTuongLoai);
            Assert.Equal("ct-001", dong.DoiTuongId);
            Assert.Equal("BAN_DAU", dong.GiaiDoan);
        }
    }

    [CanKetNoiTestFact]
    public async Task NFR01_Gan_AnhCuaNguoiKhac_TuChoiVaGiuNguyenAnh()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var maAnh = await TaiAnhAsync(moi.TaoDichVuAnh(), "BAN_DAU");
        var nguoiKhac = await moi.TaoNguoiDungKhacAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => moi.TaoDichVuAnh(nguoiKhac).GanAsync([maAnh], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Null((await moi.DocAnhAsync(maAnh)).DoiTuongId);
    }

    [CanKetNoiTestFact]
    public async Task NFR01_Gan_SaiGiaiDoanHoacSaiBucket_TuChoi()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var anhBanDau = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        var anhThietBi = await TaiAnhAsync(dichVuAnh, "THIET_BI");

        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.GanAsync([anhBanDau], "CHI_TIET_PHIEU_MUON", "ct-001", "KHI_TRA", CancellationToken.None));
        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.GanAsync([anhThietBi], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None));

        Assert.Null((await moi.DocAnhAsync(anhBanDau)).DoiTuongId);
        Assert.Null((await moi.DocAnhAsync(anhThietBi)).DoiTuongId);
    }

    [CanKetNoiTestFact]
    public async Task NFR07_Gan_AnhDaGan_KhongGanLanHai()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var maAnh = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        await dichVuAnh.GanAsync([maAnh], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);

        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.GanAsync([maAnh], "CHI_TIET_PHIEU_MUON", "ct-002", "BAN_DAU", CancellationToken.None));

        Assert.Equal("ct-001", (await moi.DocAnhAsync(maAnh)).DoiTuongId);
    }

    [CanKetNoiTestFact]
    public async Task UC07_Gan_QuaNamAnhMoiThietBiMoiGiaiDoan_TuChoi()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var dsMa = new List<Guid>();
        for (var i = 0; i < 6; i++)
        {
            dsMa.Add(await TaiAnhAsync(dichVuAnh, "BAN_DAU"));
        }

        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.GanAsync(dsMa, "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None));
        Assert.Equal(0, await moi.DemAnhDaGanAsync());

        await dichVuAnh.GanAsync(dsMa.Take(5).ToArray(), "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);
        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.GanAsync([dsMa[5]], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None));

        Assert.Equal(5, await moi.DemAnhDaGanAsync());
    }

    [CanKetNoiTestFact]
    public async Task NFR01_Gan_MaAnhKhongTonTaiHoacTrung_TuChoi()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var maAnh = await TaiAnhAsync(dichVuAnh, "BAN_DAU");

        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.GanAsync([Guid.CreateVersion7()], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None));
        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => dichVuAnh.GanAsync([maAnh, maAnh], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task UC12_Gan_ToHopDoiTuongGiaiDoanSai_NemArgumentException()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var maAnh = await TaiAnhAsync(dichVuAnh, "THIET_BI");

        await Assert.ThrowsAsync<ArgumentException>(
            () => dichVuAnh.GanAsync([maAnh], "THIET_BI", "TB-001", "BAN_DAU", CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task UC12_GoAnhThietBi_GoLienKetNhungKhongXoaDong()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var maAnh = await TaiAnhAsync(dichVuAnh, "THIET_BI");
        await dichVuAnh.GanAsync([maAnh], "THIET_BI", "TB-001", "THIET_BI", CancellationToken.None);

        await dichVuAnh.GoAnhThietBiAsync("TB-001", [maAnh], CancellationToken.None);

        var dong = await moi.DocAnhAsync(maAnh);
        Assert.Null(dong.DoiTuongLoai);
        Assert.Null(dong.DoiTuongId);
        Assert.Null(dong.GiaiDoan);
    }

    [CanKetNoiTestFact]
    public async Task NFR07_GoAnhThietBi_KhongDungToiAnhCuaPhieu()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var maAnh = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        await dichVuAnh.GanAsync([maAnh], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);

        await dichVuAnh.GoAnhThietBiAsync("ct-001", [maAnh], CancellationToken.None);

        Assert.Equal("ct-001", (await moi.DocAnhAsync(maAnh)).DoiTuongId);
    }

    [CanKetNoiTestFact]
    public async Task NFR01_LayDsAnhKy_ChiTraAnhDungDoiTuongVaKyMuoiPhut()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var a1 = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        var a2 = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        var khac = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        var khiTra = await TaiAnhAsync(dichVuAnh, "KHI_TRA");
        await dichVuAnh.GanAsync([a1, a2], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);
        await dichVuAnh.GanAsync([khac], "CHI_TIET_PHIEU_MUON", "ct-002", "BAN_DAU", CancellationToken.None);
        await dichVuAnh.GanAsync([khiTra], "CHI_TIET_PHIEU_MUON", "ct-001", "KHI_TRA", CancellationToken.None);

        var dsAnh = await dichVuAnh.LayDsAnhKyAsync("CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);

        Assert.Equal(new HashSet<Guid> { a1, a2 }, dsAnh.Select(a => a.MaAnh).ToHashSet());
        Assert.All(dsAnh, a => Assert.StartsWith("https://luu-tru-gia.test/tinh-trang-anh/ban-dau/", a.Url));
        Assert.All(moi.LuuTru.HieuLucDaKy, h => Assert.Equal(TimeSpan.FromMinutes(10), h));
        Assert.Equal(2, moi.LuuTru.HieuLucDaKy.Count);
    }

    [CanKetNoiTestFact]
    public async Task NFR01_LayDsAnhKy_AnhCongKhaiThietBi_NemArgumentException()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();

        await Assert.ThrowsAsync<ArgumentException>(
            () => dichVuAnh.LayDsAnhKyAsync("THIET_BI", "TB-001", "THIET_BI", CancellationToken.None));
        Assert.Empty(moi.LuuTru.HieuLucDaKy);
    }

    [CanKetNoiTestFact]
    public async Task UC12_Gan_AdminKhacGanAnhThietBiDoAdminCuTai_ThanhCong()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var maAnh = await TaiAnhAsync(moi.TaoDichVuAnh(), "THIET_BI");
        var adminKhac = await moi.TaoNguoiDungKhacAsync();

        await moi.TaoDichVuAnh(adminKhac).GanAsync([maAnh], "THIET_BI", "TB-001", "THIET_BI", CancellationToken.None);

        var dong = await moi.DocAnhAsync(maAnh);
        Assert.Equal("THIET_BI", dong.DoiTuongLoai);
        Assert.Equal("TB-001", dong.DoiTuongId);
        Assert.Equal("THIET_BI", dong.GiaiDoan);
    }

    [CanKetNoiTestFact]
    public async Task UC12_LayDsAnhCongKhai_ThietBi_TraUrlCongKhaiDung()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var a1 = await TaiAnhAsync(dichVuAnh, "THIET_BI");
        var a2 = await TaiAnhAsync(dichVuAnh, "THIET_BI");
        await dichVuAnh.GanAsync([a1, a2], "THIET_BI", "TB-001", "THIET_BI", CancellationToken.None);

        var dsAnh = await dichVuAnh.LayDsAnhCongKhaiAsync("THIET_BI", "TB-001", CancellationToken.None);

        Assert.Equal(new HashSet<Guid> { a1, a2 }, dsAnh.Select(a => a.MaAnh).ToHashSet());
        Assert.All(dsAnh, a => Assert.StartsWith("https://luu-tru-gia.test/thiet-bi-anh/thiet-bi/", a.Url));
    }

    [CanKetNoiTestFact]
    public async Task UC07_LayDsAnhCongKhai_KhongPhaiThietBi_NemArgumentException()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();

        await Assert.ThrowsAsync<ArgumentException>(
            () => dichVuAnh.LayDsAnhCongKhaiAsync("CHI_TIET_PHIEU_MUON", "ct-001", CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task NFR01_LayDsAnhKy_MotAnhLoiStorage_BoQuaVaTraCacAnhConLai()
    {
        await using var moi = await MoiTruong.TaoAsync();
        var dichVuAnh = moi.TaoDichVuAnh();
        var a1 = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        var a2 = await TaiAnhAsync(dichVuAnh, "BAN_DAU");
        await dichVuAnh.GanAsync([a1, a2], "CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);

        var dong1 = await moi.DocAnhAsync(a1);
        moi.LuuTru.KhoaLoiKy.Add(dong1.KhoaLuuTru);

        var dsAnh = await dichVuAnh.LayDsAnhKyAsync("CHI_TIET_PHIEU_MUON", "ct-001", "BAN_DAU", CancellationToken.None);

        var anhDuyNhat = Assert.Single(dsAnh);
        Assert.Equal(a2, anhDuyNhat.MaAnh);
    }

    private static Task<Guid> TaiAnhAsync(AnhService dichVuAnh, string giaiDoan) =>
        dichVuAnh.TaiLenAsync(giaiDoan, new MemoryStream(AnhJpeg), CancellationToken.None);

    private static byte[] TaoAnhKichThuoc(int kichThuoc)
    {
        var duLieu = new byte[kichThuoc];
        duLieu[0] = 0xFF;
        duLieu[1] = 0xD8;
        duLieu[2] = 0xFF;

        return duLieu;
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

        public AnhStorageGia LuuTru { get; } = new();

        public Guid MaNv { get; }

        public static async Task<MoiTruong> TaoAsync()
        {
            var dichVu = new ServiceCollection()
                .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi())
                .BuildServiceProvider();
            var phamVi = dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            var giaoDich = await db.Database.BeginTransactionAsync();
            var maNv = await TaoNguoiDungAsync(db);

            return new MoiTruong(dichVu, phamVi, db, giaoDich, maNv);
        }

        public Task<Guid> TaoNguoiDungKhacAsync() => TaoNguoiDungAsync(Db);

        public AnhService TaoDichVuAnh(Guid? maNguoiDung = null) =>
            new(
                Db,
                LuuTru,
                new NguoiDungGia(maNguoiDung ?? MaNv),
                new DongHoCoDinh(BayGio),
                NullLogger<AnhService>.Instance);

        public Task<AnhTaiLen> DocAnhAsync(Guid maAnh) =>
            Db.Set<AnhTaiLen>().AsNoTracking().SingleAsync(a => a.MaAnh == maAnh);

        public Task<int> DemAnhCuaMinhAsync() =>
            Db.Set<AnhTaiLen>().CountAsync(a => a.NguoiTaiLen == MaNv);

        public Task<int> DemAnhDaGanAsync() =>
            Db.Set<AnhTaiLen>().CountAsync(a => a.NguoiTaiLen == MaNv && a.DoiTuongId != null);

        public async ValueTask DisposeAsync()
        {
            await _giaoDich.DisposeAsync();
            await _phamVi.DisposeAsync();
            await _dichVu.DisposeAsync();
        }

        private static async Task<Guid> TaoNguoiDungAsync(AppDbContext db)
        {
            var ma = Guid.CreateVersion7();
            var tenDangNhap = $"d08k-{ma:N}";
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
                VALUES ({ma}, {tenDangNhap}, 'khong-dung', 'Nguoi dung kiem thu D08-K')
                """);

            return ma;
        }
    }
}

