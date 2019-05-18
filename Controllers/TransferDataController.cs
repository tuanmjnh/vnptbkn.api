using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using VNPTBKN.API.Common;

namespace VNPTBKN.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class TransferDataController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    await db.Connection.GetAllAsync<Authentication.Core.Users>();
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost]
        public async Task<IActionResult> TransferDataPortal(string dataVal, string database = "SQL_Portal")
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var SQLServer = new TM.Core.Connection.SQLServer(database);
                    var Oracle = new TM.Core.Connection.Oracle("PORTAL");
                    var qry = $"SELECT * FROM {dataVal}";
                    var table = await SQLServer.Connection.QueryAsync<Authentication.Core.Users>(qry);
                    foreach (var i in table)
                    {
                        i.id = i.id.ToUpper();
                        i.full_name = string.IsNullOrEmpty(i.full_name) ? i.full_name : i.full_name;
                        i.created_by = string.IsNullOrEmpty(i.created_by) ? "Admin" : i.created_by;
                        i.created_at = i.created_at.HasValue ? i.created_at.Value : DateTime.Now;
                    }
                    Oracle.Connection.Insert(table);

                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost("TransferDataCuoc/{table}")]
        public async Task<IActionResult> TransferDataCuoc(string table, string database = "SQL_CUOC")
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var SQLServer = new TM.Core.Connection.SQLServer(database);
                    var Oracle = new TM.Core.Connection.Oracle("VNPTBK");
                    var qry = $"SELECT * FROM {table}";
                    var data = await SQLServer.Connection.QueryAsync<Models.Core.Groups>(qry);
                    Oracle.Connection.InsertOra(data);
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
    }
}