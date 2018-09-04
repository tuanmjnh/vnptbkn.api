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
  public class ModulesController : Controller {
    private TM.Core.Connection.Oracle db;

    [HttpGet]
    public async Task<IActionResult> Get() {
      try {
        db = new TM.Core.Connection.Oracle();
        var qry = "select * from MODULES";
        var data = await db.Connection.QueryAsync(qry);
        db.Close();
        return Json(new { data = data, message = new { type = "success", text = "Lấy dữ liệu thành công!" } });
      } catch (System.Exception ex) {
        return Json(new { danger = ex.Message });
      }
    }
  }
}