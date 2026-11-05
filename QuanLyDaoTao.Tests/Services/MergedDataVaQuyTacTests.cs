using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử xác thực các tính năng, quy tắc dữ liệu và danh mục tổ chức được sáp nhập từ repository LTMT-demo.
/// </summary>
public class MergedDataVaQuyTacTests
{
    [Fact]
    public void QuyTacDuLieu_TenKhopTuKhoa_KhopChinhXacTen()
    {
        // Assert: Tên cuối khớp chính xác
        Assert.True(QuyTacDuLieu.TenKhopTuKhoa("Nguyễn Văn Anh", "Anh"));
        Assert.False(QuyTacDuLieu.TenKhopTuKhoa("Nguyễn Văn Thanh", "Anh"));
        Assert.False(QuyTacDuLieu.TenKhopTuKhoa("Nguyễn Anh Thư", "Anh"));
    }

    [Fact]
    public void QuyTacDuLieu_DanhSachTinhThanh_ChuaDu34TinhThanh()
    {
        // Assert
        Assert.Equal(34, QuyTacDuLieu.TinhThanh.Length);
        Assert.Contains("Hà Nội", QuyTacDuLieu.TinhThanh);
        Assert.Contains("Thành phố Hồ Chí Minh", QuyTacDuLieu.TinhThanh);
        Assert.Contains("Đà Nẵng", QuyTacDuLieu.TinhThanh);
    }

    [Fact]
    public void QuyTacDuLieu_KhoaSapXep_SapXepTheoTenTiengViet()
    {
        // Arrange
        var ds = new[] { "Trần Thu Hà", "Nguyễn Minh Đức", "Lê Hoàng Nam", "Phạm Mai Anh" };

        // Act: Sắp xếp theo khóa tiếng Việt
        var sapXep = ds.OrderBy(QuyTacDuLieu.KhoaSapXep).ToArray();

        // Assert: "Anh" -> "Đức" -> "Hà" -> "Nam"
        Assert.Equal("Phạm Mai Anh", sapXep[0]);
        Assert.Equal("Nguyễn Minh Đức", sapXep[1]);
        Assert.Equal("Trần Thu Hà", sapXep[2]);
        Assert.Equal("Lê Hoàng Nam", sapXep[3]);
    }

    [Fact]
    public void QuyTacDuLieu_TaoHoTen_SinhTenOnDinhTheoSo()
    {
        // Act
        var tenNam1 = QuyTacDuLieu.TaoHoTen(5001, "Nam");
        var tenNam1LapLai = QuyTacDuLieu.TaoHoTen(5001, "Nam");
        var tenNu1 = QuyTacDuLieu.TaoHoTen(5002, "Nữ");

        // Assert: Hoán vị ổn định không đổi qua các lần gọi
        Assert.Equal(tenNam1, tenNam1LapLai);
        Assert.False(string.IsNullOrWhiteSpace(tenNam1));
        Assert.False(string.IsNullOrWhiteSpace(tenNu1));
    }

    [Fact]
    public void KiemTraTrungHoSo_TrungCCCDHoacDienThoai_BaoLoiRanhMach()
    {
        // Arrange
        var canBoCu = new List<NhanSu>
        {
            new()
            {
                NhanSuId = 1,
                MaNhanSu = "CB001",
                HoVaTen = "Nguyễn Văn A",
                SoCCCD = "001090001234",
                SoDienThoai = "0912345678",
                Email = "a@c500.edu.vn"
            }
        };

        var canBoMoiTrungCCCD = new NhanSu
        {
            NhanSuId = 2,
            MaNhanSu = "CB002",
            HoVaTen = "Trần Văn B",
            SoCCCD = "001090001234",
            SoDienThoai = "0987654321",
            Email = "b@c500.edu.vn"
        };

        // Act
        var loi = KiemTraTrungHoSo.KiemTraCanBo(canBoMoiTrungCCCD, canBoCu, Enumerable.Empty<HocVien>());

        // Assert
        Assert.NotNull(loi);
        Assert.Contains("Số CCCD", loi);
    }

    [Fact]
    public async Task DuLieuKhoiTao_LamGiauDuLieuDemo_TaoDu12KhoaVaBoMon()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();

        // Khởi tạo trước Chức vụ và Trình độ để đảm bảo khóa ngoại
        db.ChucVus.Add(new ChucVu { MaChucVu = "CV_TK", TenChucVu = "Trưởng khoa", CoHoatDong = true });
        db.ChucVus.Add(new ChucVu { MaChucVu = "CV_PK", TenChucVu = "Phó khoa", CoHoatDong = true });
        db.ChucVus.Add(new ChucVu { MaChucVu = "CV_GV", TenChucVu = "Giảng viên", CoHoatDong = true });
        db.TrinhDoChuyenMons.Add(new TrinhDoChuyenMon { MaTrinhDo = "TD_TS", TenTrinhDo = "Tiến sĩ", CoHoatDong = true });
        db.TrinhDoChuyenMons.Add(new TrinhDoChuyenMon { MaTrinhDo = "TD_ThS", TenTrinhDo = "Thạc sĩ", CoHoatDong = true });
        await db.SaveChangesAsync();


        // Gọi trực tiếp làm giàu dữ liệu
        var method = typeof(DuLieuKhoiTao).GetMethod("LamGiauDuLieuTuDemoAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
        var task = (Task)method.Invoke(null, new object[] { db })!;
        await task;

        // Assert: 12 Khoa chuyên ngành C500
        var soKhoa = await db.Khoas.CountAsync(k => k.MaKhoa.StartsWith("KH-"));
        Assert.Equal(12, soKhoa);

        // Xác nhận có Khoa An ninh mạng (KH-10) và Khoa Nghiệp vụ cơ bản (KH-03)
        var khoaAnm = await db.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "KH-10");
        Assert.NotNull(khoaAnm);
        Assert.Contains("Nghiệp vụ 8", khoaAnm.TenKhoa);
        Assert.Contains("An ninh mạng", khoaAnm.MoTa);

        // 24 Bộ môn tương ứng
        var soBoMon = await db.BoMons.CountAsync(b => b.MaBoMon.StartsWith("KH-"));
        Assert.True(soBoMon >= 24);

        // Các lớp đào tạo chuyên ngành C500
        var lopD58 = await db.LopHocs.FirstOrDefaultAsync(l => l.MaLop == "B10D58");
        Assert.NotNull(lopD58);
        Assert.Equal("2023-2027", lopD58.NienKhoa);

        // Cán bộ giảng viên CAND
        var canBoC500 = await db.NhanSus.FirstOrDefaultAsync(n => n.MaNhanSu == "CB_C500_001");
        Assert.NotNull(canBoC500);
        Assert.Contains("Đại tá", canBoC500.HoVaTen);
    }
}
