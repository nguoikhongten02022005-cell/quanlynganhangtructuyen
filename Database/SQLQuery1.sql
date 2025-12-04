ALTER DATABASE QuanLyNganHangTrucTuyen 
SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE QuanLyNganHangTrucTuyen;
GO


USE QuanLyNganHangTrucTuyen;
GO
SELECT name FROM sys.databases WHERE name = 'QuanLyNganHangTrucTuyen';
USE master;
GO


CREATE DATABASE QuanLyNganHangTrucTuyen;
go
use QuanLyNganHangTrucTuyen;
go

CREATE TABLE NganHang (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten NVARCHAR(255) NOT NULL,
    tru_so_chinh NVARCHAR(255) NOT NULL
);

CREATE TABLE ChiNhanh (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten NVARCHAR(255) NOT NULL,
    dia_diem NVARCHAR(255) NOT NULL,
    ngan_hang_id INT,
    FOREIGN KEY (ngan_hang_id) REFERENCES NganHang(id)
);

CREATE TABLE LoaiTaiKhoan (
    ma NVARCHAR(50) PRIMARY KEY,
    ten NVARCHAR(255) NOT NULL,
    ky_hieu NVARCHAR(10) NOT NULL
);

CREATE TABLE TienTe (
    ma NVARCHAR(10) PRIMARY KEY,
    ten NVARCHAR(100) NOT NULL,
    ky_hieu NVARCHAR(10) NOT NULL
);

CREATE TABLE TrangThaiThe (
    ma NVARCHAR(50) PRIMARY KEY,
    nhan NVARCHAR(255) NOT NULL
);

CREATE TABLE DiaChi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    duong NVARCHAR(255),
    thanh_pho NVARCHAR(100),
    tinh NVARCHAR(100),
    ma_buu_dien NVARCHAR(20),
    quoc_gia NVARCHAR(100)
);

CREATE TABLE KhachHang (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten NVARCHAR(255) NOT NULL,
    email NVARCHAR(255) UNIQUE,
    ngay_sinh DATE NOT NULL,
    dia_chi_id INT,
    FOREIGN KEY (dia_chi_id) REFERENCES DiaChi(id)
);

CREATE TABLE TaiKhoan (
    id INT IDENTITY(1,1) PRIMARY KEY,
    so NVARCHAR(50) UNIQUE NOT NULL,
    so_du DECIMAL(15, 2) DEFAULT 0,
    ngay_tao DATETIME DEFAULT GETDATE(),
    ngay_dong DATE,
    bi_khoa BIT DEFAULT 0,
    ma_loai_tai_khoan NVARCHAR(50),
    chi_nhanh_id INT,
    khach_hang_id INT,
    ma_tien_te NVARCHAR(10),
    FOREIGN KEY (ma_loai_tai_khoan) REFERENCES LoaiTaiKhoan(ma),
    FOREIGN KEY (chi_nhanh_id) REFERENCES ChiNhanh(id),
    FOREIGN KEY (khach_hang_id) REFERENCES KhachHang(id),
    FOREIGN KEY (ma_tien_te) REFERENCES TienTe(ma)
);

CREATE TABLE The (
    id INT IDENTITY(1,1) PRIMARY KEY,
    so NVARCHAR(50) UNIQUE NOT NULL,
    hieu_luc_den DATETIME NOT NULL,
    tai_khoan_id INT,
    ma_trang_thai NVARCHAR(50),
    FOREIGN KEY (tai_khoan_id) REFERENCES TaiKhoan(id),
    FOREIGN KEY (ma_trang_thai) REFERENCES TrangThaiThe(ma)
);

CREATE TABLE GiayToDanhTinh (
    id INT IDENTITY(1,1) PRIMARY KEY,
    loai NVARCHAR(100) NOT NULL,
    so NVARCHAR(100) NOT NULL,
    ngay_cap DATE,
    ngay_het_han DATE,
    khach_hang_id INT,
    FOREIGN KEY (khach_hang_id) REFERENCES KhachHang(id)
);

CREATE TABLE TrangThaiKhoanVay (
    ma NVARCHAR(50) PRIMARY KEY,
    nhan NVARCHAR(255) NOT NULL
);

CREATE TABLE LoaiKhoanVay (
    ma NVARCHAR(50) PRIMARY KEY,
    ten NVARCHAR(255) NOT NULL
);

CREATE TABLE NhanVien (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ten NVARCHAR(255) NOT NULL,
    chuc_vu NVARCHAR(255) NOT NULL,
    email NVARCHAR(255) UNIQUE,
    chi_nhanh_id INT,
    FOREIGN KEY (chi_nhanh_id) REFERENCES ChiNhanh(id)
);

CREATE TABLE KhoanVay (
    id INT IDENTITY(1,1) PRIMARY KEY,
    so_tien DECIMAL(15, 2) NOT NULL,
    lai_suat DECIMAL(5, 2) NOT NULL,
    ngay_bat_dau DATE NOT NULL,
    ngay_ket_thuc DATE NOT NULL,
    ma_trang_thai NVARCHAR(50),
    ma_loai_vay NVARCHAR(50),
    khach_hang_id INT,
    nhan_vien_id INT,
    FOREIGN KEY (ma_trang_thai) REFERENCES TrangThaiKhoanVay(ma),
    FOREIGN KEY (ma_loai_vay) REFERENCES LoaiKhoanVay(ma),
    FOREIGN KEY (khach_hang_id) REFERENCES KhachHang(id),
    FOREIGN KEY (nhan_vien_id) REFERENCES NhanVien(id)
);

CREATE TABLE ThanhToan (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ngay DATE NOT NULL,
    so_tien DECIMAL(15, 2) NOT NULL,
    so_du_con_lai DECIMAL(15, 2) NOT NULL,
    khoan_vay_id INT,
    FOREIGN KEY (khoan_vay_id) REFERENCES KhoanVay(id)
);

CREATE TABLE LoaiGiaoDich (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ma NVARCHAR(20) UNIQUE NOT NULL,
    nhan NVARCHAR(100) NOT NULL
);

CREATE TABLE GiaoDich (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ngay DATETIME DEFAULT GETDATE(),
    ghi_no DECIMAL(15, 2) DEFAULT 0,
    ghi_co DECIMAL(15, 2) DEFAULT 0,
    mo_ta NVARCHAR(MAX),
    chi_tiet NVARCHAR(MAX),
    tai_khoan_id INT,
    loai_giao_dich_id INT,
    FOREIGN KEY (tai_khoan_id) REFERENCES TaiKhoan(id),
    FOREIGN KEY (loai_giao_dich_id) REFERENCES LoaiGiaoDich(id)
);


INSERT INTO NganHang (ten, tru_so_chinh) VALUES
(N'Ngân hàng Vietcombank', N'Hà Nội'),
(N'Ngân hàng ACB', N'Hồ Chí Minh'),
(N'Ngân hàng Techcombank', N'Hà Nội');

INSERT INTO ChiNhanh (ten, dia_diem, ngan_hang_id) VALUES
(N'Chi nhánh Hoàn Kiếm', N'Quận Hoàn Kiếm, Hà Nội', 1),
(N'Chi nhánh Cầu Giấy', N'Quận Cầu Giấy, Hà Nội', 1),
(N'Chi nhánh Quận 1', N'Quận 1, TP.HCM', 2),
(N'Chi nhánh Quận 3', N'Quận 3, TP.HCM', 2),
(N'Chi nhánh Thanh Xuân', N'Quận Thanh Xuân, Hà Nội', 3),
(N'Chi nhánh Hải Châu', N'Quận Hải Châu, Đà Nẵng', 3);

INSERT INTO LoaiTaiKhoan (ma, ten, ky_hieu) VALUES
(N'TKTT', N'Tài khoản thanh toán', N'TT'),
(N'TKTK', N'Tài khoản tiết kiệm', N'TK'),
(N'TKDN', N'Tài khoản doanh nghiệp', N'DN'),
(N'TKVIP', N'Tài khoản VIP', N'VP');

INSERT INTO TienTe (ma, ten, ky_hieu) VALUES
(N'VND', N'Việt Nam Đồng', N'₫'),
(N'USD', N'Đô la Mỹ', N'$'),
(N'EUR', N'Euro', N'€');

INSERT INTO TrangThaiThe (ma, nhan) VALUES
(N'HOAT_DONG', N'Hoạt động'),
(N'KHOA_TAM', N'Khóa tạm thời'),
(N'VO_HIEU', N'Vô hiệu');

INSERT INTO DiaChi (duong, thanh_pho, tinh, ma_buu_dien, quoc_gia) VALUES
(N'12 Lê Lợi', N'Hà Nội', N'Hà Nội', N'100000', N'Việt Nam'),
(N'25 Hai Bà Trưng', N'Hà Nội', N'Hà Nội', N'100100', N'Việt Nam'),
(N'30 Nguyễn Huệ', N'TP.HCM', N'Hồ Chí Minh', N'700000', N'Việt Nam'),
(N'150 Lê Duẩn', N'Đà Nẵng', N'Đà Nẵng', N'550000', N'Việt Nam'),
(N'89 Trần Phú', N'Nha Trang', N'Khánh Hòa', N'650000', N'Việt Nam'),
(N'77 Lê Lợi', N'Huế', N'Thừa Thiên Huế', N'530000', N'Việt Nam'),
(N'233 Trần Hưng Đạo', N'Cần Thơ', N'Cần Thơ', N'900000', N'Việt Nam'),
(N'45 Nguyễn Trãi', N'Nam Định', N'Nam Định', N'420000', N'Việt Nam'),
(N'18 Điện Biên Phủ', N'Đà Lạt', N'Lâm Đồng', N'670000', N'Việt Nam'),
(N'90 Võ Thị Sáu', N'Vũng Tàu', N'Bà Rịa - Vũng Tàu', N'780000', N'Việt Nam');

INSERT INTO KhachHang (ten, email, ngay_sinh, dia_chi_id) VALUES
(N'Nguyễn Văn A', N'a1@example.com', '1990-01-10', 1),
(N'Trần Thị B', N'b2@example.com', '1992-03-12', 2),
(N'Lê Văn C', N'c3@example.com', '1985-07-22', 3),
(N'Phạm Thị D', N'd4@example.com', '1998-11-02', 4),
(N'Hoàng Văn E', N'e5@example.com', '1994-05-30', 5),
(N'Vũ Thị F', N'f6@example.com', '1999-06-18', 6),
(N'Đỗ Văn G', N'g7@example.com', '1980-12-15', 7),
(N'Bùi Thị H', N'h8@example.com', '1995-09-19', 8),
(N'Huỳnh Văn I', N'i9@example.com', '1988-10-05', 9),
(N'Mai Thị K', N'k10@example.com', '1993-08-25', 10);

INSERT INTO TrangThaiKhoanVay (ma, nhan) VALUES
(N'DANG_VAY', N'Đang vay'),
(N'DA_TRA', N'Đã tất toán'),
(N'QUA_HAN', N'Quá hạn');

INSERT INTO LoaiKhoanVay (ma, ten) VALUES
(N'TD', N'Vay tiêu dùng'),
(N'MB', N'Vay mua nhà'),
(N'OTO', N'Vay mua ô tô');

INSERT INTO NhanVien (ten, chuc_vu, email, chi_nhanh_id) VALUES
(N'Lê Văn Nhân', N'Chuyên viên tín dụng', N'nhan.lv@bank.com', 1),
(N'Phạm Mai Hoa', N'Giao dịch viên', N'hoa.pm@bank.com', 2),
(N'Ngô Quốc Huy', N'Quản lý chi nhánh', N'huy.nq@bank.com', 3),
(N'Đặng Thùy Trang', N'Chuyên viên khách hàng', N'trang.dt@bank.com', 4),
(N'Vũ Đức Minh', N'Chuyên viên tín dụng', N'minh.vd@bank.com', 5),
(N'Hoàng Nhật Nam', N'Giao dịch viên', N'nam.hn@bank.com', 6);

INSERT INTO TaiKhoan (so, so_du, ma_loai_tai_khoan, chi_nhanh_id, khach_hang_id, ma_tien_te) VALUES
(N'10000001', 5000000, N'TKTT', 1, 1, N'VND'),
(N'10000002', 12000000, N'TKTK', 1, 2, N'VND'),
(N'10000003', 2500000, N'TKTT', 2, 3, N'VND'),
(N'10000004', 8000000, N'TKVIP', 2, 4, N'VND'),
(N'10000005', 1500, N'TKTT', 3, 5, N'USD'),
(N'10000006', 20000000, N'TKDN', 3, 6, N'VND'),
(N'10000007', 6000000, N'TKTT', 4, 7, N'VND'),
(N'10000008', 9000000, N'TKTK', 4, 8, N'VND'),
(N'10000009', 3000000, N'TKTT', 5, 9, N'EUR'),
(N'10000010', 4500000, N'TKTT', 5, 10, N'VND'),
(N'10000011', 7000000, N'TKTT', 6, 1, N'VND'),
(N'10000012', 50000000, N'TKDN', 6, 2, N'VND'),
(N'10000013', 2200000, N'TKTT', 1, 3, N'VND'),
(N'10000014', 11000000, N'TKTK', 2, 4, N'VND'),
(N'10000015', 9500000, N'TKVIP', 3, 5, N'VND');

INSERT INTO The (so, hieu_luc_den, tai_khoan_id, ma_trang_thai) VALUES
(N'4111111111110001', '2027-12-31', 1, N'HOAT_DONG'),
(N'4111111111110002', '2028-05-31', 2, N'HOAT_DONG'),
(N'4111111111110003', '2026-09-30', 3, N'KHOA_TAM'),
(N'4111111111110004', '2025-11-30', 4, N'VO_HIEU'),
(N'4111111111110005', '2029-01-31', 5, N'HOAT_DONG'),
(N'4111111111110006', '2027-04-30', 6, N'HOAT_DONG'),
(N'4111111111110007', '2028-07-31', 7, N'KHOA_TAM'),
(N'4111111111110008', '2029-10-31', 8, N'HOAT_DONG'),
(N'4111111111110009', '2026-03-31', 9, N'HOAT_DONG'),
(N'4111111111110010', '2027-08-31', 10, N'HOAT_DONG');

INSERT INTO GiayToDanhTinh (loai, so, ngay_cap, ngay_het_han, khach_hang_id) VALUES
(N'CCCD', N'012345678001', '2015-01-10', '2035-01-10', 1),
(N'CCCD', N'012345678002', '2016-03-12', '2036-03-12', 2),
(N'CMND', N'123456789', '2010-07-22', '2030-07-22', 3),
(N'Hộ chiếu', N'B1234567', '2019-11-02', '2029-11-02', 4),
(N'CCCD', N'012345678005', '2017-05-30', '2037-05-30', 5),
(N'CCCD', N'012345678006', '2018-06-18', '2038-06-18', 6),
(N'CMND', N'987654321', '2012-12-15', '2032-12-15', 7),
(N'Hộ chiếu', N'C7654321', '2020-09-19', '2030-09-19', 8),
(N'CCCD', N'012345678009', '2019-10-05', '2039-10-05', 9),
(N'CCCD', N'012345678010', '2020-08-25', '2040-08-25', 10);

INSERT INTO KhoanVay (so_tien, lai_suat, ngay_bat_dau, ngay_ket_thuc,
                      ma_trang_thai, ma_loai_vay, khach_hang_id, nhan_vien_id)
VALUES
(200000000, 12.50, '2023-01-01', '2028-01-01', N'DANG_VAY', N'MB', 1, 1),
(50000000, 14.00, '2022-03-15', '2025-03-15', N'DANG_VAY', N'TD', 2, 2),
(300000000, 10.00, '2021-06-20', '2031-06-20', N'DANG_VAY', N'OTO', 3, 3),
(80000000, 13.50, '2020-09-10', '2025-09-10', N'DA_TRA', N'TD', 4, 4),
(150000000, 11.20, '2019-11-05', '2029-11-05', N'QUA_HAN', N'MB', 5, 5),
(60000000, 15.00, '2021-02-18', '2024-02-18', N'DA_TRA', N'TD', 6, 6),
(120000000, 12.00, '2022-07-01', '2027-07-01', N'DANG_VAY', N'MB', 7, 1),
(90000000, 13.00, '2023-04-25', '2028-04-25', N'DANG_VAY', N'TD', 8, 2);

INSERT INTO ThanhToan (ngay, so_tien, so_du_con_lai, khoan_vay_id) VALUES
('2023-06-01', 5000000, 195000000, 1),
('2023-12-01', 7000000, 188000000, 1),
('2022-08-01', 3000000, 47000000, 2),
('2023-02-01', 3000000, 44000000, 2),
('2021-12-20', 10000000, 290000000, 3),
('2022-06-20', 15000000, 275000000, 3),
('2021-03-10', 8000000, 72000000, 4),
('2022-03-10', 8000000, 64000000, 4),
('2020-05-05', 5000000, 145000000, 5),
('2021-05-05', 5000000, 140000000, 5),
('2021-10-18', 6000000, 54000000, 6),
('2022-10-18', 6000000, 48000000, 6),
('2022-12-01', 7000000, 113000000, 7),
('2023-06-01', 7000000, 106000000, 7),
('2023-10-25', 4500000, 85500000, 8),
('2024-04-25', 4500000, 81000000, 8);

INSERT INTO LoaiGiaoDich (ma, nhan) VALUES
(N'NAP', N'Nạp tiền'),
(N'RUT', N'Rút tiền'),
(N'CK', N'Chuyển khoản'),
(N'PHI', N'Thu phí');

INSERT INTO GiaoDich (ngay, ghi_no, ghi_co, mo_ta, chi_tiet, tai_khoan_id, loai_giao_dich_id) VALUES
('2024-01-10 09:15:00', 0, 2000000, N'Nạp tiền mặt', N'Nạp tại quầy chi nhánh Hoàn Kiếm', 1, 1),
('2024-01-11 14:20:00', 500000, 0, N'Rút tiền ATM', N'Rút ATM Vietcombank Cầu Giấy', 1, 2),
('2024-01-12 16:45:00', 0, 3000000, N'Nhận chuyển khoản', N'Nhận từ tài khoản 10000003', 2, 3),
('2024-01-13 10:00:00', 1000000, 0, N'Chuyển khoản', N'Chuyển cho tài khoản 10000004', 2, 3),
('2024-01-14 11:30:00', 20000, 0, N'Thu phí duy trì', N'Phí duy trì tài khoản tháng 1', 1, 4),
('2024-01-15 09:00:00', 0, 5000000, N'Nạp tiền online', N'Nạp qua Internet Banking', 3, 1),
('2024-01-16 13:15:00', 2000000, 0, N'Rút tiền tại quầy', N'Rút chi nhánh Quận 1', 3, 2),
('2024-01-17 08:50:00', 0, 1500000, N'Nhận lương', N'Lương tháng 1', 4, 1),
('2024-01-18 18:25:00', 300000, 0, N'Chuyển khoản', N'Thanh toán hóa đơn điện', 4, 3),
('2024-01-19 19:40:00', 15000, 0, N'Thu phí SMS', N'Phí dịch vụ SMS Banking', 4, 4),
('2024-01-20 09:10:00', 0, 800000, N'Nạp tiền', N'Nạp tiền tại ATM', 5, 1),
('2024-01-21 10:05:00', 300000, 0, N'Rút tiền', N'Rút tiền ATM quận 3', 5, 2),
('2024-01-22 14:30:00', 0, 2500000, N'Nhận chuyển khoản', N'Nhận từ 10000001', 6, 3),
('2024-01-23 16:00:00', 1500000, 0, N'Chuyển khoản', N'Chuyển đến 10000010', 6, 3),
('2024-01-24 08:40:00', 25000, 0, N'Thu phí thường niên', N'Phí thường niên thẻ ATM', 6, 4),
('2024-01-25 09:25:00', 0, 4000000, N'Nạp tiền', N'Nạp tiền chi nhánh Thanh Xuân', 7, 1),
('2024-01-26 12:35:00', 1000000, 0, N'Rút tiền', N'Rút tiền ATM Đà Nẵng', 7, 2),
('2024-01-27 17:10:00', 0, 3500000, N'Nhận chuyển khoản', N'Nhận từ 10000002', 8, 3),
('2024-01-28 20:50:00', 500000, 0, N'Thanh toán mua sắm', N'Thanh toán siêu thị', 8, 3),
('2024-01-29 07:55:00', 10000, 0, N'Thu phí dịch vụ', N'Phí Internet Banking', 8, 4);
