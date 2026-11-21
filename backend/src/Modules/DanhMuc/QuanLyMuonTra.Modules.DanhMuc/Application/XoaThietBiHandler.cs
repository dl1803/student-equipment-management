using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class XoaThietBiHandler
{
    private readonly DbContext _db;
    private readonly IAdvisoryLockService _khoa;
    private readonly IReadOnlyList<IThamChieuDanhMucProvider> _cacProvider;
    private readonly IAnhService _anh;

    public XoaThietBiHandler(
        DbContext db,
        IAdvisoryLockService khoa,
        IEnumerable<IThamChieuDanhMucProvider> cacProvider,
        IAnhService anh)
    {
        _db = db;
        _khoa = khoa;
        _cacProvider = cacProvider.ToList();
        _anh = anh;
    }

    public async Task XuLyAsync(string ma, CancellationToken ct)
    {
        if (_cacProvider.Count == 0)
        {
            throw new InvalidOperationException(
                "Chưa đăng ký provider tham chiếu danh mục; từ chối xóa để không xóa nhầm thiết bị đã có lịch sử.");
        }

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

        if (thietBi.TinhTrang != TinhTrangThietBi.SanSang)
        {
            throw ConLichSu();
        }

        foreach (var provider in _cacProvider)
        {
            if (await provider.CoThamChieuThietBiAsync(ma, ct))
            {
                throw ConLichSu();
            }
        }

        var cacGiaTri = await _db.Set<LichSuGiaTri>().Where(g => g.MaThietBi == ma).ToListAsync(ct);
        var cacNhaCungCap = await _db.Set<ThietBiNhaCungCap>().Where(n => n.MaThietBi == ma).ToListAsync(ct);
        _db.Set<LichSuGiaTri>().RemoveRange(cacGiaTri);
        _db.Set<ThietBiNhaCungCap>().RemoveRange(cacNhaCungCap);
        _db.Set<ThietBi>().Remove(thietBi);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw XungDot();
        }

        await AnhThietBi.GoTatCaAsync(_anh, ma, ct);

        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }
    }

    private static LoiNghiepVuException KhongTimThay() =>
        new("KHONG_TIM_THAY", "Không tìm thấy thiết bị.");

    private static LoiNghiepVuException XungDot() =>
        new("XUNG_DOT_CAP_NHAT", "Thiết bị vừa được thay đổi bởi giao dịch khác. Vui lòng tải lại.");

    private static LoiNghiepVuException ConLichSu() =>
        new("DA_CO_LICH_SU_CHI_NGUNG_SU_DUNG", "Thiết bị đã có lịch sử hoặc không ở trạng thái sẵn sàng, chỉ được ngưng sử dụng.");
}
