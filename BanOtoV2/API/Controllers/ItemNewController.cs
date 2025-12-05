using Microsoft.AspNetCore.Mvc;
using Model;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemNewController : Controller
    {
       private  string constring;
        public ItemNewController(IConfiguration configuration)
        {
            constring = configuration["ConnectionStrings:DefaultConnection"];
        }

        [Route("get-by-id/{id}")]
        [HttpGet]
        public ItemModel GetDatabyID(string id)
        {
            
            SqlConnection con = new SqlConnection(constring);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Select * from item where item_id='" + id + "'";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable tb = new DataTable();
            da.Fill(tb);
            ItemModel tg = new ItemModel();
            for (int i = 0; i < tb.Rows.Count; ++i)
            {
                tg.item_name = tb.Rows[i]["item_name"].ToString();
                tg.item_id = tb.Rows[i]["item_id"].ToString();
                tg.item_group_id = tb.Rows[i]["item_group_id"].ToString();
                break;
            }
            return tg;
        }
    }
}
