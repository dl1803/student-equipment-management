using QuanLyMuonTra.IntegrationTests.C3;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

[Collection(C3BoSuuTap.Ten)]
public sealed class DanhMucReaderCsdlTests
{
    private const string MaPhongA = "019c0000-0000-7000-8000-000000000001";
    private const string MaPhongB = "019c0000-0000-7000-8000-000000000002";
    private const string MaLoaiLaptop = "019c0000-0000-7000-8000-000000000011";
    private const string MaLoaiMayChieu = "019c0000-0000-7000-8000-000000000012";

    [CanKetNoiTestFact]
    public async Task UC12_LayThietBi_NamTinhTrangTrongDuLieuThu_TraDungMaTinhTrang()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);
        var mongDoi = new (string Ma, string TinhTrang)[]
        {
            ("TEST-LAP-001", "SAN_SANG"),
            ("TEST-LAP-003", "DANG_DUOC_MUON"),
            ("TEST-LAP-004", "DANG_BAO_TRI_SUA_CHUA"),
            ("TEST-LAP-102", "DA_THANH_LY"),
            ("TEST-MC-002", "DA_BAO_MAT"),
        };

        foreach (var (ma, tinhTrang) in mongDoi)
        {
            var thietBi = await reader.LayThietBiAsync(ma, CancellationToken.None);

            Assert.NotNull(thietBi);
            Assert.Equal(tinhTrang, thietBi!.TinhTrang);
        }
    }

    [CanKetNoiTestFact]
    public async Task UC12_LayThietBi_ThietBiThu001_TraDuTruongVaGiaTriHienHanh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);

        var ketQua = await reader.LayThietBiAsync("TEST-LAP-001", CancellationToken.None);

        Assert.NotNull(ketQua);
        var thietBi = ketQua!;
        Assert.Equal("TEST-LAP-001", thietBi.MaThietBi);
        Assert.Equal(Guid.Parse(MaLoaiLaptop), thietBi.MaLoai);
        Assert.Equal(Guid.Parse(MaPhongA), thietBi.MaPhong);
        Assert.Equal("Laptop thử 001", thietBi.TenThietBi);
        Assert.Equal(new DateOnly(2026, 1, 5), thietBi.NgayNhapKho);
        Assert.False(thietBi.NgungSuDung);
        Assert.Null(thietBi.ViTriKe);
        Assert.Null(thietBi.ViTriTu);
        Assert.Equal(15000000m, thietBi.GiaTriHienHanh);
    }

    [CanKetNoiTestFact]
    public async Task UC12_LayThietBi_CoNhieuLichSuGiaTri_LayBanGhiMoiNhat()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await C3TestSupport.ThucThiAsync(
            gd,
            "INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri, ngay_cap_nhat) VALUES ('019c0000-0000-7000-8000-0000000f0001', 'TEST-LAP-001', 16000000, now() + interval '1 hour')");
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);

        var ketQua = await reader.LayThietBiAsync("TEST-LAP-001", CancellationToken.None);

        Assert.NotNull(ketQua);
        Assert.Equal(16000000m, ketQua!.GiaTriHienHanh);
    }

    [CanKetNoiTestFact]
    public async Task UC12_LayThietBi_MaKhongTonTai_TraNull()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);

        Assert.Null(await reader.LayThietBiAsync("KHONG-TON-TAI", CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task UC12_LayLoai_LoaiLaptopVaMaKhongTonTai()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);
        var ct = CancellationToken.None;

        var laptop = await reader.LayLoaiAsync(Guid.Parse(MaLoaiLaptop), ct);

        Assert.NotNull(laptop);
        Assert.Equal("Laptop", laptop!.TenLoai);
        Assert.Equal(180, laptop.ChuKyBaoTri);
        Assert.Equal(50000m, laptop.PhiTreMoiNgay);
        Assert.False(laptop.NgungSuDung);
        Assert.Null(await reader.LayLoaiAsync(Guid.NewGuid(), ct));
    }

    [CanKetNoiTestFact]
    public async Task UC12_LayPhong_PhongAVaMaKhongTonTai()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);
        var ct = CancellationToken.None;

        var phong = await reader.LayPhongAsync(Guid.Parse(MaPhongA), ct);

        Assert.NotNull(phong);
        Assert.Equal("Phòng Thiết bị A", phong!.TenPhong);
        Assert.Equal("Cơ sở 1", phong.CoSo);
        Assert.Null(await reader.LayPhongAsync(Guid.NewGuid(), ct));
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsPhong_CoHaiPhongThuVaSapTheoTen()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);

        var danhSach = await reader.DsPhongAsync(CancellationToken.None);

        var thuTu = danhSach.Select(p => p.MaPhong).ToList();
        var viTriA = thuTu.IndexOf(Guid.Parse(MaPhongA));
        var viTriB = thuTu.IndexOf(Guid.Parse(MaPhongB));
        Assert.True(viTriA >= 0);
        Assert.True(viTriB > viTriA);
    }

    [CanKetNoiTestFact]
    public async Task UC12_DsLoaiDangSuDung_LoaiNgungSuDungKhongXuatHien()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var reader = new DanhMucReader(db);
        var ct = CancellationToken.None;

        var truoc = (await reader.DsLoaiDangSuDungAsync(ct)).Select(l => l.MaLoai).ToList();
        await C3TestSupport.ThucThiAsync(
            gd,
            $"UPDATE danh_muc.loai_thiet_bi SET ngung_su_dung = true WHERE ma_loai = '{MaLoaiMayChieu}'");
        var sau = (await reader.DsLoaiDangSuDungAsync(ct)).Select(l => l.MaLoai).ToList();

        Assert.Contains(Guid.Parse(MaLoaiLaptop), truoc);
        Assert.Contains(Guid.Parse(MaLoaiMayChieu), truoc);
        Assert.Contains(Guid.Parse(MaLoaiLaptop), sau);
        Assert.DoesNotContain(Guid.Parse(MaLoaiMayChieu), sau);
        Assert.True(truoc.IndexOf(Guid.Parse(MaLoaiLaptop)) < truoc.IndexOf(Guid.Parse(MaLoaiMayChieu)));
    }
}
