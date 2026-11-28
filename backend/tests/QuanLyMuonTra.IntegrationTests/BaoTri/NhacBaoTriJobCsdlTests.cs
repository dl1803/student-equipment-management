using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

public sealed class NhacBaoTriJobTests
{
    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    [Fact]
    public void UC17_J6_DuocDangKyTrongAddBaoTriModule()
    {
        var dichVu = new ServiceCollection().AddBaoTriModule();

        Assert.Contains(
            dichVu,
            d => d.ServiceType == typeof(IJob) && d.ImplementationType == typeof(NhacBaoTriJob));
    }

    [Fact]
    public async Task UC17_J6_KhongCoGiaoDich_BiTuChoiVaCoThongTinJobDung()
    {
        using var dichVu = new ServiceCollection()
            .AddInfrastructure("Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung")
            .BuildServiceProvider();
        using var phamVi = dichVu.CreateScope();
        var job = new NhacBaoTriJob(
            phamVi.ServiceProvider.GetRequiredService<AppDbContext>(),
            new DongHoCoDinh(BaoTriTestSupport.BayGio),
            null!);

        Assert.Equal("J6", job.MaJob);
        Assert.Equal("NhacBaoTri", job.TenJob);
        Assert.Equal(TimeSpan.FromHours(1), job.ChuKy);
        await Assert.ThrowsAsync<InvalidOperationException>(() => job.ChayAsync(CancellationToken.None));
    }
}

[Collection(ThanhLyDuLieu.TenBoSuuTap)]
public sealed class NhacBaoTriJobCsdlTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static NhacBaoTriJob TaoJob(BaoTriMoiTruong mt) =>
        new(mt.Db, mt.Lay<IClock>(), mt.Lay<IThongBaoOutbox>());

    private static Task<int> DemAsync(BaoTriMoiTruong mt, string khoa, string kenh) =>
        mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao
            WHERE khoa_trung_lap = {khoa} AND kenh_gui = {kenh}
            """);

    private static Task<int> DemTongAsync(BaoTriMoiTruong mt) =>
        mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao
            WHERE ma_nv_nhan IN ({ThanhLyDuLieu.MaNv1}::uuid, {ThanhLyDuLieu.MaNv2}::uuid, {ThanhLyDuLieu.MaNv3}::uuid)
            """);

    [CanKetNoiTestFact]
    public async Task UC17_J6_DenHanHomNay_GhiChoNvCoQuyenPhongDungPhong_NvThieuEmailChiCoUngDung()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);

        var soPhanTu = await TaoJob(mt).ChayAsync(Ct);

        Assert.True(soPhanTu >= 3);
        var khoaNv1 = $"BAO_TRI:D16-TB-11:2026-11-23:{ThanhLyDuLieu.MaNv1}";
        var khoaNv2 = $"BAO_TRI:D16-TB-11:2026-11-23:{ThanhLyDuLieu.MaNv2}";
        var khoaNv3 = $"BAO_TRI:D16-TB-13:2026-01-31:{ThanhLyDuLieu.MaNv3}";
        Assert.Equal(1, await DemAsync(mt, khoaNv1, "UNG_DUNG"));
        Assert.Equal(1, await DemAsync(mt, khoaNv1, "EMAIL"));
        Assert.Equal(1, await DemAsync(mt, khoaNv2, "UNG_DUNG"));
        Assert.Equal(0, await DemAsync(mt, khoaNv2, "EMAIL"));
        Assert.Equal(1, await DemAsync(mt, khoaNv3, "UNG_DUNG"));
        Assert.Equal(1, await DemAsync(mt, khoaNv3, "EMAIL"));
        Assert.Equal(5, await DemTongAsync(mt));
        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao WHERE khoa_trung_lap LIKE 'BAO_TRI:D16-TB-12:%'
            """));
        Assert.Equal(1, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao
            WHERE khoa_trung_lap = {khoaNv1} AND kenh_gui = 'UNG_DUNG' AND loai_thong_bao = 'NHAC_BAO_TRI'
              AND doi_tuong_loai = 'THIET_BI' AND doi_tuong_id = 'D16-TB-11'
              AND noi_dung = 'Thiết bị D16-TB-11 đến hạn bảo trì định kỳ vào ngày 23/11/2026.'
            """));
    }

    [CanKetNoiTestFact]
    public async Task UC17_J6_ChayLai_KhongThemDongTrung()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);
        var job = TaoJob(mt);

        await job.ChayAsync(Ct);
        var sauLanMot = await DemTongAsync(mt);
        await job.ChayAsync(Ct);

        Assert.Equal(5, sauLanMot);
        Assert.Equal(sauLanMot, await DemTongAsync(mt));
    }

    [CanKetNoiTestFact]
    public async Task UC17_J6_CungTapThietBiVoiDanhSachDenHanCuaNhanVien()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);
        var dsDenHan = mt.Lay<DsThietBiDenHanBaoTriHandler>();

        var cuaNv1 = await dsDenHan.XuLyAsync(ThanhLyDuLieu.MaNv1Guid, null, 1, 100, Ct);
        var cuaNv3 = await dsDenHan.XuLyAsync(ThanhLyDuLieu.MaNv3Guid, null, 1, 100, Ct);

        Assert.Equal(
            ["D16-TB-11"],
            cuaNv1.DanhSach.Where(d => d.MaThietBi.StartsWith("D16-TB-", StringComparison.Ordinal)).Select(d => d.MaThietBi).ToArray());
        Assert.Equal(
            ["D16-TB-13"],
            cuaNv3.DanhSach.Where(d => d.MaThietBi.StartsWith("D16-TB-", StringComparison.Ordinal)).Select(d => d.MaThietBi).ToArray());
    }

    [CanKetNoiTestFact]
    public async Task UC17_J6_ThietBiNgungSuDungDenHan_KhongTaoThongBao()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);
        await mt.ThucThiAsync("""
            UPDATE danh_muc.thiet_bi
            SET ngung_su_dung = true
            WHERE ma_thiet_bi = 'D16-TB-11'
            """);

        await TaoJob(mt).ChayAsync(Ct);

        Assert.Equal(0, await mt.DemAsync($"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao
            WHERE khoa_trung_lap LIKE 'BAO_TRI:D16-TB-11:%'
            """));
        Assert.Equal(2, await DemTongAsync(mt));
    }

    [CanKetNoiTestFact]
    public async Task UC17_J6_NhanVienKhongHoatDong_KhongTaoThongBaoVaEmail()
    {
        await using var mt = await BaoTriMoiTruong.TaoAsync(ThanhLyDuLieu.TenTep);
        await mt.ThucThiAsync($"""
            UPDATE tai_khoan.nguoi_dung_he_thong
            SET trang_thai_khoa = 'BI_KHOA'
            WHERE ma_nguoi_dung = '{ThanhLyDuLieu.MaNv1}';

            UPDATE tai_khoan.nguoi_dung_he_thong
            SET trang_thai_khoa = 'TAM_KHOA_DANG_NHAP'
            WHERE ma_nguoi_dung = '{ThanhLyDuLieu.MaNv3}';
            """);

        await TaoJob(mt).ChayAsync(Ct);

        var khoaNv1 = $"BAO_TRI:D16-TB-11:2026-11-23:{ThanhLyDuLieu.MaNv1}";
        var khoaNv2 = $"BAO_TRI:D16-TB-11:2026-11-23:{ThanhLyDuLieu.MaNv2}";
        var khoaNv3 = $"BAO_TRI:D16-TB-13:2026-01-31:{ThanhLyDuLieu.MaNv3}";
        Assert.Equal(0, await DemAsync(mt, khoaNv1, "UNG_DUNG"));
        Assert.Equal(0, await DemAsync(mt, khoaNv1, "EMAIL"));
        Assert.Equal(0, await DemAsync(mt, khoaNv3, "UNG_DUNG"));
        Assert.Equal(0, await DemAsync(mt, khoaNv3, "EMAIL"));
        Assert.Equal(1, await DemAsync(mt, khoaNv2, "UNG_DUNG"));
        Assert.Equal(1, await DemTongAsync(mt));
    }
}
