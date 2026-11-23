using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.ViPham;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class XoaThietBiCsdlTests
{
    private const string MaQt = "019c0000-0000-7000-8000-0000000d0a09";
    private const string MaBaoTri = "019c0000-0000-7000-8000-0000000d0f01";

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_CoChiTietPhieuMuon_ProviderMuonTraChan409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddMuonTraModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);

        await Nem409VaGiuThietBiAsync(phamVi, db, "D02-TB-01");
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_CoChiTietBienBan_ProviderViPhamChan409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddViPhamModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);

        await Nem409VaGiuThietBiAsync(phamVi, db, "D02-TB-07");
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_CoBaoTriHoacThanhLy_ProviderBaoTriChan409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddBaoTriModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bao_tri.bao_tri_sua_chua (ma_bao_tri, ma_thiet_bi, ma_nv_thuc_hien, loai_thuc_hien)
            VALUES ({Guid.Parse(MaBaoTri)}, 'D02-TB-08', {Guid.Parse(XoaNgungTestSupport.MaNv)}, 'BAO_TRI_DINH_KY')
            """);

        await Nem409VaGiuThietBiAsync(phamVi, db, "D02-TB-08");

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM bao_tri.bao_tri_sua_chua WHERE ma_bao_tri = {Guid.Parse(MaBaoTri)}
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
            VALUES ({Guid.Parse(MaQt)}, 'd08-qt', 'khong-dung-de-dang-nhap', 'Quản trị D08')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.quan_tri_vien (ma_nguoi_dung, ma_qt) VALUES ({Guid.Parse(MaQt)}, 'D08-QT')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bao_tri.quyet_dinh_thanh_ly (ma_quyet_dinh, ma_qt_lap, ngay_thanh_ly)
            VALUES ('QD-261120-990101', {Guid.Parse(MaQt)}, DATE '2026-11-20')
            """);
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO bao_tri.chi_tiet_thanh_ly (ma_quyet_dinh, ma_thiet_bi) VALUES ('QD-261120-990101', 'D02-TB-08')");
        db.ChangeTracker.Clear();

        await Nem409VaGiuThietBiAsync(phamVi, db, "D02-TB-08");
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_KhongPhaiSanSang_Nem409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);

        foreach (var tinhTrang in new[] { "DANG_DUOC_MUON", "DANG_BAO_TRI_SUA_CHUA", "DA_THANH_LY", "DA_BAO_MAT" })
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE danh_muc.thiet_bi SET tinh_trang = {tinhTrang} WHERE ma_thiet_bi = 'D02-TB-08'
                """);
            db.ChangeTracker.Clear();

            await Nem409VaGiuThietBiAsync(phamVi, db, "D02-TB-08");
        }
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_ChiCoGiaTriBanDau_XoaKemGiaTriNccVaGhiNhatKyXoa()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var (maPhong, maLoai) = await ThietBiGhiCsdlTests.TaoPhongLoaiAsync(phamVi.ServiceProvider, "D08");
        var maNcc = await ThietBiGhiCsdlTests.TaoNccAsync(db, "NCC D08 thiet bi");
        const string ma = "TB-D08-XOA";
        await phamVi.ServiceProvider.GetRequiredService<TaoThietBiHandler>().XuLyAsync(
            new ThietBiTaoMoiRequest(ma, maLoai, maPhong, "Thiet bi D08", null, null, new DateOnly(2026, 1, 5), null, null, null, [maNcc], null, 1_500_000),
            CancellationToken.None);
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaThietBiHandler>();

        await xoa.XuLyAsync(ma, CancellationToken.None);

        Assert.Equal(0, await DemAsync(db, $"""SELECT count(*)::int AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {ma}"""));
        Assert.Equal(0, await DemAsync(db, $"""SELECT count(*)::int AS "Value" FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi = {ma}"""));
        Assert.Equal(0, await DemAsync(db, $"""SELECT count(*)::int AS "Value" FROM danh_muc.thiet_bi_nha_cung_cap WHERE ma_thiet_bi = {ma}"""));
        Assert.Equal(1, await DemAsync(db, $"""SELECT count(*)::int AS "Value" FROM danh_muc.nha_cung_cap WHERE ma_ncc = {maNcc}"""));
        Assert.Equal(1, await DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'XOA' AND doi_tuong = 'danh_muc.thiet_bi' AND doi_tuong_id = {ma}
            """));
        var mauGiaTri = $"%{ma}%";
        Assert.Equal(1, await DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'XOA' AND doi_tuong = 'danh_muc.lich_su_gia_tri' AND gia_tri_truoc::text LIKE {mauGiaTri}
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_GiuKhoaTuVanCapPhongLoaiTrongGiaoDich()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaThietBiHandler>();
        var truoc = await DemKhoaTuVanAsync(db);

        await xoa.XuLyAsync("D02-TB-08", CancellationToken.None);

        Assert.Equal(truoc + 1, await DemKhoaTuVanAsync(db));
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_KhongTonTai_Nem404()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaThietBiHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => xoa.XuLyAsync("KHONG-CO-D08", CancellationToken.None));

        Assert.Equal(404, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaThietBi_KhaDungKhongDuDoGiuCho_Nem409VaKhongXoa()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var khaDung = phamVi.ServiceProvider.GetRequiredService<IAvailabilityService>();
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaThietBiHandler>();
        var maPhong = Guid.Parse("019c0000-0000-7000-8000-0000000d0b01");
        var maLoai = Guid.Parse("019c0000-0000-7000-8000-0000000d0c01");

        // Tạo thiết bị mới hoàn toàn không có tham chiếu
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_phong, ma_loai, ten_thiet_bi, tinh_trang, ngung_su_dung, ngay_nhap_kho)
            VALUES ('D08-TB-MOI', {maPhong}, {maLoai}, 'Thiet bi test giu cho', 'SAN_SANG', false, '2026-01-01');
            """);

        var tongKhaDung = await khaDung.TinhKhaDungAsync(maPhong, maLoai, CancellationToken.None);

        // Tạo giữ chỗ hết hạn ngạch (khaDung = 0)
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, trang_thai, ma_nv_duyet, thoi_diem_duyet, han_giu_cho, thoi_gian_muon_du_kien_gio)
            VALUES ('YC-261123-999002', 'D02-SV-A', {maPhong}, 'DA_DUYET', '019c0000-0000-7000-8000-0000000d0a01', now(), now() + interval '1 hour', 4);
            INSERT INTO muon_tra.chi_tiet_yeu_cau_muon (ma_yeu_cau, ma_loai, so_luong)
            VALUES ('YC-261123-999002', {maLoai}, {tongKhaDung});
            """);

        Assert.Equal(0, await khaDung.TinhKhaDungAsync(maPhong, maLoai, CancellationToken.None));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => xoa.XuLyAsync("D08-TB-MOI", CancellationToken.None));

        Assert.Equal("SO_LUONG_KHA_DUNG_THAY_DOI", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal(1, await DemAsync(db, $"""SELECT count(*)::int AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = 'D08-TB-MOI'"""));
    }

    private static async Task Nem409VaGiuThietBiAsync(AsyncServiceScope phamVi, AppDbContext db, string ma)
    {
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaThietBiHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => xoa.XuLyAsync(ma, CancellationToken.None));

        Assert.Equal("DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal(1, await DemAsync(db, $"""SELECT count(*)::int AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {ma}"""));
    }

    private static Task<int> DemAsync(AppDbContext db, FormattableString sql) =>
        XoaNgungTestSupport.DemAsync(db, sql);

    private static Task<int> DemKhoaTuVanAsync(AppDbContext db) =>
        DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM pg_locks
            WHERE locktype = 'advisory' AND pid = pg_backend_pid() AND granted
            """);
}
