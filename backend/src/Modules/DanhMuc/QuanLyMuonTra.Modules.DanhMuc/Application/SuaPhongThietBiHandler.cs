using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;
using QuanLyMuonTra.Modules.DanhMuc.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.DanhMuc.Application;

public sealed class SuaPhongThietBiHandler
{
    private readonly DbContext _db;

    public SuaPhongThietBiHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<PhongThietBiDto> XuLyAsync(Guid ma, PhongThietBiRequest yeuCau, CancellationToken ct)
    {
        var phong = await _db.Set<PhongThietBi>().FirstOrDefaultAsync(p => p.MaPhong == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy phòng thiết bị.");

        phong.CapNhat(yeuCau.TenPhong, yeuCau.CoSo);
        await _db.SaveChangesAsync(ct);

        return new PhongThietBiDto(phong.MaPhong, phong.TenPhong, phong.CoSo);
    }
}
