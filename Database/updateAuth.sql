ALTER TABLE NguoiDung 
ADD TrangThai VARCHAR(20) DEFAULT 'ACTIVE';
GO

UPDATE NguoiDung SET TrangThai = 'ACTIVE' WHERE TrangThai IS NULL;
GO

-- xem tài khoản ADMIN / STAFF
SELECT 
    MaNguoiDung,
    TenDangNhap,
    MatKhauHash,
    VaiTro,
    TrangThai,
    NgayTao
FROM NguoiDung
WHERE VaiTro IN ('ADMIN', 'STAFF');

DELETE FROM NguoiDung
WHERE TenDangNhap = 'admin1';

DBCC CHECKIDENT ('NguoiDung', RESEED, 0);

--Sửa dữ liệu cũ trong DB (đã tạo nhầm PENDING)
UPDATE KhachHang
SET TrangThaiKYC = 'NONE'
WHERE TrangThaiKYC = 'PENDING' AND SoCCCD IS NULL;



--Xoá UNIQUE constraint trên SoCCCD
ALTER TABLE dbo.KhachHang
DROP CONSTRAINT [UQ__KhachHan__8A547D3AA839022B];
GO

