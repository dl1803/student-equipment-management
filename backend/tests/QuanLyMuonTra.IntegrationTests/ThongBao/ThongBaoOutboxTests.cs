using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.Infrastructure.Persistence;
using QuanLyMuonTra.IntegrationTests.Anh;
using QuanLyMuonTra.IntegrationTests.MuonTra;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Infrastructure;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class ThongBaoOutboxTests
{
    private const string ChuoiKetNoiKhongDung = "Host=localhost;Database=khong-dung;Username=khong-dung;Password=khong-dung";
    private static readonly DateTimeOffset BayGio = new(2026, 11, 21, 3, 0, 0, TimeSpan.Zero);

    [CanKetNoiTestFact]
    public async Task UC17_GhiChoSinhVien_SinhHaiDongUngDungDaGuiVaEmailChoGui()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();
        var khoa = $"KT-D09K-{Guid.NewGuid():N}";

        await moi.TaoOutbox(BayGio).GhiAsync(MoiTruongThongBao.TaoYeuCau(maSv, null, khoa), CancellationToken.None);

        var cacDong = await moi.DocTheoKhoaAsync(khoa);
        Assert.Equal(2, cacDong.Count);
        var email = cacDong[0];
        var ungDung = cacDong[1];
        Assert.Equal("EMAIL", email.KenhGui);
        Assert.Equal("CHO_GUI", email.TrangThaiGui);
        Assert.StartsWith("<p>", email.NoiDung);
        Assert.Equal("UNG_DUNG", ungDung.KenhGui);
        Assert.Equal("DA_GUI", ungDung.TrangThaiGui);
        Assert.DoesNotContain("<p>", ungDung.NoiDung);
        Assert.Contains("YC-D09K", ungDung.NoiDung);
        Assert.All(cacDong, d =>
        {
            Assert.Equal(maSv, d.MaSvNhan);
            Assert.Null(d.MaNvNhan);
            Assert.Equal(0, d.SoLanThu);
        });
    }

    [CanKetNoiTestFact]
    public async Task UC17_GhiChoNhanVienCoEmail_SinhHaiDong()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maNv = await moi.TaoNhanVienAsync("nv-d09k@example.test");
        var khoa = $"KT-D09K-{Guid.NewGuid():N}";

        await moi.TaoOutbox(BayGio).GhiAsync(MoiTruongThongBao.TaoYeuCau(null, maNv, khoa), CancellationToken.None);

        var cacDong = await moi.DocTheoKhoaAsync(khoa);
        Assert.Equal(["EMAIL", "UNG_DUNG"], cacDong.Select(d => d.KenhGui).ToArray());
        Assert.All(cacDong, d =>
        {
            Assert.Equal(maNv, d.MaNvNhan);
            Assert.Null(d.MaSvNhan);
        });
    }

    [CanKetNoiTestFact]
    public async Task KT10_NhanVienKhongCoEmail_ChiGhiDongUngDung()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maNv = await moi.TaoNhanVienAsync(null);
        var khoa = $"KT-D09K-{Guid.NewGuid():N}";

        await moi.TaoOutbox(BayGio).GhiAsync(MoiTruongThongBao.TaoYeuCau(null, maNv, khoa), CancellationToken.None);

        var dong = Assert.Single(await moi.DocTheoKhoaAsync(khoa));
        Assert.Equal("UNG_DUNG", dong.KenhGui);
        Assert.Equal("DA_GUI", dong.TrangThaiGui);
    }

    [CanKetNoiTestFact]
    public async Task NFR08_CungKhoaTrungLap_GhiLanHaiBiBoQuaVaKhongDoiNoiDung()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();
        var khoa = $"KT-D09K-{Guid.NewGuid():N}";
        var outbox = moi.TaoOutbox(BayGio);

        await outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau(maSv, null, khoa, "YC-LAN-1"), CancellationToken.None);
        await outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau(maSv, null, khoa, "YC-LAN-2"), CancellationToken.None);

        var cacDong = await moi.DocTheoKhoaAsync(khoa);
        Assert.Equal(2, cacDong.Count);
        Assert.All(cacDong, d => Assert.Contains("YC-LAN-1", d.NoiDung));
        Assert.DoesNotContain(cacDong, d => d.NoiDung.Contains("YC-LAN-2"));
    }

    [CanKetNoiTestFact]
    public async Task NFR08_KhoaKhacNhau_KhongBiChanLanNhau()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();
        var khoaA = $"KT-D09K-{Guid.NewGuid():N}";
        var khoaB = $"KT-D09K-{Guid.NewGuid():N}";
        var outbox = moi.TaoOutbox(BayGio);

        await outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau(maSv, null, khoaA), CancellationToken.None);
        await outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau(maSv, null, khoaB), CancellationToken.None);

        Assert.Equal(2, (await moi.DocTheoKhoaAsync(khoaA)).Count);
        Assert.Equal(2, (await moi.DocTheoKhoaAsync(khoaB)).Count);
    }

    [CanKetNoiTestFact]
    public async Task NFR02_GiaoDichBiHoanTac_KhongConThongBaoMa()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();
        var khoa = $"KT-D09K-{Guid.NewGuid():N}";
        await moi.GiaoDich.CreateSavepointAsync("truoc_nghiep_vu");

        await moi.TaoOutbox(BayGio).GhiAsync(MoiTruongThongBao.TaoYeuCau(maSv, null, khoa), CancellationToken.None);
        Assert.Equal(2, (await moi.DocTheoKhoaAsync(khoa)).Count);

        await moi.GiaoDich.RollbackToSavepointAsync("truoc_nghiep_vu");

        Assert.Empty(await moi.DocTheoKhoaAsync(khoa));
    }

    [CanKetNoiTestFact]
    public async Task KhongKhoaTrungLap_MoiLanGhiLaDongMoi()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();
        var outbox = moi.TaoOutbox(BayGio);
        var yeuCau = MoiTruongThongBao.TaoYeuCau(maSv, null, "bo-di") with { KhoaTrungLap = null };

        await outbox.GhiAsync(yeuCau, CancellationToken.None);
        await outbox.GhiAsync(yeuCau, CancellationToken.None);

        var soDong = await moi.Db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM thong_bao.thong_bao WHERE ma_sv_nhan = {maSv}")
            .SingleAsync();
        Assert.Equal(4, soDong);
    }

    [Fact]
    public async Task KhongCoGiaoDich_NemInvalidOperationException()
    {
        await using var dichVu = new ServiceCollection().AddInfrastructure(ChuoiKetNoiKhongDung).BuildServiceProvider();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var outbox = new ThongBaoOutbox(db, new DongHoCoDinh(BayGio), new NhanVienEmailReader(db));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau("SV-X", null, "k"), CancellationToken.None));
    }

    [Fact]
    public async Task CoCaHaiNguoiNhanHoacKhongCoAi_NemArgumentException()
    {
        await using var dichVu = new ServiceCollection().AddInfrastructure(ChuoiKetNoiKhongDung).BuildServiceProvider();
        await using var phamVi = dichVu.CreateAsyncScope();
        var db = phamVi.ServiceProvider.GetRequiredService<AppDbContext>();
        var outbox = new ThongBaoOutbox(db, new DongHoCoDinh(BayGio), new NhanVienEmailReader(db));

        await Assert.ThrowsAsync<ArgumentException>(
            () => outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau("SV-X", Guid.NewGuid(), "k"), CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(
            () => outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau(null, null, "k"), CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(
            () => outbox.GhiAsync(MoiTruongThongBao.TaoYeuCau("SV-X", null, "k") with { DoiTuongId = null }, CancellationToken.None));
    }

    [CanKetNoiTestFact]
    public async Task GhiChoNhanVien_MaSvNhanChuoiRong_VanHopLeVaKhongLoiCheckConstraint()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maNv = await moi.TaoNhanVienAsync("nv-sanit@example.test");
        var khoa = $"KT-D09K-{Guid.NewGuid():N}";
        var yeuCau = MoiTruongThongBao.TaoYeuCau(null, maNv, khoa) with { MaSvNhan = "   " };

        await moi.TaoOutbox(BayGio).GhiAsync(yeuCau, CancellationToken.None);

        var cacDong = await moi.DocTheoKhoaAsync(khoa);
        Assert.Equal(2, cacDong.Count);
        Assert.All(cacDong, d =>
        {
            Assert.Equal(maNv, d.MaNvNhan);
            Assert.Null(d.MaSvNhan);
        });
    }

    [CanKetNoiTestFact]
    public async Task KhoaTrungLapChuoiKhoangTrang_CoiNhuNullVaMoiLanGhiLaDongMoi()
    {
        await using var moi = await MoiTruongThongBao.TaoAsync();
        var maSv = await moi.TaoSinhVienAsync();
        var outbox = moi.TaoOutbox(BayGio);
        var yeuCau = MoiTruongThongBao.TaoYeuCau(maSv, null, "bo-di") with { KhoaTrungLap = "   " };

        await outbox.GhiAsync(yeuCau, CancellationToken.None);
        await outbox.GhiAsync(yeuCau, CancellationToken.None);

        var soDong = await moi.Db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM thong_bao.thong_bao WHERE ma_sv_nhan = {maSv}")
            .SingleAsync();
        Assert.Equal(4, soDong);
    }
}

