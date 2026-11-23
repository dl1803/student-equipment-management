using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class QuyenMuonServiceTests
{
    [Fact]
    public async Task UC09_Khoa_BinhThuong_ChuyenTamKhoaVaGhiNhatKyHeThong()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-KHOA");
        await DongBoSinhVienTestSupport.ThemSvUngDungAsync(moiTruong.Db, ma, "SV Khoa", "BINH_THUONG");

        var ketQua = await moiTruong.DichVu.KhoaAsync(ma, CancellationToken.None);

        Assert.Equal("BINH_THUONG", ketQua.TrangThaiTruoc);
        Assert.Equal("TAM_KHOA", ketQua.TrangThaiSau);
        Assert.True(ketQua.DaThayDoi);
        Assert.Equal("TAM_KHOA", (await DongBoSinhVienTestSupport.DocSvUngDungAsync(moiTruong.Db, ma))!.TrangThaiQuyenMuon);
        var dong = Assert.Single(await DocNhatKyAsync(moiTruong.Db, ma));
        Assert.Equal("SUA", dong.ThaoTac);
        Assert.Contains("BINH_THUONG", dong.Truoc);
        Assert.Contains("TAM_KHOA", dong.Sau);
        Assert.True(dong.LaHeThong);
    }

    [Fact]
    public async Task UC09_MoKhoa_TamKhoa_ChuyenBinhThuongVaGhiNhatKy()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-MO");
        await DongBoSinhVienTestSupport.ThemSvUngDungAsync(moiTruong.Db, ma, "SV Mo Khoa", "TAM_KHOA");

        var ketQua = await moiTruong.DichVu.MoKhoaAsync(ma, CancellationToken.None);

        Assert.Equal("TAM_KHOA", ketQua.TrangThaiTruoc);
        Assert.Equal("BINH_THUONG", ketQua.TrangThaiSau);
        Assert.True(ketQua.DaThayDoi);
        Assert.Equal("BINH_THUONG", (await DongBoSinhVienTestSupport.DocSvUngDungAsync(moiTruong.Db, ma))!.TrangThaiQuyenMuon);
        var dong = Assert.Single(await DocNhatKyAsync(moiTruong.Db, ma));
        Assert.Contains("TAM_KHOA", dong.Truoc);
        Assert.Contains("BINH_THUONG", dong.Sau);
    }

    [Theory]
    [InlineData(true, "TAM_KHOA")]
    [InlineData(true, "CAM_MUON_DAI_HAN")]
    [InlineData(false, "BINH_THUONG")]
    [InlineData(false, "CAM_MUON_DAI_HAN")]
    public async Task UC09_KhoaMoKhoa_TrangThaiKhongPhaiNguon_KhongDoiVaKhongGhiNhatKy(bool khoa, string trangThai)
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-KDOI");
        await DongBoSinhVienTestSupport.ThemSvUngDungAsync(moiTruong.Db, ma, "SV Khong Doi", trangThai);

        var ketQua = khoa
            ? await moiTruong.DichVu.KhoaAsync(ma, CancellationToken.None)
            : await moiTruong.DichVu.MoKhoaAsync(ma, CancellationToken.None);

        Assert.Equal(trangThai, ketQua.TrangThaiTruoc);
        Assert.Equal(trangThai, ketQua.TrangThaiSau);
        Assert.False(ketQua.DaThayDoi);
        Assert.Equal(trangThai, (await DongBoSinhVienTestSupport.DocSvUngDungAsync(moiTruong.Db, ma))!.TrangThaiQuyenMuon);
        Assert.Empty(await DocNhatKyAsync(moiTruong.Db, ma));
    }

    [Fact]
    public async Task UC09_SinhVienKhongTonTai_NemKhongTimThay()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var ma = DongBoSinhVienTestSupport.TaoMa("KHONG-CO");

        var loiKhoa = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => moiTruong.DichVu.KhoaAsync(ma, CancellationToken.None));
        var loiMo = await Assert.ThrowsAsync<LoiNghiepVuException>(
            () => moiTruong.DichVu.MoKhoaAsync(ma, CancellationToken.None));

        Assert.Equal("KHONG_TIM_THAY", loiKhoa.MaLoi);
        Assert.Equal("KHONG_TIM_THAY", loiMo.MaLoi);
    }

    [Fact]
    public async Task UC09_KhongCoGiaoDich_NemInvalidOperation()
    {
        await using var moiTruong = await MoiTruong.TaoAsync(moGiaoDich: false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => moiTruong.DichVu.KhoaAsync("SV-KHONG-CO", CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => moiTruong.DichVu.MoKhoaAsync("SV-KHONG-CO", CancellationToken.None));
    }

    [Fact]
    public async Task UC09_Khoa_HoanTacCungGiaoDich_MatCaTrangThaiVaNhatKy()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var ma = DongBoSinhVienTestSupport.TaoMa("SV-D11-HT");
        await DongBoSinhVienTestSupport.ThemSvUngDungAsync(moiTruong.Db, ma, "SV Hoan Tac", "BINH_THUONG");
        var giaoDich = moiTruong.Db.Database.CurrentTransaction!;
        await giaoDich.CreateSavepointAsync("d11k_hoan_tac");

        await moiTruong.DichVu.KhoaAsync(ma, CancellationToken.None);
        await giaoDich.RollbackToSavepointAsync("d11k_hoan_tac");

        Assert.Equal("BINH_THUONG", (await DongBoSinhVienTestSupport.DocSvUngDungAsync(moiTruong.Db, ma))!.TrangThaiQuyenMuon);
        Assert.Empty(await DocNhatKyAsync(moiTruong.Db, ma));
    }

    private static Task<List<DongNhatKyQuyenMuon>> DocNhatKyAsync(AppDbContext db, string ma) =>
        db.Database.SqlQuery<DongNhatKyQuyenMuon>($"""
                SELECT thao_tac AS "ThaoTac", gia_tri_truoc::text AS "Truoc", gia_tri_sau::text AS "Sau",
                       (ma_nguoi_dung IS NULL AND ma_sv IS NULL) AS "LaHeThong"
                FROM nhat_ky.nhat_ky_he_thong
                WHERE doi_tuong = 'sinh_vien.sinh_vien' AND doi_tuong_id = {ma}
                """)
            .ToListAsync();

    public sealed class DongNhatKyQuyenMuon
    {
        public string ThaoTac { get; set; } = string.Empty;

        public string Truoc { get; set; } = string.Empty;

        public string Sau { get; set; } = string.Empty;

        public bool LaHeThong { get; set; }
    }

    private sealed class MoiTruong : IAsyncDisposable
    {
        private readonly ServiceProvider _dichVu;
        private readonly AsyncServiceScope _phamVi;
        private readonly IDbContextTransaction? _giaoDich;

        private MoiTruong(ServiceProvider dichVu, AsyncServiceScope phamVi, AppDbContext db, IDbContextTransaction? giaoDich)
        {
            _dichVu = dichVu;
            _phamVi = phamVi;
            Db = db;
            _giaoDich = giaoDich;
        }

        public AppDbContext Db { get; }

        public IQuyenMuonService DichVu => _phamVi.ServiceProvider.GetRequiredService<IQuyenMuonService>();

        public static async Task<MoiTruong> TaoAsync(bool moGiaoDich = true)
        {
            var dichVu = DongBoSinhVienTestSupport.TaoDichVu();
            var phamVi = dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
            var giaoDich = moGiaoDich ? await db.Database.BeginTransactionAsync() : null;

            return new MoiTruong(dichVu, phamVi, db, giaoDich);
        }

        public async ValueTask DisposeAsync()
        {
            if (_giaoDich is not null)
            {
                await _giaoDich.DisposeAsync();
            }

            await _phamVi.DisposeAsync();
            await _dichVu.DisposeAsync();
        }
    }
}

