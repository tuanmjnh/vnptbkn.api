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
    [Route("api/appkey")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class AppKeyController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Paging paging)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select * from app_key where flag in({paging.flag}) ";
                    if (paging.app_key != null && paging.app_key.Count > 0)
                        qry += $"and app_key in('{String.Join("','", paging.app_key)}') ";
                    qry += "order by orders";
                    return Json(new
                    {
                        data = await db.Connection.QueryAsync<Models.Core.APP_KEY>(qry),
                        msg = TM.Core.Common.Message.success.ToString()
                    });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]/{key:int}")]
        public async Task<IActionResult> GetByFlag(int key)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select * from APP_KEY where flag in({key})";
                    var data = await db.Connection.QueryAsync<Models.Core.APP_KEY>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetByKey([FromQuery] string key, string code)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select * from APP_KEY where flag in(1)";
                    if (!string.IsNullOrEmpty(key)) qry += $" and app_key in('{key}')";
                    if (!string.IsNullOrEmpty(code)) qry += $" and code in('{code}')";
                    var data = await db.Connection.QueryAsync<Models.Core.APP_KEY>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var data = await db.Connection.GetAsync<Models.Core.APP_KEY>(id);
                    return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]/{code}")]
        public IActionResult ExistCode(string code)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    if (db.Connection.isExist("APP_KEY", "code", code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.APP_KEY data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    data.flag = 1;
                    await db.Connection.InsertOraAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.APP_KEY data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var _data = await db.Connection.GetAsync<Models.Core.APP_KEY>(data.app_key);
                    if (_data != null)
                    {
                        _data.title = data.title;
                        _data.flag = data.flag;
                    }
                    await db.Connection.UpdateAsync(_data);
                    return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
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
                        qry += $"update APP_KEY set flag={item.flag},deleted_ip='{TM.Core.HttpContext.Header("LocalIP")}',deleted_by='{nd.ma_nd}',deleted_at={now.ParseDateTime()} where id='{item.id}';\r\n";
                    qry += "END;";
                    await db.Connection.QueryAsync(qry);
                    await db.Connection.QueryAsync("COMMIT");
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.APP_KEY> data)
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
            finally { }
        }
        public partial class Paging : TM.Core.Common.Paging
        {
            public List<string> app_key { get; set; }
        }
    }
}