using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.ViPham.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

public class DeXuatBienBanTests
{
    [Fact]
    public async Task UC09_DeXuat_TinhDungBaKhoanVaBoQuaThietBiChuaCoKetQua()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var kichBan = await TaoKichBanAsync(db);
        var boXuLy = TaoHandler(db, kichBan.MaPhong);

        var ketQua = await boXuLy.XuLyAsync(kichBan.MaNv, kichBan.MaPhieu, CancellationToken.None);

        Assert.Equal(kichBan.MaPhieu, ketQua.MaPhieu);
        Assert.Equal(5350000L, ketQua.TongTien);
        Assert.Collection(
            ketQua.DsKhoan,
            khoan =>
            {
                Assert.Equal(kichBan.ThietBi1, khoan.MaThietBi);
                Assert.Equal($"Thiet bi {kichBan.ThietBi1}", khoan.TenThietBi);
                Assert.Equal("TRA_TRE", khoan.LoaiViPham);
                Assert.Equal(2, khoan.SoNgayTre);
                Assert.Equal(100000L, khoan.SoTien);
                Assert.Null(khoan.MaKhungPhat);
            },
            khoan =>
            {
                Assert.Equal(kichBan.ThietBi2, khoan.MaThietBi);
                Assert.Equal("HU_HONG", khoan.LoaiViPham);
                Assert.Null(khoan.SoNgayTre);
                Assert.Equal(250000L, khoan.SoTien);
                Assert.Equal(kichBan.MaKhung, khoan.MaKhungPhat);
            },
            khoan =>
            {
                Assert.Equal(kichBan.ThietBi3, khoan.MaThietBi);
                Assert.Equal("MAT", khoan.LoaiViPham);
                Assert.Null(khoan.SoNgayTre);
                Assert.Equal(5000000L, khoan.SoTien);
                Assert.Null(khoan.MaKhungPhat);
            });
        Assert.DoesNotContain(ketQua.DsKhoan, k => k.MaThietBi == kichBan.ThietBi4);
    }

    [Fact]
    public async Task UC09_DeXuat_LaThaoTacChiDoc_KhongTangSoDongCuaBangNaoLienQuan()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var kichBan = await TaoKichBanAsync(db);
        var boXuLy = TaoHandler(db, kichBan.MaPhong);
        var truoc = await DemAsync(db);

        await boXuLy.XuLyAsync(kichBan.MaNv, kichBan.MaPhieu, CancellationToken.None);
        await boXuLy.XuLyAsync(kichBan.MaNv, kichBan.MaPhieu, CancellationToken.None);
        var sau = await DemAsync(db);

        Assert.Equal(truoc, sau);
    }

    [Fact]
    public async Task UC09_DeXuat_PhieuKhongTonTai_Tra404()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var boXuLy = TaoHandler(db, Guid.CreateVersion7());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Guid.CreateVersion7(), $"PM-999999-{ViPhamDuLieuThu.MaHau()}", CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    [Fact]
    public async Task UC09_DeXuat_MaPhieuSaiDinhDang_Tra400()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var boXuLy = TaoHandler(db, Guid.CreateVersion7());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Guid.CreateVersion7(), "PM-123", CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(400, loi.MaHttp);
    }

    [Fact]
    public async Task UC09_DeXuat_NhanVienPhongKhac_Tra403()
    {
        await using var dichVu = ViPhamDuLieuThu.TaoDichVu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var kichBan = await TaoKichBanAsync(db);
        var boXuLy = TaoHandler(db, Guid.CreateVersion7());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(kichBan.MaNv, kichBan.MaPhieu, CancellationToken.None));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
    }

    private static DeXuatBienBanHandler TaoHandler(AppDbContext db, params Guid[] dsMaPhong) =>
        new(
            new PhieuMuonReader(db, new DocPhieuMuonTestSupport.DongHoCoDinh(DateTimeOffset.UtcNow)),
            new DanhMucReader(db),
            new QuyDinhReader(db),
            new DocPhieuMuonTestSupport.QuyenPhongGia(dsMaPhong));

    private static async Task<long> DemAsync(AppDbContext db)
    {
        return await db.Database.SqlQuery<long>($"""
            SELECT (SELECT count(*) FROM vi_pham.vi_pham)
                 + (SELECT count(*) FROM vi_pham.bien_ban_vi_pham)
                 + (SELECT count(*) FROM vi_pham.chi_tiet_bien_ban)
                 + (SELECT count(*) FROM muon_tra.chi_tiet_phieu_muon) AS "Value"
            """).SingleAsync();
    }

    private static async Task<KichBan> TaoKichBanAsync(AppDbContext db)
    {
        var hau = ViPhamDuLieuThu.MaHau();
        var maSv = $"KT14-SV-{hau}";
        var maNv = await ViPhamDuLieuThu.TaoNhanVienAsync(db, hau);
        var maPhong = await ViPhamDuLieuThu.TaoPhongAsync(db);
        var maLoai = await ViPhamDuLieuThu.TaoLoaiAsync(db, 50000);
        var maKhung = await ViPhamDuLieuThu.TaoKhungPhatAsync(db, 12.5m);
        var maPhieu = $"PM-999993-{hau}";
        var hanTra = new DateTimeOffset(2026, 10, 10, 9, 0, 0, TimeSpan.Zero);
        var thietBi1 = $"KT14-TB-{hau}-1";
        var thietBi2 = $"KT14-TB-{hau}-2";
        var thietBi3 = $"KT14-TB-{hau}-3";
        var thietBi4 = $"KT14-TB-{hau}-4";

        await ViPhamDuLieuThu.TaoSinhVienAsync(db, maSv);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, thietBi1, maLoai, maPhong, 10000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, thietBi2, maLoai, maPhong, 2000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, thietBi3, maLoai, maPhong, 5000000);
        await ViPhamDuLieuThu.TaoThietBiAsync(db, thietBi4, maLoai, maPhong, 1000000);
        await ViPhamDuLieuThu.TaoPhieuAsync(db, maPhieu, maSv, maNv, hanTra);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(
            db, maPhieu, thietBi1, 50000, "NGUYEN_VEN", "Nguyen ven", new DateTimeOffset(2026, 10, 12, 5, 0, 0, TimeSpan.Zero), maNv, null);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(
            db, maPhieu, thietBi2, 50000, "HU_HONG", "Vo man hinh", new DateTimeOffset(2026, 10, 10, 8, 0, 0, TimeSpan.Zero), maNv, maKhung);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, maPhieu, thietBi3, 50000, "MAT", null, null, maNv, null);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, maPhieu, thietBi4, 50000, null, null, null, null, null);

        return new KichBan(maNv, maPhong, maKhung, maPhieu, thietBi1, thietBi2, thietBi3, thietBi4);
    }

    private sealed record KichBan(
        Guid MaNv,
        Guid MaPhong,
        Guid MaKhung,
        string MaPhieu,
        string ThietBi1,
        string ThietBi2,
        string ThietBi3,
        string ThietBi4);
}

