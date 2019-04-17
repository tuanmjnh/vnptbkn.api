using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VNPTBKN.API.Common;
namespace VNPTBKN.API.Controllers
{
    [Route("api/permissions")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class PermissionsController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var data = await db.Connection().GetAllAsync<Models.Core.Permissions>();
                return Json(new { data = data, msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query)
        {
            try
            {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from Permissions where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.Permissions>(qry);
                return Json(new { data = data, msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var data = await db.Connection().GetAsync<Models.Core.Permissions>(id);
                return Json(new { data = id, msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{code}")]
        public IActionResult ExistCode(string code)
        {
            try
            {
                if (db.Connection().isExist("Permissions", "code", code))
                    return Json(new { msg = "exist" });
                return Json(new { msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.Permissions data)
        {
            try
            {
                if (db.Connection().isExist("Permissions", "code", data.code)) return Json(new { msg = "exist" });
                // data.id = Guid.NewGuid().ToString("N");
                data.created_by = TM.Core.HttpContext.Header();
                data.created_at = DateTime.Now;
                await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, msg = "success" });
            }
            catch (System.Exception)
            {
                return Json(new { msg = "danger" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.Permissions data)
        {
            try
            {
                var _data = await db.Connection().GetAsync<Models.Core.Permissions>(data.id);
                if (_data != null)
                {
                    //_data.code = data.code;
                    _data.title = data.title;
                    _data.orders = data.orders;
                    _data.descs = data.descs;
                    _data.updated_by = TM.Core.HttpContext.Header();
                    _data.updated_at = DateTime.Now;
                    _data.flag = data.flag;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data)
        {
            try
            {
                var delete_by = TM.Core.HttpContext.Header();
                var delete_at = DateTime.Now;
                var qry = "BEGIN ";
                foreach (var item in data)
                    qry += $"update Permissions set flag={item.flag} where id='{item.id}';\r\n";
                qry += "END;";
                await db.Connection().QueryAsync(qry);
                await db.Connection().QueryAsync("COMMIT");
                return Json(new { msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Permissions> data)
        {
            try
            {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOne(int id)
        {
            try
            {
                await db.Connection().GetAllAsync<Models.Core.Permissions>();
                return Json(new { data = id, msg = "success" });
            }
            catch (System.Exception) { return Json(new { msg = "danger" }); }
        }
    }
}