using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyMuonTra.Modules.MuonTra.Domain;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

internal sealed class ChiTietPhieuMuonConfiguration : IEntityTypeConfiguration<ChiTietPhieuMuon>
{
    public void Configure(EntityTypeBuilder<ChiTietPhieuMuon> builder)
    {
        builder.ToTable("chi_tiet_phieu_muon", "muon_tra");
        builder.HasKey(c => c.MaChiTiet);
        builder.Property(c => c.MaChiTiet).HasColumnName("ma_chi_tiet").ValueGeneratedNever();
        builder.Property(c => c.MaPhieu).HasColumnName("ma_phieu");
        builder.Property(c => c.MaThietBi).HasColumnName("ma_thiet_bi");
        builder.Property(c => c.TinhTrangBanDau).HasColumnName("tinh_trang_ban_dau");
        builder.Property(c => c.PhiTreMoiNgay).HasColumnName("phi_tre_moi_ngay").HasColumnType("numeric(14,0)");
        builder.Property(c => c.TinhTrangKhiTra).HasColumnName("tinh_trang_khi_tra");
        builder.Property(c => c.KetQuaKiemTra).HasColumnName("ket_qua_kiem_tra").HasConversion(new MaTrangThaiConverter<KetQuaKiemTra>());
        builder.Property(c => c.NgayGioTraThucTe).HasColumnName("ngay_gio_tra_thuc_te");
        builder.Property(c => c.MaNvNhanTra).HasColumnName("ma_nv_nhan_tra");
        builder.Property(c => c.MaKhung).HasColumnName("ma_khung");
    }
}
