using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class TaoThietBiHandler
{
    private readonly DbContext _db;
    private readonly IClock _dongHo;
    private readonly IAnhService _anh;

    public TaoThietBiHandler(DbContext db, IClock dongHo, IAnhService anh)
    {
        _db = db;
        _dongHo = dongHo;
        _anh = anh;
    }

    public async Task<ThietBiQuanTriDto> XuLyAsync(ThietBiTaoMoiRequest yeuCau, CancellationToken ct)
    {
        var thietBi = ThietBi.NhapKho(
            yeuCau.MaThietBi,
            yeuCau.MaLoai,
            yeuCau.MaPhong,
            yeuCau.TenThietBi,
            yeuCau.MoTa,
            yeuCau.ThongSoKyThuat,
            yeuCau.NgayNhapKho,
            yeuCau.ThoiHanBaoHanh,
            yeuCau.ViTriKe,
            yeuCau.ViTriTu);
        var giaTriDau = new LichSuGiaTri(yeuCau.MaThietBi, yeuCau.GiaTri, _dongHo.UtcNow);
        var dsMaNcc = yeuCau.DsMaNCC?.ToList() ?? new List<Guid>();

        var giaoDichNgoai = _db.Database.CurrentTransaction;
        await using var giaoDichRieng = giaoDichNgoai is null ? await _db.Database.BeginTransactionAsync(ct) : null;

        await ThamChieuThietBi.KiemTraAsync(_db, yeuCau.MaLoai, yeuCau.MaPhong, dsMaNcc, ct);
        await KiemTraKhongTrungMaAsync(yeuCau.MaThietBi, ct);

        _db.Set<ThietBi>().Add(thietBi);
        _db.Set<LichSuGiaTri>().Add(giaTriDau);
        foreach (var maNcc in dsMaNcc)
        {
            _db.Set<ThietBiNhaCungCap>().Add(new ThietBiNhaCungCap(yeuCau.MaThietBi, maNcc));
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            await KiemTraKhongTrungMaAsync(yeuCau.MaThietBi, ct);
            throw;
        }

        await AnhThietBi.GanMoiAsync(_anh, yeuCau.MaThietBi, yeuCau.DsMaAnh, ct);

        var ketQua = await DocThietBiQuanTri.LayAsync(_db, _anh, yeuCau.MaThietBi, ct);
        if (giaoDichRieng is not null)
        {
            await giaoDichRieng.CommitAsync(ct);
        }

        return ketQua;
    }

    private async Task KiemTraKhongTrungMaAsync(string maThietBi, CancellationToken ct)
    {
        if (await _db.Set<ThietBi>().AsNoTracking().AnyAsync(t => t.MaThietBi == maThietBi, ct))
        {
            throw new LoiNghiepVuException("MA_THIET_BI_TRUNG", "Mã thiết bị đã tồn tại.");
        }
    }
}
