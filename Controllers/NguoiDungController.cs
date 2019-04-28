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
    [Route("api/nguoidung")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class NguoiDungController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var qry = $"select db.*,nd.roles_id,r.name roles_name,r.color from ttkd_bkn.db_nguoidung db,ttkd_bkn.nguoidung nd,ttkd_bkn.roles r where db.nguoidung_id=nd.nguoidung_id(+) and nd.roles_id=r.id(+)";
                var data = await db.Connection().QueryAsync<Authentication.Core.DBnguoidungRoles>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var qry = $"select db.*,nd.*,r.roles from ttkd_bkn.db_nguoidung db,ttkd_bkn.nguoidung nd,ttkd_bkn.roles r where db.nguoidung_id=nd.nguoidung_id(+) and nd.roles_id=r.id(+) and db.nguoidung_id={id}";
                var data = await db.Connection().QueryFirstOrDefaultAsync<Authentication.Core.nguoidung_auth>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{id:int}")]
        public async Task<IActionResult> GetByDonvi(int id)
        {
            try
            {
                var qry = $"select * from db_nguoidung where donvi_id={id}";
                var data = await db.Connection().QueryFirstOrDefaultAsync<Authentication.Core.DBNguoidung>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        [HttpPut("[action]/{id:int}")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var matkhau = "vnptbkn@123";
                var _data = await db.Connection().GetAsync<Authentication.Core.nguoidung>(id);
                if (_data != null)
                {
                    // _data.app_key = data.app_key;
                    _data.matkhau = TM.Core.Encrypt.MD5.CryptoMD5TM(matkhau + _data.salt);
                    _data.change_pass_by = nd.ma_nd;
                    _data.change_pass_at = DateTime.Now;
                    await db.Connection().UpdateAsync(_data);
                }
                else
                {
                    var tmp = new Authentication.Core.nguoidung();
                    tmp.nguoidung_id = id;
                    tmp.salt = Guid.NewGuid().ToString("N");
                    tmp.matkhau = TM.Core.Encrypt.MD5.CryptoMD5TM(matkhau + tmp.salt);
                    tmp.change_pass_by = nd.ma_nd;
                    tmp.change_pass_at = DateTime.Now;
                    await db.Connection().InsertOraAsync(tmp);
                }
                return Json(new { data = matkhau, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SetRoles([FromBody] List<Authentication.Core.nguoidung_role> data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var nguoidung = await db.Connection().GetAllAsync<Authentication.Core.nguoidung>();
                var index = 0;
                var qry = "BEGIN ";
                foreach (Authentication.Core.nguoidung_role item in data)
                {
                    // var _data = await db.Connection().GetAsync<Authentication.Core.nguoidung>(item.nguoidung_id);
                    if (nguoidung.Any(x => x.nguoidung_id == item.nguoidung_id))
                    {
                        qry += $"update nguoidung set roles_id='{item.roles_id}' where nguoidung_id={item.nguoidung_id};\r\n";
                        index++;
                    }
                    else
                    {
                        var matkhau = "vnptbkn@123";
                        var tmp = new Authentication.Core.nguoidung();
                        tmp.nguoidung_id = item.nguoidung_id;
                        tmp.salt = Guid.NewGuid().ToString("N");
                        tmp.matkhau = TM.Core.Encrypt.MD5.CryptoMD5TM(matkhau + tmp.salt);
                        tmp.updated_by = nd.ma_nd;
                        tmp.updated_at = DateTime.Now;
                        tmp.roles_id = item.roles_id;
                        await db.Connection().InsertOraAsync(tmp);
                    }
                }
                qry += "END;";
                if (index > 0)
                {
                    await db.Connection().QueryAsync(qry);
                    await db.Connection().QueryAsync("COMMIT");
                }
                return Json(new { msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
    }
}