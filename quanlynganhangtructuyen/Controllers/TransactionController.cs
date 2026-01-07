using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Requests;
using System.Security.Claims;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/transaction")]
    [Authorize(Roles = "CUSTOMER")]
    public class TransactionController : BaseController
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
                return ThatBai("Vui lòng cung cấp số tài khoản.");

            try
            {
                var ketQua = await _transactionService.TraCuuTaiKhoanNhanAsync(accountNumber);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> TaoGiaoDichVoiOTP([FromBody] TaoGiaoDichRequest req)
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            if (req == null || string.IsNullOrWhiteSpace(req.ToAccount) || req.Amount <= 0 || string.IsNullOrWhiteSpace(req.Message))
                return ThatBai("Dữ liệu không hợp lệ.");

            try
            {
                var ketQua = await _transactionService.TaoGiaoDichVoiOTPAsync(maNguoiDung.Value, req.ToAccount, req.Amount, req.Message);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> XacNhanOTP([FromBody] XacNhanOTPRequest req)
        {
            if (req == null || req.MaGiaoDich <= 0 || string.IsNullOrWhiteSpace(req.MaOTP) || req.MaOTP.Length != 6)
                return ThatBai("Dữ liệu không hợp lệ.");

            try
            {
                var ketQua = await _transactionService.XacNhanOTPVaChuyenTienAsync(req.MaGiaoDich, req.MaOTP);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> LichSuGiaoDich([FromQuery] int pageSize = 20, [FromQuery] int pageNumber = 1)
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            if (pageSize <= 0 || pageSize > 100 || pageNumber <= 0)
                return ThatBai("Thông số phân trang không hợp lệ.");

            try
            {
                var ketQua = await _transactionService.LayLichSuGiaoDichAsync(maNguoiDung.Value, pageSize, pageNumber);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> LayChiTietGiaoDich(int id)
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            try
            {
                var giaoDich = await _transactionService.LayChiTietGiaoDichAsync(maNguoiDung.Value, id);
                return Ok(giaoDich);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpGet("received")]
        public async Task<IActionResult> LayGiaoDichNhanDuoc([FromQuery] int pageSize = 20, [FromQuery] int pageNumber = 1)
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            if (pageSize <= 0 || pageSize > 100 || pageNumber <= 0)
                return ThatBai("Thông số phân trang không hợp lệ.");

            try
            {
                var ketQua = await _transactionService.LayGiaoDichNhanDuocAsync(maNguoiDung.Value, pageSize, pageNumber);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }
    }
}
