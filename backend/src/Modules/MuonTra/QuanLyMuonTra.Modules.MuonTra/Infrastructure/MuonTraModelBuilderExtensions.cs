using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.MuonTra.Infrastructure;

public static class MuonTraModelBuilderExtensions
{
    public static ModelBuilder ApplyMuonTraConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PhieuYeuCauMuonConfiguration());
        modelBuilder.ApplyConfiguration(new ChiTietYeuCauMuonConfiguration());
        modelBuilder.ApplyConfiguration(new DangKyChoConfiguration());
        modelBuilder.ApplyConfiguration(new PhieuMuonConfiguration());
        modelBuilder.ApplyConfiguration(new ChiTietPhieuMuonConfiguration());
        return modelBuilder;
    }
}
