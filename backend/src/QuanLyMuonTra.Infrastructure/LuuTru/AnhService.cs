using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Infrastructure.LuuTru;

public sealed class AnhService(
    DbContext db,
    IAnhStorage luuTru,
    ICurrentUser nguoiDung,
    IClock dongHo,
    ILogger<AnhService> nhatKy) : IAnhService
{
    public const int KichThuocToiDa = 2_097_152;

    public const int SoAnhToiDa = 5;

    public static readonly TimeSpan ThoiHanUrlKy = TimeSpan.FromMinutes(10);

    public async Task<Guid> TaiLenAsync(string giaiDoan, Stream noiDung, CancellationToken ct)
    {
        var cauHinh = LayCauHinhGiaiDoan(giaiDoan)
            ?? throw new LoiNghiepVuException(
                "DU_LIEU_KHONG_HOP_LE",
                "Giai đoạn ảnh phải là BAN_DAU, KHI_TRA hoặc THIET_BI.");
        var maNguoiTai = LayMaNguoiDung();

        var duLieu = await DocToiDaAsync(noiDung, ct);
        if (duLieu.Length == 0)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Tệp ảnh rỗng.");
        }

        var loaiNoiDung = DinhDangAnh.NhanDang(duLieu)
            ?? throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Tệp phải là ảnh JPEG, PNG hoặc WebP.");

        var maAnh = Guid.CreateVersion7();
        var bayGio = dongHo.UtcNow;
        var khoaLuuTru = string.Create(
            CultureInfo.InvariantCulture,
            $"{cauHinh.TienTo}/{bayGio:yyyy}/{bayGio:MM}/{maAnh}");

        await luuTru.UploadAsync(cauHinh.Bucket, khoaLuuTru, loaiNoiDung, duLieu, ct);

        var anh = new AnhTaiLen
        {
            MaAnh = maAnh,
            Bucket = cauHinh.Bucket,
            KhoaLuuTru = khoaLuuTru,
            LoaiNoiDung = loaiNoiDung,
            KichThuoc = duLieu.Length,
            NguoiTaiLen = maNguoiTai,
            ThoiDiem = bayGio
        };

        try
        {
            db.Set<AnhTaiLen>().Add(anh);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await XoaBuAsync(cauHinh.Bucket, khoaLuuTru);
            throw;
        }

        db.Entry(anh).State = EntityState.Detached;

        return maAnh;
    }

    public async Task GanAsync(
        IReadOnlyCollection<Guid> dsMaAnh,
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan,
        CancellationToken ct)
    {
        var cauHinh = CauHinhGan(doiTuongLoai, giaiDoan);
        ArgumentException.ThrowIfNullOrWhiteSpace(doiTuongId);
        if (dsMaAnh.Count == 0)
        {
            return;
        }

        var dsMa = dsMaAnh.Distinct().ToArray();
        if (dsMa.Length != dsMaAnh.Count)
        {
            throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Danh sách mã ảnh có mã trùng.");
        }

        var maNguoiGan = LayMaNguoiDung();
        var khoaAdvisory = string.Create(CultureInfo.InvariantCulture, $"anh:{doiTuongLoai}:{doiTuongId}:{giaiDoan}");
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({khoaAdvisory}))", ct);

        var soDaGan = await db.Set<AnhTaiLen>().CountAsync(
            a => a.DoiTuongLoai == doiTuongLoai && a.DoiTuongId == doiTuongId && a.GiaiDoan == giaiDoan,
            ct);
        if (soDaGan + dsMa.Length > SoAnhToiDa)
        {
            throw new LoiNghiepVuException(
                "DU_LIEU_KHONG_HOP_LE",
                $"Mỗi thiết bị tối đa {SoAnhToiDa} ảnh cho mỗi giai đoạn.");
        }

        var tienToKhoa = $"{cauHinh.TienTo}/";
        var soDong = await db.Set<AnhTaiLen>()
            .Where(a => dsMa.Contains(a.MaAnh)
                && (doiTuongLoai == MaDoiTuongAnh.ThietBi || a.NguoiTaiLen == maNguoiGan)
                && a.Bucket == cauHinh.Bucket
                && a.KhoaLuuTru.StartsWith(tienToKhoa)
                && a.DoiTuongId == null)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(a => a.DoiTuongLoai, doiTuongLoai)
                    .SetProperty(a => a.DoiTuongId, doiTuongId)
                    .SetProperty(a => a.GiaiDoan, giaiDoan),
                ct);

        if (soDong != dsMa.Length)
        {
            throw new LoiNghiepVuException(
                "DU_LIEU_KHONG_HOP_LE",
                "Có ảnh không tồn tại, không do bạn tải lên, sai giai đoạn hoặc đã được gắn.");
        }
    }

    public async Task GoAnhThietBiAsync(string maThietBi, IReadOnlyCollection<Guid> dsMaAnh, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(maThietBi);
        if (dsMaAnh.Count == 0)
        {
            return;
        }

        var dsMa = dsMaAnh.Distinct().ToArray();
        await db.Set<AnhTaiLen>()
            .Where(a => dsMa.Contains(a.MaAnh)
                && a.DoiTuongLoai == MaDoiTuongAnh.ThietBi
                && a.DoiTuongId == maThietBi)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(a => a.DoiTuongLoai, (string?)null)
                    .SetProperty(a => a.DoiTuongId, (string?)null)
                    .SetProperty(a => a.GiaiDoan, (string?)null),
                ct);
    }

    public async Task<IReadOnlyList<AnhRefDto>> LayDsAnhKyAsync(
        string doiTuongLoai,
        string doiTuongId,
        string giaiDoan,
        CancellationToken ct)
    {
        var cauHinh = CauHinhGan(doiTuongLoai, giaiDoan);
        if (cauHinh.Bucket != TenBucketAnh.TinhTrang)
        {
            throw new ArgumentException("Chỉ ảnh tình trạng (bucket riêng tư) mới cấp URL có chữ ký.", nameof(giaiDoan));
        }

        var dsAnh = await db.Set<AnhTaiLen>()
            .AsNoTracking()
            .Where(a => a.DoiTuongLoai == doiTuongLoai && a.DoiTuongId == doiTuongId && a.GiaiDoan == giaiDoan)
            .OrderBy(a => a.ThoiDiem)
            .ThenBy(a => a.MaAnh)
            .Select(a => new { a.MaAnh, a.Bucket, a.KhoaLuuTru })
            .ToListAsync(ct);

        var ketQua = new List<AnhRefDto>(dsAnh.Count);
        foreach (var anh in dsAnh)
        {
            try
            {
                var url = await luuTru.TaoUrlKyAsync(anh.Bucket, anh.KhoaLuuTru, ThoiHanUrlKy, ct);
                ketQua.Add(new AnhRefDto(anh.MaAnh, url));
            }
            catch (Exception ex)
            {
                nhatKy.LogWarning(
                    ex,
                    "Không thể tạo URL có chữ ký cho ảnh {MaAnh} ({Bucket}/{Khoa}).",
                    anh.MaAnh,
                    anh.Bucket,
                    anh.KhoaLuuTru);
            }
        }

        return ketQua;
    }

    public async Task<IReadOnlyList<AnhRefDto>> LayDsAnhCongKhaiAsync(
        string doiTuongLoai,
        string doiTuongId,
        CancellationToken ct)
    {
        if (doiTuongLoai != MaDoiTuongAnh.ThietBi)
        {
            throw new ArgumentException("Chỉ ảnh thiết bị (bucket công khai) mới lấy URL công khai.", nameof(doiTuongLoai));
        }

        var dsAnh = await db.Set<AnhTaiLen>()
            .AsNoTracking()
            .Where(a => a.DoiTuongLoai == doiTuongLoai
                && a.DoiTuongId == doiTuongId
                && a.GiaiDoan == MaGiaiDoanAnh.ThietBi)
            .OrderBy(a => a.ThoiDiem)
            .ThenBy(a => a.MaAnh)
            .Select(a => new { a.MaAnh, a.Bucket, a.KhoaLuuTru })
            .ToListAsync(ct);

        return dsAnh
            .Select(anh => new AnhRefDto(anh.MaAnh, luuTru.TaoUrlCongKhai(anh.Bucket, anh.KhoaLuuTru)))
            .ToList();
    }

    private static (string Bucket, string TienTo)? LayCauHinhGiaiDoan(string giaiDoan) => giaiDoan switch
    {
        MaGiaiDoanAnh.ThietBi => (TenBucketAnh.ThietBi, "thiet-bi"),
        MaGiaiDoanAnh.BanDau => (TenBucketAnh.TinhTrang, "ban-dau"),
        MaGiaiDoanAnh.KhiTra => (TenBucketAnh.TinhTrang, "khi-tra"),
        _ => null
    };

    private static (string Bucket, string TienTo) CauHinhGan(string doiTuongLoai, string giaiDoan)
    {
        var hopLe = doiTuongLoai switch
        {
            MaDoiTuongAnh.ThietBi => giaiDoan == MaGiaiDoanAnh.ThietBi,
            MaDoiTuongAnh.ChiTietPhieuMuon => giaiDoan is MaGiaiDoanAnh.BanDau or MaGiaiDoanAnh.KhiTra,
            _ => false
        };
        if (!hopLe)
        {
            throw new ArgumentException($"Tổ hợp đối tượng '{doiTuongLoai}' và giai đoạn '{giaiDoan}' không hợp lệ.");
        }

        return LayCauHinhGiaiDoan(giaiDoan)!.Value;
    }

    private static async Task<byte[]> DocToiDaAsync(Stream noiDung, CancellationToken ct)
    {
        using var boDem = new MemoryStream();
        var vung = new byte[81920];
        int daDoc;
        while ((daDoc = await noiDung.ReadAsync(vung, ct)) > 0)
        {
            boDem.Write(vung, 0, daDoc);
            if (boDem.Length > KichThuocToiDa)
            {
                throw new LoiNghiepVuException("DU_LIEU_KHONG_HOP_LE", "Ảnh vượt quá 2 MB.");
            }
        }

        return boDem.ToArray();
    }

    private Guid LayMaNguoiDung() =>
        nguoiDung.MaNguoiDung
            ?? throw new LoiNghiepVuException("CHUA_XAC_THUC", "Thiếu thông tin người dùng đăng nhập.");

    private async Task XoaBuAsync(string bucket, string khoaLuuTru)
    {
        try
        {
            await luuTru.XoaAsync(bucket, khoaLuuTru, CancellationToken.None);
        }
        catch (Exception loi)
        {
            nhatKy.LogWarning(
                loi,
                "Không xóa được ảnh {Bucket}/{Khoa} sau khi ghi siêu dữ liệu thất bại.",
                bucket,
                khoaLuuTru);
        }
    }
}

