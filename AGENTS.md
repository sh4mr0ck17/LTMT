# AGENT INSTRUCTIONS: HỆ THỐNG QUẢN LÝ KHOA & LỚP ĐÀO TẠO (.NET 8 C#)

## 1. NGUYÊN TẮC THIẾT KẾ & KIẾN TRÚC
- Kiến trúc phẳng thực dụng (3-Tier): Controllers -> Services -> Data (DbContext).
- Áp dụng nguyên tắc SOLID:
  + S (Single Responsibility): Mỗi class/file chỉ chịu một trách nhiệm duy nhất. Tách riêng UI, Service và Data Access.
  + D (Dependency Inversion): Tầng trên chỉ phụ thuộc vào Abstraction (Interface bắt đầu bằng chữ 'I').
- Không dùng kiến trúc phức tạp (CQRS, MediatR đa project) trừ khi có yêu cầu rõ ràng.

## 2. QUY CHUẨN ĐẶT TÊN BẮT BUỘC (NAMING CONVENTIONS)
### 2.1. Thành phần Giao diện (UI Controls / HTML Elements ID)
- Cú pháp: `tiềnTố` + `TênChứcNăng` (camelCase Tiếng Việt).
- Tiền tố quy định:
  + `btn`: Nút bấm (ví dụ: `btnThemKhoa`, `btnXoaLop`, `btnNhapExcel`, `btnXuatBaoCao`).
  + `lbl`: Nhãn văn bản (ví dụ: `lblTieuDe`, `lblTongSoHocVien`, `lblThongBaoLoi`).
  + `pnl`: Khung chứa/Card (ví dụ: `pnlThaoTac`, `pnlDanhSachKhoa`).
  + `txt`: Ô nhập văn bản (ví dụ: `txtMaKhoa`, `txtTenKhoa`, `txtTimKiem`).
  + `cbo`: Hộp chọn thả xuống (ví dụ: `cboChonKhoa`, `cboNienKhoa`).
  + `dgv`: Bảng hiển thị dữ liệu (ví dụ: `dgvDanhSachKhoa`, `dgvDanhSachLop`).

### 2.2. Mã nguồn C#
- Tên File & Class/Struct/Enum: PascalCase Tiếng Việt danh từ (`Khoa.cs`, `LopHoc.cs`, `KhoaDichVu.cs`).
- Interface: `I` + PascalCase Tiếng Việt (`IKhoaDichVu.cs`, `ILopHocDichVu.cs`).
- Properties (Thuộc tính public): PascalCase Tiếng Việt, dùng Auto-Properties (`MaKhoa`, `TenKhoa`, `SoLuongToiDa`).
- Biến nội bộ (Private fields): `_` + camelCase Tiếng Việt (`_coSoDuLieu`, `_khoaDichVu`, `_danhSachLop`).
- Biến cục bộ & Tham số hàm: camelCase Tiếng Việt (`soLuong`, `maKhoaCanXoa`).
- Phương thức (Methods): PascalCase bắt đầu bằng động từ Tiếng Việt (`ThemKhoa()`, `XoaLopHoc()`, `XuatDanhSachExcel()`).
- Biến Boolean: Bắt đầu bằng `Co`, `La`, `Da`, `Is`, `Can` (`CoHoatDong`, `DaKhoa`, `CoTheXoa`).
- Hằng số (Constants): PascalCase có ý nghĩa, TUYỆT ĐỐI KHÔNG dùng magic numbers (`SoLuongToiDaMacDinh`, `SoLopToiThieuDeChanXoa`).

## 3. CHUẨN CODE (CODING STYLE GUIDE)
- Tài nguyên: Mọi đối tượng thực thi `IDisposable` (Stream, MemoryStream, XLWorkbook) BẮT BUỘC dùng khối `using`.
- Ngoại lệ:
  + Bắt ngoại lệ cụ thể (`InvalidOperationException`, `KeyNotFoundException`), TUYỆT ĐỐI KHÔNG bắt `catch (Exception)` rỗng để nuốt lỗi.
  + Kiểm tra hợp lệ đầu vào bằng Guard Clauses (`ArgumentNullException.ThrowIfNull`).
- Documentation: Viết XML Document (`/// <summary>`) cho mọi class, interface và method public.
- Format: Mọi khối lệnh `if`, `else`, `for`, `while` bắt buộc có dấu ngoặc `{}` kể cả khi chỉ có 1 dòng lệnh.