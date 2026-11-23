using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class HoanTacThatTests
{
    private const string YcDuyetD07 = "YC-261118-997001";
    private const string YcGiaoD09 = "YC-261121-998001";
    private static readonly Guid NvD7 = Guid.Parse("019c0000-0000-7000-8000-0000000d7a01");
    private static readonly Guid NvD9 = Guid.Parse("019c0000-0000-7000-8000-0000000d9a01");
    private static readonly DateTimeOffset BayGioD7 = new(2026, 11, 18, 3, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset BayGioD9 = new(2026, 11, 21, 3, 0, 0, TimeSpan.Zero);
    private static readonly byte[] AnhJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];
    private static readonly CancellationToken Ct = CancellationToken.None;

    private const string QuyenD7 = """
        INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
        VALUES ('019c0000-0000-7000-8000-0000000d7a01', '019c0000-0000-7000-8000-0000000d7b01');
        """;

    private const string QuyenD9 = """
        INSERT INTO tai_khoan.nhan_vien_phong (ma_nguoi_dung, ma_phong)
        VALUES ('019c0000-0000-7000-8000-0000000d9a01', '019c0000-0000-7000-8000-0000000d9b01');
        """;

    private const string XoaD7 = """
        DELETE FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D07-SV-%';
        DELETE FROM muon_tra.chi_tiet_phieu_muon WHERE ma_thiet_bi LIKE 'D07-TB-%';
        DELETE FROM muon_tra.phieu_muon WHERE ma_sv LIKE 'D07-SV-%';
        DELETE FROM muon_tra.chi_tiet_yeu_cau_muon WHERE ma_yeu_cau LIKE 'YC-261118-997%';
        DELETE FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau LIKE 'YC-261118-997%';
        DELETE FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D07-TB-%';
        DELETE FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D07-TB-%';
        DELETE FROM danh_muc.loai_thiet_bi WHERE ma_loai IN ('019c0000-0000-7000-8000-0000000d7c01', '019c0000-0000-7000-8000-0000000d7c02', '019c0000-0000-7000-8000-0000000d7c03');
        DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d7a01', '019c0000-0000-7000-8000-0000000d7a02');
        DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong IN ('019c0000-0000-7000-8000-0000000d7b01', '019c0000-0000-7000-8000-0000000d7b02');
        DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d7a01', '019c0000-0000-7000-8000-0000000d7a02');
        DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d7a01', '019c0000-0000-7000-8000-0000000d7a02');
        DELETE FROM sinh_vien.sinh_vien WHERE ma_sv LIKE 'D07-SV-%';
        DELETE FROM quy_dinh.quy_dinh_muon WHERE ma_nhom = 'D07-NHOM';
        DELETE FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = 'D07-NHOM';
        """;

    private const string XoaD9 = """
        DELETE FROM luu_tru.anh_tai_len WHERE nguoi_tai_len IN ('019c0000-0000-7000-8000-0000000d9a01', '019c0000-0000-7000-8000-0000000d9a02');
        DELETE FROM thong_bao.thong_bao WHERE ma_sv_nhan LIKE 'D09-SV-%';
        DELETE FROM muon_tra.chi_tiet_phieu_muon WHERE ma_thiet_bi LIKE 'D09-TB-%';
        DELETE FROM muon_tra.phieu_muon WHERE ma_sv LIKE 'D09-SV-%';
        DELETE FROM muon_tra.chi_tiet_yeu_cau_muon WHERE ma_yeu_cau LIKE 'YC-261121-998%';
        DELETE FROM muon_tra.dang_ky_cho WHERE ma_dang_ky LIKE 'DK-261121-998%';
        DELETE FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau LIKE 'YC-261121-998%';
        DELETE FROM danh_muc.lich_su_gia_tri WHERE ma_thiet_bi LIKE 'D09-TB-%';
        DELETE FROM danh_muc.thiet_bi WHERE ma_thiet_bi LIKE 'D09-TB-%';
        DELETE FROM danh_muc.loai_thiet_bi WHERE ma_loai IN ('019c0000-0000-7000-8000-0000000d9c01', '019c0000-0000-7000-8000-0000000d9c02', '019c0000-0000-7000-8000-0000000d9c03');
        DELETE FROM tai_khoan.nhan_vien_phong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d9a01', '019c0000-0000-7000-8000-0000000d9a02');
        DELETE FROM danh_muc.phong_thiet_bi WHERE ma_phong IN ('019c0000-0000-7000-8000-0000000d9b01', '019c0000-0000-7000-8000-0000000d9b02');
        DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d9a01', '019c0000-0000-7000-8000-0000000d9a02');
        DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung IN ('019c0000-0000-7000-8000-0000000d9a01', '019c0000-0000-7000-8000-0000000d9a02');
        DELETE FROM sinh_vien.sinh_vien WHERE ma_sv LIKE 'D09-SV-%';
        DELETE FROM quy_dinh.quy_dinh_muon WHERE ma_nhom = 'D09-NHOM';
        DELETE FROM sinh_vien.nhom_sinh_vien WHERE ma_nhom = 'D09-NHOM';
        """;

    private sealed class OutboxLoi : IThongBaoOutbox
    {
        public Task GhiAsync(YeuCauGhiThongBao yeuCau, CancellationToken ct) =>
            Task.FromException(new InvalidOperationException("Giả lập lỗi ghi outbox."));
    }

    private sealed class DongAnhGan
    {
        public string? DoiTuongLoai { get; set; }

        public string? DoiTuongId { get; set; }
    }

    private static Task<int> DemAsync(DbContext db, FormattableString cauLenh) =>
        db.Database.SqlQuery<int>(cauLenh).SingleAsync();

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_OutboxLoi_HoanTacToanBoVaPhieuVanChoDuyet()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(
            NvD7,
            BayGioD7,
            false,
            dichVu => dichVu.AddScoped<IThongBaoOutbox, OutboxLoi>());

        try
        {
            await moi.ThucThiSqlAsync(XoaD7);
            await moi.NapDuLieuAsync("D07-V-du-lieu-thu.sql", QuyenD7);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => moi.Lay<DuyetPhieuYeuCauMuonHandler>().XuLyAsync(NvD7, YcDuyetD07, Ct));

            Assert.Equal(1, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_yeu_cau_muon
                WHERE ma_yeu_cau = {YcDuyetD07} AND trang_thai = 'CHO_DUYET' AND han_giu_cho IS NULL
                """));
            Assert.Equal(0, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan = 'D07-SV-A'
                """));
            Assert.Equal(0, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM nhat_ky.nhat_ky_he_thong
                WHERE thao_tac = 'DUYET_YEU_CAU_MUON' AND doi_tuong_id = {YcDuyetD07}
                """));
        }
        finally
        {
            await moi.ThucThiSqlAsync(XoaD7);
        }
    }

    [CanKetNoiTestFact]
    public async Task UC07_Giao_LoiOThietBiThuHai_HoanTacTrangThaiPhieuVaLienKetAnh()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD9, BayGioD9, false);

        try
        {
            await moi.ThucThiSqlAsync(XoaD9);
            await moi.NapDuLieuAsync("D09-V-du-lieu-thu.sql", QuyenD9);
            var maAnh = await moi.Lay<IAnhService>().TaiLenAsync("BAN_DAU", new MemoryStream(AnhJpeg), Ct);
            IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi =
            [
                new ThietBiGiaoYeuCau("D09-TB-X1", "Tốt", [maAnh]),
                new ThietBiGiaoYeuCau("D09-TB-X2", "Tốt", [Guid.CreateVersion7()]),
                new ThietBiGiaoYeuCau("D09-TB-Y1", "Tốt", null)
            ];

            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
                () => moi.Lay<GiaoThietBiHandler>().GiaoTheoPhieuYeuCauAsync(NvD9, YcGiaoD09, dsThietBi, Ct));

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
            Assert.Equal(3, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM danh_muc.thiet_bi
                WHERE ma_thiet_bi IN ('D09-TB-X1', 'D09-TB-X2', 'D09-TB-Y1') AND tinh_trang = 'SAN_SANG'
                """));
            Assert.Equal(1, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_yeu_cau_muon
                WHERE ma_yeu_cau = {YcGiaoD09} AND trang_thai = 'DA_DUYET'
                """));
            Assert.Equal(0, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_muon WHERE ma_sv = 'D09-SV-A'
                """));
            Assert.Equal(0, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM muon_tra.chi_tiet_phieu_muon WHERE ma_thiet_bi LIKE 'D09-TB-%'
                """));
            var anh = await moi.Db.Database.SqlQuery<DongAnhGan>($"""
                SELECT doi_tuong_loai AS "DoiTuongLoai", doi_tuong_id AS "DoiTuongId"
                FROM luu_tru.anh_tai_len WHERE ma_anh = {maAnh}
                """).SingleAsync();
            Assert.Null(anh.DoiTuongLoai);
            Assert.Null(anh.DoiTuongId);
            Assert.Equal(0, await DemAsync(moi.Db, $"""
                SELECT count(*)::integer AS "Value" FROM nhat_ky.nhat_ky_he_thong
                WHERE thao_tac = 'SUA' AND doi_tuong_id IN ('D09-TB-X1', 'D09-TB-X2', 'D09-TB-Y1')
                """));
        }
        finally
        {
            await moi.ThucThiSqlAsync(XoaD9);
        }
    }
}
