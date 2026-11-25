using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsDangKyChoSvHandler(DbContext db, IDanhMucReader danhMuc)
{
    public Task<KetQuaPhanTrang<DangKyChoSvDto>> XuLyAsync(string maSv, int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        return DangKyChoTruyVan.DsTheoSvAsync(db, danhMuc, maSv, trang, kichThuoc, ct);
    }
}
