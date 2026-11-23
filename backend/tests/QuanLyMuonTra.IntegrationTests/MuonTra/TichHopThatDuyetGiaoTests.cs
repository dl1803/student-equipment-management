using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.IntegrationTests.ThongBao;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class TichHopThatDuyetGiaoTests
{
    private const string FileD07 = "D07-V-du-lieu-thu.sql";
    private const string FileD09 = "D09-V-du-lieu-thu.sql";
    private const string YcDuyetD07 = "YC-261118-997001";
    private const string YcGiaoD09 = "YC-261121-998001";
    private const string DkGiaoD09 = "DK-261121-998001";
    private const string X1 = "D09-TB-X1";
    private const string X2 = "D09-TB-X2";
    private const string X3 = "D09-TB-X3";
    private const string X4 = "D09-TB-X4";
    private const string X5 = "D09-TB-X5";
    private const string X6 = "D09-TB-X6";
    private const string Y1 = "D09-TB-Y1";
    private static readonly Guid NvD7 = Guid.Parse("019c0000-0000-7000-8000-0000000d7a01");
    private static readonly Guid NvD9 = Guid.Parse("019c0000-0000-7000-8000-0000000d9a01");
    private static readonly Guid P1D9 = Guid.Parse("019c0000-0000-7000-8000-0000000d9b01");
    private static readonly Guid LoaiXD9 = Guid.Parse("019c0000-0000-7000-8000-0000000d9c01");
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

    private sealed class DongAnhGan
    {
        public string? DoiTuongLoai { get; set; }

        public string? DoiTuongId { get; set; }

        public string? GiaiDoan { get; set; }
    }

    private static Task<string> TrangThaiYeuCauAsync(DbContext db, string maYeuCau) =>
        db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.phieu_yeu_cau_muon WHERE ma_yeu_cau = {maYeuCau}
            """).SingleAsync();

    private static Task<string> TrangThaiDangKyAsync(DbContext db, string maDangKy) =>
        db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM muon_tra.dang_ky_cho WHERE ma_dang_ky = {maDangKy}
            """).SingleAsync();

    private static Task<string> TinhTrangThietBiAsync(DbContext db, string maThietBi) =>
        db.Database.SqlQuery<string>($"""
            SELECT tinh_trang AS "Value" FROM danh_muc.thiet_bi WHERE ma_thiet_bi = {maThietBi}
            """).SingleAsync();

    private static Task<int> DemPhieuAsync(DbContext db, string maSv) =>
        db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM muon_tra.phieu_muon WHERE ma_sv = {maSv}
            """).SingleAsync();

    private static Task<int> DemThongBaoSvAsync(DbContext db, string maSv) =>
        db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM thong_bao.thong_bao WHERE ma_sv_nhan = {maSv}
            """).SingleAsync();

    private static Task<int> DemViPhamAsync(DbContext db) =>
        db.Database.SqlQuery<int>($"""
            SELECT ((SELECT count(*) FROM vi_pham.vi_pham) + (SELECT count(*) FROM vi_pham.bien_ban_vi_pham))::integer AS "Value"
            """).SingleAsync();

    private static Task<int> DemNhatKyAsync(DbContext db, string thaoTac, string doiTuongId, Guid maNv) =>
        db.Database.SqlQuery<int>($"""
            SELECT count(*)::integer AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = {thaoTac} AND doi_tuong_id = {doiTuongId} AND ma_nguoi_dung = {maNv}
            """).SingleAsync();

    private static Task<long> PhiLuuAsync(DbContext db, string maPhieu, string maThietBi) =>
        db.Database.SqlQuery<long>($"""
            SELECT phi_tre_moi_ngay::bigint AS "Value" FROM muon_tra.chi_tiet_phieu_muon
            WHERE ma_phieu = {maPhieu} AND ma_thiet_bi = {maThietBi}
            """).SingleAsync();

    private static Task<List<DongThongBaoKiemTra>> ThongBaoTheoKhoaAsync(DbContext db, string khoa) =>
        db.Database.SqlQuery<DongThongBaoKiemTra>($"""
            SELECT kenh_gui AS "KenhGui", trang_thai_gui AS "TrangThaiGui", noi_dung AS "NoiDung",
                   ma_sv_nhan AS "MaSvNhan", ma_nv_nhan AS "MaNvNhan", so_lan_thu AS "SoLanThu"
            FROM thong_bao.thong_bao
            WHERE khoa_trung_lap = {khoa}
            """).OrderBy(d => d.KenhGui).ToListAsync();

    private static Task<DongAnhGan> AnhGanAsync(DbContext db, Guid maAnh) =>
        db.Database.SqlQuery<DongAnhGan>($"""
            SELECT doi_tuong_loai AS "DoiTuongLoai", doi_tuong_id AS "DoiTuongId", giai_doan AS "GiaiDoan"
            FROM luu_tru.anh_tai_len WHERE ma_anh = {maAnh}
            """).SingleAsync();

    private static IReadOnlyList<ThietBiGiaoYeuCau> Ds(params string[] dsMa) =>
        dsMa.Select(ma => new ThietBiGiaoYeuCau(ma, "Tình trạng tốt", null)).ToList();

    [CanKetNoiTestFact]
    public async Task UC06_Duyet_ThuongThat_GhiEmailVaUngDungVaNhatKyCungGiaoDich()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD7, BayGioD7, true);
        await moi.NapDuLieuAsync(FileD07, QuyenD7);
        var thamSo = await moi.Lay<IQuyDinhReader>().LayThamSoAsync(Ct);

        var phieu = await moi.Lay<DuyetPhieuYeuCauMuonHandler>().XuLyAsync(NvD7, YcDuyetD07, Ct);

        Assert.Equal("DA_DUYET", phieu.TrangThai);
        Assert.Equal(BayGioD7.AddMinutes(thamSo.ThoiGianGiuChoPhut), phieu.HanGiuCho);
        Assert.Equal("DA_DUYET", await TrangThaiYeuCauAsync(moi.Db, YcDuyetD07));
        var cacDong = await ThongBaoTheoKhoaAsync(moi.Db, $"DUYET:{YcDuyetD07}");
        Assert.Equal(["EMAIL", "UNG_DUNG"], cacDong.Select(d => d.KenhGui).ToArray());
        var email = cacDong[0];
        var ungDung = cacDong[1];
        Assert.Equal("CHO_GUI", email.TrangThaiGui);
        Assert.StartsWith("<p>", email.NoiDung);
        Assert.Contains(YcDuyetD07, email.NoiDung);
        Assert.Equal("DA_GUI", ungDung.TrangThaiGui);
        Assert.Contains("đã được duyệt", ungDung.NoiDung);
        Assert.Contains("giữ chỗ đến", ungDung.NoiDung);
        Assert.All(cacDong, d =>
        {
            Assert.Equal("D07-SV-A", d.MaSvNhan);
            Assert.Null(d.MaNvNhan);
            Assert.Equal(0, d.SoLanThu);
        });
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "DUYET_YEU_CAU_MUON", YcDuyetD07, NvD7));
    }

    [CanKetNoiTestFact]
    public async Task UC06_TuChoi_ThuongThat_GhiLyDoVaOutboxVaKhongGhiViPham()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD7, BayGioD7, true);
        await moi.NapDuLieuAsync(FileD07, QuyenD7);
        var viPhamTruoc = await DemViPhamAsync(moi.Db);

        var phieu = await moi.Lay<TuChoiPhieuYeuCauMuonHandler>().XuLyAsync(NvD7, YcDuyetD07, "  Hết hàng  ", Ct);

        Assert.Equal("TU_CHOI", phieu.TrangThai);
        Assert.Equal("Hết hàng", phieu.LyDoTuChoi);
        Assert.Null(phieu.HanGiuCho);
        Assert.Equal(viPhamTruoc, await DemViPhamAsync(moi.Db));
        var cacDong = await ThongBaoTheoKhoaAsync(moi.Db, $"DUYET:{YcDuyetD07}");
        Assert.Equal(["EMAIL", "UNG_DUNG"], cacDong.Select(d => d.KenhGui).ToArray());
        Assert.Contains("đã bị từ chối", cacDong[1].NoiDung);
        Assert.Contains("Lý do: Hết hàng", cacDong[1].NoiDung);
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "TU_CHOI_YEU_CAU_MUON", YcDuyetD07, NvD7));
    }

    [CanKetNoiTestFact]
    public async Task UC07_PhieuYeuCau_CoAnh_GanAnhBanDauGhiNhatKyThatVaLuuPhiHan()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD9, BayGioD9, true);
        await moi.NapDuLieuAsync(FileD09, QuyenD9);
        var maAnh = await moi.Lay<IAnhService>().TaiLenAsync("BAN_DAU", new MemoryStream(AnhJpeg), Ct);
        IReadOnlyList<ThietBiGiaoYeuCau> dsThietBi =
        [
            new ThietBiGiaoYeuCau(X1, "Xước nhẹ vỏ máy", [maAnh]),
            new ThietBiGiaoYeuCau(X2, "Tốt", null),
            new ThietBiGiaoYeuCau(Y1, "Tốt", null)
        ];

        var phieu = await moi.Lay<GiaoThietBiHandler>().GiaoTheoPhieuYeuCauAsync(NvD9, YcGiaoD09, dsThietBi, Ct);

        Assert.Equal("D09-SV-A", phieu.MaSV);
        Assert.Equal(BayGioD9, phieu.NgayGioMuon);
        Assert.Equal(BayGioD9.AddHours(2), phieu.NgayGioHenTra);
        Assert.Equal("DA_NHAN", await TrangThaiYeuCauAsync(moi.Db, YcGiaoD09));
        Assert.Equal("DANG_DUOC_MUON", await TinhTrangThietBiAsync(moi.Db, X1));
        Assert.Equal("DANG_DUOC_MUON", await TinhTrangThietBiAsync(moi.Db, X2));
        Assert.Equal("DANG_DUOC_MUON", await TinhTrangThietBiAsync(moi.Db, Y1));
        Assert.Equal(50000, await PhiLuuAsync(moi.Db, phieu.MaPhieu, X1));
        Assert.Equal(30000, await PhiLuuAsync(moi.Db, phieu.MaPhieu, Y1));
        var chiTietX1 = phieu.DsChiTiet.Single(c => c.MaThietBi == X1);
        var anh = await AnhGanAsync(moi.Db, maAnh);
        Assert.Equal("CHI_TIET_PHIEU_MUON", anh.DoiTuongLoai);
        Assert.Equal(chiTietX1.MaChiTiet.ToString(), anh.DoiTuongId);
        Assert.Equal("BAN_DAU", anh.GiaiDoan);
        Assert.StartsWith("https://luu-tru-gia.test/tinh-trang-anh/", Assert.Single(chiTietX1.DsAnhBanDau).Url);
        Assert.Equal(TimeSpan.FromMinutes(10), Assert.Single(moi.LuuTru.HieuLucDaKy));
        Assert.Equal("tinh-trang-anh", Assert.Single(moi.LuuTru.DaTai).Bucket);
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "LAP_PHIEU_MUON", phieu.MaPhieu, NvD9));
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "NHAN_YEU_CAU_MUON", YcGiaoD09, NvD9));
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "SUA", X1, NvD9));
        Assert.Equal(0, await DemThongBaoSvAsync(moi.Db, "D09-SV-A"));
    }

    [CanKetNoiTestFact]
    public async Task UC07_DangKyCho_ThuongThat_ChuyenDaNhanVaGhiNhatKy()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD9, BayGioD9, true);
        await moi.NapDuLieuAsync(FileD09, QuyenD9);

        var phieu = await moi.Lay<GiaoThietBiHandler>().GiaoTheoDangKyChoAsync(NvD9, DkGiaoD09, Ds(X3), Ct);

        Assert.Equal("D09-SV-B", phieu.MaSV);
        Assert.Equal(DkGiaoD09, phieu.MaDangKy);
        Assert.Equal(BayGioD9.AddHours(5), phieu.NgayGioHenTra);
        Assert.Equal("DA_NHAN", await TrangThaiDangKyAsync(moi.Db, DkGiaoD09));
        Assert.Equal("DANG_DUOC_MUON", await TinhTrangThietBiAsync(moi.Db, X3));
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "LAP_PHIEU_MUON", phieu.MaPhieu, NvD9));
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "NHAN_DANG_KY_CHO", DkGiaoD09, NvD9));
    }

    [CanKetNoiTestFact]
    public async Task UC07_TrucTiep_ThuongThat_KhongCoNguonVaGhiNhatKyLapPhieu()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD9, BayGioD9, true);
        await moi.NapDuLieuAsync(FileD09, QuyenD9);

        var phieu = await moi.Lay<GiaoThietBiHandler>().GiaoTrucTiepAsync(
            NvD9,
            "D09-SV-C",
            P1D9,
            [new LoaiVaSoLuongYeuCau(LoaiXD9, 3)],
            Ds(X4, X5, X6),
            Ct);

        Assert.Null(phieu.MaYeuCau);
        Assert.Null(phieu.MaDangKy);
        Assert.Equal(3, phieu.SoThietBi);
        Assert.Equal(BayGioD9.AddHours(5), phieu.NgayGioHenTra);
        Assert.Equal("DANG_DUOC_MUON", await TinhTrangThietBiAsync(moi.Db, X6));
        Assert.Equal(1, await DemNhatKyAsync(moi.Db, "LAP_PHIEU_MUON", phieu.MaPhieu, NvD9));
    }

    [CanKetNoiTestFact]
    public async Task UC07_GuiLai_CungNguon_Tra409VaChiCoMotPhieu()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD9, BayGioD9, true);
        await moi.NapDuLieuAsync(FileD09, QuyenD9);
        var giao = moi.Lay<GiaoThietBiHandler>();
        await giao.GiaoTheoPhieuYeuCauAsync(NvD9, YcGiaoD09, Ds(X1, X2, Y1), Ct);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => giao.GiaoTheoPhieuYeuCauAsync(NvD9, YcGiaoD09, Ds(X3, X4, Y1), Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal(1, await DemPhieuAsync(moi.Db, "D09-SV-A"));
        Assert.Equal("SAN_SANG", await TinhTrangThietBiAsync(moi.Db, X3));
    }

    [CanKetNoiTestFact]
    public async Task UC07_CungThietBi_NguonKhac_Tra422VaKhongTaoPhieuMoi()
    {
        await using var moi = await MoiTruongTichHopMuonTra.TaoAsync(NvD9, BayGioD9, true);
        await moi.NapDuLieuAsync(FileD09, QuyenD9);
        var giao = moi.Lay<GiaoThietBiHandler>();
        await giao.GiaoTheoPhieuYeuCauAsync(NvD9, YcGiaoD09, Ds(X1, X2, Y1), Ct);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => giao.GiaoTrucTiepAsync(
            NvD9,
            "D09-SV-D",
            P1D9,
            [new LoaiVaSoLuongYeuCau(LoaiXD9, 1)],
            Ds(X1),
            Ct));

        Assert.Equal("THIET_BI_KHONG_SAN_SANG", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
        Assert.Equal(0, await DemPhieuAsync(moi.Db, "D09-SV-D"));
    }
}
