using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyMuonTra.Api.Loi;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.Api;

public class XuLyLoiToanCucTests
{
    [Fact]
    public async Task KT14_LoiNghiepVu_TraDungMaHttpMaLoiVaTraceId()
    {
        var (maHttp, kieu, noiDung) = await XuLyAsync(new LoiNghiepVuException("TRANG_THAI_KHONG_HOP_LE", "Phiếu không ở trạng thái cho phép."));

        Assert.Equal(409, maHttp);
        Assert.Equal("application/problem+json", kieu);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", noiDung.GetProperty("maLoi").GetString());
        Assert.Equal(409, noiDung.GetProperty("status").GetInt32());
        Assert.Equal("Phiếu không ở trạng thái cho phép.", noiDung.GetProperty("detail").GetString());
        Assert.Equal("/api/v1/thu", noiDung.GetProperty("instance").GetString());
        Assert.False(string.IsNullOrWhiteSpace(noiDung.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task KT14_NghiepVuException_MaLoiCoTrongBang_TraDungMaHttpVaMaLoi()
    {
        var (maHttp, kieu, noiDung) = await XuLyAsync(new NghiepVuException("TRANG_THAI_KHONG_HOP_LE", "Phiếu không ở trạng thái cho phép."));

        Assert.Equal(409, maHttp);
        Assert.Equal("application/problem+json", kieu);
        Assert.Equal("TRANG_THAI_KHONG_HOP_LE", noiDung.GetProperty("maLoi").GetString());
        Assert.Equal("Phiếu không ở trạng thái cho phép.", noiDung.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task KT14_ValidationException_Tra400VoiErrorsTheoCamelCase()
    {
        var loi = new ValidationException(
        [
            new ValidationFailure("DsLoaiVaSoLuong[0].SoLuong", "Số lượng phải lớn hơn 0.")
        ]);

        var (maHttp, _, noiDung) = await XuLyAsync(loi);

        Assert.Equal(400, maHttp);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", noiDung.GetProperty("maLoi").GetString());
        var thongDiep = noiDung.GetProperty("errors").GetProperty("dsLoaiVaSoLuong[0].soLuong")[0].GetString();
        Assert.Equal("Số lượng phải lớn hơn 0.", thongDiep);
    }

    [Fact]
    public async Task KT14_BadHttpRequest_Tra400()
    {
        var (maHttp, _, noiDung) = await XuLyAsync(new BadHttpRequestException("json hong"));

        Assert.Equal(400, maHttp);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", noiDung.GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task KT14_XungDotCapNhat_Tra409()
    {
        var (maHttp, _, noiDung) = await XuLyAsync(new DbUpdateConcurrencyException("va cham xmin"));

        Assert.Equal(409, maHttp);
        Assert.Equal("XUNG_DOT_CAP_NHAT", noiDung.GetProperty("maLoi").GetString());
    }

    [Fact]
    public async Task KT14_ValidationException_PropertyNameRongHoacNull_KhongNemNullReferenceException()
    {
        var loi = new ValidationException(
        [
            new ValidationFailure(string.Empty, "Lỗi toàn cục trên model."),
            new ValidationFailure(null!, "Lỗi không gắn với thuộc tính cụ thể.")
        ]);

        var (maHttp, _, noiDung) = await XuLyAsync(loi);

        Assert.Equal(400, maHttp);
        Assert.Equal("DU_LIEU_KHONG_HOP_LE", noiDung.GetProperty("maLoi").GetString());
        Assert.True(noiDung.GetProperty("errors").TryGetProperty(string.Empty, out var danhSachLoi));
        Assert.Equal(2, danhSachLoi.GetArrayLength());
    }

    [Fact]
    public async Task KT14_ResponseDaBatDau_TraFalseVaKhongGhiDe()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IHttpResponseFeature>(new ResponseDaBatDauFeature());
        var xuLy = new XuLyLoiToanCuc(NullLogger<XuLyLoiToanCuc>.Instance);

        var daXuLy = await xuLy.TryHandleAsync(httpContext, new Exception("loi"), CancellationToken.None);

        Assert.False(daXuLy);
    }

    private sealed class ResponseDaBatDauFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = 200;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = Stream.Null;

        public bool HasStarted => true;

        public void OnStarting(Func<object, Task> callback, object state) { }

        public void OnCompleted(Func<object, Task> callback, object state) { }
    }

    [Fact]
    public async Task NFR05_LoiKhongXacDinh_Tra500KhongLoChiTietNoiBo()
    {
        var (maHttp, _, noiDung) = await XuLyAsync(new InvalidOperationException("chi tiet noi bo SQL"));

        Assert.Equal(500, maHttp);
        Assert.False(noiDung.TryGetProperty("maLoi", out _));
        Assert.False(noiDung.TryGetProperty("detail", out _));
        Assert.DoesNotContain("chi tiet noi bo SQL", noiDung.GetRawText());
        Assert.False(string.IsNullOrWhiteSpace(noiDung.GetProperty("traceId").GetString()));
    }

    private static async Task<(int MaHttp, string KieuNoiDung, JsonElement NoiDung)> XuLyAsync(Exception loi)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/thu";
        httpContext.Response.Body = new MemoryStream();
        var xuLy = new XuLyLoiToanCuc(NullLogger<XuLyLoiToanCuc>.Instance);

        var daXuLy = await xuLy.TryHandleAsync(httpContext, loi, CancellationToken.None);

        Assert.True(daXuLy);
        httpContext.Response.Body.Position = 0;
        using var tai = await JsonDocument.ParseAsync(httpContext.Response.Body);

        return (httpContext.Response.StatusCode, httpContext.Response.ContentType ?? string.Empty, tai.RootElement.Clone());
    }
}

