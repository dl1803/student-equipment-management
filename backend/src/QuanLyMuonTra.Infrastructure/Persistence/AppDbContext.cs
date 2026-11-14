using Microsoft.EntityFrameworkCore;

namespace QuanLyMuonTra.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    EfConfigurationAssemblies cauHinh) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var assembly in cauHinh.Assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }
}

