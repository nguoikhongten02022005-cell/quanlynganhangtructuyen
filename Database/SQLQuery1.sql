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


CREATE INDEX IX_NguoiDung_VaiTro_TrangThai ON NguoiDung(VaiTro, TrangThai);  

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

-- Bang lich su dang nhap
CREATE TABLE LichSuDangNhap (
    MaLichSu INT PRIMARY KEY IDENTITY(1,1),
    MaNguoiDung INT NOT NULL,
    ThoiGianDangNhap DATETIME DEFAULT GETDATE(),
    DiaChiIP VARCHAR(45),
    ThietBi NVARCHAR(255),
    TrinhDuyet NVARCHAR(255),
    TrangThai VARCHAR(10) NOT NULL DEFAULT 'SUCCESS' CHECK (TrangThai IN ('SUCCESS', 'FAILED')),
    LyDoThatBai NVARCHAR(255),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);
GO

-- Bang ghi chu cua nhan vien (Staff Module)
CREATE TABLE GhiChuKhachHang (
    MaGhiChu INT PRIMARY KEY IDENTITY(1,1),
    MaKhachHang INT NOT NULL,
    MaNhanVien INT NOT NULL,
    NoiDung NVARCHAR(1000) NOT NULL,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    FOREIGN KEY (MaNhanVien) REFERENCES NguoiDung(MaNguoiDung)
);
GO

-- Index cho LichSuDangNhap
CREATE INDEX IX_LichSuDangNhap_MaNguoiDung ON LichSuDangNhap(MaNguoiDung);
CREATE INDEX IX_LichSuDangNhap_ThoiGianDangNhap ON LichSuDangNhap(ThoiGianDangNhap DESC);
GO

-- Index cho GhiChuKhachHang
CREATE INDEX IX_GhiChuKhachHang_MaKhachHang ON GhiChuKhachHang(MaKhachHang);
CREATE INDEX IX_GhiChuKhachHang_NgayTao ON GhiChuKhachHang(NgayTao DESC);
GO

-- FUNCTION: TAO SO TAI KHOAN TU DONG
CREATE FUNCTION dbo.FN_TaoSoTaiKhoan()
RETURNS VARCHAR(14)
AS
BEGIN
    DECLARE @SoTaiKhoan VARCHAR(14);
    DECLARE @MaTaiKhoanMax INT;
    
    -- Lay MaTaiKhoan lon nhat (tranh trung khi xoa tai khoan)
    SELECT @MaTaiKhoanMax = ISNULL(MAX(MaTaiKhoan), 0) FROM TaiKhoan;
    
    -- Tang len 1 de tao so moi
    SET @MaTaiKhoanMax = @MaTaiKhoanMax + 1;
    
    -- Format: 10 + 12 chu so (VD: 10000000000001)
    SET @SoTaiKhoan = '10' + RIGHT('000000000000' + CAST(@MaTaiKhoanMax AS VARCHAR), 12);
    
    RETURN @SoTaiKhoan;
END
GO

-- STORED PROCEDURE: CHUYEN TIEN AN TOAN (CO KIEM TRA TAI KHOAN NHAN)
CREATE PROCEDURE SP_ChuyenTien
    @MaTaiKhoanGui INT,
    @MaTaiKhoanNhan INT,
    @SoTien DECIMAL(15,2),
    @NoiDung NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @SoDuGui DECIMAL(15,2);
        DECLARE @SoDuNhan DECIMAL(15,2);
        DECLARE @MaGiaoDich INT;
        
        -- 1. Kiem tra so du TAI KHOAN GUI (UPDLOCK: khoa row de tranh race condition)
        SELECT @SoDuGui = SoDu 
        FROM TaiKhoan WITH (UPDLOCK)
        WHERE MaTaiKhoan = @MaTaiKhoanGui;
        
        IF @SoDuGui IS NULL
        BEGIN
            RAISERROR('Tai khoan gui khong ton tai!', 16, 1);
            RETURN;
        END
        
        IF @SoDuGui < @SoTien
        BEGIN
            RAISERROR('So du khong du!', 16, 1);
            RETURN;
        END
        
        -- 2. Kiem tra TAI KHOAN NHAN co ton tai khong (UPDLOCK de dam bao ton tai)
        SELECT @SoDuNhan = SoDu 
        FROM TaiKhoan WITH (UPDLOCK)
        WHERE MaTaiKhoan = @MaTaiKhoanNhan;
        
        IF @SoDuNhan IS NULL
        BEGIN
            RAISERROR('Tai khoan nhan khong ton tai!', 16, 1);
            RETURN;
        END
        
        -- 3. Tru tien tai khoan gui
        UPDATE TaiKhoan 
        SET SoDu = SoDu - @SoTien
        WHERE MaTaiKhoan = @MaTaiKhoanGui;
        
        -- 4. Cong tien tai khoan nhan
        UPDATE TaiKhoan 
        SET SoDu = SoDu + @SoTien
        WHERE MaTaiKhoan = @MaTaiKhoanNhan;
        
        -- 5. Luu giao dich SUCCESS
        INSERT INTO GiaoDich (MaTaiKhoanGui, MaTaiKhoanNhan, SoTien, NoiDung, TrangThai, NgayGiaoDich)
        VALUES (@MaTaiKhoanGui, @MaTaiKhoanNhan, @SoTien, @NoiDung, 'SUCCESS', GETDATE());
        
        SET @MaGiaoDich = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        -- Tra ve ket qua thanh cong
        SELECT 
            'SUCCESS' AS Result, 
            @MaGiaoDich AS MaGiaoDich,
            'Chuyen tien thanh cong!' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Tra ve loi
        SELECT 
            'FAILED' AS Result,
            NULL AS MaGiaoDich,
            ERROR_MESSAGE() AS Message;
    END CATCH
END
GO




