# TÀI LIỆU TỔNG HỢP KIẾN TRÚC, TÍNH NĂNG VÀ HƯỚNG DẪN DỰ ÁN QUANLYDAOTAO
> **Hệ thống Quản lý Khoa, Lớp Đào tạo & Hồ sơ Nhân sự - Học viện An ninh Nhân dân (C500)**  
> **Thời điểm cập nhật**: Tháng 11/2026 | **Phiên bản nền tảng**: .NET 8 LTS (ASP.NET Core MVC & Blazor)

---

## 1. TỔNG QUAN DỰ ÁN & TECH STACK

### 1.1. Mục tiêu Dự án
Xây dựng hệ thống quản lý phẳng (Pragmatic 3-Tier) phục vụ công tác đào tạo, quản lý tổ chức theo cây đơn vị, hồ sơ cán bộ/giảng viên, lớp đào tạo, học viên khóa D57, phân công giảng dạy, và đặc biệt là hệ thống Nhập/Xuất Excel chuyên nghiệp với tính năng kiểm tra lỗi từng ô (Cell-by-cell), bảng xem trước (Preview) và cơ chế bù đắp dữ liệu linh hoạt (Flexible/Tolerant Import).

### 1.2. Công nghệ Cốt lõi
- **Ngôn ngữ & Runtime**: C# 12, .NET 8.0 SDK (`net8.0`).
- **Giao diện Người dùng**: ASP.NET Core MVC (Razor Views) kết hợp Blazor Interactive Server Components cho các module cần phản hồi trực tiếp (Live CRUD, Inline Editing).
- **Cơ sở Dữ liệu**: SQLite 3 qua Entity Framework Core 8.0 (`Microsoft.EntityFrameworkCore.Sqlite` 8.0.11), tương thích hoàn toàn môi trường Server và Container.
- **Xử lý Tệp Excel**: ClosedXML (`ClosedXML` 0.105.1) + OpenXML, tối ưu hóa tài nguyên (MemoryStream dispose, streaming đọc/ghi an toàn).
- **Xác thực & Phân quyền**: ASP.NET Core Identity tích hợp (RBAC - Role-Based Access Control) với bảng phân quyền người dùng, Cookie Authentication an toàn và AntiForgery Token.
- **Kiểm thử**: xUnit (`178/178 tests pass`), `Microsoft.AspNetCore.Mvc.Testing` (Integration Tests).
- **CSS / UI Controls**: Bootstrap 5.1.0 (local asset, không phụ thuộc CDN bên ngoài), Bootstrap Icons 1.11.3.

---

## 2. NGUYÊN TẮC THIẾT KẾ & QUY CHUẨN ĐẶT TÊN (AGENTS.MD)

### 2.1. Kiến trúc 3 Tầng Thực dụng
```mermaid
graph TD
    UI[Controllers / Razor Views / Blazor Pages] --> Services[Services Layer: KhoaDichVu, HocVienDichVu, ExcelDichVu, QuanTriDuLieuDichVu...]
    Services --> Data[DbContext: CoSoDuLieuContext]
    Data --> DB[(SQLite: quanlydaotao.db)]
```
- **Controller / UI**: Chỉ tiếp nhận request, gọi Service qua Interface (Dependency Inversion), trả về ViewModel / View. Tuyệt đối không query `DbContext` trực tiếp trong Controller.
- **Service**: Đảm nhiệm validation nghiệp vụ, xử lý quan hệ logic, transaction và lưu dữ liệu.
- **Data (DbContext)**: Lưu trữ mô hình thực thể và cấu hình quan hệ ORM.

### 2.2. Quy chuẩn Đặt tên Bắt buộc (Code Standards)
- **UI Elements ID**: `tiềnTố` + `TênChứcNăng` (camelCase Tiếng Việt):
  - `btn`: Nút bấm (`btnThemKhoa`, `btnXoaLop`, `btnNhapExcel`, `btnXacNhanCommit`).
  - `txt`: Ô nhập văn bản (`txtMaHocVien`, `txtHoVaTen`, `txtEmail`, `txtMatKhau`).
  - `lbl`: Nhãn văn bản (`lblTieuDe`, `lblThongBaoLoi`, `lblThongKeHopLe`).
  - `pnl`: Khung chứa/Card (`pnlDanhSachKhoa`, `pnlBangDuLieu`, `pnlXemTruocPreview`).
  - `cbo`: Hộp chọn thả xuống (`cboChonKhoa`, `cboNienKhoa`).
  - `dgv`: Bảng dữ liệu (`dgvDanhSachHocVien`, `dgvDanhSachLop`, `dgvXemTruocDuLieu`).
- **Mã nguồn C#**:
  - Class, Struct, Enum, Interface, Property, Method: **PascalCase Tiếng Việt không dấu** (`Khoa.cs`, `HocVienDichVu.cs`, `IKhoaDichVu.cs`, `ThemHocVienAsync()`).
  - Interface: Luôn có tiền tố `I` (`IHocVienDichVu`, `IExcelDichVu`, `IQuanTriDuLieuDichVu`).
  - Private Fields: `_` + camelCase (`_coSoDuLieu`, `_excelDichVu`).
  - Biến Boolean: Bắt đầu bằng `Co`, `La`, `Da` (`CoHoatDong`, `LaHopLe`, `LaChuNhiem`).
  - Guard Clauses: Dùng `ArgumentNullException.ThrowIfNull` kiểm tra tham số đầu vào.
  - Khối lệnh: Mọi nhánh `if`, `else`, `for`, `while` bắt buộc có dấu ngoặc `{}`. Không nuốt Exception bằng `catch (Exception)` rỗng.

---

## 3. CƠ SỞ DỮ LIỆU & CẤU TRÚC ENTITY

Database SQLite được quản lý tại `QuanLyDaoTao/quanlydaotao.db` với các nhóm bảng:

```mermaid
erDiagram
    Khoas ||--o{ BoMons : "1 Khoa có nhiều Bộ môn"
    Khoas ||--o{ LopHocs : "1 Khoa quản lý nhiều Lớp"
    Khoas ||--o{ NhanSus : "1 Khoa có nhiều Cán bộ"
    BoMons ||--o{ NhanSus : "1 Bộ môn có nhiều Giảng viên"
    BoMons ||--o{ MonHocs : "1 Bộ môn phụ trách Môn học"
    LopHocs ||--o{ HocViens : "1 Lớp có nhiều Học viên"
    LopHocs ||--o{ PhanCongGiangDays : "Phân công lớp đào tạo"
    MonHocs ||--o{ PhanCongGiangDays : "Phân công môn học"
    NhanSus ||--o{ PhanCongGiangDays : "Giảng viên phụ trách"
    ChucVus ||--o{ NhanSus : "Chức vụ cán bộ"
    TrinhDoChuyenMons ||--o{ NhanSus : "Học vị / Trình độ"
    TruongTuyChinhs ||--o{ GiaTriTruongTuyChinhs : "Định nghĩa trường động"
```

### 3.1. Các Entity Nghiệp vụ Chính
1. **`Khoa`**: `KhoaId`, `MaKhoa` (Unique), `TenKhoa`, `MoTa`, `SoDienThoai`, `Email`, `DiaDiem`, `CoHoatDong`, `NgayTao`.
2. **`BoMon`**: `BoMonId`, `MaBoMon` (Unique), `TenBoMon`, `MoTa`, `KhoaId` (FK), `CoHoatDong`, `NgayTao`.
3. **`LopHoc`**: `LopHocId`, `MaLop` (Unique), `TenLop`, `NienKhoa`, `KhoaId` (FK), `SoLuongToiDa`, `CoHoatDong`, `NgayTao`.
4. **`HocVien`**: `HocVienId`, `MaHocVien` (Unique), `HoVaTen`, `NgaySinh`, `GioiTinh`, `Email`, `SoDienThoai`, `DiaChi`, `LopHocId` (FK), `TrangThai`, `NgayTao`.
5. **`NhanSu`**: `NhanSuId`, `MaNhanSu` (Unique), `HoVaTen`, `NgaySinh`, `GioiTinh`, `SoCCCD`, `Email`, `SoDienThoai`, `DiaChi`, `KhoaId` (FK), `BoMonId` (FK), `ChucVuId` (FK), `TrinhDoChuyenMonId` (FK), `ChuyenMonDaoTao`, `TrangThai`, `GhiChu`, `NgayTao`.
6. **`MonHoc`**: `MonHocId`, `MaMonHoc` (Unique), `TenMonHoc`, `SoTinChi`, `SoTietLyThuyet`, `SoTietThucHanh`, `BoMonId` (FK), `CoHoatDong`.
7. **`PhanCongGiangDay`**: `PhanCongId`, `MaPhanCong` (Unique), `NhanSuId` (FK), `MonHocId` (FK), `LopHocId` (FK), `HocKyNamHoc`, `VaiTroGiangDay`, `LaChuNhiem`, `GhiChu`.

### 3.2. Nhóm Bảng Mở Rộng & Kiểm Toán
8. **`TruongTuyChinhs` & `GiaTriTruongTuyChinhs`**: Động cơ **Dynamic Custom Fields Engine** cho phép thêm các trường dữ liệu tùy biến theo thời gian thực (như `DanToc`, `ChucVu` cho Học Viên; `Sat_nhap` cho Khoa; `Chuyen_giao` cho Bộ Môn; `ChungChi` cho Nhân Sự) mà không cần can thiệp migration cơ sở dữ liệu.
9. **`NhatKyHeThongs`**: Nhật ký Audit Trail ghi lại mọi hoạt động thêm/sửa/xóa, địa chỉ IP, người thực hiện, thời điểm và thay đổi dữ liệu cũ -> mới.
10. **`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`**: Nhóm bảng Identity quản lý người dùng và phân quyền.

---

## 4. HỆ THỐNG PHÂN QUYỀN (v4 — hợp nhất với LTMT-demo)

### 4.1. Hai vùng quản lý
Khoa vẫn quản lý cả hai nhánh, nhưng dữ liệu được tách thành hai **vùng** để giao quyền rõ ràng:

| Vùng | Mã | Dữ liệu |
|---|---|---|
| Tổ chức - Cán bộ | `ToChuc` | Khoa, Bộ môn (tổ chuyên môn), Cán bộ giảng viên |
| Đào tạo | `DaoTao` | Lớp (Hệ → Khóa → Chuyên ngành), Học viên, Môn học, Phân công |

### 4.2. Bốn vai trò (`VaiTroMacDinh`)
| Vai trò | Giá trị | Quyền ghi | Thông tin cá nhân |
|---|---|---|---|
| Siêu quản trị | `Superadmin` | Toàn bộ + Live CRUD Studio | Thấy đầy đủ |
| Quản trị viên | `Admin` | Toàn bộ nghiệp vụ, tài khoản, danh mục | Thấy đầy đủ |
| Quản lý | `Manager` | Chỉ vùng được giao (`VungQuanLy`), có thể giới hạn thêm một khoa (`KhoaPhuTrachId`) | Chỉ thấy trong phạm vi quản lý |
| Người xem | `Viewer` | Không | CCCD, SĐT chỉ còn 3 số cuối; email, địa chỉ bị ẩn |

- Quyền được kiểm tra **tại tầng dịch vụ** (`IPhanQuyenDichVu`), không chỉ ẩn nút trên giao diện. Xuất Excel cũng che dữ liệu theo quyền.
- Tạo/xóa Khoa, sửa danh mục Chức vụ/Trình độ: chỉ quản trị viên.
- Vai trò cũ tự chuyển khi khởi động: `DataManager` → Manager (Đào tạo), `CanBo` → Manager (Tổ chức), `GiaoVien`/`User` → Viewer.
- Đổi vai trò, khóa tài khoản → SecurityStamp đổi, phiên cũ hết hiệu lực trong vòng 1 phút.
- Đăng nhập sai 5 lần → khóa tạm 15 phút (quản trị viên mở được tại trang Tài khoản). Có giới hạn tần suất đăng nhập.

### 4.3. Tài khoản mẫu (chỉ dùng phát triển, mật khẩu `Admin@123456`)
| Email | Vai trò |
|---|---|
| `superadmin@quanlydaotao.local` | Superadmin |
| `admin@quanlydaotao.local` | Admin |
| `quanly.tochuc@quanlydaotao.local` | Manager · Tổ chức - Cán bộ |
| `quanly.daotao@quanlydaotao.local` | Manager · Đào tạo |
| `nguoixem@quanlydaotao.local` | Viewer |

Ngoài môi trường Development: thông tin tài khoản mẫu bị ẩn khỏi trang đăng nhập và bắt buộc cấu hình `Jwt:SecretKey`.

### 4.4. Quy tắc nghiệp vụ v4 (`QuyTacNghiepVu`, các `*DichVu`)
- CCCD 12 số, SĐT di động 10 số (03/05/07/08/09); **không trùng chéo** giữa học viên và cán bộ.
- Chỉ tiêu chức vụ trong lớp: BT 1, BP 2, AT 4, AP 4, BTCĐ 1, PBTCĐ 2, UVĐ 3 (hỗ trợ viết tắt và chức vụ ghép "AP, PBTLCĐ").
- Sĩ số tối đa của lớp; không hạ sĩ số tối đa xuống dưới sĩ số thực tế.
- Lãnh đạo: 1 Trưởng khoa, 2 Phó trưởng khoa mỗi khoa; 1 Trưởng bộ môn, 1 Phó trưởng bộ môn mỗi bộ môn.
- Bộ môn phải thuộc đúng khoa; không chuyển bộ môn sang khoa khác khi còn cán bộ.
- Lớp Chính quy khai báo khóa `Dxx` phải có mã `B{số}Dxx`; bỏ trống khóa thì hệ thống tự suy từ mã lớp.
- Mọi thao tác ghi đều được ghi nhật ký (giá trị cũ → mới) và phát tín hiệu để các tab đang mở tự làm mới.

### 4.5. Giao diện thống nhất (Blazor + Radzen)
Các trang Khoa, Bộ môn, Lớp, Học viên, Cán bộ dùng chung một khung: tiêu đề phân hệ, thẻ thống kê nhanh, tìm kiếm tức thì (không phân biệt dấu), bộ lọc gõ để tìm, "Kết quả x / y", bảng phân trang/sắp xếp, biểu mẫu dạng hộp thoại (giữ bản nháp khi lỗi, chặn bấm lưu lặp), thông báo toast, nhãn **Mới** (15 phút) / **Vừa cập nhật** (2 phút). Trang chủ có **Bảng tin** gắn phạm vi (toàn hệ thống / khoa / hệ / khóa / lớp) và khối **Cần chú ý** liệt kê dữ liệu thiếu hoặc vượt ngưỡng. Các trang MVC CRUD cũ vẫn còn route nhưng giao diện không còn trỏ tới.

---

## 5. TÍNH NĂNG NHẬP / XUẤT EXCEL CHUYÊN NGHIỆP

Đây là module trung tâm của dự án với các tiêu chuẩn an toàn cao cấp:

```mermaid
sequenceDiagram
    autonumber
    actor User as Người dùng
    participant Web as ExcelController / UI
    participant Svc as ExcelDichVu
    participant DB as SQLite DB

    User->>Web: 1. Tải lên tệp .xlsx (Chọn Chế độ linh hoạt)
    Web->>Svc: KiemTraVaDocTepExcelAsync(module, stream, choPhepThieuTruong)
    Note over Svc: Xác thực Header -> Kiểu dữ liệu -> Khóa ngoại -> Bù đắp trường thiếu
    Svc-->>Web: Trả về KetQuaKiemTraExcel (Đánh dấu Xanh/Vàng/Đỏ)
    Web->>User: 2. Hiển thị Bảng Xem Trước (Preview Table)
    User->>Web: 3. Bấm "Xác Nhận Nhập" (hoặc "Tiếp Tục Nhập Tất Cả")
    Web->>Svc: CommitDuLieuExcelAsync(module, danhSachHopLe)
    Svc->>DB: Thực thi trong 1 Transaction an toàn
    DB-->>Svc: Commit thành công
    Svc-->>Web: Kết quả commit
    Web->>User: Thông báo thành công và chuyển hướng về danh sách
```

### 5.1. Nhận diện Tiêu đề & Ràng buộc Header
- Khớp tiêu đề cột nghiêm ngặt (hỗ trợ alias chuẩn hóa). Báo lỗi cụ thể khi thiếu header bắt buộc, thừa header lạ hoặc trùng header.
- Tự động bỏ qua khoảng trắng thừa, chuẩn hóa Unicode.

### 5.2. Chế độ Nhập Linh Hoạt (Flexible/Tolerant Import)
Được kích hoạt mặc định qua công tắc trên giao diện `NhapExcel.cshtml` hoặc khi bấm `Tiếp Tục Nhập Tất Cả` tại `XemTruocPreview.cshtml`:
- **Thiếu `Mã học viên` / `Mã nhân sự`**: Tự động sinh mã nghiệp vụ duy nhất (`HV_20261103_0001`, `CB_AUTOGEN_0001`).
- **Thiếu `Giới tính`**: Tự động suy luận giới tính dựa vào quy luật họ tên (tên đệm `Thị`, các tên nữ `Vân, Hân, Chi, Nhi, Thư, Yến, Ly, Trang, Huyền, Phượng...` -> `Nữ`, còn lại -> `Nam`).
- **Thiếu hoặc sai `Ngày sinh`**: Tự động phân tích nhiều định dạng ngày (`dd/MM/yyyy`, `d/M/yyyy`, `yyyy-MM-dd`); nếu thiếu -> bù đắp ngày chuẩn (`01/01/2007`).
- **Thiếu `Mã lớp` / `Mã khoa`**: Tự động liên kết tới bản ghi khả dụng trong CSDL qua cơ chế `TryGetValue` chống văng lỗi `KeyNotFoundException`.
- **Trùng mã nghiệp vụ**: Tự động sinh hậu tố phân biệt để bảo toàn hàng và tiếp tục nhập.

### 5.3. Bảng Xem Trước (Cell-by-cell Color Visuals)
- **Ô Xanh lá (`.excel-cell-valid`)**: Dữ liệu chuẩn mực, hoàn toàn hợp lệ.
- **Ô Vàng (`.excel-cell-warning`)**: Dữ liệu khuyết thiếu đã được hệ thống **Tự Động Bù Đắp** (kèm icon phép thuật `<i class="bi bi-magic"></i>` và tooltip giải thích).
- **Ô Đỏ (`.excel-cell-error`)**: Báo lỗi nghiêm trọng cần sửa nếu ở chế độ nghiêm ngặt.
- **Tải Báo Cáo Lỗi**: Cho phép xuất tệp Excel báo cáo lỗi chi tiết từng dòng, từng ô để người dùng tải về chỉnh sửa.

---

## 6. MODULE ĐÀO TẠO & HỌC VIÊN KHÓA D57 ĐÃ NẠP TRONG CSDL

### 6.1. Danh mục 15 Lớp Học Khóa D57
Đã khởi tạo và chuẩn hóa 15 lớp đào tạo thuộc Khóa D57 (niên khóa 2025-2029) trực thuộc Khoa Công nghệ Thông tin:
- `B1D57`, `B2D57`, `B3D57`, `B4D57`, `B5D57`, `B6D57`, `B7D57`, `B8D57`, `B9D57`, `B10D57`, `B11D57`, `B12D57`, `B13D57`, `B14D57`, `B15D57`.

### 6.2. Dữ liệu 380 Học viên Khóa D57
Đã nạp đầy đủ 380 học viên từ danh sách bồi dưỡng nhận thức vào cơ sở dữ liệu:
- **Mã học viên**: Chuẩn hóa từ `D57_001` đến `D57_380`.
- **Thông tin cơ bản**: Họ và tên, Ngày sinh (`dd/MM/yyyy`), Giới tính (nhận diện Nam/Nữ), Lớp đào tạo (`LopHocId`), Quê quán / Địa chỉ cư trú (`DiaChi`).
- **Trường động Live**: Tự động liên kết và hiển thị **Dân tộc** (`DanToc`: Sán Dìu, Kinh, Tày, Thái, Mường, Dao, Nùng...) và **Chức vụ** (`ChucVu`: AT, AP, UVĐ, BTCĐ, BP, A trưởng, B phó...) trên bảng giao diện.
- **Sửa nhanh tại chỗ (Inline Edit)**: Quản trị viên có thể bấm trực tiếp vào ô trên bảng để sửa đổi giá trị mà không cần mở form phức tạp.

---

## 7. CẤU TRÚC THƯ MỤC DỰ ÁN

```
LTMT3/
├── QuanLyDaoTao/                      # Dự án ASP.NET Core MVC & Blazor chính
│   ├── Components/                    # Blazor Pages & Components
│   │   ├── Pages/                     # Giao diện Live CRUD & tra cứu nhanh
│   │   │   ├── HocVienPage.razor      # Danh sách & inline edit học viên
│   │   │   ├── NhanSuPage.razor       # Danh sách, CCCD, địa chỉ, bộ lọc nhân sự
│   │   │   ├── KhoaPage.razor         # Danh sách Khoa & trường động
│   │   │   ├── BoMonPage.razor        # Danh sách Bộ môn & trường động
│   │   │   ├── LopHocPage.razor       # Danh sách Lớp & sĩ số đào tạo
│   │   │   └── PhanCongPage.razor     # Phân công giảng dạy & Ghi chú
│   │   └── Layout/                    # NavMenu, MainLayout, Header
│   ├── Controllers/                   # MVC & API Controllers
│   │   ├── HocVienController.cs       # CRUD MVC Học viên
│   │   ├── KhoaController.cs          # CRUD MVC Khoa
│   │   ├── BoMonController.cs         # CRUD MVC Bộ môn
│   │   ├── LopHocController.cs        # CRUD MVC Lớp học
│   │   ├── NhanSuController.cs        # CRUD MVC Nhân sự
│   │   ├── MonHocController.cs        # CRUD MVC Môn học
│   │   ├── PhanCongGiangDayController.cs # CRUD MVC Phân công giảng dạy
│   │   ├── ExcelController.cs         # Xử lý Upload, Preview, Commit Excel
│   │   ├── TaiKhoanController.cs      # Đăng nhập, Đăng ký, Quản lý tài khoản
│   │   ├── QuanTriDuLieuController.cs # Live CRUD Database Studio (Superadmin)
│   │   └── Api/                       # RESTful APIs (HocVien, Khoa, Lop, NhanSu, LiveCrud, Excel)
│   ├── Data/                          # DbContext, Migrations, Seed Data
│   │   ├── CoSoDuLieuContext.cs       # DbContext Entity Framework Core
│   │   └── DuLieuKhoiTao.cs           # Seed Data tài khoản, danh mục mẫu
│   ├── Models/                        # Entity models & DTOs
│   │   ├── Excel/                     # DTOs: KetQuaKiemTraExcel, LoiChiTietExcel...
│   │   ├── QuanTri/                   # DTOs: TruongTuyChinh, DuLieuBangTrucTuyen...
│   │   └── TaiKhoan/                  # Identity models: TaiKhoan, VaiTroMacDinh...
│   ├── Services/                      # Tầng Service nghiệp vụ
│   │   ├── ExcelDichVu.cs             # Engine xử lý Excel ClosedXML
│   │   ├── HocVienDichVu.cs           # Nghiệp vụ Học viên
│   │   ├── KhoaDichVu.cs              # Nghiệp vụ Khoa
│   │   ├── BoMonDichVu.cs             # Nghiệp vụ Bộ môn
│   │   ├── LopHocDichVu.cs            # Nghiệp vụ Lớp học
│   │   ├── NhanSuDichVu.cs            # Nghiệp vụ Nhân sự
│   │   ├── QuanTriDuLieuDichVu.cs     # Engine Live CRUD & Trường tùy chỉnh động
│   │   └── NhatKyDichVu.cs            # Ghi nhận Audit Log hệ thống
│   ├── Views/                         # Razor Views (Bootstrap 5)
│   │   ├── Excel/                     # NhapExcel.cshtml, XemTruocPreview.cshtml
│   │   ├── HocVien/                   # Index.cshtml, ThemMoi.cshtml, ChinhSua.cshtml...
│   │   └── Shared/                    # _Layout.cshtml, _LoginPartial.cshtml
│   ├── wwwroot/                       # Static files (CSS, JS, local Bootstrap, Icons)
│   └── Program.cs                     # Startup configuration, DI, Authentication pipeline
├── QuanLyDaoTao.Tests/                # Bộ xUnit Integration & Unit Tests (151 tests)
│   ├── Api/                           # Kiểm thử REST API endpoints
│   ├── Controllers/                   # Kiểm thử MVC Controller & Phân quyền
│   ├── Excel/                         # Kiểm thử Header, Dữ liệu, Bảo mật, Preview & Commit
│   └── Services/                      # Kiểm thử Business Logic Services
└── TongHop.md                         # Tài liệu này
```

---

## 8. HƯỚNG DẪN KHỞI CHẠY & VẬN HÀNH DÀNH CHO DEVELOPER TIẾP QUẢN

### 8.1. Lệnh Biên dịch & Chạy Test
Developer tiếp quản cần kiểm tra trạng thái toàn vẹn trước khi chỉnh sửa:

```bash
# 1. Khôi phục packages & biên dịch không lỗi / không cảnh báo
dotnet build --warnaserror

# 2. Chạy toàn bộ 151 Unit & Integration Tests
dotnet test

# 3. Khởi động ứng dụng cục bộ
dotnet run --project QuanLyDaoTao/QuanLyDaoTao.csproj --urls "http://127.0.0.1:5219"
```

### 8.2. Mở Cổng Public Ra Internet (Cloudflare Tunnel)
Để chia sẻ hệ thống cho người khác truy cập mà không cần mở port modem / NAT:
```bash
cloudflared tunnel --url http://127.0.0.1:5219
```
*Ghi chú: Lấy đường dẫn `https://<ten-ngau-nhien>.trycloudflare.com` hiển thị trên màn hình terminal để truy cập.*

---

## 9. ĐỊNH HƯỚNG PHÁT TRIỂN TIẾP THEO (BACKLOG / ROADMAP)

1. **Dashboard Biểu đồ Trực quan (Analytics & Charts)**:
   - Thêm biểu đồ cơ cấu học viên theo Lớp, theo Dân tộc, theo Giới tính bằng Chart.js / Canvas local.
   - Thống kê tỷ lệ hoàn thành hồ sơ và phân công giảng dạy.
2. **In Ấn & Xuất Hồ sơ Học viên dạng PDF / Word**:
   - Tích hợp xuất phiếu lý lịch học viên / thẻ học viên PDF theo mẫu Học viện ANND.
3. **Module Đánh Giá Điểm Rèn Luyện & Điểm Thi**:
   - Xây dựng bảng điểm môn học cho học viên theo từng lớp và học kỳ.
   - Cho phép nhập điểm hàng loạt từ file Excel.
4. **Đồng Bộ Dữ Liệu Thời Gian Thực (SignalR)**:
   - Tích hợp SignalR để thông báo cho tất cả người dùng khi có học viên mới được import hoặc có thay đổi trên bảng phân công.
5. **Sao Lưu & Phục Hồi Cơ Sở Dữ Liệu Một Chạm (Backup & Restore UI)**:
   - Thêm tính năng sao lưu file `quanlydaotao.db` và tải về trực tiếp từ trang Quản trị hệ thống dành cho Superadmin.