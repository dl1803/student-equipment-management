using QuanLyMuonTra.Modules.MuonTra.Application;
using QuanLyMuonTra.Modules.MuonTra.Infrastructure;
using QuanLyMuonTra.Modules.ThongBao.Contracts;

namespace QuanLyMuonTra.UnitTests.MuonTra;

public class ThongBaoDuyetOutboxTests
{
    private sealed class OutboxGhiNho : IThongBaoOutbox
    {
        public List<YeuCauGhiThongBao> DaGhi { get; } = [];

        public Task GhiAsync(YeuCauGhiThongBao yeuCau, CancellationToken ct)
        {
            DaGhi.Add(yeuCau);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task UC06_Duyet_ChuyenThanhYeuCauDuyetHoacTuChoiChoSinhVienVoiGioVietNam()
    {
        var outbox = new OutboxGhiNho();
        var han = new DateTimeOffset(2026, 11, 18, 7, 30, 0, TimeSpan.Zero);

        await new ThongBaoDuyetOutbox(outbox).GhiAsync(
            new ThongBaoDuyetHoacTuChoi("YC-1", "SV-1", true, han, null),
            CancellationToken.None);

        var yeuCau = Assert.Single(outbox.DaGhi);
        Assert.Equal("DUYET_HOAC_TU_CHOI", yeuCau.LoaiThongBao);
        Assert.Equal("SV-1", yeuCau.MaSvNhan);
        Assert.Null(yeuCau.MaNvNhan);
        Assert.Equal("PHIEU_YEU_CAU", yeuCau.DoiTuongLoai);
        Assert.Equal("YC-1", yeuCau.DoiTuongId);
        Assert.Equal("DUYET:YC-1", yeuCau.KhoaTrungLap);
        Assert.Equal("YC-1", yeuCau.ThamSo["maYeuCau"]);
        Assert.Equal("đã được duyệt", yeuCau.ThamSo["ketQua"]);
        Assert.Contains("14:30 ngày 18/11/2026", yeuCau.ThamSo["chiTiet"]);
    }

    [Fact]
    public async Task UC06_TuChoi_MangLyDoVaCungKhoaTrungLap()
    {
        var outbox = new OutboxGhiNho();

        await new ThongBaoDuyetOutbox(outbox).GhiAsync(
            new ThongBaoDuyetHoacTuChoi("YC-2", "SV-2", false, null, "Hết hàng"),
            CancellationToken.None);

        var yeuCau = Assert.Single(outbox.DaGhi);
        Assert.Equal("đã bị từ chối", yeuCau.ThamSo["ketQua"]);
        Assert.Equal("Lý do: Hết hàng", yeuCau.ThamSo["chiTiet"]);
        Assert.Equal("DUYET:YC-2", yeuCau.KhoaTrungLap);
    }

    [Fact]
    public async Task UC06_DuyetThieuHanGiuCho_NemLoiVaKhongGhi()
    {
        var outbox = new OutboxGhiNho();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new ThongBaoDuyetOutbox(outbox).GhiAsync(
            new ThongBaoDuyetHoacTuChoi("YC-3", "SV-3", true, null, null),
            CancellationToken.None));

        Assert.Empty(outbox.DaGhi);
    }

    [Fact]
    public async Task UC06_ThongBaoNull_NemLoi()
    {
        var outbox = new OutboxGhiNho();

        await Assert.ThrowsAsync<ArgumentNullException>(() => new ThongBaoDuyetOutbox(outbox).GhiAsync(
            null!,
            CancellationToken.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UC06_TuChoiThieuLyDo_NemLoiVaKhongGhi(string? lyDo)
    {
        var outbox = new OutboxGhiNho();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new ThongBaoDuyetOutbox(outbox).GhiAsync(
            new ThongBaoDuyetHoacTuChoi("YC-4", "SV-4", false, null, lyDo),
            CancellationToken.None));

        Assert.Empty(outbox.DaGhi);
    }
}
