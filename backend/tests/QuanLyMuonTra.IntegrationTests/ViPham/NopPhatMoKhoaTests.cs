using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Auditing;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.Modules.ViPham.Application;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

public class NopPhatMoKhoaTests
{
    private static readonly DateTimeOffset BayGio = new(2026, 11, 28, 3, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset HanTra = new(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UC09_XacNhan_KhongConNo_PhieuChuaDenHanKhongCan_MoKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        await DatDaNopAsync(db, bb.MaBienBan);
        await ThemPhieuChuaTraAsync(db, nen, "DANG_MUON", BayGio.AddDays(2), false, "2");

        var ketQua = await TaoXacNhan(db).XuLyAsync(nen.MaSv, CancellationToken.None);

        Assert.True(ketQua.DaMoKhoa);
        Assert.Equal("BINH_THUONG", ketQua.TrangThaiQuyenMuon);
        Assert.Empty(ketQua.DsLyDoConKhoa);
        Assert.Equal("BINH_THUONG", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
    }

    [Fact]
    public async Task UC09_XacNhan_ConBienBanChuaNopOPhongKhac_VanKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var phongKhac = await ViPhamDuLieuThu.TaoPhongAsync(db);
        await ThemBienBanAsync(db, nen, phongKhac, "1");

        var ketQua = await TaoXacNhan(db).XuLyAsync(nen.MaSv, CancellationToken.None);

        Assert.False(ketQua.DaMoKhoa);
        Assert.Equal("TAM_KHOA", ketQua.TrangThaiQuyenMuon);
        var lyDo = Assert.Single(ketQua.DsLyDoConKhoa);
        Assert.Equal("SV_BI_KHOA_QUYEN_MUON", lyDo.MaLoi);
        Assert.Equal("TAM_KHOA", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
    }

    [Fact]
    public async Task UC09_XacNhan_PhieuQuaHanChuaTra_VanKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        await ThemPhieuChuaTraAsync(db, nen, "QUA_HAN", HanTra, false, "1");

        var ketQua = await TaoXacNhan(db).XuLyAsync(nen.MaSv, CancellationToken.None);

        Assert.False(ketQua.DaMoKhoa);
        Assert.Equal("TAM_KHOA", ketQua.TrangThaiQuyenMuon);
        Assert.Single(ketQua.DsLyDoConKhoa);
    }

    [Fact]
    public async Task UC09_XacNhan_PhieuQuaHanDaTraHet_MoKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        await ThemPhieuChuaTraAsync(db, nen, "QUA_HAN", HanTra, true, "1");

        var ketQua = await TaoXacNhan(db).XuLyAsync(nen.MaSv, CancellationToken.None);

        Assert.True(ketQua.DaMoKhoa);
        Assert.Equal("BINH_THUONG", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
    }

    [Fact]
    public async Task UC09_XacNhan_CamMuonDaiHan_KhongTuMo()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "CAM_MUON_DAI_HAN");

        var ketQua = await TaoXacNhan(db).XuLyAsync(nen.MaSv, CancellationToken.None);

        Assert.False(ketQua.DaMoKhoa);
        Assert.Equal("CAM_MUON_DAI_HAN", ketQua.TrangThaiQuyenMuon);
        Assert.Single(ketQua.DsLyDoConKhoa);
        Assert.Equal("CAM_MUON_DAI_HAN", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
    }

    [Fact]
    public async Task UC09_XacNhan_BinhThuong_KhongDoiVaKhongCoLyDo()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "BINH_THUONG");

        var ketQua = await TaoXacNhan(db).XuLyAsync(nen.MaSv, CancellationToken.None);

        Assert.False(ketQua.DaMoKhoa);
        Assert.Equal("BINH_THUONG", ketQua.TrangThaiQuyenMuon);
        Assert.Empty(ketQua.DsLyDoConKhoa);
    }

    [Fact]
    public async Task UC09_XacNhan_SinhVienKhongTonTai_Tra404()
    {
        await using var mt = await MoiTruong.TaoAsync();

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => TaoXacNhan(mt.Db).XuLyAsync("KT16-KHONG-CO", CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    [Fact]
    public async Task UC09_DaNop_HetNo_ChuyenDaNopGhiNhatKyVaMoKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");

        var ketQua = await TaoDaNop(db, nen.MaPhong).XuLyAsync(nen.MaNv, bb.MaBienBan, CancellationToken.None);

        Assert.Equal("DA_NOP", ketQua.TrangThai);
        Assert.Equal<DateTimeOffset?>(BayGio, ketQua.ThoiDiemNop);
        Assert.Equal(250000L, ketQua.TongTien);
        Assert.Equal("BINH_THUONG", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'GHI_NHAN_NOP_BIEN_BAN_VI_PHAM' AND doi_tuong_id = {bb.MaBienBan}
            AND gia_tri_truoc IS NOT NULL AND gia_tri_sau IS NOT NULL
            """));
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'SUA' AND doi_tuong_id = {nen.MaSv}
            """));
    }

    [Fact]
    public async Task UC09_DaNop_ConBienBanKhac_VanKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb1 = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        var bb2 = await ThemBienBanAsync(db, nen, nen.MaPhong, "2");

        var ketQua = await TaoDaNop(db, nen.MaPhong).XuLyAsync(nen.MaNv, bb1.MaBienBan, CancellationToken.None);

        Assert.Equal("DA_NOP", ketQua.TrangThai);
        Assert.Equal("TAM_KHOA", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
        Assert.Equal("CHUA_NOP", await TrangThaiBienBanAsync(db, bb2.MaBienBan));
    }

    [Fact]
    public async Task UC09_DaNop_ConThietBiQuaHanChuaTra_VanKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        await ThemPhieuChuaTraAsync(db, nen, "QUA_HAN", HanTra, false, "2");

        var ketQua = await TaoDaNop(db, nen.MaPhong).XuLyAsync(nen.MaNv, bb.MaBienBan, CancellationToken.None);

        Assert.Equal("DA_NOP", ketQua.TrangThai);
        Assert.Equal("TAM_KHOA", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
    }

    [Fact]
    public async Task UC09_DaNop_NhanVienKhacPhong_Tra403VaKhongDoiDuLieu()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => TaoDaNop(db, Guid.CreateVersion7()).XuLyAsync(nen.MaNv, bb.MaBienBan, CancellationToken.None));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Equal("CHUA_NOP", await TrangThaiBienBanAsync(db, bb.MaBienBan));
    }

    [Theory]
    [InlineData("DA_NOP")]
    [InlineData("DA_HUY")]
    public async Task UC09_DaNop_BienBanKhongConChuaNop_Tra409(string trangThai)
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        if (trangThai == "DA_NOP")
        {
            await DatDaNopAsync(db, bb.MaBienBan);
        }
        else
        {
            await DatDaHuyAsync(db, bb.MaBienBan);
        }

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => TaoDaNop(db, nen.MaPhong).XuLyAsync(nen.MaNv, bb.MaBienBan, CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal(trangThai, await TrangThaiBienBanAsync(db, bb.MaBienBan));
    }

    [Fact]
    public async Task UC09_DaNop_MaSaiDinhDangHoacKhongTonTai_Tra400Va404()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var boXuLy = TaoDaNop(mt.Db, Guid.CreateVersion7());

        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Guid.CreateVersion7(), "ABC", CancellationToken.None));
        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Guid.CreateVersion7(), "BB-999999-000001", CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", saiDinhDang.MaLoi);
        Assert.Equal("KHONG_TIM_THAY", khongTonTai.MaLoi);
    }

    [Fact]
    public async Task UC09_Huy_ChuaNopHetNo_DaHuyTrigerDongBoVaMoKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");

        var ketQua = await TaoHuy(db).XuLyAsync(bb.MaBienBan, CancellationToken.None);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Equal(250000L, ketQua.TongTien);
        Assert.Equal("BINH_THUONG", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM vi_pham.chi_tiet_bien_ban
            WHERE ma_bien_ban = {bb.MaBienBan} AND bien_ban_da_huy
            """));
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'HUY_BIEN_BAN_VI_PHAM' AND doi_tuong_id = {bb.MaBienBan}
            AND gia_tri_truoc IS NOT NULL AND gia_tri_sau IS NOT NULL
            """));
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM vi_pham.vi_pham WHERE ma_bien_ban = {bb.MaBienBan}
            """));
    }

    [Fact]
    public async Task UC09_Huy_BienBanDaNop_HuyDuocVaGiuThoiDiemNop()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "BINH_THUONG");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        await DatDaNopAsync(db, bb.MaBienBan);

        var ketQua = await TaoHuy(db).XuLyAsync(bb.MaBienBan, CancellationToken.None);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Equal<DateTimeOffset?>(BayGio, ketQua.ThoiDiemNop);
    }

    [Fact]
    public async Task UC09_Huy_ConBienBanKhac_VanKhoa()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb1 = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        var bb2 = await ThemBienBanAsync(db, nen, nen.MaPhong, "2");

        await TaoHuy(db).XuLyAsync(bb1.MaBienBan, CancellationToken.None);

        Assert.Equal("TAM_KHOA", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
        Assert.Equal("CHUA_NOP", await TrangThaiBienBanAsync(db, bb2.MaBienBan));
    }

    [Fact]
    public async Task UC09_Huy_CamMuonDaiHan_KhongTuMo()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "CAM_MUON_DAI_HAN");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");

        var ketQua = await TaoHuy(db).XuLyAsync(bb.MaBienBan, CancellationToken.None);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Equal("CAM_MUON_DAI_HAN", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
    }

    [Fact]
    public async Task UC09_Huy_DaHuyRoi_Tra409()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        await DatDaHuyAsync(db, bb.MaBienBan);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => TaoHuy(db).XuLyAsync(bb.MaBienBan, CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
    }

    [Fact]
    public async Task UC09_Huy_MaSaiDinhDangHoacKhongTonTai_Tra400Va404()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var boXuLy = TaoHuy(mt.Db);

        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync("ABC", CancellationToken.None));
        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync("BB-999999-000001", CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", saiDinhDang.MaLoi);
        Assert.Equal("KHONG_TIM_THAY", khongTonTai.MaLoi);
    }

    [Fact]
    public async Task UC09_DieuChinh_MotKhoan_TinhLaiTongVaGiuKhoanKhongNeu()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1", 2);
        var lenh = new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau(bb.MaThietBi, "HU_HONG", 100000)]);

        var ketQua = await TaoDieuChinh(db).XuLyAsync(bb.MaBienBan, lenh, CancellationToken.None);

        Assert.Equal("CHUA_NOP", ketQua.TrangThai);
        Assert.Equal(5100000L, ketQua.TongTien);
        Assert.Collection(
            ketQua.DsChiTiet,
            k =>
            {
                Assert.Equal(bb.MaThietBi, k.MaThietBi);
                Assert.Equal("HU_HONG", k.LoaiViPham);
                Assert.Equal(100000L, k.SoTien);
            },
            k =>
            {
                Assert.Equal(bb.MaThietBiMat, k.MaThietBi);
                Assert.Equal("MAT", k.LoaiViPham);
                Assert.Equal(5000000L, k.SoTien);
            });
        Assert.Equal("TAM_KHOA", await TrangThaiQuyenMuonAsync(db, nen.MaSv));
        Assert.Equal(1L, await DemAsync(db, $"""
            SELECT count(*) AS "Value" FROM nhat_ky.nhat_ky_he_thong
            WHERE thao_tac = 'DIEU_CHINH_BIEN_BAN_VI_PHAM' AND doi_tuong_id = {bb.MaBienBan}
            AND gia_tri_truoc IS NOT NULL AND gia_tri_sau IS NOT NULL
            """));

        await db.Database.ExecuteSqlRawAsync("SET CONSTRAINTS ALL IMMEDIATE");
    }

    [Theory]
    [InlineData("DA_NOP")]
    [InlineData("DA_HUY")]
    public async Task UC09_DieuChinh_BienBanKhongConChuaNop_Tra409(string trangThai)
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        if (trangThai == "DA_NOP")
        {
            await DatDaNopAsync(db, bb.MaBienBan);
        }
        else
        {
            await DatDaHuyAsync(db, bb.MaBienBan);
        }

        var lenh = new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau(bb.MaThietBi, "HU_HONG", 1000)]);
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => TaoDieuChinh(db).XuLyAsync(bb.MaBienBan, lenh, CancellationToken.None));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", loi.MaLoi);
        Assert.Equal(409, loi.MaHttp);
        Assert.Equal(250000L, await DemAsync(db, $"""
            SELECT tong_tien::bigint AS "Value" FROM vi_pham.bien_ban_vi_pham WHERE ma_bien_ban = {bb.MaBienBan}
            """));
    }

    [Fact]
    public async Task UC09_DieuChinh_DauVaoKhongHopLe_Tra400VaKhongDoiDuLieu()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var db = mt.Db;
        var nen = await TaoNenAsync(db, "TAM_KHOA");
        var bb = await ThemBienBanAsync(db, nen, nen.MaPhong, "1");
        var cacLenh = new[]
        {
            new DieuChinhBienBanLenh([]),
            new DieuChinhBienBanLenh(
            [
                new KhoanDieuChinhYeuCau(bb.MaThietBi, "HU_HONG", 1),
                new KhoanDieuChinhYeuCau(bb.MaThietBi, "HU_HONG", 2)
            ]),
            new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau(bb.MaThietBi, "HU_HONG", -1)]),
            new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau(bb.MaThietBi, "HU_HONG", 100_000_000_000_000)]),
            new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau(bb.MaThietBi, "KHONG_DEN_NHAN", 1)]),
            new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau("KHONG-CO", "HU_HONG", 1)]),
            new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau(bb.MaThietBi, "MAT", 1)])
        };

        foreach (var lenh in cacLenh)
        {
            var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
                () => TaoDieuChinh(db).XuLyAsync(bb.MaBienBan, lenh, CancellationToken.None));

            Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
        }

        Assert.Equal(250000L, await DemAsync(db, $"""
            SELECT tong_tien::bigint AS "Value" FROM vi_pham.bien_ban_vi_pham WHERE ma_bien_ban = {bb.MaBienBan}
            """));
    }

    [Fact]
    public async Task UC09_DieuChinh_MaSaiDinhDangHoacKhongTonTai_Tra400Va404()
    {
        await using var mt = await MoiTruong.TaoAsync();
        var boXuLy = TaoDieuChinh(mt.Db);
        var lenh = new DieuChinhBienBanLenh([new KhoanDieuChinhYeuCau("TB", "HU_HONG", 1)]);

        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync("ABC", lenh, CancellationToken.None));
        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync("BB-999999-000001", lenh, CancellationToken.None));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", saiDinhDang.MaLoi);
        Assert.Equal("KHONG_TIM_THAY", khongTonTai.MaLoi);
    }

    private static AuditLogger TaoNhatKy(AppDbContext db) =>
        new(db, new TacNhanHeThong(), new DocPhieuMuonTestSupport.DongHoCoDinh(BayGio));

    private static XetMoKhoaQuyenMuon TaoXet(AppDbContext db, AuditLogger nhatKy) =>
        new(db, new BienBanChuaNopReader(db), new QuyenMuonService(db, nhatKy));

    private static XacNhanNhanDuThietBiHandler TaoXacNhan(AppDbContext db)
    {
        var nhatKy = TaoNhatKy(db);

        return new XacNhanNhanDuThietBiHandler(db, TaoXet(db, nhatKy));
    }

    private static GhiNhanDaNopHandler TaoDaNop(AppDbContext db, params Guid[] dsMaPhong)
    {
        var nhatKy = TaoNhatKy(db);

        return new GhiNhanDaNopHandler(
            db,
            new DocPhieuMuonTestSupport.DongHoCoDinh(BayGio),
            new DocPhieuMuonTestSupport.QuyenPhongGia(dsMaPhong),
            TaoXet(db, nhatKy),
            nhatKy);
    }

    private static HuyBienBanHandler TaoHuy(AppDbContext db)
    {
        var nhatKy = TaoNhatKy(db);

        return new HuyBienBanHandler(db, TaoXet(db, nhatKy), nhatKy);
    }

    private static DieuChinhBienBanHandler TaoDieuChinh(AppDbContext db) => new(db, TaoNhatKy(db));

    private static Task<long> DemAsync(AppDbContext db, FormattableString cauLenh) =>
        db.Database.SqlQuery<long>(cauLenh).SingleAsync();

    private static Task<string> TrangThaiQuyenMuonAsync(AppDbContext db, string maSv) =>
        db.Database.SqlQuery<string>($"""
            SELECT trang_thai_quyen_muon AS "Value" FROM sinh_vien.sinh_vien WHERE ma_sv = {maSv}
            """).SingleAsync();

    private static Task<string> TrangThaiBienBanAsync(AppDbContext db, string maBienBan) =>
        db.Database.SqlQuery<string>($"""
            SELECT trang_thai AS "Value" FROM vi_pham.bien_ban_vi_pham WHERE ma_bien_ban = {maBienBan}
            """).SingleAsync();

    private static Task DatDaNopAsync(AppDbContext db, string maBienBan) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE vi_pham.bien_ban_vi_pham
            SET trang_thai = 'DA_NOP', thoi_diem_nop = {BayGio}
            WHERE ma_bien_ban = {maBienBan}
            """);

    private static Task DatDaHuyAsync(AppDbContext db, string maBienBan) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE vi_pham.bien_ban_vi_pham SET trang_thai = 'DA_HUY' WHERE ma_bien_ban = {maBienBan}
            """);

    private sealed record Nen(Guid MaNv, Guid MaPhong, Guid MaLoai, Guid MaKhung, string MaSv, string Hau);

    private sealed record BienBanTao(string MaPhieu, string MaBienBan, string MaThietBi, string MaThietBiMat);

    private static async Task<Nen> TaoNenAsync(AppDbContext db, string trangThaiQuyenMuon)
    {
        var hau = ViPhamDuLieuThu.MaHau();
        var maSv = $"KT16-SV-{hau}";
        var maNv = await ViPhamDuLieuThu.TaoNhanVienAsync(db, hau);
        var maPhong = await ViPhamDuLieuThu.TaoPhongAsync(db);
        var maLoai = await ViPhamDuLieuThu.TaoLoaiAsync(db, 50000);
        var maKhung = await ViPhamDuLieuThu.TaoKhungPhatAsync(db, 12.5m);
        await ViPhamDuLieuThu.TaoSinhVienAsync(db, maSv);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE sinh_vien.sinh_vien SET trang_thai_quyen_muon = {trangThaiQuyenMuon} WHERE ma_sv = {maSv}
            """);

        return new Nen(maNv, maPhong, maLoai, maKhung, maSv, hau);
    }

    private static async Task<BienBanTao> ThemBienBanAsync(
        AppDbContext db,
        Nen nen,
        Guid maPhong,
        string hauCon,
        int soKhoan = 1)
    {
        var maPhieu = $"PM-999996-{nen.Hau}{hauCon}";
        var maBienBan = $"BB-999996-{nen.Hau}{hauCon}";
        var huHong = $"KT16-TB-{nen.Hau}{hauCon}-1";
        var daMat = $"KT16-TB-{nen.Hau}{hauCon}-2";
        var tong = soKhoan == 2 ? 5250000m : 250000m;

        await ViPhamDuLieuThu.TaoThietBiAsync(db, huHong, nen.MaLoai, maPhong, 2000000);
        await ViPhamDuLieuThu.TaoPhieuAsync(db, maPhieu, nen.MaSv, nen.MaNv, HanTra);
        await ViPhamDuLieuThu.ThemChiTietPhieuAsync(
            db, maPhieu, huHong, 50000, "HU_HONG", "Vo man hinh", HanTra.AddHours(-1), nen.MaNv, nen.MaKhung);
        await ViPhamDuLieuThu.TaoBienBanAsync(db, maBienBan, maPhieu, nen.MaNv, tong, HanTra.AddDays(1));
        await ViPhamDuLieuThu.ThemChiTietBienBanAsync(
            db, maBienBan, maPhieu, huHong, "HU_HONG", null, 250000, nen.MaKhung);
        await ViPhamDuLieuThu.ThemViPhamAsync(db, nen.MaSv, "HU_HONG", HanTra.AddDays(1), maBienBan, null);

        if (soKhoan == 2)
        {
            await ViPhamDuLieuThu.TaoThietBiAsync(db, daMat, nen.MaLoai, maPhong, 5000000);
            await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, maPhieu, daMat, 50000, "MAT", null, null, nen.MaNv, null);
            await ViPhamDuLieuThu.ThemChiTietBienBanAsync(
                db, maBienBan, maPhieu, daMat, "MAT", null, 5000000, null);
            await ViPhamDuLieuThu.ThemViPhamAsync(db, nen.MaSv, "MAT", HanTra.AddDays(1), maBienBan, null);
        }

        return new BienBanTao(maPhieu, maBienBan, huHong, daMat);
    }

    private static async Task ThemPhieuChuaTraAsync(
        AppDbContext db,
        Nen nen,
        string trangThaiPhieu,
        DateTimeOffset hanTra,
        bool daTra,
        string hauCon)
    {
        var maPhieu = $"PM-999997-{nen.Hau}{hauCon}";
        var maThietBi = $"KT16-TB-{nen.Hau}{hauCon}-9";

        await ViPhamDuLieuThu.TaoThietBiAsync(db, maThietBi, nen.MaLoai, nen.MaPhong, 1000000);
        await ViPhamDuLieuThu.TaoPhieuAsync(db, maPhieu, nen.MaSv, nen.MaNv, hanTra);
        if (daTra)
        {
            await ViPhamDuLieuThu.ThemChiTietPhieuAsync(
                db, maPhieu, maThietBi, 50000, "NGUYEN_VEN", "Nguyen ven", hanTra.AddHours(1), nen.MaNv, null);
        }
        else
        {
            await ViPhamDuLieuThu.ThemChiTietPhieuAsync(db, maPhieu, maThietBi, 50000, null, null, null, null, null);
        }

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE muon_tra.phieu_muon SET trang_thai = {trangThaiPhieu} WHERE ma_phieu = {maPhieu}
            """);
    }

    private sealed class MoiTruong : IAsyncDisposable
    {
        private readonly ServiceProvider dichVu;
        private readonly AsyncServiceScope phamVi;
        private readonly IDbContextTransaction giaoDich;

        private MoiTruong(ServiceProvider dichVu, AsyncServiceScope phamVi, AppDbContext db, IDbContextTransaction giaoDich)
        {
            this.dichVu = dichVu;
            this.phamVi = phamVi;
            this.giaoDich = giaoDich;
            Db = db;
        }

        public AppDbContext Db { get; }

        public static async Task<MoiTruong> TaoAsync()
        {
            var dichVu = ViPhamDuLieuThu.TaoDichVu();
            var phamVi = dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            var giaoDich = await db.Database.BeginTransactionAsync();

            return new MoiTruong(dichVu, phamVi, db, giaoDich);
        }

        public async ValueTask DisposeAsync()
        {
            await giaoDich.DisposeAsync();
            await phamVi.DisposeAsync();
            await dichVu.DisposeAsync();
        }
    }
}

