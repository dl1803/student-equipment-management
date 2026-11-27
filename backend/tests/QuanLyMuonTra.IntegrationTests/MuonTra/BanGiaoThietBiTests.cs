using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;
using AnhServiceGhiNhan = QuanLyMuonTra.IntegrationTests.MuonTra.DocPhieuMuonTestSupport.AnhServiceGhiNhan;
using QuyenPhongGia = QuanLyMuonTra.IntegrationTests.MuonTra.DocPhieuMuonTestSupport.QuyenPhongGia;
using static QuanLyMuonTra.IntegrationTests.MuonTra.BanGiaoTestSupport;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class BanGiaoThietBiTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [CanKetNoiTestFact]
    public async Task UC08_BanGiao_PhongDuocCap_TraDungPhieuSinhVienVaKyAnhBanDau()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var anh = new AnhServiceGhiNhan();
        var boXuLy = new BanGiaoThietBiQuayHandler(db, new DanhMucReader(db), anh, new QuyenPhongGia(P1, P2));

        var banGiao = await boXuLy.XuLyAsync(Nv1, T1, Ct);

        Assert.Equal(T1, banGiao.MaThietBi);
        Assert.Equal("T1", banGiao.TenThietBi);
        Assert.Equal(PmA, banGiao.MaPhieu);
        Assert.Equal(SvA, banGiao.SinhVien.MaSV);
        Assert.Equal("Sinh viên D14 A", banGiao.SinhVien.HoTen);
        Assert.Equal("L1", banGiao.SinhVien.Lop);
        Assert.Equal("Còn tốt", banGiao.TinhTrangBanDau);
        Assert.Equal(new DateTimeOffset(2026, 11, 26, 1, 0, 0, TimeSpan.Zero), banGiao.NgayGioMuon);
        Assert.Equal(new DateTimeOffset(2026, 11, 26, 9, 0, 0, TimeSpan.Zero), banGiao.NgayGioHenTra);
        Assert.Single(banGiao.DsAnhBanDau);
        Assert.Null(banGiao.PhanHoiSV);
        var daGoi = Assert.Single(anh.DaGoi);
        Assert.Equal(CtT1.ToString(), daGoi.DoiTuongId);
        Assert.Equal("BAN_DAU", daGoi.GiaiDoan);
    }

    [CanKetNoiTestFact]
    public async Task UC08_BanGiao_MoiThietBiTraDungPhieuVaSinhVienCuaNo()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new BanGiaoThietBiQuayHandler(db, new DanhMucReader(db), new AnhServiceGhiNhan(), new QuyenPhongGia(P1, P2));

        var t1 = await boXuLy.XuLyAsync(Nv1, T1, Ct);
        var t5 = await boXuLy.XuLyAsync(Nv1, T5, Ct);
        var t6 = await boXuLy.XuLyAsync(Nv1, T6, Ct);

        Assert.Equal((PmA, SvA), (t1.MaPhieu, t1.SinhVien.MaSV));
        Assert.Equal((PmB, SvB), (t5.MaPhieu, t5.SinhVien.MaSV));
        Assert.Equal((PmB, SvB), (t6.MaPhieu, t6.SinhVien.MaSV));
    }

    [CanKetNoiTestFact]
    public async Task UC08_BanGiao_SaiPhongHoacKhongCoQuyen_Tra403VaKhongKyAnh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var anh = new AnhServiceGhiNhan();
        var chiP1 = new BanGiaoThietBiQuayHandler(db, new DanhMucReader(db), anh, new QuyenPhongGia(P1));
        var khongPhong = new BanGiaoThietBiQuayHandler(db, new DanhMucReader(db), anh, new QuyenPhongGia());

        var saiPhong = await Assert.ThrowsAsync<LoiNghiepVuException>(() => chiP1.XuLyAsync(Nv1, T6, Ct));
        var khongQuyen = await Assert.ThrowsAsync<LoiNghiepVuException>(() => khongPhong.XuLyAsync(Nv1, T1, Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", saiPhong.MaLoi);
        Assert.Equal(403, saiPhong.MaHttp);
        Assert.Equal("PHONG_CHUA_CAP_QUYEN", khongQuyen.MaLoi);
        Assert.Empty(anh.DaGoi);
    }

    [CanKetNoiTestFact]
    public async Task UC08_BanGiao_PhieuKhongConHieuLucHoacChuaTungMuon_Tra422VaKhongKyAnh()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var anh = new AnhServiceGhiNhan();
        var boXuLy = new BanGiaoThietBiQuayHandler(db, new DanhMucReader(db), anh, new QuyenPhongGia(P1, P2));

        var daTra = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, T7, Ct));
        var chuaMuon = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, T8, Ct));

        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", daTra.MaLoi);
        Assert.Equal(422, daTra.MaHttp);
        Assert.Equal("THIET_BI_KHONG_THUOC_PHIEU", chuaMuon.MaLoi);
        Assert.Equal(422, chuaMuon.MaHttp);
        Assert.Empty(anh.DaGoi);
    }

    [CanKetNoiTestFact]
    public async Task UC08_BanGiao_KhongTimThayTra404_MaTrongTra400()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new BanGiaoThietBiQuayHandler(db, new DanhMucReader(db), new AnhServiceGhiNhan(), new QuyenPhongGia(P1, P2));

        var khongCo = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, "D14-TB-KHONG-CO", Ct));
        var trong = await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, "   ", Ct));

        Assert.Equal("KHONG_TIM_THAY", khongCo.MaLoi);
        Assert.Equal(404, khongCo.MaHttp);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", trong.MaLoi);
        Assert.Equal(400, trong.MaHttp);
    }

    [CanKetNoiTestFact]
    public async Task UC08_BanGiao_KhongGhiCoSoDuLieu_ThanhCongLanLoi()
    {
        await using var gd = await GiaoDichTestRollback.BatDauAsync();
        await using var db = await TaoContextAsync(gd);
        var boXuLy = new BanGiaoThietBiQuayHandler(db, new DanhMucReader(db), new AnhServiceGhiNhan(), new QuyenPhongGia(P1));
        var truoc = await DauVetAsync(db);

        await boXuLy.XuLyAsync(Nv1, T1, Ct);
        await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, T6, Ct));
        await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, T7, Ct));
        await Assert.ThrowsAsync<LoiNghiepVuException>(() => boXuLy.XuLyAsync(Nv1, "D14-TB-KHONG-CO", Ct));
        var sau = await DauVetAsync(db);

        Assert.Equal(truoc, sau);
        Assert.False(db.ChangeTracker.HasChanges());
    }
}
