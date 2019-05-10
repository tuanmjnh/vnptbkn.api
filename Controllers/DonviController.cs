using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using VNPTBKN.API.Common;

namespace VNPTBKN.API.Controllers
{
    [Route("api/donvi")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class DonviController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Paging paging)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var qry = "select db.ma_dvi,dv.donvi_id,db.donvi_ql_id,db.ma_dv,dv.ten_dv,db.ten_dv_ql,db.sdt,db.stk ";
                qry += "from admin_bkn.donvi dv,ttkd_bkn.db_donvi db ";
                qry += "where dv.donvi_id=db.donvi_id(+) and dv.donvi_cha_id=5494 and dv.muc_id=3 ";
                if (paging.ma_dvi > 0) qry += " and db.ma_dvi>0";
                qry += "order by db.ma_dvi";
                var data = await db.Connection().QueryAsync<Models.Core.DBDonvi>(qry); // await db.Connection().GetAllAsync<Models.Core.DBDonvi>();
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var data = await db.Connection().GetAsync<Models.Core.DBDonvi>(id);
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.DBDonvi data)
        {
            try
            {
                await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.DBDonvi data)
        {
            try
            {
                var _data = await db.Connection().GetAsync<Models.Core.DBDonvi>(data.donvi_id);
                if (_data != null)
                {
                    // _data.app_key = data.app_key;
                    // _data.full_name = data.full_name;
                    // _data.mobile = data.mobile;
                    // _data.email = data.email;
                    // _data.address = data.address;
                    // _data.descs = data.descs;
                    // _data.images = data.images;
                    // _data.donvi_id = data.donvi_id;
                    // _data.roles_id = data.roles_id;
                    // _data.updated_by = TM.Core.HttpContext.Header();
                    // _data.updated_at = DateTime.Now;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        // [HttpPut("[action]")]
        // public async Task<IActionResult> Delete([FromBody] List<Models.Core.DBDonvi> data) {
        //   try {
        //     var qry = "BEGIN ";
        //     foreach (var item in data)
        //       qry += $"update Users set flag={item.flag} where id='{item.user_id}';\r\n";
        //     qry += "END;";
        //     await db.Connection().QueryAsync(qry);
        //     return Json(new { msg = "success" });
        //   } catch (System.Exception) { return Json(new {msg = TM.Core.Common.Message.danger.ToString() }); }
        // }

        // [HttpPut("[action]")]
        // public async Task<IActionResult> Remove([FromBody] List<Models.Core.DBDonvi> data) {
        //   try {
        //     if (data.Count > 0) await db.Connection().DeleteAsync(data);
        //     return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
        //   } catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        // }
        public partial class Paging : TM.Core.Common.Paging
        {
            public int ma_dvi { get; set; }
        }
    }
}