using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class PhieuYeuCauMuonConfiguration : IEntityTypeConfiguration<PhieuYeuCauMuon>
{
    public void Configure(EntityTypeBuilder<PhieuYeuCauMuon> builder)
    {
        builder.ToTable("phieu_yeu_cau_muon", "muon_tra");
        builder.HasKey(p => p.MaYeuCau);
        builder.Property(p => p.MaYeuCau).HasColumnName("ma_yeu_cau");
        builder.Property(p => p.MaSv).HasColumnName("ma_sv");
        builder.Property(p => p.MaPhong).HasColumnName("ma_phong");
        builder.Property(p => p.MaNvDuyet).HasColumnName("ma_nv_duyet");
        builder.Property(p => p.ThoiGianGui).HasColumnName("thoi_gian_gui");
        builder.Property(p => p.ThoiGianMuonDuKienGio).HasColumnName("thoi_gian_muon_du_kien_gio");
        builder.Property(p => p.TrangThai).HasColumnName("trang_thai").HasConversion(new MaTrangThaiConverter<TrangThaiYeuCauMuon>());
        builder.Property(p => p.LyDoTuChoi).HasColumnName("ly_do_tu_choi");
        builder.Property(p => p.ThoiDiemDuyet).HasColumnName("thoi_diem_duyet");
        builder.Property(p => p.HanGiuCho).HasColumnName("han_giu_cho");
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
        builder.HasMany(p => p.ChiTiet).WithOne().HasForeignKey(c => c.MaYeuCau);
        builder.Navigation(p => p.ChiTiet).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
