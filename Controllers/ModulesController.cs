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
  public class ModulesController : Controller {
    [HttpGet]
    public async Task<IActionResult> Get() {
      try {
        var data = await db.Connection().GetAllAsync<Authentication.Core.Modules>();
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
        var qry = $"select * from modules where app_key in({key.Trim(',')}) and flag={query.flag}";
        var data = await db.Connection().QueryAsync<Authentication.Core.Modules>(qry);
        return Json(new { data = data, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) {
      try {
        var data = await db.Connection().GetAsync<Authentication.Core.Modules>(id);
        return Json(new { data = id, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Authentication.Core.Modules data) {
      try {
        if (db.Connection().isExist("modules", "app_key", data.app_key))
          return Json(new { message = "exist" });
        data.created_by = TM.Core.HttpContext.Header();
        data.created_at = DateTime.Now;
        await db.Connection().InsertOraAsync(data);
        var qry = "select * from modules where modules_id=(select max(modules_id) from modules)";
        var _data = await db.Connection().QueryAsync<Authentication.Core.Modules>(qry);
        return Json(new { data = _data, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Authentication.Core.Modules data) {
      try {
        var _data = await db.Connection().GetAsync<Authentication.Core.Modules>(data.modules_id);
        if (_data != null) {
          // _data.app_key = data.app_key;
          _data.name = data.name;
          _data.icon = data.icon;
          _data.descs = data.descs;
          _data.url = data.url;
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
    public async Task<IActionResult> Delete([FromBody] List<Authentication.Core.Modules> data) {
      try {
        var _data = new List<Authentication.Core.Modules>();
        foreach (var item in data) {
          var tmp = await db.Connection().GetAsync<Authentication.Core.Modules>(item.modules_id);
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
    public async Task<IActionResult> Remove([FromBody] List<Authentication.Core.Modules> data) {
      try {
        if (data.Count > 0) await db.Connection().DeleteAsync(data);
        return Json(new { data = data, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveOne(int id) {
      try {
        var data = await db.Connection().GetAsync<Authentication.Core.Modules>(id);
        await db.Connection().DeleteAsync(data);
        return Json(new { data = id, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }
  }
}