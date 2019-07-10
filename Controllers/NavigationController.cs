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
    [Route("api/navigation")]
    [ApiController]
    public class NavigationController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = "select * from ttkd_bkn.Navigation";
                    var data = await db.Connection.QueryAsync<Models.Core.Navigation>(qry);
                    return Json(data);
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
                    var qry = $"select * from ttkd_bkn.Navigation where flag in({key})";
                    var data = await db.Connection.QueryAsync<Models.Core.Navigation>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]/{key}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var tmp = key.Trim(',').Split(',');
                    key = "";
                    foreach (var item in tmp) key += $"'{item}',";
                    var qry = $"select * from ttkd_bkn.Navigation where app_key in({key.Trim(',')}) and flag={query.flag}";
                    var data = await db.Connection.QueryAsync<Models.Core.Navigation>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("{id:int}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var data = await db.Connection.GetAsync<Models.Core.Navigation>(id);
                    return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]/{code}"), Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult ExistCode(string code)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    if (db.Connection.isExist("ttkd_bkn.Navigation", "code", code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPost, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Post([FromBody] Models.Core.Navigation data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    if (db.Connection.isExist("ttkd_bkn.Navigation", "code", data.code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    data.created_by = nd.ma_nd;
                    data.created_at = DateTime.Now;
                    data.flag = 1;
                    await db.Connection.InsertOraAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPut, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Put([FromBody] Models.Core.Navigation data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var _data = await db.Connection.GetAsync<Models.Core.Navigation>(data.id);
                    if (_data != null)
                    {
                        _data.app_key = data.app_key;
                        _data.dependent = data.dependent;
                        _data.levels = data.levels;
                        _data.title = data.title;
                        _data.icon = data.icon;
                        _data.image = data.image;
                        _data.url = data.url;
                        _data.url_plus = data.url_plus;
                        _data.push = data.push;
                        _data.go = data.go;
                        _data.store = data.store;
                        _data.orders = data.orders;
                        _data.descs = data.descs;
                        _data.updated_by = nd.ma_nd;
                        _data.updated_at = DateTime.Now;
                        _data.flag = data.flag;
                    }
                    await db.Connection.UpdateAsync(_data);
                    return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var qry = "BEGIN ";
                    foreach (var item in data)
                        qry += $"update ttkd_bkn.Navigation set flag={item.flag},deleted_by='{nd.ma_nd}',deleted_at={DateTime.Now.ParseDateTime()} where id='{item.id}';\r\n";
                    qry += "END;";
                    await db.Connection.QueryAsync(qry);
                    await db.Connection.QueryAsync("COMMIT");
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Navigation> data)
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

        [HttpDelete("{id}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> RemoveOne(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    await db.Connection.GetAllAsync<Models.Core.Navigation>();
                    return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
    }
}