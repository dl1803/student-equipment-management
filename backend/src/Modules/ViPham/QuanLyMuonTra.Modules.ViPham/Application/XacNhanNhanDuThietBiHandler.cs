using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.ViPham.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.Modules.ViPham.Application;

public sealed class XacNhanNhanDuThietBiHandler(DbContext db, XetMoKhoaQuyenMuon xetMoKhoa)
{
    public Task<KetQuaXetMoKhoaDto> XuLyAsync(string maSv, CancellationToken ct)
    {
        var maSvChuan = maSv.Trim();
        if (maSvChuan.Length == 0)
        {
            throw new LoiNghiepVuException(MaLoi.DuLieuKhongHopLe, "Mã sinh viên không được để trống.");
        }

        return GiaoDichViPham.ChayAsync(db, () => xetMoKhoa.XetAsync(maSvChuan, ct), ct);
    }
}

