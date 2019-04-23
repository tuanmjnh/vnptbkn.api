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
    [Route("api/languages")]
    [ApiController]
    public class LanguagesController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var data = await db.Connection().GetAllAsync<Models.Core.Languages>();
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{key}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query)
        {
            try
            {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from Languages where lower(code) in({key.Trim(',').ToLower()}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.Languages>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("{id:int}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var data = await db.Connection().GetAsync<Models.Core.Languages>(id);
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{code}"), Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult ExistCode(string code)
        {
            try
            {
                if (db.Connection().isExist("languages", "code", code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                return Json(new { msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Post([FromBody] Models.Core.Languages data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token });
                if (db.Connection().isExist("languages", "code", data.code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                data.id = Guid.NewGuid().ToString("N");
                data.created_by = nd.ma_nd;
                data.created_at = DateTime.Now;
                await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception)
            {
                return Json(new { msg = TM.Core.Common.Message.danger.ToString() });
            }
        }

        [HttpPut, Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Put([FromBody] Models.Core.Languages data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token });
                var _data = await db.Connection().GetAsync<Models.Core.Languages>(data.id);
                if (_data != null)
                {
                    _data.code = data.code;
                    _data.title = data.title;
                    _data.icon = data.icon;
                    _data.attach = data.attach;
                    _data.descs = data.descs;
                    _data.orders = data.orders;
                    _data.updated_by = nd.ma_nd;
                    _data.updated_at = DateTime.Now;
                    _data.flag = data.flag;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token });
                var qry = "BEGIN ";
                foreach (var item in data)
                    qry += $"update Languages set flag={item.flag},deleted_by='{nd.ma_nd}',deleted_at={DateTime.Now.ParseDateTime()} where id='{item.id}';\r\n";
                qry += "END;";
                await db.Connection().QueryAsync(qry);
                await db.Connection().QueryAsync("COMMIT");
                return Json(new { msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Languages> data)
        {
            try
            {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpDelete("{id}"), Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> RemoveOne(int id)
        {
            try
            {
                await db.Connection().GetAllAsync<Models.Core.Languages>();
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
    }
}