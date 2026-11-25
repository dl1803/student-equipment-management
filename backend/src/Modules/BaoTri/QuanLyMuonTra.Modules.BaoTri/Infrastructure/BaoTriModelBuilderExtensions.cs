using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Modules.BaoTri.Infrastructure;

public static class BaoTriModelBuilderExtensions
{
    public static ModelBuilder ApplyBaoTriConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BaoTriSuaChuaConfiguration());
        return modelBuilder;
    }
}
