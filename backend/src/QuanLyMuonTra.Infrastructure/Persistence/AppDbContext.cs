using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IEnumerable<EfConfigurationAssemblies> danhSachCauHinh) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var cauHinh in danhSachCauHinh)
        {
            foreach (var assembly in cauHinh.Assemblies)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            }
        }
    }
}
