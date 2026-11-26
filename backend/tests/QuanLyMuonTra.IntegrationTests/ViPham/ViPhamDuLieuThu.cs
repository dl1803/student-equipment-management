using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.QuyDinh.Infrastructure;
using QuanLyMuonTra.Modules.ViPham.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.ViPham;

internal static class ViPhamDuLieuThu
{
    public static ServiceProvider TaoDichVu() =>
        new ServiceCollection()
            .AddInfrastructure(
                KetNoiKiemThu.LayChuoiKetNoi(),
                [
                    typeof(MuonTraModelBuilderExtensions).Assembly,
                    typeof(DanhMucModelBuilderExtensions).Assembly,
                    typeof(QuyDinhModelBuilderExtensions).Assembly,
                    typeof(ViPhamModelBuilderExtensions).Assembly
                ])
            .BuildServiceProvider();

    public static string MaHau() => Random.Shared.Next(100000, 999999).ToString();

    public static async Task TaoSinhVienAsync(AppDbContext db, string maSv)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({maSv}, 'SV kiem thu D14', 'kt14@example.com', 'DAI_HOC', DATE '2030-01-01', 'DANG_HOC', 'HK1')
            """);
    }

    public static async Task<Guid> TaoNhanVienAsync(AppDbContext db, string hau)
    {
        var maNv = Guid.CreateVersion7();
        var tenDangNhap = $"kt14-{hau}";
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten)
            VALUES ({maNv}, {tenDangNhap}, 'khong-dung', 'NV kiem thu D14')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv) VALUES ({maNv}, {tenDangNhap})
            """);

        return maNv;
    }

    public static async Task<Guid> TaoPhongAsync(AppDbContext db)
    {
        var maPhong = Guid.CreateVersion7();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.phong_thiet_bi (ma_phong, ten_phong, co_so) VALUES ({maPhong}, 'Phong KT14', 'CS1')
            """);

        return maPhong;
    }

    public static async Task<Guid> TaoLoaiAsync(AppDbContext db, decimal phiTreMoiNgay)
    {
        var maLoai = Guid.CreateVersion7();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.loai_thiet_bi (ma_loai, ten_loai, chu_ky_bao_tri, phi_tre_moi_ngay)
            VALUES ({maLoai}, 'Loai KT14', 180, {phiTreMoiNgay})
            """);

        return maLoai;
    }

    public static async Task TaoThietBiAsync(AppDbContext db, string maThietBi, Guid maLoai, Guid maPhong, decimal giaTri)
    {
        var tenThietBi = $"Thiet bi {maThietBi}";
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.thiet_bi (ma_thiet_bi, ma_loai, ma_phong, ten_thiet_bi, ngay_nhap_kho)
            VALUES ({maThietBi}, {maLoai}, {maPhong}, {tenThietBi}, DATE '2026-01-01')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO danh_muc.lich_su_gia_tri (ma_lich_su, ma_thiet_bi, gia_tri)
            VALUES ({Guid.CreateVersion7()}, {maThietBi}, {giaTri})
            """);
    }

    public static async Task<Guid> TaoKhungPhatAsync(AppDbContext db, decimal tyLePhanTram)
    {
        var maKhung = Guid.CreateVersion7();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO quy_dinh.khung_phat (ma_khung, ten_khung, ty_le_phan_tram)
            VALUES ({maKhung}, 'Khung KT14', {tyLePhanTram})
            """);

        return maKhung;
    }

    public static async Task TaoPhieuAsync(AppDbContext db, string maPhieu, string maSv, Guid maNv, DateTimeOffset hanTra)
    {
        var ngayMuon = hanTra.AddHours(-8);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.phieu_muon (ma_phieu, ma_sv, ma_nv_lap, ngay_gio_muon, ngay_gio_hen_tra)
            VALUES ({maPhieu}, {maSv}, {maNv}, {ngayMuon}, {hanTra})
            """);
    }

    public static async Task ThemChiTietPhieuAsync(
        AppDbContext db,
        string maPhieu,
        string maThietBi,
        decimal phiTreMoiNgay,
        string? ketQua,
        string? tinhTrangKhiTra,
        DateTimeOffset? ngayTra,
        Guid? maNvNhanTra,
        Guid? maKhung)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.chi_tiet_phieu_muon
                (ma_chi_tiet, ma_phieu, ma_thiet_bi, tinh_trang_ban_dau, phi_tre_moi_ngay,
                 tinh_trang_khi_tra, ket_qua_kiem_tra, ngay_gio_tra_thuc_te, ma_nv_nhan_tra, ma_khung)
            VALUES
                ({Guid.CreateVersion7()}, {maPhieu}, {maThietBi}, 'Con tot', {phiTreMoiNgay},
                 {tinhTrangKhiTra}, {ketQua}, {ngayTra}, {maNvNhanTra}, {maKhung})
            """);
    }

    public static async Task TaoBienBanAsync(
        AppDbContext db,
        string maBienBan,
        string maPhieu,
        Guid maNv,
        decimal tongTien,
        DateTimeOffset ngayLap)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO vi_pham.bien_ban_vi_pham (ma_bien_ban, ma_phieu_muon, ma_nv_lap, ngay_lap, tong_tien)
            VALUES ({maBienBan}, {maPhieu}, {maNv}, {ngayLap}, {tongTien})
            """);
    }

    public static async Task ThemChiTietBienBanAsync(
        AppDbContext db,
        string maBienBan,
        string maPhieu,
        string maThietBi,
        string loaiViPham,
        int? soNgayTre,
        decimal soTien,
        Guid? maKhung)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO vi_pham.chi_tiet_bien_ban
                (ma_bien_ban, ma_phieu_muon, ma_thiet_bi, loai_vi_pham, so_ngay_tre, so_tien, ma_khung)
            VALUES ({maBienBan}, {maPhieu}, {maThietBi}, {loaiViPham}, {soNgayTre}, {soTien}, {maKhung})
            """);
    }

    public static async Task ThemViPhamAsync(
        AppDbContext db,
        string maSv,
        string loaiViPham,
        DateTimeOffset thoiDiemGhiNhan,
        string? maBienBan,
        string? maYeuCau)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO vi_pham.vi_pham (ma_vi_pham, ma_sv, loai_vi_pham, thoi_diem_ghi_nhan, hoc_ky, ma_bien_ban, ma_yeu_cau)
            VALUES ({Guid.CreateVersion7()}, {maSv}, {loaiViPham}, {thoiDiemGhiNhan}, 'HK1', {maBienBan}, {maYeuCau})
            """);
    }

    public static async Task TaoYeuCauHetHanAsync(AppDbContext db, string maYeuCau, string maSv, Guid maPhong)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO muon_tra.phieu_yeu_cau_muon (ma_yeu_cau, ma_sv, ma_phong, thoi_gian_muon_du_kien_gio, trang_thai)
            VALUES ({maYeuCau}, {maSv}, {maPhong}, 2, 'HET_HAN')
            """);
    }
}

