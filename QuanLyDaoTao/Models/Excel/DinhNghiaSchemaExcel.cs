namespace QuanLyDaoTao.Models.Excel;

/// <summary>
/// Kho lưu trữ các lược đồ (Schema) định nghĩa cấu trúc cột Excel cho từng module trong hệ thống.
/// </summary>
public static class DinhNghiaSchemaExcel
{
    /// <summary>
    /// Lấy danh sách định nghĩa cột chuẩn theo tên module nghiệp vụ.
    /// </summary>
    /// <param name="tenModule">Tên module (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay).</param>
    /// <returns>Danh sách các cột dữ liệu theo thứ tự chuẩn.</returns>
    public static List<DinhNghiaCotExcel> LaySchema(string tenModule)
    {
        return tenModule.ToLowerInvariant() switch
        {
            "khoa" => LaySchemaKhoa(),
            "bomon" => LaySchemaBoMon(),
            "lophoc" => LaySchemaLopHoc(),
            "hocvien" => LaySchemaHocVien(),
            "nhansu" => LaySchemaNhanSu(),
            "monhoc" => LaySchemaMonHoc(),
            "phanconggiangday" or "phancong" => LaySchemaPhanCongGiangDay(),
            _ => throw new ArgumentException($"Không tìm thấy lược đồ Excel cho module '{tenModule}'.", nameof(tenModule))
        };
    }

    /// <summary>
    /// Lược đồ cột Excel cho Module Khoa.
    /// </summary>
    public static List<DinhNghiaCotExcel> LaySchemaKhoa() => new()
    {
        new() { TenCot = "Mã khoa", CacTenGoiKhac = new[] { "MaKhoa", "Mã Khoa" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Mã định danh khoa duy nhất (không trùng lặp)", ViDu = "KHOA_CNTT" },
        new() { TenCot = "Tên khoa", CacTenGoiKhac = new[] { "TenKhoa", "Tên Khoa" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 200, MoTa = "Tên đầy đủ của khoa", ViDu = "Khoa Công nghệ Thông tin" },
        new() { TenCot = "Mô tả", CacTenGoiKhac = new[] { "MoTa", "Ghi chú" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 500, MoTa = "Mô tả chi tiết về khoa đào tạo", ViDu = "Đào tạo kỹ sư công nghệ thông tin" },
        new() { TenCot = "Số điện thoại", CacTenGoiKhac = new[] { "SoDienThoai", "SĐT", "Điện thoại" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.SoDienThoai, DoDaiToiDa = 20, MoTa = "Số điện thoại văn phòng", ViDu = "024.38691234" },
        new() { TenCot = "Email", CacTenGoiKhac = new[] { "DiaChiEmail", "Thư điện tử" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Email, DoDaiToiDa = 100, MoTa = "Địa chỉ thư điện tử liên hệ", ViDu = "cntt@quanlydaotao.local" },
        new() { TenCot = "Địa điểm", CacTenGoiKhac = new[] { "DiaDiem", "Văn phòng" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 200, MoTa = "Địa điểm văn phòng khoa", ViDu = "Tòa nhà A1" },
        new() { TenCot = "Trạng thái", CacTenGoiKhac = new[] { "TrangThai", "Hoạt động" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Logic, DoDaiToiDa = 20, MoTa = "Hoạt động hoặc Đã khóa (mặc định: Hoạt động)", ViDu = "Hoạt động" }
    };

    /// <summary>
    /// Lược đồ cột Excel cho Module Bộ môn.
    /// </summary>
    public static List<DinhNghiaCotExcel> LaySchemaBoMon() => new()
    {
        new() { TenCot = "Mã bộ môn", CacTenGoiKhac = new[] { "MaBoMon", "Mã Bộ Môn" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Mã bộ môn duy nhất", ViDu = "BM_KTPM" },
        new() { TenCot = "Tên bộ môn", CacTenGoiKhac = new[] { "TenBoMon", "Tên Bộ Môn" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 200, MoTa = "Tên bộ môn đầy đủ", ViDu = "Bộ môn Kỹ thuật Phần mềm" },
        new() { TenCot = "Mô tả", CacTenGoiKhac = new[] { "MoTa" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 500, MoTa = "Mô tả lĩnh vực của bộ môn", ViDu = "Nghiên cứu công nghệ phần mềm" },
        new() { TenCot = "Mã khoa", CacTenGoiKhac = new[] { "MaKhoa", "Khoa chủ quản" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã khoa chủ quản (phải tồn tại trong hệ thống)", ViDu = "KHOA_CNTT" },
        new() { TenCot = "Trạng thái", CacTenGoiKhac = new[] { "TrangThai" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Logic, DoDaiToiDa = 20, MoTa = "Hoạt động hoặc Đã khóa", ViDu = "Hoạt động" }
    };

    /// <summary>
    /// Lược đồ cột Excel cho Module Lớp học.
    /// </summary>
    public static List<DinhNghiaCotExcel> LaySchemaLopHoc() => new()
    {
        new() { TenCot = "Mã lớp", CacTenGoiKhac = new[] { "MaLop", "Mã Lớp" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Mã lớp học duy nhất", ViDu = "CNTT_K20A" },
        new() { TenCot = "Tên lớp", CacTenGoiKhac = new[] { "TenLop", "Tên Lớp" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 200, MoTa = "Tên lớp học đầy đủ", ViDu = "Công nghệ Thông tin K20A" },
        new() { TenCot = "Niên khóa", CacTenGoiKhac = new[] { "NienKhoa", "Khóa học" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Niên khóa đào tạo (ví dụ: 2024-2028)", ViDu = "2024-2028" },
        new() { TenCot = "Mã khoa", CacTenGoiKhac = new[] { "MaKhoa", "Khoa quản lý" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã khoa quản lý lớp (phải tồn tại)", ViDu = "KHOA_CNTT" },
        new() { TenCot = "Số lượng tối đa", CacTenGoiKhac = new[] { "SoLuongToiDa", "Sĩ số tối đa" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.SoNguyen, DoDaiToiDa = 10, MoTa = "Sĩ số học viên tối đa (mặc định 50)", ViDu = "60" },
        new() { TenCot = "Trạng thái", CacTenGoiKhac = new[] { "TrangThai" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Logic, DoDaiToiDa = 20, MoTa = "Hoạt động hoặc Đã khóa", ViDu = "Hoạt động" }
    };

    /// <summary>
    /// Lược đồ cột Excel cho Module Học viên.
    /// </summary>
    public static List<DinhNghiaCotExcel> LaySchemaHocVien() => new()
    {
        new() { TenCot = "Mã học viên", CacTenGoiKhac = new[] { "MaHocVien", "Mã HV" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Mã học viên duy nhất", ViDu = "HV20260001" },
        new() { TenCot = "Họ và tên", CacTenGoiKhac = new[] { "HoVaTen", "Họ tên" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 100, MoTa = "Họ và tên đầy đủ của học viên", ViDu = "Nguyễn Văn An" },
        new() { TenCot = "Ngày sinh", CacTenGoiKhac = new[] { "NgaySinh" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.NgayThang, DoDaiToiDa = 20, MoTa = "Ngày sinh theo định dạng dd/MM/yyyy", ViDu = "15/08/2005" },
        new() { TenCot = "Giới tính", CacTenGoiKhac = new[] { "GioiTinh" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 20, MoTa = "Nam hoặc Nữ", ViDu = "Nam" },
        new() { TenCot = "Email", CacTenGoiKhac = new[] { "DiaChiEmail" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Email, DoDaiToiDa = 100, MoTa = "Địa chỉ email cá nhân", ViDu = "an.nguyen@email.local" },
        new() { TenCot = "Số điện thoại", CacTenGoiKhac = new[] { "SoDienThoai", "SĐT" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.SoDienThoai, DoDaiToiDa = 20, MoTa = "Số điện thoại di động", ViDu = "0912345678" },
        new() { TenCot = "Địa chỉ", CacTenGoiKhac = new[] { "DiaChi", "Hộ khẩu" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 255, MoTa = "Địa chỉ cư trú", ViDu = "Hà Nội" },
        new() { TenCot = "Mã lớp", CacTenGoiKhac = new[] { "MaLop", "Lớp học" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã lớp học thuộc về (phải tồn tại)", ViDu = "CNTT_K20A" },
        new() { TenCot = "Trạng thái", CacTenGoiKhac = new[] { "TrangThai" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Logic, DoDaiToiDa = 20, MoTa = "Hoạt động hoặc Đã khóa", ViDu = "Hoạt động" }
    };

    /// <summary>
    /// Lược đồ cột Excel cho Module Hồ sơ Nhân sự / Giảng viên.
    /// </summary>
    public static List<DinhNghiaCotExcel> LaySchemaNhanSu() => new()
    {
        new() { TenCot = "Mã nhân sự", CacTenGoiKhac = new[] { "MaNhanSu", "Mã Cán Bộ" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Mã nhân sự duy nhất (giữ số 0 đầu)", ViDu = "CB_C500_01" },
        new() { TenCot = "Họ và tên", CacTenGoiKhac = new[] { "HoVaTen", "Họ tên" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 100, MoTa = "Họ và tên đầy đủ", ViDu = "Nguyễn Minh Đức" },
        new() { TenCot = "Ngày sinh", CacTenGoiKhac = new[] { "NgaySinh" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.NgayThang, DoDaiToiDa = 20, MoTa = "Ngày sinh định dạng dd/MM/yyyy", ViDu = "19/05/1978" },
        new() { TenCot = "Giới tính", CacTenGoiKhac = new[] { "GioiTinh" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 20, MoTa = "Nam hoặc Nữ", ViDu = "Nam" },
        new() { TenCot = "Số CCCD", CacTenGoiKhac = new[] { "SoGiayTo", "CCCD" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Số CCCD định danh 12 số", ViDu = "001078001234" },
        new() { TenCot = "Số điện thoại", CacTenGoiKhac = new[] { "SoDienThoai", "SĐT" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.SoDienThoai, DoDaiToiDa = 20, MoTa = "Số điện thoại liên hệ", ViDu = "0912345678" },
        new() { TenCot = "Email", CacTenGoiKhac = new[] { "EmailCongViec", "Email công việc" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.Email, DoDaiToiDa = 100, MoTa = "Email cơ quan bắt buộc và duy nhất", ViDu = "duc.nm@c500.edu.vn" },
        new() { TenCot = "Địa chỉ", CacTenGoiKhac = new[] { "DiaChi", "Hộ khẩu" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 255, MoTa = "Địa chỉ cư trú", ViDu = "Hà Nội" },
        new() { TenCot = "Mã khoa", CacTenGoiKhac = new[] { "MaKhoa", "Khoa" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã khoa trực thuộc", ViDu = "KHOA_CNTT" },
        new() { TenCot = "Mã bộ môn", CacTenGoiKhac = new[] { "MaBoMon", "Bộ môn" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã bộ môn trực thuộc", ViDu = "BM_KTPM" },
        new() { TenCot = "Mã chức vụ", CacTenGoiKhac = new[] { "MaChucVu", "Chức vụ" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã chức vụ công tác", ViDu = "CV_GV" },
        new() { TenCot = "Mã trình độ", CacTenGoiKhac = new[] { "MaTrinhDo", "Trình độ" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã trình độ chuyên môn", ViDu = "TD_TS" },
        new() { TenCot = "Chuyên môn đào tạo", CacTenGoiKhac = new[] { "ChuyenMon", "Chuyên ngành" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 255, MoTa = "Tóm tắt chuyên môn và trường đào tạo", ViDu = "An ninh mạng - Học viện ANND" },
        new() { TenCot = "Trạng thái", CacTenGoiKhac = new[] { "TrangThai", "Trạng thái công tác", "TrangThaiCongTac" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Đang làm việc / Tạm nghỉ / Đã nghỉ việc / Đã khóa", ViDu = "Đang làm việc" },
        new() { TenCot = "Ghi chú", CacTenGoiKhac = new[] { "GhiChu", "Lưu ý" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 500, MoTa = "Ghi chú chuyên môn hoặc tiểu sử", ViDu = "Giảng viên chính C500" }
    };

    /// <summary>
    /// Lược đồ cột Excel cho Module Môn học.
    /// </summary>
    public static List<DinhNghiaCotExcel> LaySchemaMonHoc() => new()
    {
        new() { TenCot = "Mã môn học", CacTenGoiKhac = new[] { "MaMonHoc", "Mã MH" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Mã môn học duy nhất", ViDu = "CSHARP_NET" },
        new() { TenCot = "Tên môn học", CacTenGoiKhac = new[] { "TenMonHoc", "Tên MH" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 200, MoTa = "Tên đầy đủ của môn học", ViDu = "Lập trình ứng dụng .NET C#" },
        new() { TenCot = "Số tín chỉ", CacTenGoiKhac = new[] { "SoTinChi", "Tín chỉ" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.SoNguyen, DoDaiToiDa = 10, MoTa = "Số tín chỉ đào tạo (1 đến 20)", ViDu = "3" },
        new() { TenCot = "Số tiết lý thuyết", CacTenGoiKhac = new[] { "SoTietLyThuyet", "Lý thuyết" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.SoNguyen, DoDaiToiDa = 10, MoTa = "Số tiết học lý thuyết", ViDu = "30" },
        new() { TenCot = "Số tiết thực hành", CacTenGoiKhac = new[] { "SoTietThucHanh", "Thực hành" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.SoNguyen, DoDaiToiDa = 10, MoTa = "Số tiết học thực hành", ViDu = "15" },
        new() { TenCot = "Mã bộ môn", CacTenGoiKhac = new[] { "MaBoMon", "Bộ môn phụ trách" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã bộ môn phụ trách môn học", ViDu = "BM_KTPM" },
        new() { TenCot = "Trạng thái", CacTenGoiKhac = new[] { "TrangThai" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Logic, DoDaiToiDa = 20, MoTa = "Hoạt động hoặc Đã khóa", ViDu = "Hoạt động" }
    };

    /// <summary>
    /// Lược đồ cột Excel cho Module Phân công giảng dạy.
    /// </summary>
    public static List<DinhNghiaCotExcel> LaySchemaPhanCongGiangDay() => new()
    {
        new() { TenCot = "Mã phân công", CacTenGoiKhac = new[] { "MaPhanCong", "Mã PC" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Mã phân công duy nhất", ViDu = "PC2026_001" },
        new() { TenCot = "Mã nhân sự", CacTenGoiKhac = new[] { "MaNhanSu", "Giảng viên" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã nhân sự / giảng viên phụ trách", ViDu = "CB_C500_01" },
        new() { TenCot = "Mã môn học", CacTenGoiKhac = new[] { "MaMonHoc", "Môn học" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã môn học được phân công", ViDu = "CSHARP_NET" },
        new() { TenCot = "Mã lớp", CacTenGoiKhac = new[] { "MaLop", "Lớp học" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.KhoaNgoai, DoDaiToiDa = 50, MoTa = "Mã lớp học được giảng dạy", ViDu = "CNTT_K20A" },
        new() { TenCot = "Học kỳ năm học", CacTenGoiKhac = new[] { "HocKyNamHoc", "Học kỳ", "HocKy" }, LaBatBuoc = true, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Học kỳ & Năm học (VD: HK1 (2026-2027))", ViDu = "HK1 (2026-2027)" },
        new() { TenCot = "Vai trò giảng dạy", CacTenGoiKhac = new[] { "VaiTroGiangDay", "Vai trò" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 50, MoTa = "Giảng viên chính / Trợ giảng / Hướng dẫn thực hành", ViDu = "Giảng viên chính" },
        new() { TenCot = "Chủ nhiệm", CacTenGoiKhac = new[] { "LaChuNhiem", "GVCN" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.Logic, DoDaiToiDa = 20, MoTa = "Có hoặc Không", ViDu = "Có" },
        new() { TenCot = "Ghi chú", CacTenGoiKhac = new[] { "GhiChu" }, LaBatBuoc = false, KieuDuLieu = KieuDuLieuExcel.VanBan, DoDaiToiDa = 255, MoTa = "Ghi chú phân công", ViDu = "Giảng đường B302 - C500" }
    };
}
