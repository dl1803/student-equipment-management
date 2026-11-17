using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public class TraCuuThietBiTests
{
    private const string FileDuLieu = "D05-V-du-lieu-thu.sql";
    private const string TuKhoaDuLieuThu = "d05v";
    private static readonly Guid PA1 = Guid.Parse("019c0000-0000-7000-8000-0000000d5b01");
    private static readonly Guid PB = Guid.Parse("019c0000-0000-7000-8000-0000000d5b02");
    private static readonly Guid PC = Guid.Parse("019c0000-0000-7000-8000-0000000d5b03");
    private static readonly Guid PA4 = Guid.Parse("019c0000-0000-7000-8000-0000000d5b04");
    private static readonly Guid Projector = Guid.Parse("019c0000-0000-7000-8000-0000000d5c01");
    private static readonly Guid Laptop = Guid.Parse("019c0000-0000-7000-8000-0000000d5c02");
    private static readonly Guid Speaker = Guid.Parse("019c0000-0000-7000-8000-0000000d5c03");
    private static readonly Guid Micro = Guid.Parse("019c0000-0000-7000-8000-0000000d5c04");
    private static readonly DateTimeOffset BayGio = new(2026, 11, 14, 2, 0, 0, TimeSpan.Zero);

    private static async Task<GiaoDichTestRollback> MoAsync()
    {
        var gd = await GiaoDichTestRollback.BatDauAsync();
        await gd.NapDuLieuThuAsync(FileDuLieu);
        return gd;
    }

    private static Task<KetQuaPhanTrang<TraCuuThietBiItemDto>> TraCuuAsync(
        GiaoDichTestRollback gd,
        string? ten = TuKhoaDuLieuThu,
        Guid? maLoai = null,
        Guid? maPhong = null,
        bool? conHang = null,
        int trang = 1,
        int kichThuoc = 100)
    {
        var boXuLy = new TraCuuThietBiHandler(gd.Db, new DongHoCoDinh(BayGio));
        return boXuLy.XuLyAsync(ten, maLoai, maPhong, conHang, trang, kichThuoc, CancellationToken.None);
    }

    private static string Khoa(TraCuuThietBiItemDto dong)
    {
        return $"{dong.MaLoai}|{dong.MaPhong}";
    }

    private static string KhoaCap(Guid maLoai, Guid maPhong)
    {
        return $"{maLoai}|{maPhong}";
    }

    [CanKetNoiTestFact]
    public async Task UC01_TraCuu_MacDinh_DungCapLoaiPhongSoKhaDungVaThuTu()
    {
        await using var gd = await MoAsync();

        var ketQua = await TraCuuAsync(gd);

        Assert.Equal(5, ketQua.TongSo);
        Assert.Equal(
            new[] { KhoaCap(Laptop, PA1), KhoaCap(Laptop, PA4), KhoaCap(Micro, PB), KhoaCap(Projector, PA1), KhoaCap(Projector, PB) },
            ketQua.DanhSach.Select(Khoa));
        Assert.Equal(new[] { 1, 1, 0, 3, 0 }, ketQua.DanhSach.Select(d => d.SoLuongKhaDung));
    }

    [CanKetNoiTestFact]
    public async Task UC01_TraCuu_LoaiNgungSuDungVaPhongKhongCoThietBi_KhongXuatHien()
    {
        await using var gd = await MoAsync();

        var theoLoaiNgung = await TraCuuAsync(gd, maLoai: Speaker);
        var theoPhongRong = await TraCuuAsync(gd, maPhong: PC);

        Assert.Equal(0, theoLoaiNgung.TongSo);
        Assert.Equal(0, theoPhongRong.TongSo);
    }

    [CanKetNoiTestFact]
    public async Task UC01_TraCuu_ThietBiHongHoacNgung_TinhLaHetHangVanHienThi()
    {
        await using var gd = await MoAsync();

        var hetHang = await TraCuuAsync(gd, conHang: false);

        Assert.Equal(new[] { KhoaCap(Micro, PB), KhoaCap(Projector, PB) }, hetHang.DanhSach.Select(Khoa));
        Assert.All(hetHang.DanhSach, d => Assert.Equal(0, d.SoLuongKhaDung));
    }

    [CanKetNoiTestFact]
    public async Task UC01_TraCuu_ConHang_ChiLayKhaDungLonHonKhong()
    {
        await using var gd = await MoAsync();

        var conHang = await TraCuuAsync(gd, conHang: true);

        Assert.Equal(new[] { KhoaCap(Laptop, PA1), KhoaCap(Laptop, PA4), KhoaCap(Projector, PA1) }, conHang.DanhSach.Select(Khoa));
    }

    [CanKetNoiTestFact]
    public async Task UC01_TraCuu_LocTheoTenMaLoaiMaPhong()
    {
        await using var gd = await MoAsync();

        var theoTen = await TraCuuAsync(gd, ten: "D05V LAPTOP");
        var theoLoai = await TraCuuAsync(gd, maLoai: Projector);
        var theoPhong = await TraCuuAsync(gd, maPhong: PB);
        var giaoNhau = await TraCuuAsync(gd, maLoai: Projector, maPhong: PB);

        Assert.Equal(2, theoTen.TongSo);
        Assert.Equal(2, theoLoai.TongSo);
        Assert.Equal(2, theoPhong.TongSo);
        Assert.Equal(new[] { KhoaCap(Projector, PB) }, giaoNhau.DanhSach.Select(Khoa));
    }

    [CanKetNoiTestFact]
    public async Task ĐXAPI28_PhanTrang_ThuTuOnDinhKhiTrungTenLoaiVaTenPhong()
    {
        await using var gd = await MoAsync();

        var trang1 = await TraCuuAsync(gd, trang: 1, kichThuoc: 2);
        var trang2 = await TraCuuAsync(gd, trang: 2, kichThuoc: 2);
        var trang3 = await TraCuuAsync(gd, trang: 3, kichThuoc: 2);
        var trang4 = await TraCuuAsync(gd, trang: 4, kichThuoc: 2);

        Assert.All(new[] { trang1, trang2, trang3, trang4 }, t => Assert.Equal(5, t.TongSo));
        Assert.Equal(new[] { KhoaCap(Laptop, PA1), KhoaCap(Laptop, PA4) }, trang1.DanhSach.Select(Khoa));
        Assert.Equal(new[] { KhoaCap(Micro, PB), KhoaCap(Projector, PA1) }, trang2.DanhSach.Select(Khoa));
        Assert.Equal(new[] { KhoaCap(Projector, PB) }, trang3.DanhSach.Select(Khoa));
        Assert.Empty(trang4.DanhSach);
        Assert.Equal(4, trang4.Trang);
    }

    [CanKetNoiTestFact]
    public async Task UC01_TraCuu_TrangVaKichThuocNgoaiKhoang_Nem400()
    {
        await using var gd = await MoAsync();

        var loiTrang = await Assert.ThrowsAsync<LoiNghiepVuException>(() => TraCuuAsync(gd, trang: 0));
        var loiKichThuoc = await Assert.ThrowsAsync<LoiNghiepVuException>(() => TraCuuAsync(gd, kichThuoc: 101));

        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loiTrang.MaLoi);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", loiKichThuoc.MaLoi);
    }

    [CanKetNoiTestFact]
    public async Task BR20_TraCuu_SoKhaDungTrungVoiAvailabilityServiceChoMoiCap()
    {
        await using var gd = await MoAsync();
        var dichVu = new AvailabilityService(gd.Db, new DongHoCoDinh(BayGio));

        var ketQua = await TraCuuAsync(gd);

        Assert.NotEmpty(ketQua.DanhSach);
        foreach (var dong in ketQua.DanhSach)
        {
            var mongDoi = await dichVu.TinhKhaDungAsync(dong.MaPhong, dong.MaLoai, CancellationToken.None);
            Assert.Equal(mongDoi, dong.SoLuongKhaDung);
        }
    }

    [CanKetNoiTestFact]
    public async Task UC01_ChiTiet_ConHang_CoThongSoKhongTrungVaKhongCoNgayDuKien()
    {
        await using var gd = await MoAsync();

        var chiTiet = await new TraCuuThietBiChiTietHandler(gd.Db, new DongHoCoDinh(BayGio))
            .XuLyAsync(Projector, PA1, CancellationToken.None);

        Assert.Equal(3, chiTiet.SoLuongKhaDung);
        Assert.Equal("Mo ta projector", chiTiet.MoTa);
        Assert.Equal("Epson", chiTiet.HangSanXuat);
        Assert.Equal(new[] { "HDMI 3000lm", "VGA" }, chiTiet.DsThongSoKyThuat);
        Assert.Empty(chiTiet.DsAnh);
        Assert.Null(chiTiet.NgayDuKienCoSan);
    }

    [CanKetNoiTestFact]
    public async Task ĐXKT05_ChiTiet_HetHang_NgayDuKienLaHenTraNhoNhatCuaPhieuChuaTra()
    {
        await using var gd = await MoAsync();

        var chiTiet = await new TraCuuThietBiChiTietHandler(gd.Db, new DongHoCoDinh(BayGio))
            .XuLyAsync(Micro, PB, CancellationToken.None);

        Assert.Equal(0, chiTiet.SoLuongKhaDung);
        Assert.NotNull(chiTiet.NgayDuKienCoSan);
        Assert.Equal(new DateTime(2026, 11, 13, 10, 0, 0, DateTimeKind.Utc), chiTiet.NgayDuKienCoSan.Value.UtcDateTime);
    }

    [CanKetNoiTestFact]
    public async Task ĐXKT05_ChiTiet_HetHangDoNgungSuDungKhongCoPhieu_NgayDuKienLaNull()
    {
        await using var gd = await MoAsync();

        var chiTiet = await new TraCuuThietBiChiTietHandler(gd.Db, new DongHoCoDinh(BayGio))
            .XuLyAsync(Projector, PB, CancellationToken.None);

        Assert.Equal(0, chiTiet.SoLuongKhaDung);
        Assert.Null(chiTiet.NgayDuKienCoSan);
    }

    [CanKetNoiTestFact]
    public async Task UC01_ChiTiet_CapKhongThuocTapTraCuu_Nem404KhongTimThay()
    {
        await using var gd = await MoAsync();
        var boXuLy = new TraCuuThietBiChiTietHandler(gd.Db, new DongHoCoDinh(BayGio));

        var phongKhongCoThietBi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Projector, PC, CancellationToken.None));
        var loaiNgung = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Speaker, PA1, CancellationToken.None));
        var khongTonTai = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.All(new[] { phongKhongCoThietBi, loaiNgung, khongTonTai }, l =>
        {
            Assert.Equal("KHONG_TIM_THAY", l.MaLoi);
            Assert.Equal(404, l.MaHttp);
        });
    }

    [CanKetNoiTestFact]
    public async Task UC01_DsPhong_GomPhongChuaCoThietBiVaSapTheoTenPhongRoiMa()
    {
        await using var gd = await MoAsync();

        var ketQua = await new DsPhongThietBiSvHandler(gd.Db).XuLyAsync(1, 100, CancellationToken.None);

        var cuaDuLieuThu = ketQua.DanhSach
            .Where(p => p.TenPhong.StartsWith("D05V", StringComparison.Ordinal))
            .Select(p => p.MaPhong)
            .ToList();
        Assert.Equal(new[] { PA1, PA4, PB, PC }, cuaDuLieuThu);
    }

    [CanKetNoiTestFact]
    public async Task UC01_DsLoai_BoLoaiNgungSuDungVaSapTheoTenLoai()
    {
        await using var gd = await MoAsync();

        var ketQua = await new DsLoaiThietBiSvHandler(gd.Db).XuLyAsync(1, 100, CancellationToken.None);

        var cuaDuLieuThu = ketQua.DanhSach
            .Where(l => l.TenLoai.StartsWith("D05V", StringComparison.Ordinal))
            .ToList();
        Assert.Equal(new[] { Laptop, Micro, Projector }, cuaDuLieuThu.Select(l => l.MaLoai));
        var projector = cuaDuLieuThu.Single(l => l.MaLoai == Projector);
        Assert.Equal("Mo ta projector", projector.MoTa);
        Assert.Equal("Epson", projector.HangSanXuat);
    }

    [CanKetNoiTestFact]
    public async Task ĐXAPI22_Quay_ChiThayPhongDuocCapQuyen()
    {
        await using var gd = await MoAsync();
        var boXuLy = TaoHandlerQuay(gd, Guid.NewGuid(), PA1, PB);

        var ketQua = await boXuLy.XuLyAsync(TuKhoaDuLieuThu, null, null, null, 1, 100, CancellationToken.None);

        Assert.Equal(
            new[] { KhoaCap(Laptop, PA1), KhoaCap(Micro, PB), KhoaCap(Projector, PA1), KhoaCap(Projector, PB) },
            ketQua.DanhSach.Select(Khoa));
    }

    [CanKetNoiTestFact]
    public async Task ĐXAPI22_Quay_MaPhongNgoaiQuyen_RongKhongNem403()
    {
        await using var gd = await MoAsync();
        var boXuLy = TaoHandlerQuay(gd, Guid.NewGuid(), PA1);

        var ketQua = await boXuLy.XuLyAsync(TuKhoaDuLieuThu, null, PB, null, 1, 100, CancellationToken.None);

        Assert.Equal(0, ketQua.TongSo);
        Assert.Empty(ketQua.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC07_Quay_KhongCoQuyenPhongNao_Rong()
    {
        await using var gd = await MoAsync();
        var boXuLy = TaoHandlerQuay(gd, Guid.NewGuid());

        var ketQua = await boXuLy.XuLyAsync(TuKhoaDuLieuThu, null, null, null, 1, 100, CancellationToken.None);

        Assert.Equal(0, ketQua.TongSo);
        Assert.Empty(ketQua.DanhSach);
    }

    [CanKetNoiTestFact]
    public async Task UC07_Quay_KhongXacDinhDuocNhanVien_Nem401()
    {
        await using var gd = await MoAsync();
        var boXuLy = new QuayDsLoaiThietBiHandler(gd.Db, new DongHoCoDinh(BayGio), new NguoiDungGia(null), new QuyenPhongGia());

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => boXuLy.XuLyAsync(null, null, null, null, 1, 20, CancellationToken.None));

        Assert.Equal("CHUA_XAC_THUC", loi.MaLoi);
    }

    private static QuayDsLoaiThietBiHandler TaoHandlerQuay(GiaoDichTestRollback gd, Guid maNhanVien, params Guid[] dsMaPhong)
    {
        return new QuayDsLoaiThietBiHandler(
            gd.Db,
            new DongHoCoDinh(BayGio),
            new NguoiDungGia(maNhanVien),
            new QuyenPhongGia(dsMaPhong));
    }

    private sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    private sealed class NguoiDungGia(Guid? maNguoiDung) : ICurrentUser
    {
        public Guid? MaNguoiDung => maNguoiDung;

        public string? MaSv => null;
    }

    private sealed class QuyenPhongGia(params Guid[] dsMaPhong) : IQuyenPhongService
    {
        public Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyCollection<Guid>>(dsMaPhong);
        }

        public Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
