using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Contracts;

namespace QuanLyMuonTra.Modules.MuonTra.Application;

public sealed class DsPhieuYeuCauMuonSvHandler(DbContext db, IDanhMucReader danhMuc)
{
    public Task<KetQuaPhanTrang<PhieuYeuCauMuonDto>> XuLyAsync(string maSv, int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        return PhieuYeuCauMuonTruyVan.DsTheoSvAsync(db, danhMuc, maSv, trang, kichThuoc, ct);
    }
}
