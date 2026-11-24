using Microsoft.EntityFrameworkCore;
using QuanLyMuonTra.Modules.DanhMuc.Infrastructure;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.MuonTra;

internal static class DocPhieuMuonTestSupport
{
    public const string FileDuLieu = "D11-V-du-lieu-thu.sql";
    public const string SvA = "D11-SV-A";
    public const string SvB = "D11-SV-B";
    public const string SvC = "D11-SV-C";
    public const string PmA1 = "PM-261023-997001";
    public const string PmA2 = "PM-261024-997002";
    public const string PmA3 = "PM-261023-997003";
    public const string PmA4 = "PM-261001-997004";
    public const string PmA5 = "PM-261010-997005";
    public const string PmA6 = "PM-261123-997006";

    public static readonly Guid Nv1 = Guid.Parse("019c0000-0000-7000-8000-0000000d11a1");
    public static readonly Guid Nv2 = Guid.Parse("019c0000-0000-7000-8000-0000000d11a2");
    public static readonly Guid P1 = Guid.Parse("019c0000-0000-7000-8000-0000000d11b1");
    public static readonly Guid P2 = Guid.Parse("019c0000-0000-7000-8000-0000000d11b2");
    public static readonly Guid CtA1T1 = Guid.Parse("019c0000-0000-7000-8000-0000000d11f1");
    public static readonly Guid CtA1T2 = Guid.Parse("019c0000-0000-7000-8000-0000000d11f2");
    public static readonly Guid CtA5T6 = Guid.Parse("019c0000-0000-7000-8000-0000000d11f6");
    public static readonly Guid CtA5T7 = Guid.Parse("019c0000-0000-7000-8000-0000000d11f7");
    public static readonly DateTimeOffset BayGio = new(2026, 11, 23, 3, 0, 0, TimeSpan.Zero);

    public sealed class DocPhieuTestContext(DbContextOptions<DocPhieuTestContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyMuonTraConfiguration();
            modelBuilder.ApplyDanhMucConfigurations();
        }
    }

    public sealed class DongHoCoDinh(DateTimeOffset thoiDiem) : IClock
    {
        public DateTimeOffset UtcNow => thoiDiem;
    }

    public sealed class QuyenPhongGia(params Guid[] dsMaPhong) : IQuyenPhongService
    {
        public Task<IReadOnlyCollection<Guid>> LayDsMaPhongAsync(Guid maNguoiDung, CancellationToken ct)
        {
            return Task.FromResult<IReadOnlyCollection<Guid>>(dsMaPhong);
        }

        public Task EnsureAsync(Guid maNguoiDung, Guid maPhong, CancellationToken ct)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class AnhServiceGhiNhan : IAnhService
    {
        public List<(string DoiTuongId, string GiaiDoan)> DaGoi { get; } = [];

        public Task<Guid> TaiLenAsync(string giaiDoan, Stream noiDung, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        public Task GanAsync(
            IReadOnlyCollection<Guid> dsMaAnh,
            string doiTuongLoai,
            string doiTuongId,
            string giaiDoan,
            CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        public Task GoAnhThietBiAsync(string maThietBi, IReadOnlyCollection<Guid> dsMaAnh, CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<AnhRefDto>> LayDsAnhKyAsync(
            string doiTuongLoai,
            string doiTuongId,
            string giaiDoan,
            CancellationToken ct)
        {
            DaGoi.Add((doiTuongId, giaiDoan));
            IReadOnlyList<AnhRefDto> dsAnh = [new AnhRefDto(Guid.NewGuid(), $"https://anh.test/{doiTuongId}/{giaiDoan}")];

            return Task.FromResult(dsAnh);
        }

        public Task<IReadOnlyList<AnhRefDto>> LayDsAnhCongKhaiAsync(
            string doiTuongLoai,
            string doiTuongId,
            CancellationToken ct)
        {
            throw new NotSupportedException();
        }
    }

    public static async Task<DocPhieuTestContext> TaoContextAsync(GiaoDichTestRollback gd)
    {
        await gd.NapDuLieuThuAsync(FileDuLieu);
        var options = new DbContextOptionsBuilder<DocPhieuTestContext>().UseNpgsql(gd.KetNoi).Options;
        var db = new DocPhieuTestContext(options);
        await db.Database.UseTransactionAsync(gd.GiaoDich, CancellationToken.None);

        return db;
    }
}
