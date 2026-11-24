using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.BaoTri.Domain;

namespace QuanLyMuonTra.Modules.BaoTri.Infrastructure;

internal sealed class BaoTriSuaChuaConfiguration : IEntityTypeConfiguration<BaoTriSuaChua>
{
    public void Configure(EntityTypeBuilder<BaoTriSuaChua> builder)
    {
        builder.ToTable("bao_tri_sua_chua", "bao_tri", table =>
        {
            table.HasCheckConstraint("ck_bt_loai_thuc_hien", "loai_thuc_hien IN ('BAO_TRI_DINH_KY','SUA_CHUA')");
            table.HasCheckConstraint("ck_bt_ket_qua", "ket_qua IS NULL OR ket_qua IN ('DUA_VE_SAN_SANG','DE_XUAT_THANH_LY')");
            table.HasCheckConstraint("ck_bt_hoan_thanh_dong_bo", "(ket_qua IS NULL) = (ngay_hoan_thanh IS NULL)");
            table.HasCheckConstraint("ck_bt_noi_dung", "ket_qua IS NULL OR btrim(coalesce(noi_dung_thuc_hien, '')) <> ''");
            table.HasCheckConstraint("ck_bt_ngay_hoan_thanh", "ngay_hoan_thanh IS NULL OR ngay_hoan_thanh >= ngay_lap");
            table.HasCheckConstraint("ck_bt_phieu_chi_cho_sua_chua", "loai_thuc_hien = 'SUA_CHUA' OR ma_phieu_muon IS NULL");
        });

        builder.HasKey(x => x.MaBaoTri).HasName("pk_bao_tri_sua_chua");

        builder.Property(x => x.MaBaoTri).HasColumnName("ma_bao_tri").ValueGeneratedNever();
        builder.Property(x => x.MaThietBi).HasColumnName("ma_thiet_bi");
        builder.Property(x => x.MaNvThucHien).HasColumnName("ma_nv_thuc_hien");
        builder.Property(x => x.MaPhieuMuon).HasColumnName("ma_phieu_muon");
        builder.Property(x => x.LoaiThucHien)
            .HasColumnName("loai_thuc_hien")
            .HasConversion(v => LoaiThucHienBaoTriMa.ToMa(v), s => LoaiThucHienBaoTriMa.ToLoai(s));
        builder.Property(x => x.NgayLap).HasColumnName("ngay_lap").HasDefaultValueSql("now()");
        builder.Property(x => x.NoiDungThucHien).HasColumnName("noi_dung_thuc_hien");
        builder.Property(x => x.KetQua).HasColumnName("ket_qua");
        builder.Property(x => x.NgayHoanThanh).HasColumnName("ngay_hoan_thanh");

        builder.HasIndex(x => new { x.MaThietBi, x.LoaiThucHien, x.NgayHoanThanh })
            .HasDatabaseName("ix_bt_thiet_bi_hoan_thanh")
            .IsDescending(false, false, true);
        builder.HasIndex(x => x.MaThietBi)
            .HasDatabaseName("ux_bt_mot_ban_ghi_mo")
            .IsUnique()
            .HasFilter("ket_qua IS NULL");
    }
}
