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
    [Route("api/modules")]
    [ApiController]
    public class ModulesController : Controller {
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                var data = await db.Connection().GetAllAsync<Models.Core.Modules>();
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{key:int}")]
        public async Task<IActionResult> GetByFlag(int key) {
            try {
                var qry = $"select * from Modules where flag in({key})";
                var data = await db.Connection().QueryAsync<Models.Core.Modules>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{key}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from Modules where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.Modules>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("{id:int}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Get(int id) {
            try {
                var data = await db.Connection().GetAsync<Models.Core.Modules>(id);
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{code}"), Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult ExistCode(string code) {
            try {
                if (db.Connection().isExist("Modules", "code", code))
                    return Json(new { msg = "exist" });
                return Json(new { msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPost, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Post([FromBody] Models.Core.Modules data) {
            try {
                if (db.Connection().isExist("Modules", "code", data.code)) return Json(new { msg = "exist" });
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
        public async Task<IActionResult> Put([FromBody] Models.Core.Modules data) {
            try {
                var _data = await db.Connection().GetAsync<Models.Core.Modules>(data.id);
                if (_data != null) {
                    //_data.code = data.code;
                    _data.title = data.title;
                    _data.required_auth = data.required_auth;
                    _data.alias = data.alias;
                    _data.url = data.url;
                    _data.permissions = data.permissions;
                    _data.orders = data.orders;
                    _data.descs = data.descs;
                    _data.contents = data.contents;
                    _data.updated_by = TM.Core.HttpContext.Header();
                    _data.updated_at = DateTime.Now;
                    _data.flag = data.flag;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data) {
            try {
                var qry = "BEGIN ";
                foreach (var item in data)
                    qry += $"update Modules set flag={item.flag} where id='{item.id}';\r\n";
                qry += "END;";
                await db.Connection().QueryAsync("COMMIT");
                await db.Connection().QueryAsync(qry);
                return Json(new { msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Modules> data) {
            try {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpDelete("{id}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                await db.Connection().GetAllAsync<Models.Core.Modules>();
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }
    }
}