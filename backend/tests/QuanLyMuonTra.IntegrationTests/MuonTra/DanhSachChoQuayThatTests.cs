using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class DanhSachChoQuayThatTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [CanKetNoiTestFact]
    public async Task UC10_DsQuay_ChiPhongDuocCapQuyenChiDangChoVaDaThongBaoSapXepFifo()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var handler = moi.Lay<DsDanhSachChoQuayHandler>();

        var tatCa = await handler.XuLyAsync(DuLieuD13.Nv1, null, null, 1, 20, Ct);

        Assert.Equal(6, tatCa.TongSo);
        Assert.Equal(
            [DuLieuD13.Dk006, DuLieuD13.Dk001, DuLieuD13.Dk002, DuLieuD13.Dk005, DuLieuD13.Dk007, DuLieuD13.Dk003],
            tatCa.DanhSach.Select(d => d.MaDangKy).ToArray());
        var dauTien = tatCa.DanhSach[0];
        Assert.Equal("DA_THONG_BAO", dauTien.TrangThai);
        Assert.Null(dauTien.SoThuTuCho);
        Assert.Equal(new DateTimeOffset(2026, 11, 25, 3, 10, 0, TimeSpan.Zero), dauTien.HanGiuCho);
        Assert.Equal("D13-SV-F", dauTien.SinhVien.MaSV);
        Assert.Equal("Sinh viên D13 F", dauTien.SinhVien.HoTen);
        Assert.Equal("L1", dauTien.SinhVien.Lop);
        Assert.Equal("D13 Phong P1", dauTien.TenPhong);
        Assert.Equal("D13 Loai W", dauTien.TenLoai);
        var dangCho = tatCa.DanhSach[1];
        Assert.Equal("DANG_CHO", dangCho.TrangThai);
        Assert.Equal(1, dangCho.SoThuTuCho);
        Assert.DoesNotContain(tatCa.DanhSach, d => d.MaPhong == DuLieuD13.P2);
    }

    [CanKetNoiTestFact]
    public async Task UC10_DsQuay_LocLoaiPhanTrangOnDinhVaPhongKhongQuyenTraRong()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var handler = moi.Lay<DsDanhSachChoQuayHandler>();

        var locLoaiX = await handler.XuLyAsync(DuLieuD13.Nv1, null, DuLieuD13.LoaiX, 1, 20, Ct);
        var trang2 = await handler.XuLyAsync(DuLieuD13.Nv1, null, null, 2, 2, Ct);
        var phongKhongQuyen = await handler.XuLyAsync(DuLieuD13.Nv1, DuLieuD13.P2, null, 1, 20, Ct);
        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(DuLieuD13.Nv1, null, null, 0, 20, Ct));

        Assert.Equal(3, locLoaiX.TongSo);
        Assert.Equal([DuLieuD13.Dk001, DuLieuD13.Dk002, DuLieuD13.Dk003], locLoaiX.DanhSach.Select(d => d.MaDangKy).ToArray());
        Assert.Equal(6, trang2.TongSo);
        Assert.Equal([DuLieuD13.Dk002, DuLieuD13.Dk005], trang2.DanhSach.Select(d => d.MaDangKy).ToArray());
        Assert.Equal(0, phongKhongQuyen.TongSo);
        Assert.Empty(phongKhongQuyen.DanhSach);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loi.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task UC10A2_ChuyenLuot_DaThongBao_DaHuyNguoiKeDuocGiuChoVaKhongViPham()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var viPhamTruoc = await DuLieuD13.DemViPhamAsync(moi.Db);

        var ketQua = await moi.Lay<ChuyenLuotDanhSachChoHandler>().XuLyAsync(DuLieuD13.Nv1, DuLieuD13.Dk006, Ct);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Equal("D13-SV-F", ketQua.SinhVien.MaSV);
        Assert.Equal("DA_THONG_BAO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk007)).TrangThai);
        Assert.Equal(0, await moi.Lay<IAvailabilityService>().TinhKhaDungAsync(DuLieuD13.P1, DuLieuD13.LoaiW, Ct));
        Assert.Equal(2, (await DuLieuD13.ThongBaoTheoKhoaAsync(moi.Db, $"GIU_CHO:{DuLieuD13.Dk007}")).Count);
        Assert.Equal(viPhamTruoc, await DuLieuD13.DemViPhamAsync(moi.Db));
    }

    [CanKetNoiTestFact]
    public async Task UC10A2_ChuyenLuot_KhongConAiCho_TraKhaDungKhongGhiThongBao()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        await moi.ThucThiSqlAsync(DuLieuD13.QuyenNv1P2);

        var ketQua = await moi.Lay<ChuyenLuotDanhSachChoHandler>().XuLyAsync(DuLieuD13.Nv1, DuLieuD13.Dk008, Ct);

        Assert.Equal("DA_HUY", ketQua.TrangThai);
        Assert.Equal(1, await moi.Lay<IAvailabilityService>().TinhKhaDungAsync(DuLieuD13.P2, DuLieuD13.LoaiW, Ct));
        Assert.Equal(0, await DuLieuD13.DemThongBaoSvAsync(moi.Db));
    }

    [CanKetNoiTestFact]
    public async Task UC10A2_ChuyenLuot_SaiTrangThai_Tra409VaKhongDoiDuLieu()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var handler = moi.Lay<ChuyenLuotDanhSachChoHandler>();

        var dangCho = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(DuLieuD13.Nv1, DuLieuD13.Dk003, Ct));
        var daHuy = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(DuLieuD13.Nv1, DuLieuD13.Dk009, Ct));

        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", dangCho.MaLoi);
        Assert.Equal(409, dangCho.MaHttp);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", daHuy.MaLoi);
        var dongDangCho = await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk003);
        Assert.Equal("DANG_CHO", dongDangCho.TrangThai);
        Assert.Equal(3, dongDangCho.SoThuTuCho);
        Assert.Equal(0, await DuLieuD13.DemThongBaoSvAsync(moi.Db));
    }

    [CanKetNoiTestFact]
    public async Task UC10A2_ChuyenLuot_LoiQuyenVaMa_Tra403_404_400()
    {
        await using var moi = await DuLieuD13.TaoMoiTruongAsync(true);
        await moi.NapDuLieuAsync(DuLieuD13.TenTep, DuLieuD13.QuyenNv1P1);
        var handler = moi.Lay<ChuyenLuotDanhSachChoHandler>();

        var khacPhong = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(DuLieuD13.Nv1, DuLieuD13.Dk008, Ct));
        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(DuLieuD13.Nv1, "DK-000101-999999", Ct));
        var saiDinhDang = await Assert.ThrowsAsync<LoiNghiepVuException>(() => handler.XuLyAsync(DuLieuD13.Nv1, "khong-phai-ma", Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", khacPhong.MaLoi);
        Assert.Equal(403, khacPhong.MaHttp);
        Assert.Equal("KHONG_TIM_THAY", khongTonTai.MaLoi);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", saiDinhDang.MaLoi);
        Assert.Equal("DA_THONG_BAO", (await DuLieuD13.DocAsync(moi.Db, DuLieuD13.Dk008)).TrangThai);
    }
}
