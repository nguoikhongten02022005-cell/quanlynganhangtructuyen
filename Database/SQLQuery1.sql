﻿ALTER DATABASE QuanLyNganHangTrucTuyen 
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


CREATE TABLE NguoiDung (
    MaNguoiDung INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhauHash VARCHAR(255) NOT NULL,
    VaiTro VARCHAR(20) CHECK (VaiTro IN ('ADMIN', 'STAFF', 'CUSTOMER')),
    NgayTao DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT UNIQUE NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoCCCD VARCHAR(20) UNIQUE,
    Email VARCHAR(100),
    SoDienThoai VARCHAR(15),
    TrangThaiKYC VARCHAR(20) DEFAULT 'PENDING',
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);
GO

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT NOT NULL,
    SoTaiKhoan VARCHAR(20) UNIQUE NOT NULL,
    SoDu DECIMAL(18, 2) DEFAULT 0,
    TrangThai VARCHAR(20) DEFAULT 'ACTIVE',
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);
GO

CREATE TABLE NguoiThuHuong (
    MaThuHuong INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT NOT NULL,
    TenGoiNho NVARCHAR(100),
    SoTaiKhoanNguoiNhan VARCHAR(20) NOT NULL,
    NganHang NVARCHAR(50) DEFAULT N'Nội bộ',
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);
GO

CREATE TABLE GiaoDich (
    MaGiaoDich INT PRIMARY KEY IDENTITY(1,1),
    TaiKhoanNguon INT NOT NULL,
    TaiKhoanDich INT,
    SoTien DECIMAL(18, 2) NOT NULL,
    LoaiGiaoDich VARCHAR(20),
    NoiDung NVARCHAR(200),
    ThoiGian DATETIME DEFAULT GETDATE(),
    TrangThai VARCHAR(20) DEFAULT 'SUCCESS',
    FOREIGN KEY (TaiKhoanNguon) REFERENCES TaiKhoan(MaTaiKhoan)
);
GO

CREATE TABLE HoaDon (
    MaHoaDon INT PRIMARY KEY IDENTITY(1,1),
    LoaiDichVu NVARCHAR(50),
    MaKhachHangDichVu VARCHAR(50),
    SoTien DECIMAL(18, 2) NOT NULL,
    KyCuoc VARCHAR(20),
    DaThanhToan BIT DEFAULT 0
);
GO

CREATE TABLE ThanhToan (
    MaThanhToan INT PRIMARY KEY IDENTITY(1,1),
    MaGiaoDich INT NOT NULL,
    MaHoaDon INT NOT NULL,
    NgayThanhToan DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaGiaoDich) REFERENCES GiaoDich(MaGiaoDich),
    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon)
);
GO