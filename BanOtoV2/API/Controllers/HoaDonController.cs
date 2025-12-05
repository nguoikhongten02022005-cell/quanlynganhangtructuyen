using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private IHoaDonBusiness _hoaDonBusiness;
        public HoaDonController(IHoaDonBusiness hoaDonBusiness)
        {
            _hoaDonBusiness = hoaDonBusiness;
        }

        [Route("create-hoa-don")]
        [HttpPost]
        public HoaDonModel CreateItem([FromBody] HoaDonModel model)
        {
            _hoaDonBusiness.Create(model);
            return model;
        }

        [Route("update-hoa-don")]
        [HttpPost]
        public HoaDonModel UpdateItem([FromBody] HoaDonModel model)
        {
            _hoaDonBusiness.Update(model);
            return model;
        }

        [Route("search")]
        [HttpPost]
        public ResponseModel Search([FromBody] Dictionary<string, object> formData)
        {
            var response = new ResponseModel();
            try
            {
                var page = int.Parse(formData["page"].ToString());
                var pageSize = int.Parse(formData["pageSize"].ToString());
                string hoten = "";
                if (formData.Keys.Contains("hoten") && !string.IsNullOrEmpty(Convert.ToString(formData["hoten"]))) { hoten = Convert.ToString(formData["hoten"]); }
                string diachi = "";
                if (formData.Keys.Contains("diachi") && !string.IsNullOrEmpty(Convert.ToString(formData["diachi"]))) { diachi = Convert.ToString(formData["diachi"]); }
                long total = 0;
                var data = _hoaDonBusiness.Search(page, pageSize, out total, hoten, diachi);
                response.TotalItems = total;
                response.Data = data;
                response.Page = page;
                response.PageSize = pageSize;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return response;
        }

        [Route("get-by-id/{id}")]
        [HttpGet]
        public HoaDonModel GetDatabyID(string id)
        {
            var xxx = _hoaDonBusiness.GetDatabyID(id); ;
            return _hoaDonBusiness.GetDatabyID(id);
        }

        [Route("delete")]
        [HttpPost]
        public IActionResult DeleteUser([FromBody] Dictionary<string, object> formData)
        {
            string ma_hoa_don = "";
            if (formData.Keys.Contains("ma_hoa_don") && !string.IsNullOrEmpty(Convert.ToString(formData["ma_hoa_don"]))) { ma_hoa_don = Convert.ToString(formData["ma_hoa_don"]); }
            _hoaDonBusiness.Delete(ma_hoa_don);
            return Ok();
        }
    }
}
