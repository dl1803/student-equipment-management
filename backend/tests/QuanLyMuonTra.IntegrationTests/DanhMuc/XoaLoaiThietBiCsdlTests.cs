using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.MuonTra;
using QuanLyMuonTra.Modules.QuyDinh;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

public sealed class XoaLoaiThietBiCsdlTests
{
    private const string MaPhongP1 = "019c0000-0000-7000-8000-0000000d0b01";
    private const string MaPhongP2 = "019c0000-0000-7000-8000-0000000d0b02";
    private const string MaLoaiL3 = "019c0000-0000-7000-8000-0000000d0c03";

    [CanKetNoiTestFact]
    public async Task UC12_XoaLoai_ConThietBiKeCaDaNgungHoacThanhLy_Nem409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddMuonTraModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaLoaiThietBiHandler>();
        var maLoai = Guid.Parse(MaLoaiL3);

        await Nem409Async(xoa, maLoai);

        await db.Database.ExecuteSqlRawAsync(
            "UPDATE danh_muc.thiet_bi SET ngung_su_dung = true, tinh_trang = 'DA_THANH_LY' WHERE ma_thiet_bi = 'D02-TB-08'");
        db.ChangeTracker.Clear();
        await Nem409Async(xoa, maLoai);

        var conLoai = await XoaNgungTestSupport.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.loai_thiet_bi WHERE ma_loai = {maLoai}
            """);
        Assert.Equal(1, conLoai);
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaLoai_CoYeuCauHoacDangKyCho_ProviderMuonTraChan409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddMuonTraModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoLoaiThietBiHandler>();
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaLoaiThietBiHandler>();
        var loaiYeuCau = await tao.XuLyAsync(new LoaiThietBiTaoMoiRequest("Loai D08 yeu cau", null, null, 30, 1000), CancellationToken.None);
        var loaiDangKy = await tao.XuLyAsync(new LoaiThietBiTaoMoiRequest("Loai D08 dang ky", null, null, 30, 1000), CancellationToken.None);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, thoi_gian_muon_du_kien_gio)
            VALUES ('YC-261120-990101', 'D02-SV-D', {Guid.Parse(MaPhongP1)}, 2)
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.chi_tiet_yeu_cau_muon (ma_yeu_cau, ma_loai, so_luong)
            VALUES ('YC-261120-990101', {loaiYeuCau.MaLoai}, 1)
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.dang_ky_cho (ma_dang_ky, ma_sv, ma_phong, ma_loai, so_thu_tu_cho)
            VALUES ('DK-261120-990101', 'D02-SV-D', {Guid.Parse(MaPhongP2)}, {loaiDangKy.MaLoai}, 1)
            """);

        await Nem409Async(xoa, loaiYeuCau.MaLoai);
        await Nem409Async(xoa, loaiDangKy.MaLoai);
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaLoai_CoQuyDinhMuonNgoaiLe_ProviderQuyDinhChan409()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVu(s => s.AddQuyDinhModule());
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        await XoaNgungTestSupport.NapDuLieuThuAsync(db);
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoLoaiThietBiHandler>();
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaLoaiThietBiHandler>();
        var loai = await tao.XuLyAsync(new LoaiThietBiTaoMoiRequest("Loai D08 ngoai le", null, null, 30, 1000), CancellationToken.None);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO quy_dinh.quy_dinh_muon (ma_quy_dinh, ma_nhom, ma_loai, so_thiet_bi_toi_da, thoi_gian_muon_toi_da_gio)
            VALUES ({Guid.CreateVersion7()}, 'DAI_HOC', {loai.MaLoai}, NULL, 3)
            """);

        await Nem409Async(xoa, loai.MaLoai);
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaLoai_KhongThamChieu_XoaKemNccVaGhiNhatKyXoa()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var tao = phamVi.ServiceProvider.GetRequiredService<TaoLoaiThietBiHandler>();
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaLoaiThietBiHandler>();
        var loai = await tao.XuLyAsync(new LoaiThietBiTaoMoiRequest("Loai D08 xoa duoc", null, null, 30, 1000), CancellationToken.None);
        var maNcc = await ThietBiGhiCsdlTests.TaoNccAsync(db, "NCC D08 loai");
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.loai_thiet_bi_nha_cung_cap (ma_loai, ma_ncc) VALUES ({loai.MaLoai}, {maNcc})
            """);

        await xoa.XuLyAsync(loai.MaLoai, CancellationToken.None);

        var conLoai = await XoaNgungTestSupport.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.loai_thiet_bi WHERE ma_loai = {loai.MaLoai}
            """);
        var conNoi = await XoaNgungTestSupport.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.loai_thiet_bi_nha_cung_cap WHERE ma_loai = {loai.MaLoai}
            """);
        var conNcc = await XoaNgungTestSupport.DemAsync(db, $"""
            SELECT count(*)::int AS "Value" FROM danh_muc.nha_cung_cap WHERE ma_ncc = {maNcc}
            """);
        Assert.Equal(0, conLoai);
        Assert.Equal(0, conNoi);
        Assert.Equal(1, conNcc);
        var thaoTac = await XoaNgungTestSupport.LayChuoiAsync(db, $"""
            SELECT thao_tac AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE doi_tuong = 'danh_muc.loai_thiet_bi' AND doi_tuong_id = {loai.MaLoai.ToString()}
            """);
        Assert.Contains("XOA", thaoTac);
    }

    [CanKetNoiTestFact]
    public async Task UC12_XoaLoai_KhongTonTai_Nem404()
    {
        await using var dichVu = XoaNgungTestSupport.TaoDichVuDayDu();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var giaoDich = await db.Database.BeginTransactionAsync();
        var xoa = phamVi.ServiceProvider.GetRequiredService<XoaLoaiThietBiHandler>();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => xoa.XuLyAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(404, loi.MaHttp);
    }

    private static async Task Nem409Async(XoaLoaiThietBiHandler xoa, Guid maLoai)
    {
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => xoa.XuLyAsync(maLoai, CancellationToken.None));
        Assert.Equal("DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
    }
}
