using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class TaoLoaiThietBiHandler
{
    private readonly DbContext _db;

    public TaoLoaiThietBiHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<LoaiThietBiQuanTriDto> XuLyAsync(LoaiThietBiTaoMoiRequest yeuCau, CancellationToken ct)
    {
        var loai = new LoaiThietBi(
            yeuCau.TenLoai,
            yeuCau.MoTa,
            yeuCau.HangSanXuat,
            yeuCau.ChuKyBaoTri,
            yeuCau.PhiTreMoiNgay);
        _db.Set<LoaiThietBi>().Add(loai);
        await _db.SaveChangesAsync(ct);

        return AnhXaLoaiThietBi.SangDto(loai);
    }
}
