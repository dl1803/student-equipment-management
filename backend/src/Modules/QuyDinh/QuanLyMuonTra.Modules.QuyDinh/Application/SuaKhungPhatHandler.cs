using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.QuyDinh.Contracts;
using QuanLyMuonTra.Modules.QuyDinh.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.QuyDinh.Application;

public sealed class SuaKhungPhatHandler
{
    private readonly DbContext _db;

    public SuaKhungPhatHandler(DbContext db)
    {
        _db = db;
    }

    public async Task<KhungPhatDto> XuLyAsync(Guid ma, KhungPhatRequest yeuCau, CancellationToken ct)
    {
        var khung = await _db.Set<KhungPhat>().FirstOrDefaultAsync(k => k.MaKhung == ma, ct)
            ?? throw new LoiNghiepVuException("KHONG_TIM_THAY", "Không tìm thấy khung phạt.");

        khung.CapNhat(yeuCau.TenKhung, yeuCau.TyLePhanTram);
        await _db.SaveChangesAsync(ct);

        return new KhungPhatDto(khung.MaKhung, khung.TenKhung, khung.TyLePhanTram, khung.DangSuDung);
    }
}
