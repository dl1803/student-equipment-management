using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;
using QuanLyMuonTra.SharedKernel.Contracts;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class SuaThietBiHandler
{
    private readonly DbContext _db;
    private readonly IAdvisoryLockService _khoa;
    private readonly IAnhService _anh;

    public SuaThietBiHandler(DbContext db, IAdvisoryLockService khoa, IAnhService anh)
    {
        _db = db;
        _khoa = khoa;
        _anh = anh;
    }

    public async Task<ThietBiQuanTriDto> XuLyAsync(string ma, ThietBiCapNhatRequest yeuCau, CancellationToken ct)
    {
        var giaoDichNgoai = _db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await _db.Database.BeginTransactionAsync(ct) : null;

        var capCu = await _db.Set<ThietBi>()
            .AsNoTracking()
            .Where(t => t.MaThietBi == ma)
            .Select(t => new { t.MaPhong, t.MaLoai })
            .FirstOrDefaultAsync(ct)
            ?? throw KhongTimThay();

        await _khoa.LockDanhSachPhongLoaiAsync(
            [(capCu.MaPhong, capCu.MaLoai), (yeuCau.MaPhong, yeuCau.MaLoai)],
            ct);

        var thietBi = await _db.Set<ThietBi>().FirstOrDefaultAsync(t => t.MaThietBi == ma, ct)
            ?? throw KhongTimThay();
        if (thietBi.MaPhong != capCu.MaPhong || thietBi.MaLoai != capCu.MaLoai)
        {
            throw XungDot();
        }

        var dsMaNcc = yeuCau.DsMaNCC?.ToList();
        await ThamChieuThietBi.KiemTraAsync(_db, yeuCau.MaLoai, yeuCau.MaPhong, dsMaNcc ?? new List<Guid>(), ct);

        thietBi.CapNhatThongTin(
            yeuCau.MaLoai,
            yeuCau.MaPhong,
            yeuCau.TenThietBi,
            yeuCau.MoTa,
            yeuCau.ThongSoKyThuat,
            yeuCau.NgayNhapKho,
            yeuCau.ThoiHanBaoHanh,
            yeuCau.ViTriKe,
            yeuCau.ViTriTu);

        if (dsMaNcc is not null)
        {
            await ThayNhaCungCapAsync(ma, dsMaNcc, ct);
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw XungDot();
        }

        await AnhThietBi.ThayAsync(_anh, ma, yeuCau.DsMaAnh, ct);

        var ketQua = await DocThietBiQuanTri.LayAsync(_db, _anh, ma, ct);
        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }

        return ketQua;
    }

    private async Task ThayNhaCungCapAsync(string ma, List<Guid> dsMaNcc, CancellationToken ct)
    {
        var hienCo = await _db.Set<ThietBiNhaCungCap>().Where(g => g.MaThietBi == ma).ToListAsync(ct);
        var moi = dsMaNcc.ToHashSet();
        var daCo = hienCo.Select(g => g.MaNCC).ToHashSet();

        foreach (var gan in hienCo.Where(g => !moi.Contains(g.MaNCC)))
        {
            _db.Set<ThietBiNhaCungCap>().Remove(gan);
        }

        foreach (var maNcc in moi.Where(m => !daCo.Contains(m)))
        {
            _db.Set<ThietBiNhaCungCap>().Add(new ThietBiNhaCungCap(ma, maNcc));
        }
    }

    private static LoiNghiepVuException KhongTimThay() =>
        new("KHONG_TIM_THAY", "Không tìm thấy thiết bị.");

    private static LoiNghiepVuException XungDot() =>
        new("XUNG_DOT_CAP_NHAT", "Thiết bị vừa được thay đổi bởi giao dịch khác. Vui lòng tải lại.");
}
