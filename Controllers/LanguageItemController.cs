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
    [Route("api/language-items")]
    [ApiController]
    public class LanguageItemController : Controller {
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                var data = await db.Connection().GetAllAsync<Models.Core.LanguageItems>();
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("getlang/{key}")]
        public async Task<IActionResult> GetLang(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from Language_items where lower(lang_code) in({key.Trim(',').ToLower()})";
                var data = await db.Connection().QueryAsync<Models.Core.LanguageItems>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("getmodule/{key}")]
        public async Task<IActionResult> GetModule(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from Language_items where lower(module_code) in({key.Trim(',').ToLower()})";
                var data = await db.Connection().QueryAsync<Models.Core.LanguageItems>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("getkey/{key}")]
        public async Task<IActionResult> getKey(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from Language_items where lower(key) in({key.Trim(',').ToLower()})";
                var data = await db.Connection().QueryAsync<Models.Core.LanguageItems>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id) {
            try {
                var data = await db.Connection().GetAsync<Models.Core.LanguageItems>(id);
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPost, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Post([FromBody] Models.Core.LanguageItems data) {
            try {
                // var qry = $"select * from Language_items where lower(lang_code)='{data.lang_code.ToLower()}' and lower(module_code)='{data.module_code.ToLower()}' and lower(key)='{data.key.ToLower()}'";
                var qry = $"select * from Language_items where lower(lang_code)='{data.lang_code.ToLower()}'";
                var _data = await db.Connection().QueryFirstOrDefaultAsync<Models.Core.LanguageItems>(qry);
                if (_data != null) {
                    _data.lang_data = data.lang_data;
                    await db.Connection().UpdateAsync(_data);
                } else
                    await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) {
                return Json(new { msg = "danger" });
            }
        }

        [HttpPut, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Put([FromBody] Models.Core.LanguageItems data) {
            try {
                // var qry = $"select * from Language_items where lower(lang_code)='{data.lang_code.ToLower()}' and lower(module_code)='{data.module_code.ToLower()}' and lower(key)='{data.key.ToLower()}'";
                var qry = $"select * from Language_items where lower(lang_code)='{data.lang_code.ToLower()}'";
                var _data = await db.Connection().QueryFirstOrDefaultAsync<Models.Core.LanguageItems>(qry);
                if (_data != null) {
                    _data.lang_data = data.lang_data;
                    await db.Connection().UpdateAsync(_data);
                }
                return Json(new { data = _data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("delete"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data) {
            try {
                var qry = "";
                foreach (var item in data)
                    qry += $"delete Language_items where id='{item.lang_code}'";
                await db.Connection().QueryAsync(qry);
                return Json(new { msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.LanguageItems> data) {
            try {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpDelete("{id}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                await db.Connection().GetAllAsync<Models.Core.LanguageItems>();
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }
    }
}