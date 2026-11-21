using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Anh;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class DongThongBaoKiemTra
{
    public string KenhGui { get; set; } = string.Empty;

    public string TrangThaiGui { get; set; } = string.Empty;

    public string NoiDung { get; set; } = string.Empty;

    public string? MaSvNhan { get; set; }

    public Guid? MaNvNhan { get; set; }

    public int SoLanThu { get; set; }
}

public sealed class MoiTruongThongBao : IAsyncDisposable
{
    private readonly ServiceProvider _dichVu;
    private readonly AsyncServiceScope _phamVi;

    private MoiTruongThongBao(ServiceProvider dichVu, AsyncServiceScope phamVi, AppDbContext db, IDbContextTransaction giaoDich)
    {
        _dichVu = dichVu;
        _phamVi = phamVi;
        Db = db;
        GiaoDich = giaoDich;
    }

    public AppDbContext Db { get; }

    public IDbContextTransaction GiaoDich { get; }

    public static async Task<MoiTruongThongBao> TaoAsync()
    {
        var dichVu = new ServiceCollection()
            .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi())
            .BuildServiceProvider();
        var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var giaoDich = await db.Database.BeginTransactionAsync();

        return new MoiTruongThongBao(dichVu, phamVi, db, giaoDich);
    }

    public ThongBaoOutbox TaoOutbox(DateTimeOffset bayGio) =>
        new(Db, new DongHoCoDinh(bayGio), new NhanVienEmailReader(Db));

    public async Task<string> TaoSinhVienAsync()
    {
        var maSv = $"D09K-SV-{Guid.NewGuid():N}";
        await Db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
            VALUES ({maSv}, 'SV kiem thu D09-K', 'd09k@example.test', 'DAI_HOC', DATE '2030-01-01', 'DANG_HOC', 'HK1')
            """);

        return maSv;
    }

    public async Task<Guid> TaoNhanVienAsync(string? email)
    {
        var ma = Guid.CreateVersion7();
        var tenDangNhap = $"d09k-{ma:N}";
        await Db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten, email)
            VALUES ({ma}, {tenDangNhap}, 'khong-dung', 'NV kiem thu D09-K', {email})
            """);
        await Db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
            VALUES ({ma}, {tenDangNhap})
            """);

        return ma;
    }

    public Task<List<DongThongBaoKiemTra>> DocTheoKhoaAsync(string khoa) =>
        Db.Database.SqlQuery<DongThongBaoKiemTra>($"""
                SELECT kenh_gui AS "KenhGui", trang_thai_gui AS "TrangThaiGui", noi_dung AS "NoiDung",
                       ma_sv_nhan AS "MaSvNhan", ma_nv_nhan AS "MaNvNhan", so_lan_thu AS "SoLanThu"
                FROM thong_bao.thong_bao
                WHERE khoa_trung_lap = {khoa}
                """)
            .OrderBy(d => d.KenhGui)
            .ToListAsync();

    public static YeuCauGhiThongBao TaoYeuCau(string? maSv, Guid? maNv, string khoa, string maYeuCau = "YC-D09K") =>
        new(
            MaLoaiThongBao.DuyetHoacTuChoi,
            maSv,
            maNv,
            new Dictionary<string, string>
            {
                [ThamSoThongBao.MaYeuCau] = maYeuCau,
                [ThamSoThongBao.KetQua] = "đã được duyệt",
                [ThamSoThongBao.ChiTiet] = "Thiết bị được giữ chỗ 30 phút."
            },
            "PHIEU_YEU_CAU",
            maYeuCau,
            khoa);

    public async ValueTask DisposeAsync()
    {
        await GiaoDich.DisposeAsync();
        await _phamVi.DisposeAsync();
        await _dichVu.DisposeAsync();
    }
}

