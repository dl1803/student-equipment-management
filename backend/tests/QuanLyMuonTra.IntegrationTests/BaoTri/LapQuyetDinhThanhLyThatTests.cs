using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.BaoTri.Application;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.BaoTri;

[Collection(ThanhLyDuLieu.TenBoSuuTap)]
public sealed class LapQuyetDinhThanhLyThatTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateOnly Ngay = new(2026, 11, 23);

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

    private static async Task<LoiNghiepVuException?> LapMotAsync(ServiceProvider dichVu, params string[] dsMa)
    {
        await using var phamVi = dichVu.CreateAsyncScope();
        try
        {
            await phamVi.ServiceProvider.GetRequiredService<LapQuyetDinhThanhLyHandler>().XuLyAsync(
                ThanhLyDuLieu.MaQtGuid,
                new LapQuyetDinhThanhLyRequest(Ngay, dsMa),
                Ct);
            return null;
        }
        catch (LoiNghiepVuException loi)
        {
            return loi;
        }
    }

    private static async Task KiemTraNguyenTrangAsync(ServiceProvider dichVu, params string[] dsMa)
    {
        foreach (var ma in dsMa)
        {
            Assert.Equal("SAN_SANG", await ThanhLyDuLieu.ChuoiAsync(dichVu, $"""
                SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {ma}
                """));
        }

        Assert.Equal(0, await ThanhLyDuLieu.DemQuyetDinhMoiAsync(dichVu));
    }

    [CanKetNoiTestFact]
    public async Task UC15_MotThietBiSaiTrongLo_HoanTacCaLo()
    {
        await ThanhLyDuLieu.ChayAsync(async dichVu =>
        {
            var loi = await LapMotAsync(dichVu, "D16-TB-04", "D16-TB-06");

            Assert.NotNull(loi);
            Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
            await KiemTraNguyenTrangAsync(dichVu, "D16-TB-04");
        });
    }

    [CanKetNoiTestFact]
    public async Task UC15_LoiGhiNhatKy_HoanTacToanBoGiaoDich()
    {
        await ThanhLyDuLieu.ChayAsync(
            async dichVu =>
            {
                await using var phamVi = dichVu.CreateAsyncScope();
                var boXuLy = phamVi.ServiceProvider.GetRequiredService<LapQuyetDinhThanhLyHandler>();

                await Assert.ThrowsAsync<InvalidOperationException>(() => boXuLy.XuLyAsync(
                    ThanhLyDuLieu.MaQtGuid,
                    new LapQuyetDinhThanhLyRequest(Ngay, ["D16-TB-04", "D16-TB-05"]),
                    Ct));

                await KiemTraNguyenTrangAsync(dichVu, "D16-TB-04", "D16-TB-05");
            },
            s => s.AddScoped<IAuditLogger, NhatKyLoi>());
    }

    [CanKetNoiTestFact]
    public async Task UC15_HaiQuyetDinhSongSongCungThietBi_ChiMotThanhCong()
    {
        await ThanhLyDuLieu.ChayAsync(async dichVu =>
        {
            var cacKetQua = await Task.WhenAll(
                LapMotAsync(dichVu, "D16-TB-04"),
                LapMotAsync(dichVu, "D16-TB-04"));

            Assert.Equal(1, cacKetQua.Count(k => k is null));
            var loi = Assert.Single(cacKetQua.OfType<LoiNghiepVuException>());
            Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(409, loi.MaHttp);
            Assert.Equal("DA_THANH_LY", await ThanhLyDuLieu.ChuoiAsync(dichVu, $"""
                SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = 'D16-TB-04'
                """));
            Assert.Equal(1, await ThanhLyDuLieu.DemQuyetDinhMoiAsync(dichVu));
            Assert.Equal(1, await ThanhLyDuLieu.DemAsync(dichVu, $"""
                SELECT count(*)::int AS "Value" FROM bao_tri.chi_tiet_thanh_ly WHERE ma_thiet_bi = 'D16-TB-04'
                """));
        });
    }
}
