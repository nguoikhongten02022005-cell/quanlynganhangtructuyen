using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}