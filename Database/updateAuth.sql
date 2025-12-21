ALTER TABLE NguoiDung 
ADD TrangThai VARCHAR(20) DEFAULT 'ACTIVE';
GO


UPDATE NguoiDung SET TrangThai = 'ACTIVE' WHERE TrangThai IS NULL;
GO

SELECT TenDangNhap, MatKhauHash 
FROM NguoiDung 
WHERE TenDangNhap = 'test_admin';

UPDATE NguoiDung
SET MatKhauHash = '$2a$11$/8WAIyUDklt3BPwN39nCu.3Kij9bU2PnOz.D/4tLMfGIbEOpyorHO'
WHERE TenDangNhap = 'admin';

INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, VaiTro, TrangThai, NgayTao)
SELECT 'admin', MatKhauHash, 'ADMIN', 'ACTIVE', GETDATE()
FROM NguoiDung
WHERE TenDangNhap = 'test_admin';
GO


SELECT TenDangNhap, VaiTro, MatKhauHash
FROM NguoiDung
WHERE TenDangNhap = 'staff03';
