USE QuanLyNganHangTrucTuyen;
GO

-- Bổ sung thông tin KYC cho KhachHang
ALTER TABLE KhachHang
ADD 
    AnhCCCDTruoc VARCHAR(255),
    AnhCCCDSau VARCHAR(255),
    ThoiGianGuiKYC DATETIME;
GO
