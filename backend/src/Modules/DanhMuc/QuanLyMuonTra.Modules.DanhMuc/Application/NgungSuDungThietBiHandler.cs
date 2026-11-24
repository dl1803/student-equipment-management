using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class NgungSuDungThietBiHandler
{
    private readonly DbContext _db;
    private readonly IAdvisoryLockService _khoa;
    private readonly IAnhService _anh;
    private readonly IAvailabilityService _khaDung;

    public NgungSuDungThietBiHandler(
        DbContext db,
        IAdvisoryLockService khoa,
        IAnhService anh,
        IAvailabilityService khaDung)
    {
        _db = db;
        _khoa = khoa;
        _anh = anh;
        _khaDung = khaDung;
    }

    public async Task<ThietBiQuanTriDto> XuLyAsync(string ma, CancellationToken ct)
    {
        var giaoDichNgoai = _db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await _db.Database.BeginTransactionAsync(ct) : null;

        var cap = await _db.Set<ThietBi>()
            .AsNoTracking()
            .Where(t => t.MaThietBi == ma)
            .Select(t => new { t.MaPhong, t.MaLoai })
            .FirstOrDefaultAsync(ct)
            ?? throw KhongTimThay();

        await _khoa.LockPhongLoaiAsync(cap.MaPhong, cap.MaLoai, ct);

        var thietBi = await _db.Set<ThietBi>().FirstOrDefaultAsync(t => t.MaThietBi == ma, ct)
            ?? throw KhongTimThay();
        if (thietBi.MaPhong != cap.MaPhong || thietBi.MaLoai != cap.MaLoai)
        {
            throw XungDot();
        }

        if (thietBi.TinhTrang == TinhTrangThietBi.SanSang && !thietBi.NgungSuDung)
        {
            var soLuongKhaDung = await _khaDung.TinhKhaDungAsync(cap.MaPhong, cap.MaLoai, ct);
            if (soLuongKhaDung <= 0)
            {
                throw new LoiNghiepVuException(
                    "SO_LUONG_KHA_DUNG_THAY_DOI",
                    "Thiết bị đang thuộc hạn ngạch giữ chỗ của phiếu mượn hoặc đăng ký chờ, không thể ngưng sử dụng.");
            }
        }

        thietBi.NgungSuDungHoa();

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw XungDot();
        }

        var ketQua = await DocThietBiQuanTri.LayAsync(_db, _anh, ma, ct);
        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }

        return ketQua;
    }

    private static LoiNghiepVuException KhongTimThay() =>
        new("KHONG_TIM_THAY", "Không tìm thấy thiết bị.");

    private static LoiNghiepVuException XungDot() =>
        new("XUNG_DOT_CAP_NHAT", "Thiết bị vừa được thay đổi bởi giao dịch khác. Vui lòng tải lại.");
}
