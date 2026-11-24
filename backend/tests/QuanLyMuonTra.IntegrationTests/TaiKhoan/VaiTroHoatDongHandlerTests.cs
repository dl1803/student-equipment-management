using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using QuanLyMuonTra.Api.Auth;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;

namespace QuanLyMuonTra.IntegrationTests.TaiKhoan;

public class VaiTroHoatDongHandlerTests
{
    private static readonly VaiTroHoatDongRequirement YeuCauNhanVien = new("NHAN_VIEN");

    [Fact]
    public async Task NFR01_DungVaiTroVaTaiKhoanHoatDong_ThanhCong()
    {
        var boXuLy = new VaiTroHoatDongHandler(new BoDocGia("HOAT_DONG"));
        var boiCanh = TaoBoiCanh("NHAN_VIEN", Guid.NewGuid().ToString());

        await boXuLy.HandleAsync(boiCanh);

        Assert.True(boiCanh.HasSucceeded);
    }

    [Theory]
    [InlineData("BI_KHOA")]
    [InlineData("TAM_KHOA_DANG_NHAP")]
    public async Task NFR01_TaiKhoanBiKhoa_ThatBaiVoiLyDoTaiKhoanBiKhoa(string trangThai)
    {
        var boXuLy = new VaiTroHoatDongHandler(new BoDocGia(trangThai));
        var boiCanh = TaoBoiCanh("NHAN_VIEN", Guid.NewGuid().ToString());

        await boXuLy.HandleAsync(boiCanh);

        Assert.True(boiCanh.HasFailed);
        Assert.Contains(boiCanh.FailureReasons, lyDo => lyDo.Message == VaiTroHoatDongHandler.LyDoTaiKhoanBiKhoa);
    }

    [Fact]
    public async Task NFR01_SaiVaiTro_KhongThanhCongVaKhongCoLyDoKhoa()
    {
        var boXuLy = new VaiTroHoatDongHandler(new BoDocGia("HOAT_DONG"));
        var boiCanh = TaoBoiCanh("QUAN_TRI", Guid.NewGuid().ToString());

        await boXuLy.HandleAsync(boiCanh);

        Assert.False(boiCanh.HasSucceeded);
        Assert.Empty(boiCanh.FailureReasons);
    }

    [Fact]
    public async Task NFR01_SubKhongPhaiGuid_KhongThanhCong()
    {
        var boXuLy = new VaiTroHoatDongHandler(new BoDocGia("HOAT_DONG"));
        var boiCanh = TaoBoiCanh("NHAN_VIEN", "MSSV-KHONG-PHAI-GUID");

        await boXuLy.HandleAsync(boiCanh);

        Assert.False(boiCanh.HasSucceeded);
    }

    [Fact]
    public async Task NFR01_TaiKhoanKhongTonTai_KhongThanhCongVaKhongCoLyDoKhoa()
    {
        var boXuLy = new VaiTroHoatDongHandler(new BoDocGia(null));
        var boiCanh = TaoBoiCanh("NHAN_VIEN", Guid.NewGuid().ToString());

        await boXuLy.HandleAsync(boiCanh);

        Assert.False(boiCanh.HasSucceeded);
        Assert.Empty(boiCanh.FailureReasons);
    }

    private static AuthorizationHandlerContext TaoBoiCanh(string vaiTro, string sub)
    {
        var danhTinh = new ClaimsIdentity(
            [new Claim("role", vaiTro), new Claim("sub", sub)],
            "kiem-thu");
        return new AuthorizationHandlerContext([YeuCauNhanVien], new ClaimsPrincipal(danhTinh), null);
    }

    private sealed class BoDocGia(string? trangThai) : ITrangThaiTaiKhoanReader
    {
        public Task<string?> LayTrangThaiKhoaAsync(Guid maNguoiDung, CancellationToken ct) => Task.FromResult(trangThai);
    }
}

