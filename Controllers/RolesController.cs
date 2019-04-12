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
namespace VNPTBKN.API.Controllers {
    [Route("api/roles")]
    [ApiController]
    public class RolesController : Controller {
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                var data = await db.Connection().GetAllAsync<Authentication.Core.Roles>();
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id) {
            try {
                var data = await db.Connection().GetAsync<Authentication.Core.Roles>(id);
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Authentication.Core.Roles data) {
            try {
                data.id = Guid.NewGuid().ToString("N");
                data.created_by = TM.Core.HttpContext.Header();
                data.created_at = DateTime.Now;
                await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) {
                return Json(new { msg = "danger" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Authentication.Core.Roles data) {
            try {
                var _data = await db.Connection().GetAsync<Authentication.Core.Roles>(data.id);
                if (_data != null) {
                    _data.name = data.name;
                    _data.roles = data.roles;
                    _data.orders = data.orders;
                    _data.descs = data.descs;
                    _data.updated_by = TM.Core.HttpContext.Header();
                    _data.updated_at = DateTime.Now;
                    _data.flag = data.flag;
                    _data.color = data.color;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data) {
            try {
                var qry = "BEGIN ";
                foreach (var item in data)
                    qry += $"update roles set flag={item.flag} where id='{item.id}';\r\n";
                qry += "END;";
                await db.Connection().QueryAsync(qry);
                await db.Connection().QueryAsync("COMMIT");
                return Json(new { msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Authentication.Core.Roles> data) {
            try {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                await db.Connection().GetAllAsync<Authentication.Core.Roles>();
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }
    }
}