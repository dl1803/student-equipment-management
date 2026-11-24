using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class CapNhatPhiTreHandler
{
    private readonly DbContext _db;

    public CapNhatPhiTreHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<PhiTreDto> XuLyAsync(Guid maLoai, PhiTreRequest yeuCau, CancellationToken ct)
    {
        var loai = await _db.Set<LoaiThietBi>().FirstOrDefaultAsync(l => l.MaLoai == maLoai, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy loại thiết bị.");

        loai.CapNhatPhiTre(yeuCau.PhiTreMoiNgay);
        await _db.SaveChangesAsync(ct);

        return new PhiTreDto(loai.MaLoai, yeuCau.PhiTreMoiNgay);
    }
}
