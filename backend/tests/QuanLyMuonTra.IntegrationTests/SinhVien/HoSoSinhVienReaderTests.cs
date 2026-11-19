using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.SinhVien;

public sealed class HoSoSinhVienReaderTests
{
    [Fact]
    public async Task UC07_Reader_SvCoHoSo_TraDayDuTruongVaTenNhom()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);
        var reader = phien.DichVu.GetRequiredService<IHoSoSinhVienReader>();

        var hoSo = await reader.LayTheoMaSvAsync("D02-SV-A", CancellationToken.None);

        Assert.IsType<HoSoSinhVienReader>(reader);
        Assert.NotNull(hoSo);
        Assert.Equal("D02-SV-A", hoSo.MaSv);
        Assert.Equal("Sinh viên D02 A", hoSo.HoTen);
        Assert.Equal("L1", hoSo.Lop);
        Assert.Equal("K1", hoSo.Khoa);
        Assert.Equal("DAI_HOC", hoSo.MaNhom);
        Assert.False(string.IsNullOrWhiteSpace(hoSo.TenNhom));
        Assert.Equal(new DateOnly(2030, 12, 31), hoSo.HanThe);
        Assert.Equal("DANG_HOC", hoSo.TrangThaiHoc);
        Assert.Equal("BINH_THUONG", hoSo.TrangThaiQuyenMuon);
    }

    [Fact]
    public async Task UC07_Reader_MaCoKhoangTrang_VanTimDuocVaTraMaChuan()
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);
        var reader = phien.DichVu.GetRequiredService<IHoSoSinhVienReader>();

        var hoSo = await reader.LayTheoMaSvAsync("  D02-SV-B  ", CancellationToken.None);

        Assert.NotNull(hoSo);
        Assert.Equal("D02-SV-B", hoSo.MaSv);
    }

    [Theory]
    [InlineData("D02-SV-KHONG-TON-TAI")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UC07_Reader_MaKhongTonTaiHoacRong_TraNull(string maSv)
    {
        await using var phien = await PhienKiemThuHoSo.BatDauAsync(2);
        var reader = phien.DichVu.GetRequiredService<IHoSoSinhVienReader>();

        var hoSo = await reader.LayTheoMaSvAsync(maSv, CancellationToken.None);

        Assert.Null(hoSo);
    }
}

