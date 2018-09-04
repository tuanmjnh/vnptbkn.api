using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace VNPTBKN.API.Controllers {
    [Route("api/[controller]")]
    [ApiController] //, Microsoft.AspNetCore.Authorization.Authorize
    public class ServicesController : Controller {
        private TM.Core.Connection.Oracle db;
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                db = new TM.Core.Connection.Oracle();
                var data = await db.Connection.GetAllAsync<Models.Core.Groups>();
                db.Close();
                return Json(new { data = data, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                db = new TM.Core.Connection.Oracle();
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp)
                    key += $"'{item}',";
                var qry = $"select * from groups where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection.QueryAsync<Models.Core.Groups>(qry);
                db.Close();
                return Json(new { data = data, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id) {
            try {
                return Json(new { data = id, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.Groups data) {
            try {
                db = new TM.Core.Connection.Oracle();
                data.created_by = TM.Core.HttpContext.Header();
                data.created_at = DateTime.Now;
                db.Connection.InsertOra(data);
                var qry = "select * from groups where group_id=(select max(group_id) from groups)";
                var _data = await db.Connection.QueryAsync<Models.Core.Groups>(qry);
                return Json(new { data = _data, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.Groups data) {
            try {
                db = new TM.Core.Connection.Oracle();
                var _data = await db.Connection.GetAsync<Models.Core.Groups>(data.group_id);
                if (_data != null) {
                    _data.title = data.title;
                    _data.descriptions = data.descriptions;
                    _data.parent_id = data.parent_id;
                    _data.parents_id = data.parents_id;
                    _data.levels = data.levels;
                    _data.images = data.images;
                    _data.icons = data.icons;
                    _data.quantity = data.quantity;
                    _data.positions = data.positions;
                    _data.orders = data.orders;
                    _data.updated_by = TM.Core.HttpContext.Header();
                    _data.updated_at = DateTime.Now;
                }
                await db.Connection.UpdateAsync(_data);
                return Json(new { data = _data, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<Models.Core.Groups> data) {
            try {
                db = new TM.Core.Connection.Oracle();
                var _data = new List<Models.Core.Groups>();
                foreach (var item in data) {
                    var tmp = await db.Connection.GetAsync<Models.Core.Groups>(item.group_id);
                    if (tmp != null) {
                        tmp.flag = item.flag;
                        _data.Add(tmp);
                    }
                }
                if (_data.Count > 0) await db.Connection.UpdateAsync(_data);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Groups> data) {
            try {
                db = new TM.Core.Connection.Oracle();
                if (data.Count > 0) await db.Connection.DeleteAsync(data);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                return Json(new { data = id, message = "success" });
            } catch (System.Exception ex) { return Json(new { message = "danger" }); }
        }
    }
}