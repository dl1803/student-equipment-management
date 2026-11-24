using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.IntegrationTests.C3;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.DanhMuc.Application;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.DanhMuc;

[Collection(C3BoSuuTap.Ten)]
public sealed class GhiPhongLoaiCsdlTests
{
    private const string MaPhongA = "019c0000-0000-7000-8000-000000000001";
    private const string MaLoaiLaptop = "019c0000-0000-7000-8000-000000000011";
    private const string MaKhongTonTai = "019c0000-0000-7000-8000-0000000fffff";

    [CanKetNoiTestFact]
    public async Task UC12_TaoPhong_LuuVaoCsdlVaDocLaiDuoc()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new TaoPhongThietBiHandler(db);

        var ketQua = await handler.XuLyAsync(new PhongThietBiRequest("Phòng D05-N", "Cơ sở D05-N"), CancellationToken.None);

        var daLuu = await db.Set<PhongThietBi>().AsNoTracking().SingleAsync(p => p.MaPhong == ketQua.MaPhong);
        Assert.Equal("Phòng D05-N", daLuu.TenPhong);
        Assert.Equal("Cơ sở D05-N", daLuu.CoSo);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaPhong_DoiTenVaCoSoTrongCsdl()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new SuaPhongThietBiHandler(db);

        var ketQua = await handler.XuLyAsync(
            Guid.Parse(MaPhongA),
            new PhongThietBiRequest("Phòng đã sửa", "Cơ sở đã sửa"),
            CancellationToken.None);

        db.ChangeTracker.Clear();
        var daLuu = await db.Set<PhongThietBi>().AsNoTracking().SingleAsync(p => p.MaPhong == Guid.Parse(MaPhongA));
        Assert.Equal(Guid.Parse(MaPhongA), ketQua.MaPhong);
        Assert.Equal("Phòng đã sửa", daLuu.TenPhong);
        Assert.Equal("Cơ sở đã sửa", daLuu.CoSo);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaPhong_KhongTonTai_Nem404()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new SuaPhongThietBiHandler(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(Guid.Parse(MaKhongTonTai), new PhongThietBiRequest("A", "B"), CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC12_TaoLoai_LuuPhiBanDauVaMacDinhConSuDung()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new TaoLoaiThietBiHandler(db);

        var ketQua = await handler.XuLyAsync(
            new LoaiThietBiTaoMoiRequest("Loại D05-N", "Mô tả", "Hãng", 30, 12345),
            CancellationToken.None);

        var daLuu = await db.Set<LoaiThietBi>().AsNoTracking().SingleAsync(l => l.MaLoai == ketQua.MaLoai);
        Assert.Equal("Loại D05-N", daLuu.TenLoai);
        Assert.Equal("Mô tả", daLuu.MoTa);
        Assert.Equal("Hãng", daLuu.HangSanXuat);
        Assert.Equal(30, daLuu.ChuKyBaoTri);
        Assert.Equal(12345m, daLuu.PhiTreMoiNgay);
        Assert.False(daLuu.NgungSuDung);
        Assert.Equal(12345L, ketQua.PhiTreMoiNgay);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaLoai_DoiMetadataNhungKhongDoiPhiVaNgungSuDung()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await C3TestSupport.NapDuLieuThuAsync(gd);
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var truoc = await db.Set<LoaiThietBi>().AsNoTracking().SingleAsync(l => l.MaLoai == Guid.Parse(MaLoaiLaptop));
        var handler = new SuaLoaiThietBiHandler(db);

        var ketQua = await handler.XuLyAsync(
            Guid.Parse(MaLoaiLaptop),
            new LoaiThietBiCapNhatRequest("Laptop đã sửa", "Mô tả mới", "Hãng mới", 365),
            CancellationToken.None);

        db.ChangeTracker.Clear();
        var sau = await db.Set<LoaiThietBi>().AsNoTracking().SingleAsync(l => l.MaLoai == Guid.Parse(MaLoaiLaptop));
        Assert.Equal("Laptop đã sửa", sau.TenLoai);
        Assert.Equal("Mô tả mới", sau.MoTa);
        Assert.Equal("Hãng mới", sau.HangSanXuat);
        Assert.Equal(365, sau.ChuKyBaoTri);
        Assert.Equal(truoc.PhiTreMoiNgay, sau.PhiTreMoiNgay);
        Assert.Equal(truoc.NgungSuDung, sau.NgungSuDung);
        Assert.Equal(Convert.ToInt64(truoc.PhiTreMoiNgay), ketQua.PhiTreMoiNgay);
    }

    [CanKetNoiTestFact]
    public async Task UC12_SuaLoai_KhongTonTai_Nem404()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await C3TestSupport.TaoContextAsync(gd);
        var handler = new SuaLoaiThietBiHandler(db);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => handler.XuLyAsync(Guid.Parse(MaKhongTonTai), new LoaiThietBiCapNhatRequest("A", null, null, 30), CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }
}
