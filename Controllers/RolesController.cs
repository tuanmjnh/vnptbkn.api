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
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
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

        [HttpPost, Microsoft.AspNetCore.Authorization.Authorize]
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

        [HttpPut, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Put([FromBody] Authentication.Core.Roles data) {
            try {
                var _data = await db.Connection().GetAsync<Authentication.Core.Roles>(data.id);
                if (_data != null) {
                    _data.name = data.name;
                    _data.orders = data.orders;
                    _data.descs = data.descs;
                    _data.updated_by = TM.Core.HttpContext.Header();
                    _data.updated_at = DateTime.Now;
                    _data.flag = data.flag;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("delete"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data) {
            try {
                var qry = "";
                foreach (var item in data)
                    qry += $"update roles set flag={item.flag} where id='{item.id}'";
                await db.Connection().QueryAsync(qry);
                return Json(new { msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Remove([FromBody] List<Authentication.Core.Roles> data) {
            try {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpDelete("{id}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                await db.Connection().GetAllAsync<Authentication.Core.Roles>();
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }
    }
}