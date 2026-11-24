using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

public sealed class MuonTraTestContext : DbContext
{
    public MuonTraTestContext(DbContextOptions<MuonTraTestContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyMuonTraConfiguration();
    }
}
