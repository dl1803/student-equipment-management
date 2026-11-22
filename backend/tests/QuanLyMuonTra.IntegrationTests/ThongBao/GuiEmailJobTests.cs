using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLyMuonTra.Infrastructure;
using QuanLyMuonTra.IntegrationTests.Database;
using QuanLyMuonTra.Modules.SinhVien.Contracts;
using QuanLyMuonTra.Modules.SinhVien.Infrastructure;
using QuanLyMuonTra.Modules.TaiKhoan.Contracts;
using QuanLyMuonTra.Modules.TaiKhoan.Infrastructure;
using QuanLyMuonTra.Modules.ThongBao.Application;
using QuanLyMuonTra.Modules.ThongBao.Contracts;
using QuanLyMuonTra.Modules.ThongBao.Domain;
using QuanLyMuonTra.SharedKernel;

namespace QuanLyMuonTra.IntegrationTests.ThongBao;

public sealed class DongTrangThaiEmail
{
    public string TrangThaiGui { get; set; } = string.Empty;

    public int SoLanThu { get; set; }
}

public sealed class GuiEmailJobTests
{
    private const string NoiDungEmail = "<p>Nội dung kiểm thử D10-K</p>";
    private const string EmailSv = "d10k@example.test";
    private static readonly DateTimeOffset MocDau = new(2026, 11, 22, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UC17_J7_GuiThanhCong_DanhDauDaGuiVaDungNguoiNhanTieuDe()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var ma = await moiTruong.ChenEmailAsync("CHO_GUI", 0, null);

        var soDaXuLy = await moiTruong.Job.ChayAsync(CancellationToken.None);

        Assert.Equal(1, soDaXuLy);
        var dong = await moiTruong.DocAsync(ma);
        Assert.Equal("DA_GUI", dong.TrangThaiGui);
        Assert.Equal(1, dong.SoLanThu);
        var goi = Assert.Single(moiTruong.Email.DsGoi);
        Assert.Equal(EmailSv, goi.NguoiNhan);
        Assert.Equal(MauThongBao.TieuDe(MaLoaiThongBao.DuyetHoacTuChoi), goi.TieuDe);
        Assert.Equal(NoiDungEmail, goi.NoiDung);
    }

    [Fact]
    public async Task UC17_J7_GuiLoi_ThuLaiDungLichRoiLoiVinhVien()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        moiTruong.Email.NemLoi = true;
        var ma = await moiTruong.ChenEmailAsync("CHO_GUI", 0, null);

        await moiTruong.Job.ChayAsync(CancellationToken.None);
        var dong = await moiTruong.DocAsync(ma);
        Assert.Equal("LOI", dong.TrangThaiGui);
        Assert.Equal(1, dong.SoLanThu);

        moiTruong.DongHo.UtcNow = MocDau.AddSeconds(30);
        await moiTruong.Job.ChayAsync(CancellationToken.None);
        Assert.Single(moiTruong.Email.DsGoi);

        var lanCuoi = MocDau;
        for (var lanThu = 1; lanThu <= 4; lanThu++)
        {
            lanCuoi += LichGuiLaiEmail.GianCachSauLanThu(lanThu);
            moiTruong.DongHo.UtcNow = lanCuoi;
            await moiTruong.Job.ChayAsync(CancellationToken.None);

            dong = await moiTruong.DocAsync(ma);
            Assert.Equal(lanThu + 1, dong.SoLanThu);
            Assert.Equal(lanThu + 1 >= 5 ? "LOI_VINH_VIEN" : "LOI", dong.TrangThaiGui);
        }

        Assert.Equal(5, moiTruong.Email.DsGoi.Count);

        moiTruong.DongHo.UtcNow = lanCuoi.AddDays(1);
        await moiTruong.Job.ChayAsync(CancellationToken.None);

        Assert.Equal(5, moiTruong.Email.DsGoi.Count);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning && d.NoiDung.Contains("lỗi vĩnh viễn"));
    }

    [Fact]
    public async Task UC17_J7_DangGuiBoDo_QuaHanThiPhucHoi_ConHanThiGiu()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var boDo = await moiTruong.ChenEmailAsync("DANG_GUI", 1, MocDau.AddMinutes(-6));
        var conHan = await moiTruong.ChenEmailAsync("DANG_GUI", 1, MocDau.AddMinutes(-1));
        var heLanThu = await moiTruong.ChenEmailAsync("DANG_GUI", 5, MocDau.AddMinutes(-6));

        var soDaXuLy = await moiTruong.Job.ChayAsync(CancellationToken.None);

        Assert.Equal(1, soDaXuLy);
        var dongBoDo = await moiTruong.DocAsync(boDo);
        Assert.Equal("DA_GUI", dongBoDo.TrangThaiGui);
        Assert.Equal(2, dongBoDo.SoLanThu);
        var dongConHan = await moiTruong.DocAsync(conHan);
        Assert.Equal("DANG_GUI", dongConHan.TrangThaiGui);
        Assert.Equal(1, dongConHan.SoLanThu);
        var dongHeLanThu = await moiTruong.DocAsync(heLanThu);
        Assert.Equal("LOI_VINH_VIEN", dongHeLanThu.TrangThaiGui);
        Assert.Equal(5, dongHeLanThu.SoLanThu);
        Assert.Single(moiTruong.Email.DsGoi);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning && d.NoiDung.Contains("Đã phục hồi"));
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning && d.NoiDung.Contains("hết số lần thử"));
    }

    [Fact]
    public async Task UC17_J7_ChiLayToiDa50DongMoiLan()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        await moiTruong.ChenNhieuAsync(55);

        var soDaXuLy = await moiTruong.Job.ChayAsync(CancellationToken.None);

        Assert.Equal(50, soDaXuLy);
        Assert.Equal(50, moiTruong.Email.DsGoi.Count);
        Assert.Equal(50, await moiTruong.DemAsync("DA_GUI"));
        Assert.Equal(5, await moiTruong.DemAsync("CHO_GUI"));
    }

    [Fact]
    public async Task UC17_J7_HaiLanChaySongSong_MoiDongChiGuiMotLan()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        moiTruong.Email.DoTre = TimeSpan.FromMilliseconds(20);
        await moiTruong.ChenNhieuAsync(10);

        var ketQua = await Task.WhenAll(
            moiTruong.Job.ChayAsync(CancellationToken.None),
            moiTruong.Job.ChayAsync(CancellationToken.None));

        Assert.Equal(10, ketQua.Sum());
        Assert.Equal(10, moiTruong.Email.DsGoi.Count);
        Assert.Equal(10, await moiTruong.DemAsync("DA_GUI"));
    }

    [Fact]
    public async Task UC17_J7_NhanVienKhongConEmail_ChuyenLoiVinhVienKhongGui()
    {
        await using var moiTruong = await MoiTruong.TaoAsync();
        var maNv = await moiTruong.TaoNhanVienKhongEmailAsync();
        var ma = await moiTruong.ChenEmailAsync("CHO_GUI", 0, null, maNv);

        var soDaXuLy = await moiTruong.Job.ChayAsync(CancellationToken.None);

        Assert.Equal(1, soDaXuLy);
        Assert.Equal("LOI_VINH_VIEN", (await moiTruong.DocAsync(ma)).TrangThaiGui);
        Assert.Empty(moiTruong.Email.DsGoi);
        Assert.Contains(moiTruong.NhatKy.Dong, d => d.Muc == LogLevel.Warning && d.NoiDung.Contains("không còn địa chỉ nhận"));
    }

    private sealed class DongHoDieuKhien : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }

    private sealed class EmailGia : IEmailSender
    {
        private readonly object _khoa = new();
        private readonly List<(string NguoiNhan, string TieuDe, string NoiDung)> _dsGoi = [];

        public bool NemLoi { get; set; }

        public TimeSpan DoTre { get; set; }

        public IReadOnlyList<(string NguoiNhan, string TieuDe, string NoiDung)> DsGoi
        {
            get
            {
                lock (_khoa)
                {
                    return _dsGoi.ToList();
                }
            }
        }

        public async Task GuiAsync(string nguoiNhan, string tieuDe, string noiDungHtml, CancellationToken ct)
        {
            lock (_khoa)
            {
                _dsGoi.Add((nguoiNhan, tieuDe, noiDungHtml));
            }

            if (DoTre > TimeSpan.Zero)
            {
                await Task.Delay(DoTre, ct);
            }

            if (NemLoi)
            {
                throw new InvalidOperationException("Lỗi email giả lập.");
            }
        }
    }

    private sealed class NhatKyThuThap : ILogger<GuiEmailJob>
    {
        private readonly object _khoa = new();
        private readonly List<(LogLevel Muc, string NoiDung)> _dong = [];

        public IReadOnlyList<(LogLevel Muc, string NoiDung)> Dong
        {
            get
            {
                lock (_khoa)
                {
                    return _dong.ToList();
                }
            }
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            lock (_khoa)
            {
                _dong.Add((logLevel, formatter(state, exception)));
            }
        }
    }

    private sealed class MoiTruong : IAsyncDisposable
    {
        private readonly ServiceProvider _dichVu;
        private readonly List<Guid> _dsNhanVien = [];

        private MoiTruong(ServiceProvider dichVu, DongHoDieuKhien dongHo, EmailGia email, NhatKyThuThap nhatKy)
        {
            _dichVu = dichVu;
            DongHo = dongHo;
            Email = email;
            NhatKy = nhatKy;
            Job = new GuiEmailJob(dichVu.GetRequiredService<IServiceScopeFactory>(), email, dongHo, nhatKy);
        }

        public DongHoDieuKhien DongHo { get; }

        public EmailGia Email { get; }

        public NhatKyThuThap NhatKy { get; }

        public GuiEmailJob Job { get; }

        public string MaSv { get; private set; } = string.Empty;

        public static async Task<MoiTruong> TaoAsync()
        {
            var dongHo = new DongHoDieuKhien { UtcNow = MocDau };
            var email = new EmailGia();
            var dichVu = new ServiceCollection()
                .AddLogging()
                .AddInfrastructure(KetNoiKiemThu.LayChuoiKetNoi());
            dichVu.AddSingleton<IClock>(dongHo);
            dichVu.AddSingleton<IEmailSender>(email);
            dichVu.AddScoped<ISinhVienEmailReader, SinhVienEmailReader>();
            dichVu.AddScoped<INhanVienEmailReader, NhanVienEmailReader>();

            var moiTruong = new MoiTruong(dichVu.BuildServiceProvider(), dongHo, email, new NhatKyThuThap());
            await moiTruong.CoLapEmailTonDongAsync();
            await moiTruong.TaoSinhVienAsync();

            return moiTruong;
        }

        public async Task<Guid> ChenEmailAsync(string trangThai, int soLanThu, DateTimeOffset? lanThuCuoi, Guid? maNv = null)
        {
            var ma = Guid.CreateVersion7();
            var maSvNhan = maNv is null ? MaSv : null;
            await ThucThiAsync($"""
                INSERT INTO thong_bao.thong_bao
                    (ma_thong_bao, loai_thong_bao, noi_dung, kenh_gui, thoi_diem_gui, ma_sv_nhan, ma_nv_nhan,
                     trang_thai_gui, so_lan_thu, lan_thu_cuoi)
                VALUES
                    ({ma}, 'DUYET_HOAC_TU_CHOI', {NoiDungEmail}, 'EMAIL', {MocDau}, {maSvNhan}, {maNv},
                     {trangThai}, {soLanThu}, {lanThuCuoi})
                """);

            return ma;
        }

        public Task ChenNhieuAsync(int soDong) =>
            ThucThiAsync($"""
                INSERT INTO thong_bao.thong_bao
                    (ma_thong_bao, loai_thong_bao, noi_dung, kenh_gui, thoi_diem_gui, ma_sv_nhan, trang_thai_gui, so_lan_thu)
                SELECT gen_random_uuid(), 'DUYET_HOAC_TU_CHOI', {NoiDungEmail}, 'EMAIL',
                       {MocDau} + n * interval '1 second', {MaSv}, 'CHO_GUI', 0
                FROM generate_series(1, {soDong}) AS n
                """);

        public async Task<DongTrangThaiEmail> DocAsync(Guid ma)
        {
            await using var phamVi = _dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<DbContext>();

            return await db.Database.SqlQuery<DongTrangThaiEmail>($"""
                    SELECT trang_thai_gui AS "TrangThaiGui", so_lan_thu AS "SoLanThu"
                    FROM thong_bao.thong_bao
                    WHERE ma_thong_bao = {ma}
                    """)
                .SingleAsync();
        }

        public async Task<int> DemAsync(string trangThai)
        {
            await using var phamVi = _dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<DbContext>();

            return await db.Database.SqlQuery<int>($"""
                    SELECT count(*)::int AS "Value"
                    FROM thong_bao.thong_bao
                    WHERE ma_sv_nhan = {MaSv} AND kenh_gui = 'EMAIL' AND trang_thai_gui = {trangThai}
                    """)
                .SingleAsync();
        }

        public async Task<Guid> TaoNhanVienKhongEmailAsync()
        {
            var ma = Guid.CreateVersion7();
            var tenDangNhap = $"d10k-{ma:N}";
            await ThucThiAsync($"""
                INSERT INTO tai_khoan.nguoi_dung_he_thong (ma_nguoi_dung, ten_dang_nhap, mat_khau_bam, ho_ten, email)
                VALUES ({ma}, {tenDangNhap}, 'khong-dung', 'NV kiem thu D10-K', NULL)
                """);
            await ThucThiAsync($"""
                INSERT INTO tai_khoan.nhan_vien_quan_ly (ma_nguoi_dung, ma_nv)
                VALUES ({ma}, {tenDangNhap})
                """);
            _dsNhanVien.Add(ma);

            return ma;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                foreach (var maNv in _dsNhanVien)
                {
                    await ThucThiAsync($"DELETE FROM thong_bao.thong_bao WHERE ma_nv_nhan = {maNv}");
                    await ThucThiAsync($"DELETE FROM tai_khoan.nhan_vien_quan_ly WHERE ma_nguoi_dung = {maNv}");
                    await ThucThiAsync($"DELETE FROM tai_khoan.nguoi_dung_he_thong WHERE ma_nguoi_dung = {maNv}");
                }

                await ThucThiAsync($"DELETE FROM thong_bao.thong_bao WHERE ma_sv_nhan = {MaSv}");
                await ThucThiAsync($"DELETE FROM sinh_vien.sinh_vien WHERE ma_sv = {MaSv}");
            }
            finally
            {
                await _dichVu.DisposeAsync();
            }
        }

        private Task CoLapEmailTonDongAsync() =>
            ThucThiAsync($"""
                UPDATE thong_bao.thong_bao
                SET trang_thai_gui = 'DA_GUI'
                WHERE kenh_gui = 'EMAIL' AND trang_thai_gui IN ('CHO_GUI', 'LOI', 'DANG_GUI')
                """);

        private async Task TaoSinhVienAsync()
        {
            MaSv = $"D10K-SV-{Guid.NewGuid():N}";
            await ThucThiAsync($"""
                INSERT INTO sinh_vien.sinh_vien (ma_sv, ho_ten, email, ma_nhom, han_the, trang_thai_hoc, hoc_ky)
                VALUES ({MaSv}, 'SV kiem thu D10-K', {EmailSv}, 'DAI_HOC', DATE '2030-01-01', 'DANG_HOC', 'HK1')
                """);
        }

        private async Task ThucThiAsync(FormattableString cauLenh)
        {
            await using var phamVi = _dichVu.CreateAsyncScope();
            var db = phamVi.ServiceProvider.GetRequiredService<DbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync(cauLenh);
        }
    }
}

