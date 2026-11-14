using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;

namespace QuanLyMuonTra.IntegrationTests.Database;

public class AppDbContextTests
{
    [Fact]
    public async Task AppDbContext_NapCauHinhTuAssemblyVaDocDuocSeedThat()
    {
        var dichVu = new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi(), [typeof(AppDbContextTests).Assembly])
            .BuildServiceProvider();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();

        var cacNhom = await db.Set<NhomSinhVienKiemThu>().Select(n => n.MaNhom).ToListAsync();

        Assert.Equal(3, cacNhom.Count);
    }

    public class NhomSinhVienKiemThu
    {
        public string MaNhom { get; set; } = string.Empty;
    }

    public class NhomSinhVienKiemThuCauHinh : IEntityTypeConfiguration<NhomSinhVienKiemThu>
    {
        public void Configure(EntityTypeBuilder<NhomSinhVienKiemThu> builder)
        {
            builder.ToTable("nhom_sinh_vien", "sinh_vien");
            builder.HasKey(n => n.MaNhom);
            builder.Property(n => n.MaNhom).HasColumnName("ma_nhom");
        }
    }
}

