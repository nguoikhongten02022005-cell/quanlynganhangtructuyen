using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Requests;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    [Authorize(Roles = "CUSTOMER")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> TraCuuTaiKhoanNhan([FromQuery] string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BadRequest(new { thongBao = "Vui lòng cung cấp số tài khoản." });
            }

            try
            {
                var ketQua = await _transactionService.TraCuuTaiKhoanNhanAsync(accountNumber);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        /// <summary>
        /// Khởi tạo giao dịch chuyển tiền và sinh mã OTP
        /// </summary>
        [HttpPost("verify")]
        public async Task<IActionResult> TaoGiaoDichVoiOTP([FromBody] TaoGiaoDichRequest req)
        {
            // Validation input
            if (req == null)
            {
                return BadRequest(new { thongBao = "Dữ liệu không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(req.ToAccount))
            {
                return BadRequest(new { thongBao = "Vui lòng cung cấp số tài khoản nhận." });
            }

            if (req.Amount <= 0)
            {
                return BadRequest(new { thongBao = "Số tiền phải lớn hơn 0." });
            }

            if (string.IsNullOrWhiteSpace(req.Message))
            {
                return BadRequest(new { thongBao = "Vui lòng nhập nội dung chuyển khoản." });
            }

            // Lấy maNguoiDung từ JWT token
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out int maNguoiDung))
            {
                return Unauthorized(new { thongBao = "Token không hợp lệ." });
            }

            try
            {
                var ketQua = await _transactionService.TaoGiaoDichVoiOTPAsync(
                    maNguoiDung,
                    req.ToAccount,
                    req.Amount,
                    req.Message
                );
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        /// <summary>
        /// Xác nhận OTP và hoàn tất chuyển tiền
        /// </summary>
        [HttpPost("confirm")]
        public async Task<IActionResult> XacNhanOTP([FromBody] XacNhanOTPRequest req)
        {
            // Validation input
            if (req == null)
            {
                return BadRequest(new { thongBao = "Dữ liệu không hợp lệ." });
            }

            if (req.MaGiaoDich <= 0)
            {
                return BadRequest(new { thongBao = "Mã giao dịch không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(req.MaOTP) || req.MaOTP.Length != 6)
            {
                return BadRequest(new { thongBao = "Mã OTP phải có 6 chữ số." });
            }

            try
            {
                var ketQua = await _transactionService.XacNhanOTPVaChuyenTienAsync(
                    req.MaGiaoDich,
                    req.MaOTP
                );
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử giao dịch của người dùng
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> LichSuGiaoDich(
            [FromQuery] int pageSize = 20,
            [FromQuery] int pageNumber = 1)
        {
            // Validation
            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest(new { thongBao = "Kích thước trang phải từ 1 đến 100." });
            }

            if (pageNumber <= 0)
            {
                return BadRequest(new { thongBao = "Số trang phải lớn hơn 0." });
            }

            // Lấy maNguoiDung từ JWT token
            string? maNguoiDungStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(maNguoiDungStr) || !int.TryParse(maNguoiDungStr, out int maNguoiDung))
            {
                return Unauthorized(new { thongBao = "Token không hợp lệ." });
            }

            try
            {
                var ketQua = await _transactionService.LayLichSuGiaoDichAsync(
                    maNguoiDung,
                    pageSize,
                    pageNumber
                );
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}