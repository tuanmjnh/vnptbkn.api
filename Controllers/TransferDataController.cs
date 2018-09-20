using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace VNPTBKN.API.Controllers {
    [Route("api/[controller]")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class TransferDataController : Controller {
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                return Json(new { success = "Lấy dữ liệu thành công!" });
            } catch (System.Exception ex) {
                return Json(new { danger = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TransferDataPortal(string dataVal, string database = "SQL_Portal") {
            try {
                var SQLServer = new TM.Core.Connection.SQLServer(database);
                var Oracle = new TM.Core.Connection.Oracle("PORTAL");
                var qry = $"SELECT * FROM {dataVal}";
                var table = await SQLServer.Connection.QueryAsync<Authentication.Core.Users>(qry);
                foreach (var i in table) {
                    i.user_id = i.user_id.ToUpper();
                    i.full_name = string.IsNullOrEmpty(i.full_name) ? i.full_name : i.full_name;
                    i.created_by = string.IsNullOrEmpty(i.created_by) ? "Admin" : i.created_by;
                    i.created_at = i.created_at.HasValue ? i.created_at.Value : DateTime.Now;
                }
                Oracle.Connection.Insert(table);

                return Json(new { success = "Cập nhật thành công" });
            } catch (System.Exception ex) {
                return Json(new { danger = ex.Message });
            }
        }

        [HttpPost("TransferDataCuoc/{table}")]
        public async Task<IActionResult> TransferDataCuoc(string table, string database = "SQL_CUOC") {
            try {
                var SQLServer = new TM.Core.Connection.SQLServer(database);
                var Oracle = new TM.Core.Connection.Oracle("VNPTBK");
                var qry = $"SELECT * FROM {table}";
                var data = await SQLServer.Connection.QueryAsync<Models.Core.Groups>(qry);
                Oracle.Connection.InsertOra(data);

                return Json(new { success = "Cập nhật thành công" });
            } catch (System.Exception ex) {
                return Json(new { danger = ex.Message });
            }
        }
    }
}