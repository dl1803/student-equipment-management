using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class SuaLoaiThietBiHandler
{
    private readonly DbContext _db;

    public SuaLoaiThietBiHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<LoaiThietBiQuanTriDto> XuLyAsync(Guid ma, LoaiThietBiCapNhatRequest yeuCau, CancellationToken ct)
    {
        var loai = await _db.Set<LoaiThietBi>().FirstOrDefaultAsync(l => l.MaLoai == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy loại thiết bị.");

        loai.CapNhat(yeuCau.TenLoai, yeuCau.MoTa, yeuCau.HangSanXuat, yeuCau.ChuKyBaoTri);
        await _db.SaveChangesAsync(ct);

        return AnhXaLoaiThietBi.SangDto(loai);
    }
}
