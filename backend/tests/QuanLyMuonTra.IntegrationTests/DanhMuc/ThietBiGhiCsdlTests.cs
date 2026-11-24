using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.LuuTru;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Anh;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class ThietBiGhiCsdlTests
{
    [CanKetNoiTestFact]
    public async Task UC12_TaoThietBi_GhiThietBiGiaTriVaNccCungGiaoDichCoNhatKy()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "tao");
        var maNcc = await TaoNccAsync(db, "NCC D07-N tao");
        var handler = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();
        var ma = "TB-D07-TAO";

        var ketQua = await handler.XuLyAsync(YeuCauTao(ma, maLoai, maPhong, 2_500_000, [maNcc]), CancellationToken.None);

        Assert.Equal("SAN_SANG", ketQua.TinhTrang);
        Assert.False(ketQua.NgungSuDung);
        Assert.Equal(2_500_000L, ketQua.GiaTriHienHanh);
        Assert.Equal(maNcc, Assert.Single(ketQua.DsNhaCungCap).MaNCC);
        var soLichSu = await DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi = {ma}
            """);
        Assert.Equal(1, soLichSu);
        var nhatKyThietBi = await DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'THEM' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {ma}
            """);
        Assert.Equal(1, nhatKyThietBi);
        var mau = $"%{ma}%";
        var nhatKyGiaTri = await DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'THEM' AND doi_tuong = 'danh_muc.lich_su_gia_tri' AND gia_tri_sau::text LIKE {mau}
            """);
        Assert.Equal(1, nhatKyGiaTri);
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaoThietBi_TrungMa_Nem409()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "trung");
        var handler = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();
        await handler.XuLyAsync(YeuCauTao("TB-D07-TRUNG", maLoai, maPhong), CancellationToken.None);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(YeuCauTao("TB-D07-TRUNG", maLoai, maPhong), CancellationToken.None));

        Assert.Equal("MA_THIET_BI_TRUNG", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaoThietBi_ThamChieuKhongTonTai_Nem400()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "thamchieu");
        var ncc = await TaoNccAsync(db, "NCC Hop Le");
        var handler = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();

        var loaiSai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(YeuCauTao("TB-D07-L", Guid.NewGuid(), maPhong), CancellationToken.None));
        var phongSai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(YeuCauTao("TB-D07-P", maLoai, Guid.NewGuid()), CancellationToken.None));
        var nccSai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(YeuCauTao("TB-D07-N", maLoai, maPhong, 1, [Guid.NewGuid()]), CancellationToken.None));
        var nccTrung = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(YeuCauTao("TB-D07-TRUNG-NCC", maLoai, maPhong, 1, [ncc, ncc]), CancellationToken.None));

        Assert.All(new[] { loaiSai, phongSai, nccSai, nccTrung }, l =>
        {
            Assert.Equal("DU_LIEU_KHONG_HOP_LE", l.MaLoi);
            Assert.Equal(400, l.MaHttp);
        });
        Assert.Equal("Danh sách nhà cung cấp không được trùng.", nccTrung.Message);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaThietBi_DoiPhongLoaiVaThayNccGiuNguyenMaTinhTrangGiaTri()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhongA, maLoaiA) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "A");
        var (maPhongB, maLoaiB) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "B");
        var nccA = await TaoNccAsync(db, "NCC D07-N A");
        var nccB = await TaoNccAsync(db, "NCC D07-N B");
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();
        var sua = phamVi.ServiceProvider.GetRequiredService<SuaThietBiHandler>();
        await tao.XuLyAsync(YeuCauTao("TB-D07-SUA", maLoaiA, maPhongA, 1_000_000, [nccA]), CancellationToken.None);

        var ketQua = await sua.XuLyAsync(
            "TB-D07-SUA",
            new ThietBiCapNhatRequest(
                maLoaiB, maPhongB, "Ten moi", "Mo ta", null, new DateOnly(2026, 2, 1), new DateOnly(2027, 2, 1), "Ke 1", "Tu 2", [nccB], null),
            CancellationToken.None);

        Assert.Equal("TB-D07-SUA", ketQua.MaThietBi);
        Assert.Equal(maLoaiB, ketQua.MaLoai);
        Assert.Equal(maPhongB, ketQua.MaPhong);
        Assert.Equal("Ten moi", ketQua.TenThietBi);
        Assert.Equal("SAN_SANG", ketQua.TinhTrang);
        Assert.Equal(1_000_000L, ketQua.GiaTriHienHanh);
        Assert.Equal(new DateOnly(2027, 2, 1), ketQua.ThoiHanBaoHanh);
        Assert.Equal(nccB, Assert.Single(ketQua.DsNhaCungCap).MaNCC);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaThietBi_KhongGuiDsNccGiuNguyenVaMangRongGoHet()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "ncc");
        var ncc = await TaoNccAsync(db, "NCC D07-N giu");
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();
        var sua = phamVi.ServiceProvider.GetRequiredService<SuaThietBiHandler>();
        await tao.XuLyAsync(YeuCauTao("TB-D07-NCC", maLoai, maPhong, 1, [ncc]), CancellationToken.None);

        var giuNguyen = await sua.XuLyAsync("TB-D07-NCC", YeuCauSua(maLoai, maPhong, null), CancellationToken.None);
        var goHet = await sua.XuLyAsync("TB-D07-NCC", YeuCauSua(maLoai, maPhong, []), CancellationToken.None);

        Assert.Single(giuNguyen.DsNhaCungCap);
        Assert.Empty(goHet.DsNhaCungCap);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaThietBi_KhongTonTaiHoacThamChieuSai_NemDungMa()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "sua-loi");
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();
        var sua = phamVi.ServiceProvider.GetRequiredService<SuaThietBiHandler>();
        await tao.XuLyAsync(YeuCauTao("TB-D07-LOI", maLoai, maPhong), CancellationToken.None);

        var khongCo = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => sua.XuLyAsync("TB-KHONG-CO", YeuCauSua(maLoai, maPhong, null), CancellationToken.None));
        var loaiSai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => sua.XuLyAsync("TB-D07-LOI", YeuCauSua(Guid.NewGuid(), maPhong, null), CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", khongCo.MaLoi);
        Assert.Equal(404, khongCo.MaHttp);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loaiSai.MaLoi);
        Assert.Equal(400, loaiSai.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC12_CapNhatGiaTri_ThemBanGhiMoiVaGiuLichSu()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await TaoPhongLoaiAsync(phamVi.ServiceProvider, "giatri");
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>();
        var capNhat = phamVi.ServiceProvider.GetRequiredService<CapNhatGiaTriThietBiHandler>();
        var danhSach = phamVi.ServiceProvider.GetRequiredService<DsThietBiHandler>();
        var ma = "TB-D07-GT";
        await tao.XuLyAsync(YeuCauTao(ma, maLoai, maPhong, 1_000_000), CancellationToken.None);

        var ketQua = await capNhat.XuLyAsync(ma, new GiaTriThietBiRequest(2_000_000), CancellationToken.None);

        Assert.Equal(ma, ketQua.MaThietBi);
        Assert.Equal(2_000_000L, ketQua.GiaTri);
        var soLichSu = await DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi = {ma}
            """);
        Assert.Equal(2, soLichSu);
        var tim = await danhSach.XuLyAsync("Thiet bi TB-D07-GT", null, null, null, 1, 10, CancellationToken.None);
        Assert.Equal(2_000_000L, Assert.Single(tim.DanhSach).GiaTriHienHanh);
    }

    [CanKetNoiTestFact]
    public async Task UC12_CapNhatGiaTri_KhongTonTai_Nem404()
    {
        await using var dichVu = TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var capNhat = phamVi.ServiceProvider.GetRequiredService<CapNhatGiaTriThietBiHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => capNhat.XuLyAsync("TB-KHONG-CO", new GiaTriThietBiRequest(1), CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    internal static async Task<(Guid MaPhong, Guid MaLoai)> TaoPhongLoaiAsync(IServiceProvider dv, string hau)
    {
        var phong = await dv.GetRequiredService<TaoPhongThietBiHandler>()
            .XuLyAsync(new PhongThietBiRequest($"Phong D07-N {hau}", "Co so D07-N"), CancellationToken.None);
        var loai = await dv.GetRequiredService<TaoLoaiThietBiHandler>()
            .XuLyAsync(new LoaiThietBiTaoMoiRequest($"Loai D07-N {hau}", null, null, 30, 1000), CancellationToken.None);
        return (phong.MaPhong, loai.MaLoai);
    }

    internal static async Task<Guid> TaoNccAsync(AppDbContext db, string ten)
    {
        var ma = Guid.CreateVersion7();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.nha_cung_cap (ma_ncc, ten_ncc) VALUES ({ma}, {ten})
            """);
        return ma;
    }

    internal static Task<int> DemAsync(AppDbContext db, FormattableString sql) =>
        db.Database.SqlQuery<int>(sql).SingleAsync();

    internal static ServiceProvider TaoDichVu(
        NhatKyTests.NguoiDungGia? nguoiDung = null,
        AnhStorageGia? luuTru = null,
        IClock? dongHo = null)
    {
        var dichVu = new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(ThietBi).Assembly])
            .AddSingleton<ICurrentUser>(nguoiDung ?? new NhatKyTests.NguoiDungGia())
            .AddSingleton<IAnhStorage>(luuTru ?? new AnhStorageGia())
            .AddScoped<IAnhService, AnhService>()
            .AddLogging()
            .AddScoped<IAdvisoryLockService, AdvisoryLockService>()
            .AddDanhMucModule();
        if (dongHo is not null)
        {
            dichVu.AddSingleton(dongHo);
        }

        return dichVu.BuildServiceProvider();
    }

    private static ThietBiTaoMoiRequest YeuCauTao(
        string ma,
        Guid maLoai,
        Guid maPhong,
        long giaTri = 1_000_000,
        IReadOnlyList<Guid>? dsNcc = null) =>
        new(ma, maLoai, maPhong, $"Thiet bi {ma}", null, null, new DateOnly(2026, 1, 5), null, null, null, dsNcc, null, giaTri);

    private static ThietBiCapNhatRequest YeuCauSua(Guid maLoai, Guid maPhong, IReadOnlyList<Guid>? dsNcc) =>
        new(maLoai, maPhong, "Thiet bi sua", null, null, new DateOnly(2026, 1, 5), null, null, null, dsNcc, null);
}
