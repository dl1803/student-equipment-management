using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.ThongBao.Application;

public sealed class DsThongBaoSvHandler(DbContext db)
{
    public async Task<KetQuaPhanTrang<ThongBaoDto>> XuLyAsync(string maSv, int trang, int kichThuoc, CancellationToken ct)
    {
        PhanTrang.KiemTra(trang, kichThuoc);

        var goc = db.Database.SqlQuery<DongThongBao>($"""
            SELECT ma_thong_bao AS "MaThongBao", loai_thong_bao AS "LoaiThongBao", noi_dung AS "NoiDung",
                   thoi_diem_gui AS "ThoiDiemGui", doi_tuong_loai AS "DoiTuongLoai", doi_tuong_id AS "DoiTuongId"
            FROM thong_bao.thong_bao
            WHERE ma_sv_nhan = {maSv} AND kenh_gui = 'UNG_DUNG'
            """);

        var tongSo = await goc.CountAsync(ct);
        var batDau = PhanTrang.ViTriBatDau(trang, kichThuoc);
        if (batDau >= tongSo)
        {
            return PhanTrang.Rong<ThongBaoDto>(tongSo, trang, kichThuoc);
        }

        var cacDong = await goc
            .OrderByDescending(d => d.ThoiDiemGui)
            .ThenBy(d => d.MaThongBao)
            .Skip((int)batDau)
            .Take(kichThuoc)
            .ToListAsync(ct);

        var danhSach = cacDong
            .Select(d => new ThongBaoDto(d.MaThongBao, d.LoaiThongBao, d.NoiDung, d.ThoiDiemGui, d.DoiTuongLoai, d.DoiTuongId))
            .ToList();

        return new KetQuaPhanTrang<ThongBaoDto>(tongSo, trang, kichThuoc, danhSach);
    }

    public sealed class DongThongBao
    {
        public Guid MaThongBao { get; set; }

        public string LoaiThongBao { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public DateTimeOffset ThoiDiemGui { get; set; }

        public string? DoiTuongLoai { get; set; }

        public string? DoiTuongId { get; set; }
    }
}

