using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.UnitTests.MoHinhEf;

public sealed class TaiKhoanMoHinhEfTests
{
    private static readonly IModel MoHinh =
        MoHinhEfKiemTra.LayMoHinh<TaiKhoanMoHinhDbContext>(o => new TaiKhoanMoHinhDbContext(o));

    [Fact]
    public void UC19_NguoiDungHeThong_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(NguoiDungHeThong), "tai_khoan", "nguoi_dung_he_thong");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_nguoi_dung_he_thong", "ma_nguoi_dung");
        MoHinhEfKiemTra.KiemTraCot(
            entity,
            "ma_nguoi_dung:NOT NULL",
            "ten_dang_nhap:NOT NULL",
            "mat_khau_bam:NOT NULL",
            "ho_ten:NOT NULL",
            "email:NULL",
            "trang_thai_khoa:NOT NULL",
            "so_lan_dang_nhap_sai:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity, "uq_ndht_ten_dang_nhap|ten_dang_nhap|UNIQUE|");
        MoHinhEfKiemTra.KiemTraRangBuocCheck(
            entity,
            "ck_ndht_ten_dang_nhap_khong_rong",
            "ck_ndht_trang_thai_khoa",
            "ck_ndht_so_lan_dang_nhap_sai",
            "ck_ndht_email");
    }

    [Fact]
    public void UC19_BangCon_NhanVienVaQuanTri_TachBangRiengTheoTpt()
    {
        var nhanVien = MoHinhEfKiemTra.LayBang(MoHinh, typeof(NhanVienQuanLy), "tai_khoan", "nhan_vien_quan_ly");
        var quanTri = MoHinhEfKiemTra.LayBang(MoHinh, typeof(QuanTriVien), "tai_khoan", "quan_tri_vien");

        Assert.Equal("ma_nv", nhanVien.FindProperty(nameof(NhanVienQuanLy.MaNv))!.GetColumnName(MoHinhEfKiemTra.Dinh(nhanVien)));
        Assert.Equal("ma_qt", quanTri.FindProperty(nameof(QuanTriVien.MaQt))!.GetColumnName(MoHinhEfKiemTra.Dinh(quanTri)));
    }

    [Fact]
    public void UC14_NhanVienPhong_KhopDdl()
    {
        var entity = MoHinhEfKiemTra.LayBang(MoHinh, typeof(NhanVienPhong), "tai_khoan", "nhan_vien_phong");
        MoHinhEfKiemTra.KiemTraKhoaChinh(entity, "pk_nhan_vien_phong", "ma_nguoi_dung", "ma_phong");
        MoHinhEfKiemTra.KiemTraCot(entity, "ma_nguoi_dung:NOT NULL", "ma_phong:NOT NULL");
        MoHinhEfKiemTra.KiemTraChiMuc(entity, "ix_nvp_phong|ma_phong||");
        MoHinhEfKiemTra.KiemTraKhoaNgoai(entity, "fk_nvp_nhan_vien|ma_nguoi_dung|tai_khoan.nhan_vien_quan_ly|NoAction");
    }

    private sealed class TaiKhoanMoHinhDbContext(DbContextOptions<TaiKhoanMoHinhDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NguoiDungHeThong).Assembly);
    }
}

