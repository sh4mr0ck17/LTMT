using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Excel;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Excel;

/// <summary>
/// Bộ kiểm thử Nhóm D: Quản lý phiên xem trước (Preview) và tính toàn vẹn Transaction khi Commit dữ liệu.
/// </summary>
public class ExcelPreviewVaCommitTests
{
    private static MemoryStream TaoWorkbookInMemory(Action<IXLWorksheet> setupAction)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DuLieu");
        setupAction(worksheet);
        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task Preview_KhongLamThayDoiDatabase()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            ws.Cell(2, 1).SetValue("K_PREVIEW");
            ws.Cell(2, 2).SetValue("Khoa Xem Trước");
        });

        // Act - Chạy bước kiểm tra và preview
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.True(ketQua.LaHopLeToanBo);
        // Xác nhận trong DB vẫn chưa hề có bản ghi nào được ghi
        var khoaDb = await db.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "K_PREVIEW");
        Assert.Null(khoaDb);
    }

    [Fact]
    public async Task Commit_DongHopLe_LuuDungVaoDatabase()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            ws.Cell(2, 1).SetValue("K_COMMIT");
            ws.Cell(2, 2).SetValue("Khoa Đã Commit");
        });

        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");
        var danhSachHopLe = ketQua.DanhSachDongDuLieu.Where(d => d.LaHopLe).ToList();

        // Act - Thực hiện commit
        var ketQuaCommit = await excelDichVu.CommitDuLieuExcelAsync("Khoa", danhSachHopLe, "tester");

        // Assert
        Assert.True(ketQuaCommit.ThanhCong);
        Assert.Equal(1, ketQuaCommit.SoBanGhiDaThem);

        var khoaDb = await db.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "K_COMMIT");
        Assert.NotNull(khoaDb);
        Assert.Equal("Khoa Đã Commit", khoaDb.TenKhoa);
    }

    [Fact]
    public void LuuVaLayPhienPreview_HoatDongDung()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        var ketQuaKiemTra = new KetQuaKiemTraExcel { TenTep = "test.xlsx", TenModule = "Khoa", TongSoDong = 5 };

        // Act
        var phien = excelDichVu.LuuPhienPreview("Khoa", ketQuaKiemTra, "test_user");
        var phienLay = excelDichVu.LayPhienPreview(phien.MaPhien);

        // Assert
        Assert.NotNull(phienLay);
        Assert.Equal("Khoa", phienLay.TenModule);
        Assert.Equal("test_user", phienLay.NguoiTao);

        excelDichVu.XoaPhienPreview(phien.MaPhien);
        var phienSauXoa = excelDichVu.LayPhienPreview(phien.MaPhien);
        Assert.Null(phienSauXoa);
    }

    [Fact]
    public async Task Commit_HangThieuTruong_CheDoLinhHoat_TuDongBuDapVaLuuThanhCong()
    {
        // Arrange: Tạo lớp B1D57 sẵn trong DB
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "KHOA_CNTT", TenKhoa = "CNTT" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var lop = new LopHoc { MaLop = "B1D57", TenLop = "Lớp B1D57", KhoaId = khoa.KhoaId, NienKhoa = "2025-2029" };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();

        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Tạo tệp Excel chỉ có Họ và tên và Địa chỉ (thiếu Mã học viên, Ngày sinh, Giới tính, Mã lớp)
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã học viên";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Email";
            ws.Cell(1, 6).Value = "Số điện thoại";
            ws.Cell(1, 7).Value = "Địa chỉ";
            ws.Cell(1, 8).Value = "Mã lớp";
            ws.Cell(1, 9).Value = "Trạng thái";

            // Hàng 2: Để trống Mã học viên, Ngày sinh, Giới tính, Mã lớp (thiếu nhiều trường)
            ws.Cell(2, 1).SetValue("");
            ws.Cell(2, 2).SetValue("Nguyễn Thị Linh Hoạt");
            ws.Cell(2, 3).SetValue("");
            ws.Cell(2, 4).SetValue("");
            ws.Cell(2, 7).SetValue("Hà Nội, Việt Nam");
            ws.Cell(2, 8).SetValue("");
        });

        // Act 1: Kiểm tra tệp ở chế độ cho phép thiếu trường (choPhepThieuTruong = true)
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("HocVien", stream, "partial.xlsx", choPhepThieuTruong: true);

        // Assert 1: Dòng được tự động bù đắp và vẫn HỢP LỆ để tiếp tục nhập
        Assert.Equal(1, ketQua.SoDongHopLe);
        Assert.Equal(0, ketQua.SoDongLoi);
        Assert.True(ketQua.DanhSachDongDuLieu[0].LaHopLe);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi.StartsWith("WARN_"));

        // Act 2: Commit dữ liệu
        var danhSachHopLe = ketQua.DanhSachDongDuLieu.Where(d => d.LaHopLe).ToList();
        var ketQuaCommit = await excelDichVu.CommitDuLieuExcelAsync("HocVien", danhSachHopLe, "tester");

        // Assert 2: Lưu thành công vào CSDL
        Assert.True(ketQuaCommit.ThanhCong);
        Assert.Equal(1, ketQuaCommit.SoBanGhiDaThem);

        var hvDb = await db.HocViens.FirstOrDefaultAsync(h => h.HoVaTen == "Nguyễn Thị Linh Hoạt");
        Assert.NotNull(hvDb);
        Assert.StartsWith("HV_", hvDb.MaHocVien);
        Assert.Equal("Nữ", hvDb.GioiTinh); // Tự động suy luận giới tính qua chữ 'Thị'
        Assert.Equal("Hà Nội, Việt Nam", hvDb.DiaChi);
        Assert.Equal(lop.LopHocId, hvDb.LopHocId); // Tự động gán lớp khả dụng
    }
}
