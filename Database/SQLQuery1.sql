﻿USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QuanLyNganHangTrucTuyen')
BEGIN
    ALTER DATABASE QuanLyNganHangTrucTuyen SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyNganHangTrucTuyen;
END
GO

CREATE DATABASE QuanLyNganHangTrucTuyen;
GO

USE QuanLyNganHangTrucTuyen;
GO

CREATE TABLE NguoiDung (
    MaNguoiDung INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhauHash VARCHAR(255) NOT NULL,  
    VaiTro VARCHAR(10) NOT NULL DEFAULT 'CUSTOMER' CHECK (VaiTro IN ('ADMIN', 'STAFF', 'CUSTOMER')),
    NgayTao DATETIME DEFAULT GETDATE(),
    TrangThai VARCHAR(10) NOT NULL DEFAULT 'ACTIVE' CHECK (TrangThai IN ('ACTIVE', 'LOCKED'))
);
GO

CREATE TABLE KhachHang (
    MaKhachHang INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT UNIQUE NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoCCCD VARCHAR(12),
    Email VARCHAR(100),
    SoDienThoai VARCHAR(10),
    TrangThaiKYC VARCHAR(10) NOT NULL DEFAULT 'NONE' CHECK (TrangThaiKYC IN ('NONE', 'PENDING', 'APPROVED', 'REJECTED')),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);
GO

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT NOT NULL,
    SoTaiKhoan VARCHAR(14) UNIQUE NOT NULL,
    SoDu DECIMAL(15, 2) DEFAULT 0 CHECK (SoDu >= 0), 
    TrangThai VARCHAR(10) NOT NULL DEFAULT 'ACTIVE' CHECK (TrangThai IN ('ACTIVE', 'LOCKED', 'CLOSED')),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);
GO

CREATE TABLE GiaoDich (
    MaGiaoDich INT PRIMARY KEY IDENTITY(1,1),
    MaTaiKhoanGui INT NOT NULL,
    MaTaiKhoanNhan INT NOT NULL,
    SoTien DECIMAL(15, 2) NOT NULL CHECK (SoTien > 0),  
    NoiDung NVARCHAR(200),
    NgayGiaoDich DATETIME DEFAULT GETDATE(),
    TrangThai VARCHAR(8) NOT NULL DEFAULT 'PENDING' CHECK (TrangThai IN ('PENDING', 'SUCCESS', 'FAILED')),
    MaOTP VARCHAR(6),
    ThoiHanOTP DATETIME,
    FOREIGN KEY (MaTaiKhoanGui) REFERENCES TaiKhoan(MaTaiKhoan),
    FOREIGN KEY (MaTaiKhoanNhan) REFERENCES TaiKhoan(MaTaiKhoan),
    CHECK (MaTaiKhoanGui <> MaTaiKhoanNhan) 
);
GO


CREATE INDEX IX_NguoiDung_VaiTro_TrangThai ON NguoiDung(VaiTro, TrangThai);  -- Composite index

-- KhachHang: Index cho tim kiem va JOIN
CREATE INDEX IX_KhachHang_TrangThaiKYC ON KhachHang(TrangThaiKYC);
CREATE INDEX IX_KhachHang_SoCCCD ON KhachHang(SoCCCD);

-- TaiKhoan: Index cho JOIN
CREATE INDEX IX_TaiKhoan_MaKhachHang ON TaiKhoan(MaKhachHang);
CREATE INDEX IX_TaiKhoan_TrangThai ON TaiKhoan(TrangThai);

-- GiaoDich: Index cho truy van lich su va filter
CREATE INDEX IX_GiaoDich_TrangThai ON GiaoDich(TrangThai);
CREATE INDEX IX_GiaoDich_NgayGiaoDich ON GiaoDich(NgayGiaoDich DESC);
CREATE INDEX IX_GiaoDich_MaTaiKhoanGui_NgayGiaoDich ON GiaoDich(MaTaiKhoanGui, NgayGiaoDich DESC);  -- Composite
CREATE INDEX IX_GiaoDich_MaTaiKhoanNhan_NgayGiaoDich ON GiaoDich(MaTaiKhoanNhan, NgayGiaoDich DESC);  -- Composite
GO

