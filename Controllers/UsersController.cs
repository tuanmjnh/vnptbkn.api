using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using VNPTBKN.API.Common;

namespace VNPTBKN.API.Controllers {
  [Route("api/users")]
  [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
  public class UsersController : Controller {
    [HttpGet]
    public async Task<IActionResult> Get() {
      try {
        var data = await db.Connection().GetAllAsync<Authentication.Core.Users>();
        return Json(new { data = data, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) {
      try {
        var data = await db.Connection().GetAsync<Authentication.Core.Users>(id);
        return Json(new { data = id, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Authentication.Core.Users data) {
      try {
        if (db.Connection().isExist("users", "username", data.username))
          return Json(new { message = "exist" });
        data.user_id = Guid.NewGuid().ToString("N");
        data.created_by = TM.Core.HttpContext.Header();
        data.created_at = DateTime.Now;
        await db.Connection().InsertOraAsync(data);
        return Json(new { data = data, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Authentication.Core.Users data) {
      try {
        var _data = await db.Connection().GetAsync<Authentication.Core.Users>(data.user_id);
        if (_data != null) {
          // _data.app_key = data.app_key;
          _data.full_name = data.full_name;
          _data.mobile = data.mobile;
          _data.email = data.email;
          _data.address = data.address;
          _data.descs = data.descs;
          _data.images = data.images;
          _data.donvi_id = data.donvi_id;
          _data.roles_id = data.roles_id;
          _data.updated_by = TM.Core.HttpContext.Header();
          _data.updated_at = DateTime.Now;
        }
        await db.Connection().UpdateAsync(_data);
        return Json(new { data = _data, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> Delete([FromBody] List<Authentication.Core.Users> data) {
      try {
        var qry = "BEGIN ";
        foreach (var item in data)
          qry += $"update Users set flag={item.flag} where id='{item.user_id}';\r\n";
        qry += "END;";
        await db.Connection().QueryAsync(qry);
        await db.Connection().QueryAsync("COMMIT");
        return Json(new { msg = "success" });
      } catch (System.Exception) { return Json(new { msg = "danger" }); }
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> Remove([FromBody] List<Authentication.Core.Users> data) {
      try {
        if (data.Count > 0) await db.Connection().DeleteAsync(data);
        return Json(new { data = data, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveOne(int id) {
      try {
        var data = await db.Connection().GetAsync<Authentication.Core.Users>(id);
        await db.Connection().DeleteAsync(data);
        return Json(new { data = id, message = "success" });
      } catch (System.Exception) { return Json(new { message = "danger" }); }
    }
  }
}