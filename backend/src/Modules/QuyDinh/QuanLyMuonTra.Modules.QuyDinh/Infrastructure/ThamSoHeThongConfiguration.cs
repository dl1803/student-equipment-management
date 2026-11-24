using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.QuyDinh.Domain;

namespace QuanLyMuonTra.Modules.QuyDinh.Infrastructure;

internal sealed class ThamSoHeThongConfiguration : IEntityTypeConfiguration<ThamSoHeThong>
{
    public void Configure(EntityTypeBuilder<ThamSoHeThong> builder)
    {
        builder.ToTable("tham_so_he_thong", "quy_dinh", table =>
        {
            table.HasCheckConstraint("ck_tshs_mot_dong", "id = 1");
            table.HasCheckConstraint("ck_tshs_so_ngay_khoa", "so_ngay_khoa >= 0");
            table.HasCheckConstraint("ck_tshs_thoi_gian_giu_cho", "thoi_gian_giu_cho_phut > 0");
            table.HasCheckConstraint("ck_tshs_an_han", "an_han_phut >= 0");
            table.HasCheckConstraint("ck_tshs_so_gio_gia_han", "so_gio_gia_han_toi_da > 0");
            table.HasCheckConstraint("ck_tshs_nguong_vi_pham", "nguong_vi_pham > 0");
            table.HasCheckConstraint("ck_tshs_so_lan_dang_nhap_sai", "so_lan_dang_nhap_sai_toi_da > 0");
            table.HasCheckConstraint("ck_tshs_gio_quay", "(gio_mo_cua IS NULL AND gio_dong_cua IS NULL) OR (gio_mo_cua IS NOT NULL AND gio_dong_cua IS NOT NULL AND gio_mo_cua < gio_dong_cua)");
        });

        builder.HasKey(x => x.Id).HasName("pk_tham_so_he_thong");

        builder.Property(x => x.Id).HasColumnName("id").HasDefaultValue((short)1).ValueGeneratedNever();
        builder.Property(x => x.SoNgayKhoa).HasColumnName("so_ngay_khoa").HasDefaultValue(3);
        builder.Property(x => x.ThoiGianGiuChoPhut).HasColumnName("thoi_gian_giu_cho_phut").HasDefaultValue(30);
        builder.Property(x => x.AnHanPhut).HasColumnName("an_han_phut").HasDefaultValue(15);
        builder.Property(x => x.SoGioGiaHanToiDa).HasColumnName("so_gio_gia_han_toi_da").HasDefaultValue(2);
        builder.Property(x => x.NguongViPham).HasColumnName("nguong_vi_pham").HasDefaultValue(3);
        builder.Property(x => x.SoLanDangNhapSaiToiDa).HasColumnName("so_lan_dang_nhap_sai_toi_da").HasDefaultValue(5);
        builder.Property(x => x.GioMoCua).HasColumnName("gio_mo_cua");
        builder.Property(x => x.GioDongCua).HasColumnName("gio_dong_cua");
    }
}
