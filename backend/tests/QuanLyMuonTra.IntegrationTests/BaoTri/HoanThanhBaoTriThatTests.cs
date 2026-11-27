using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

[Collection(HoanThanhBaoTriDuLieu.TenBoSuuTap)]
public sealed class HoanThanhBaoTriThatTests
{
    private static readonly Guid MaNv = Guid.Parse(HoanThanhBaoTriDuLieu.MaNv);
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateTimeOffset NgaySuaChua = new(2026, 11, 22, 10, 0, 0, TimeSpan.FromHours(7));

    private sealed class OutboxLoi : IThongBaoOutbox
    {
        public Task GhiAsync(YeuCauGhiThongBao yeuCau, CancellationToken ct) =>
            Task.FromException(new InvalidOperationException("Giả lập lỗi ghi outbox."));
    }

    private sealed class NhatKyLoi : IAuditLogger
    {
        public Task GhiAsync(
            string thaoTac,
            string doiTuong,
            string? doiTuongId,
            IReadOnlyDictionary<string, object?>? giaTriTruoc,
            IReadOnlyDictionary<string, object?>? giaTriSau,
            CancellationToken ct) =>
            Task.FromException(new InvalidOperationException("Giả lập lỗi ghi nhật ký."));
    }

    [CanKetNoiTestFact]
    public async Task UC11_LoiOutbox_KhiCapLuot_HoanTacToanBoGiaoDich()
    {
        await ChayAsync(
            async dichVu =>
            {
                await using var phamVi = dichVu.CreateAsyncScope();
                var boXuLy = phamVi.ServiceProvider.GetRequiredService<HoanThanhBaoTriHandler>();

                await Assert.ThrowsAsync<InvalidOperationException>(() => boXuLy.XuLyAsync(
                    MaNv,
                    HoanThanhBaoTriDuLieu.BaoTri2,
                    new HoanThanhBaoTriRequest("Thay pin", "DUA_VE_SAN_SANG", NgaySuaChua),
                    Ct));

                await KiemTraNguyenTrangAsync(dichVu);
            },
            s => s.AddScoped<IThongBaoOutbox, OutboxLoi>());
    }

    [CanKetNoiTestFact]
    public async Task UC11_LoiNhatKy_KhiDoiTinhTrang_HoanTacToanBoGiaoDich()
    {
        await ChayAsync(
            async dichVu =>
            {
                await using var phamVi = dichVu.CreateAsyncScope();
                var boXuLy = phamVi.ServiceProvider.GetRequiredService<HoanThanhBaoTriHandler>();

                await Assert.ThrowsAsync<InvalidOperationException>(() => boXuLy.XuLyAsync(
                    MaNv,
                    HoanThanhBaoTriDuLieu.BaoTri2,
                    new HoanThanhBaoTriRequest("Thay pin", "DUA_VE_SAN_SANG", NgaySuaChua),
                    Ct));

                await KiemTraNguyenTrangAsync(dichVu);
            },
            s => s.AddScoped<IAuditLogger, NhatKyLoi>());
    }

    [CanKetNoiTestFact]
    public async Task UC11_HaiYeuCauHoanThanhSongSong_ChiMotThanhCongVaChiCapMotLuot()
    {
        await ChayAsync(async dichVu =>
        {
            var yeuCau = new HoanThanhBaoTriRequest("Thay pin", "DUA_VE_SAN_SANG", NgaySuaChua);

            async Task<LoiNghiepVuException?> ChayMotAsync()
            {
                await using var phamVi = dichVu.CreateAsyncScope();
                try
                {
                    await phamVi.ServiceProvider.GetRequiredService<HoanThanhBaoTriHandler>()
                        .XuLyAsync(MaNv, HoanThanhBaoTriDuLieu.BaoTri2, yeuCau, Ct);
                    return null;
                }
                catch (LoiNghiepVuException loi)
                {
                    return loi;
                }
            }

            var cacKetQua = await Task.WhenAll(ChayMotAsync(), ChayMotAsync());

            Assert.Equal(1, cacKetQua.Count(k => k is null));
            var cacLoi = cacKetQua.OfType<LoiNghiepVuException>().ToList();
            var loi = Assert.Single(cacLoi);
            Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(409, loi.MaHttp);

            Assert.Equal("SAN_SANG", await ChuoiAsync(dichVu, $"""
                SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = 'D15-TB-02'
                """));
            Assert.Equal(1, await DemAsync(dichVu, $"""
                SELECT count(*)::int AS "Value" FROM muon_tra.dang_ky_cho
                WHERE ma_dang_ky = {HoanThanhBaoTriDuLieu.Dk1} AND trang_thai = 'DA_THONG_BAO'
                """));
            Assert.Equal(1, await DemAsync(dichVu, $"""
                SELECT count(*)::int AS "Value" FROM muon_tra.dang_ky_cho
                WHERE ma_dang_ky = {HoanThanhBaoTriDuLieu.Dk2} AND trang_thai = 'DANG_CHO' AND so_thu_tu_cho = 1
                """));
            Assert.Equal(1, await DemAsync(dichVu, $"""
                SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao
                WHERE ma_sv_nhan = 'D15-SV-A' AND kenh_gui = 'UNG_DUNG'
                """));
            Assert.Equal(1, await DemAsync(dichVu, $"""
                SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
                WHERE ma_bao_tri = {HoanThanhBaoTriDuLieu.BaoTri2} AND ket_qua = 'DUA_VE_SAN_SANG'
                """));
        });
    }

    private static async Task KiemTraNguyenTrangAsync(ServiceProvider dichVu)
    {
        Assert.Equal(1, await DemAsync(dichVu, $"""
            SELECT count(*)::int AS "Value" FROM bao_tri.bao_tri_sua_chua
            WHERE ma_bao_tri = {HoanThanhBaoTriDuLieu.BaoTri2} AND ket_qua IS NULL
              AND ngay_hoan_thanh IS NULL AND noi_dung_thuc_hien IS NULL
            """));
        Assert.Equal("DANG_BAO_TRI_SUA_CHUA", await ChuoiAsync(dichVu, $"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = 'D15-TB-02'
            """));
        Assert.Equal(1, await DemAsync(dichVu, $"""
            SELECT count(*)::int AS "Value" FROM muon_tra.dang_ky_cho
            WHERE ma_dang_ky = {HoanThanhBaoTriDuLieu.Dk1} AND trang_thai = 'DANG_CHO' AND so_thu_tu_cho = 1
            """));
        Assert.Equal(1, await DemAsync(dichVu, $"""
            SELECT count(*)::int AS "Value" FROM muon_tra.dang_ky_cho
            WHERE ma_dang_ky = {HoanThanhBaoTriDuLieu.Dk2} AND trang_thai = 'DANG_CHO' AND so_thu_tu_cho = 2
            """));
        Assert.Equal(0, await DemAsync(dichVu, $"""
            SELECT count(*)::int AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D15-SV-%'
            """));
    }

    private static async Task ChayAsync(Func<ServiceProvider, Task> kiemTra, Action<IServiceCollection>? tuyChinh = null)
    {
        await using var dichVu = BaoTriTestSupport.TaoDichVu(tuyChinh);
        var sqlNap = await HoanThanhBaoTriDuLieu.DocTepAsync();
        await ThucThiAsync(dichVu, HoanThanhBaoTriDuLieu.XoaTatCa);
        await ThucThiAsync(dichVu, sqlNap);
        try
        {
            await kiemTra(dichVu);
        }
        finally
        {
            await ThucThiAsync(dichVu, HoanThanhBaoTriDuLieu.XoaTatCa);
        }
    }

    private static async Task ThucThiAsync(ServiceProvider dichVu, string sql)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task<int> DemAsync(ServiceProvider dichVu, FormattableString sql)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<int>(sql).SingleAsync();
    }

    private static async Task<string> ChuoiAsync(ServiceProvider dichVu, FormattableString sql)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<string>(sql).SingleAsync();
    }
}
