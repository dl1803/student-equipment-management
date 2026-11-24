using System.Net;
using QuanLyMuonTra.Infrastructure.LuuTru;

namespace QuanLyMuonTra.IntegrationTests.Anh;

public sealed class SupabaseAnhStorageThatTests
{
    private static readonly byte[] AnhMau = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01, 0x02, 0x03];

    [CanStorageThatFact]
    public async Task BucketRiengTu_UrlKyDocDuocConUrlCongKhaiKhongDocDuoc()
    {
        var tuyChon = CauHinhStorageThat.Doc()!;
        using var http = new HttpClient();
        var luuTru = new SupabaseAnhStorage(http, tuyChon);
        var khoa = $"kiem-thu-d08k/{Guid.CreateVersion7()}";
        await luuTru.UploadAsync(TenBucketAnh.TinhTrang, khoa, DinhDangAnh.Png, AnhMau, CancellationToken.None);
        try
        {
            var urlKy = await luuTru.TaoUrlKyAsync(TenBucketAnh.TinhTrang, khoa, TimeSpan.FromMinutes(10), CancellationToken.None);
            using var docKy = await http.GetAsync(urlKy);
            var duLieu = await docKy.Content.ReadAsByteArrayAsync();
            using var docCongKhai = await http.GetAsync(
                $"{tuyChon.Url!.TrimEnd('/')}/storage/v1/object/public/{TenBucketAnh.TinhTrang}/{khoa}");

            Assert.Equal(HttpStatusCode.OK, docKy.StatusCode);
            Assert.Equal(AnhMau, duLieu);
            Assert.False(docCongKhai.IsSuccessStatusCode);
        }
        finally
        {
            await luuTru.XoaAsync(TenBucketAnh.TinhTrang, khoa, CancellationToken.None);
        }
    }

    [CanStorageThatFact]
    public async Task BucketCongKhai_DocDuocQuaUrlCongKhaiVaXoaLapKhongLoi()
    {
        var tuyChon = CauHinhStorageThat.Doc()!;
        using var http = new HttpClient();
        var luuTru = new SupabaseAnhStorage(http, tuyChon);
        var khoa = $"kiem-thu-d08k/{Guid.CreateVersion7()}";
        await luuTru.UploadAsync(TenBucketAnh.ThietBi, khoa, DinhDangAnh.Png, AnhMau, CancellationToken.None);
        try
        {
            using var doc = await http.GetAsync(
                $"{tuyChon.Url!.TrimEnd('/')}/storage/v1/object/public/{TenBucketAnh.ThietBi}/{khoa}");
            await luuTru.XoaAsync(TenBucketAnh.ThietBi, khoa, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, doc.StatusCode);
        }
        finally
        {
            await luuTru.XoaAsync(TenBucketAnh.ThietBi, khoa, CancellationToken.None);
        }
    }
}

