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

DELETE FROM NguoiDung WHERE TenDangNhap = 'admin1';



--Xoá UNIQUE constraint trên SoCCCD
ALTER TABLE dbo.KhachHang
DROP CONSTRAINT [UQ__KhachHan__8A547D3AA839022B];
GO

