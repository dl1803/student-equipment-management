using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.TaiKhoan.Domain;

namespace QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;

internal sealed class NguoiDungHeThongConfiguration : IEntityTypeConfiguration<NguoiDungHeThong>
{
    public void Configure(EntityTypeBuilder<NguoiDungHeThong> builder)
    {
        builder.UseTptMappingStrategy();

        builder.ToTable("nguoi_dung_he_thong", "tai_khoan", table =>
        {
            table.HasCheckConstraint("ck_ndht_ten_dang_nhap_khong_rong", "btrim(ten_dang_nhap) <> ''");
            table.HasCheckConstraint("ck_ndht_trang_thai_khoa", "trang_thai_khoa IN ('HOAT_DONG','TAM_KHOA_DANG_NHAP','BI_KHOA')");
            table.HasCheckConstraint("ck_ndht_so_lan_dang_nhap_sai", "so_lan_dang_nhap_sai >= 0");
            table.HasCheckConstraint("ck_ndht_email", "email IS NULL OR position('@' in email) > 1");
        });

        builder.HasKey(x => x.MaNguoiDung).HasName("pk_nguoi_dung_he_thong");

        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung").ValueGeneratedNever();
        builder.Property(x => x.TenDangNhap).HasColumnName("ten_dang_nhap");
        builder.Property(x => x.MatKhauBam).HasColumnName("mat_khau_bam");
        builder.Property(x => x.HoTen).HasColumnName("ho_ten");
        builder.Property(x => x.Email).HasColumnName("email");
        builder.Property(x => x.TrangThaiKhoa).HasColumnName("trang_thai_khoa").HasDefaultValue(MaTrangThaiKhoa.HoatDong);
        builder.Property(x => x.SoLanDangNhapSai).HasColumnName("so_lan_dang_nhap_sai").HasDefaultValue(0);

        builder.HasIndex(x => x.TenDangNhap).IsUnique().HasDatabaseName("uq_ndht_ten_dang_nhap");
    }
}

