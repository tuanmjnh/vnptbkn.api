using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using VNPTBKN.API.Common;
namespace VNPTBKN.API.Controllers {
    [Route("api/[controller]")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class ServicesController : Controller {
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                var data = await db.Connection().GetAllAsync<Models.Core.Groups>();
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp)
                    key += $"'{item}',";
                var qry = $"select * from groups where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.Groups>(qry);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id) {
            try {
                var data = await db.Connection().GetAsync<Models.Core.Groups>(id);
                return Json(new { data = id, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.Groups data) {
            try {
                data.created_by = TM.Core.HttpContext.Header();
                data.created_at = DateTime.Now;
                await db.Connection().InsertOraAsync(data);
                var qry = "select * from groups where group_id=(select max(group_id) from groups)";
                var _data = await db.Connection().QueryAsync<Models.Core.Groups>(qry);
                return Json(new { data = _data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.Groups data) {
            try {
                var _data = await db.Connection().GetAsync<Models.Core.Groups>(data.id);
                if (_data != null) {
                    _data.title = data.title;
                    _data.descs = data.descs;
                    _data.contents = data.contents;
                    _data.parent_id = data.parent_id;
                    _data.parents = data.parents;
                    _data.levels = data.levels;
                    _data.image = data.image;
                    _data.icon = data.icon;
                    _data.quantity = data.quantity;
                    _data.positions = data.positions;
                    _data.orders = data.orders;
                    _data.updated_by = TM.Core.HttpContext.Header();
                    _data.updated_at = DateTime.Now;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<Models.Core.Groups> data) {
            try {
                var _data = new List<Models.Core.Groups>();
                foreach (var item in data) {
                    var tmp = await db.Connection().GetAsync<Models.Core.Groups>(item.id);
                    if (tmp != null) {
                        tmp.flag = item.flag;
                        _data.Add(tmp);
                    }
                }
                if (_data.Count > 0) await db.Connection().UpdateAsync(_data);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Groups> data) {
            try {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                var data = await db.Connection().GetAsync<Models.Core.Groups>(id);
                await db.Connection().DeleteAsync(data);
                return Json(new { data = id, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }
    }
}