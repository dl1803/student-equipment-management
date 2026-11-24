using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Application;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public class DangNhapNhanVienHandlerTests
{
    private const string MatKhauDung = "MatKhauKiemThu-1";
    private const int NguongKhoa = 3;

    [Fact]
    public async Task UC19_DangNhapDung_TraTokenHopLeVaLuuBamRefreshToken()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tenDangNhap = await ThemNhanVienAsync(db, "HOAT_DONG");
        var boXuLy = TaoBoXuLy(db);

        var ketQua = await boXuLy.XuLyAsync(tenDangNhap, MatKhauDung, CancellationToken.None);

        Assert.Equal("NHAN_VIEN", ketQua.VaiTro);
        Assert.False(string.IsNullOrWhiteSpace(ketQua.AccessToken));
        var maBamDaLuu = await db.Database.SqlQuery<string>($"""
            SELECT ma_bam AS "Value" FROM tai_khoan.refresh_token WHERE ma_bam = {RefreshToken.Bam(ketQua.RefreshToken)}
            """).ToListAsync();
        Assert.Single(maBamDaLuu);
        Assert.NotEqual(ketQua.RefreshToken, maBamDaLuu[0]);
    }

    [Fact]
    public async Task UC19_SaiMatKhau_Nem401VaTangBoDem()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tenDangNhap = await ThemNhanVienAsync(db, "HOAT_DONG");
        var boXuLy = TaoBoXuLy(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(tenDangNhap, "sai-mat-khau", CancellationToken.None));

        Assert.Equal("DANG_NHAP_SAI", loi.MaLoi);
        Assert.Equal(1, await LaySoLanSaiAsync(db, tenDangNhap));
    }

    [Fact]
    public async Task UC19_DatNguongSai_TamKhoaVaTuChoiCaKhiNhapDungMatKhau()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tenDangNhap = await ThemNhanVienAsync(db, "HOAT_DONG");
        var boXuLy = TaoBoXuLy(db);

        for (var lan = 0; lan < NguongKhoa; lan++)
        {
            var loiSai = await Assert.ThrowsAsync<LoiNghiepVuException>(
                () => boXuLy.XuLyAsync(tenDangNhap, "sai-mat-khau", CancellationToken.None));
            Assert.Equal("DANG_NHAP_SAI", loiSai.MaLoi);
        }

        Assert.Equal("TAM_KHOA_DANG_NHAP", await LayTrangThaiAsync(db, tenDangNhap));
        var loiKhoa = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(tenDangNhap, MatKhauDung, CancellationToken.None));
        Assert.Equal("TAI_KHOAN_BI_KHOA", loiKhoa.MaLoi);
    }

    [Fact]
    public async Task UC19_DangNhapDungSauKhiSaiMotVai_DatLaiBoDem()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tenDangNhap = await ThemNhanVienAsync(db, "HOAT_DONG");
        var boXuLy = TaoBoXuLy(db);
        await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(tenDangNhap, "sai-mat-khau", CancellationToken.None));

        await boXuLy.XuLyAsync(tenDangNhap, MatKhauDung, CancellationToken.None);

        Assert.Equal(0, await LaySoLanSaiAsync(db, tenDangNhap));
    }

    [Fact]
    public async Task UC19_TaiKhoanBiKhoaDoQuanTri_TuChoi403()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tenDangNhap = await ThemNhanVienAsync(db, "BI_KHOA");
        var boXuLy = TaoBoXuLy(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(tenDangNhap, MatKhauDung, CancellationToken.None));

        Assert.Equal("TAI_KHOAN_BI_KHOA", loi.MaLoi);
    }

    [Fact]
    public async Task UC19_TenDangNhapKhongTonTai_Nem401()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var boXuLy = TaoBoXuLy(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync($"khong-co-{Guid.NewGuid():N}", MatKhauDung, CancellationToken.None));

        Assert.Equal("DANG_NHAP_SAI", loi.MaLoi);
    }

    private static DangNhapNhanVienHandler TaoBoXuLy(AppDbContext db) =>
        new(
            db,
            new PasswordHasher<NguoiDungHeThong>(),
            new QuyDinhGia(),
            new PhatHanhAccessToken(
                new JwtOptions("khoa-kiem-thu-khong-phai-bi-mat-0123456789", "kt", "kt"),
                new DongHoGia()),
            new DongHoGia());

    private static async Task<string> ThemNhanVienAsync(AppDbContext db, string trangThaiKhoa)
    {
        var ma = await QuyenPhongTests.ThemNhanVienAsync(db, trangThaiKhoa);
        return await db.Database.SqlQuery<string>($"""
            SELECT ten_dang_nhap AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {ma}
            """).SingleAsync();
    }

    private static Task<int> LaySoLanSaiAsync(AppDbContext db, string tenDangNhap) =>
        db.Database.SqlQuery<int>($"""
            SELECT so_lan_dang_nhap_sai AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ten_dang_nhap = {tenDangNhap}
            """).SingleAsync();

    private static Task<string> LayTrangThaiAsync(AppDbContext db, string tenDangNhap) =>
        db.Database.SqlQuery<string>($"""
            SELECT trang_thai_khoa AS "Value" FROM tai_khoan.nguoi_dung_he_thong WHERE ten_dang_nhap = {tenDangNhap}
            """).SingleAsync();

    private static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(NguoiDungHeThong).Assembly])
            .BuildServiceProvider();

    private sealed class DongHoGia : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 11, 16, 3, 0, 0, TimeSpan.Zero);
    }

    private sealed class QuyDinhGia : IQuyDinhReader
    {
        public Task<ThamSoHeThongDto> LayThamSoAsync(CancellationToken ct) =>
            Task.FromResult(new ThamSoHeThongDto(3, 30, 15, 2, 3, NguongKhoa, null, null));

        public Task<QuyDinhNhomDto?> LayQuyDinhNhomAsync(string maNhom, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<KhungPhatDto?> LayKhungPhatAsync(Guid maKhung, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<KhungPhatDto>> DsKhungPhatDangSuDungAsync(CancellationToken ct) =>
            throw new NotSupportedException();
    }
}

