using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "ADMIN")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _dichVuNguoiDung;

        public UserController(IUserService dichVuNguoiDung)
        {
            _dichVuNguoiDung = dichVuNguoiDung;
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSachNguoiDung([FromQuery] string? vaiTro, [FromQuery] string? trangThai)
        {
            var ketQua = await _dichVuNguoiDung.GetUsersAsync(vaiTro, trangThai);
            return Ok(ketQua);
        }
    }
}