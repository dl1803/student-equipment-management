using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class KiemTraThietBiGiaoHandlerTests
{
    private static readonly Guid Nv = Guid.Parse("019c0000-0000-7000-8000-0000000d8a01");
    private static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d8b01");
    private static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d8b02");
    private static readonly Guid LoaiX = Guid.Parse("019c0000-0000-7000-8000-0000000d8c01");
    private static readonly Guid LoaiY = Guid.Parse("019c0000-0000-7000-8000-0000000d8c02");
    private static readonly CancellationToken Ct = CancellationToken.None;

    private sealed class DanhMucGia(IReadOnlyList<ThietBiDto> dsThietBi) : IDanhMucReader
    {
        public int SoLanDocThietBi { get; private set; }

        public Task<ThietBiDto?> LayThietBiAsync(string maThietBi, CancellationToken ct)
        {
            SoLanDocThietBi++;
            return Task.FromResult<ThietBiDto?>(dsThietBi.FirstOrDefault(t => t.MaThietBi == maThietBi));
        }

        public Task<LoaiThietBiDto?> LayLoaiAsync(Guid maLoai, CancellationToken ct)
        {
            var ten = maLoai == LoaiX ? "Loại X" : "Loại Y";
            return Task.FromResult<LoaiThietBiDto?>(new LoaiThietBiDto(maLoai, ten, 30, 50000m, false));
        }

        public Task<PhongThietBiDto?> LayPhongAsync(Guid maPhong, CancellationToken ct)
        {
            var ten = maPhong == P1 ? "Phòng P1" : "Phòng P2";
            return Task.FromResult<PhongThietBiDto?>(new PhongThietBiDto(maPhong, ten, "Cơ sở 1"));
        }

        public Task<IReadOnlyList<PhongThietBiDto>> DsPhongAsync(CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<LoaiThietBiDto>> DsLoaiDangSuDungAsync(CancellationToken ct)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class QuyenPhongGia(IReadOnlyCollection<Guid> dsPhong) : IQuyenPhongService
    {
        public Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct)
        {
            return Task.FromResult(dsPhong);
        }

        public Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct)
        {
            if (!dsPhong.Contains(maPhong))
            {
                throw new LoiNghiepVuException("PHONG_CHUA_CAP_QUYEN", "Phòng chưa được cấp quyền.");
            }

            return Task.CompletedTask;
        }
    }

    private static ThietBiDto TaoThietBi(
        string ma,
        Guid maPhong,
        Guid maLoai,
        string tinhTrang = "SAN_SANG",
        bool ngungSuDung = false)
    {
        return new ThietBiDto(
            ma,
            maLoai,
            maPhong,
            $"Thiết bị {ma}",
            new DateOnly(2026, 1, 1),
            tinhTrang,
            ngungSuDung,
            "Kệ A",
            "Tủ 1",
            15000000m);
    }

    private static (KiemTraThietBiGiaoHandler Bo, DanhMucGia DanhMuc) TaoBo(
        IReadOnlyCollection<Guid> phongDuocCap,
        params ThietBiDto[] dsThietBi)
    {
        var danhMuc = new DanhMucGia(dsThietBi);
        return (new KiemTraThietBiGiaoHandler(danhMuc, new QuyenPhongGia(phongDuocCap)), danhMuc);
    }

    [Fact]
    public async Task HopLe_TraThongTinThietBiPhongLoaiVaViTri()
    {
        var (bo, _) = TaoBo([P1], TaoThietBi("TB-01", P1, LoaiX));

        var ketQua = await bo.XuLyAsync(Nv, "TB-01", P1, LoaiX, Ct);

        Assert.Equal("TB-01", ketQua.MaThietBi);
        Assert.Equal("Thiết bị TB-01", ketQua.TenThietBi);
        Assert.Equal(LoaiX, ketQua.MaLoai);
        Assert.Equal("Loại X", ketQua.TenLoai);
        Assert.Equal(P1, ketQua.MaPhong);
        Assert.Equal("Phòng P1", ketQua.TenPhong);
        Assert.Equal("Kệ A", ketQua.ViTriKe);
        Assert.Equal("Tủ 1", ketQua.ViTriTu);
    }

    [Fact]
    public async Task KhongTonTai_Tra404KhongTimThay()
    {
        var (bo, _) = TaoBo([P1]);

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.XuLyAsync(Nv, "TB-KHONG-CO", P1, LoaiX, Ct));

        Assert.Equal("KHONG_TIM_THAY", loi.MaLoi);
        Assert.Equal(404, loi.MaHttp);
    }

    [Fact]
    public async Task PhongChuaCapQuyen_Tra403VaKhongDocThietBi()
    {
        var (bo, danhMuc) = TaoBo([P1], TaoThietBi("TB-02", P2, LoaiX));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.XuLyAsync(Nv, "TB-02", P2, LoaiX, Ct));

        Assert.Equal("PHONG_CHUA_CAP_QUYEN", loi.MaLoi);
        Assert.Equal(403, loi.MaHttp);
        Assert.Equal(0, danhMuc.SoLanDocThietBi);
    }

    [Theory]
    [InlineData("DANG_DUOC_MUON")]
    [InlineData("DANG_BAO_TRI_SUA_CHUA")]
    [InlineData("DA_THANH_LY")]
    [InlineData("DA_BAO_MAT")]
    public async Task KhongSanSang_Tra422(string tinhTrang)
    {
        var (bo, _) = TaoBo([P1], TaoThietBi("TB-03", P1, LoaiX, tinhTrang));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.XuLyAsync(Nv, "TB-03", P1, LoaiX, Ct));

        Assert.Equal("THIET_BI_KHONG_SAN_SANG", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public async Task SanSangNhungNgungSuDung_Tra422KhongSanSang()
    {
        var (bo, _) = TaoBo([P1], TaoThietBi("TB-04", P1, LoaiX, ngungSuDung: true));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.XuLyAsync(Nv, "TB-04", P1, LoaiX, Ct));

        Assert.Equal("THIET_BI_KHONG_SAN_SANG", loi.MaLoi);
    }

    [Fact]
    public async Task SaiPhong_Tra422()
    {
        var (bo, _) = TaoBo([P1], TaoThietBi("TB-05", P2, LoaiX));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.XuLyAsync(Nv, "TB-05", P1, LoaiX, Ct));

        Assert.Equal("THIET_BI_SAI_PHONG", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public async Task SaiLoai_Tra422()
    {
        var (bo, _) = TaoBo([P1], TaoThietBi("TB-06", P1, LoaiY));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.XuLyAsync(Nv, "TB-06", P1, LoaiX, Ct));

        Assert.Equal("THIET_BI_SAI_LOAI", loi.MaLoi);
        Assert.Equal(422, loi.MaHttp);
    }

    [Fact]
    public async Task VuaKhongSanSangVuaSaiPhong_UuTienKhongSanSang()
    {
        var (bo, _) = TaoBo([P1], TaoThietBi("TB-07", P2, LoaiX, "DANG_DUOC_MUON"));

        var loi = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => bo.XuLyAsync(Nv, "TB-07", P1, LoaiX, Ct));

        Assert.Equal("THIET_BI_KHONG_SAN_SANG", loi.MaLoi);
    }
}
