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
namespace VNPTBKN.API.Controllers
{
    [Route("api/Groups")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class GroupsController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Paging paging)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    // var data = await db.Connection.GetAllAsync<Models.Core.Groups>();
                    // return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString()});
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    // Query
                    var qry = "app_key,code,dependent,levels,title,icon,image,url,orders,quantity,position,descs,content,tags,";
                    qry += "to_char(start_at,'dd/MM/yyyy')start_at,to_char(end_at,'dd/MM/yyyy')end_at,";
                    qry += "created_by,to_char(created_at,'dd/MM/yyyy')created_at,created_ip,";
                    qry += "updated_by,to_char(updated_at,'dd/MM/yyyy')updated_at,updated_ip,";
                    qry += "deleted_by,to_char(deleted_at,'dd/MM/yyyy')deleted_at,deleted_ip,flag";
                    qry = $"select {(paging.is_export ? qry : "*")} from Groups where flag in({paging.flag})";
                    // Extras
                    if (!string.IsNullOrEmpty(paging.app_key))
                        qry += $" and app_key in('{paging.app_key}')";
                    if (!string.IsNullOrEmpty(paging.dependent))
                        qry += $" and dependent like('%,{paging.dependent},%')";
                    // Search
                    if (!string.IsNullOrEmpty(paging.search))
                        qry += $@" and (or CONVERTTOUNSIGN(title) like CONVERTTOUNSIGN('%{paging.search}%'))";
                    // Paging Params
                    if (paging.is_export) paging.rowsPerPage = 0;
                    var param = new Dapper.Oracle.OracleDynamicParameters("v_data");
                    param.Add("v_sql", qry);
                    param.Add("v_offset", paging.page);
                    param.Add("v_limmit", paging.rowsPerPage);
                    param.Add("v_order", paging.sortBy);
                    param.Add("v_total", 0);
                    if (paging.is_export) // Export data
                        return Json(new
                        {
                            data = await db.Connection.QueryAsync("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                            total = param.Get<int>("v_total"),
                            msg = TM.Core.Common.Message.success.ToString()
                        });
                    else // View data
                        return Json(new
                        {
                            data = await db.Connection.QueryAsync<Models.Core.Groups>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                            total = param.Get<int>("v_total"),
                            msg = TM.Core.Common.Message.success.ToString()
                        });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{key:int}")]
        public async Task<IActionResult> GetByFlag(int key)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select * from Groups where flag in({key})";
                    var data = await db.Connection.QueryAsync<Models.Core.Groups>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByKey([FromQuery] string key, string code)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select * from Groups where flag in(1)";
                    if (!string.IsNullOrEmpty(key)) qry += $" and app_key in('{key}')";
                    if (!string.IsNullOrEmpty(code)) qry += $" and code in('{code}')";
                    qry += " order by orders";
                    var data = await db.Connection.QueryAsync<Models.Core.Groups>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var data = await db.Connection.GetAsync<Models.Core.Groups>(id);
                    return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{code}")]
        public IActionResult ExistCode(string code)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    if (db.Connection.isExist("Groups", "code", code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.Groups data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    // if (db.Connection.isExist("Groups", "code", data.code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    data.created_by = nd.ma_nd;
                    data.created_at = DateTime.Now;
                    data.created_ip = TM.Core.HttpContext.Header("LocalIP");
                    data.flag = 1;
                    await db.Connection.InsertOraAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.Groups data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var _data = await db.Connection.GetAsync<Models.Core.Groups>(data.id);
                    if (_data != null)
                    {
                        _data.app_key = data.app_key;
                        _data.code = data.code;
                        _data.dependent = data.dependent;
                        _data.levels = data.levels;
                        _data.title = data.title;
                        _data.icon = data.icon;
                        _data.image = data.image;
                        _data.url = data.url;
                        _data.orders = data.orders;
                        _data.quantity = data.quantity;
                        _data.descs = data.descs;
                        _data.content = data.content;
                        _data.tags = data.tags;
                        _data.attach = data.attach;
                        _data.start_at = data.start_at;
                        _data.end_at = data.end_at;
                        _data.updated_by = nd.ma_nd;
                        _data.updated_at = DateTime.Now;
                        _data.updated_ip = TM.Core.HttpContext.Header("LocalIP");
                        _data.flag = data.flag;
                    }
                    await db.Connection.UpdateAsync(_data);
                    return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var now = DateTime.Now;
                    var qry = "BEGIN ";
                    foreach (var item in data)
                        qry += $"update Groups set flag={item.flag},deleted_ip='{TM.Core.HttpContext.Header("LocalIP")}',deleted_by='{nd.ma_nd}',deleted_at={now.ParseDateTime()} where id='{item.id}';\r\n";
                    qry += "END;";
                    await db.Connection.QueryAsync(qry);
                    await db.Connection.QueryAsync("COMMIT");
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Groups> data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    if (data.Count > 0) await db.Connection.DeleteAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        public partial class Paging : TM.Core.Common.Paging
        {
            public string app_key { get; set; }
            public string dependent { get; set; }
        }
    }
}