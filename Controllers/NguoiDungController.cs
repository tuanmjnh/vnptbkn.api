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
        public async Task<IActionResult> Get([FromQuery] Paging paging)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    // var data = await db.Connection.GetAllAsync<Models.Core.Items>();
                    // return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString()});
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    // Query
                    var qry = "";
                    if (paging.is_export)
                    {
                        qry = "select db.* ";
                        qry += "from ttkd_bkn.db_nguoidung db,ttkd_bkn.nguoidung nd,ttkd_bkn.roles r ";
                        qry += $"where db.nguoidung_id=nd.nguoidung_id(+) and nd.roles_id=r.id(+) and db.trangthai in({paging.flag})";
                    }
                    else
                    {
                        qry = "select db.*,nd.roles_id,r.name roles_name,r.color ";
                        qry += "from ttkd_bkn.db_nguoidung db,ttkd_bkn.nguoidung nd,ttkd_bkn.roles r ";
                        qry += $"where db.nguoidung_id=nd.nguoidung_id(+) and nd.roles_id=r.id(+) and db.trangthai in({paging.flag})";
                    }
                    // Đơn vị

                    if (nd.inRoles("donvi.select"))
                    {
                        if (paging.donvi_id != null && paging.donvi_id.Count > 0)
                            qry += $" and db.donvi_id in({String.Join(",", paging.donvi_id)})";
                        else qry += $" and db.donvi_id in(-1)";
                    }
                    else
                        qry += $" and db.donvi_id in({nd.donvi_id})";

                    // Search
                    if (!string.IsNullOrEmpty(paging.search))
                    {
                        qry += $@" and (CONVERTTOUNSIGN(tb.ma_tb) like CONVERTTOUNSIGN('%{paging.search}%')";
                        qry += $@" or CONVERTTOUNSIGN(tb.diachi_tb) like CONVERTTOUNSIGN('%{paging.search}%')";
                        qry += $@" or CONVERTTOUNSIGN(tb.so_dt) like CONVERTTOUNSIGN('%{paging.search}%')";
                        qry += $@" or CONVERTTOUNSIGN(tb.ma_nd) like CONVERTTOUNSIGN('%{paging.search}%'))";
                    }
                    // Paging Params
                    if (paging.is_export)
                    {
                        paging.rowsPerPage = 0;
                        // paging.sortBy="ten_dv";
                    }
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
                            data = await db.Connection.QueryAsync<Authentication.Core.DBnguoidungRoles>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                            total = param.Get<int>("v_total"),
                            msg = TM.Core.Common.Message.success.ToString()
                        });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            // try
            // {
            //     var qry = $"select db.*,nd.roles_id,r.name roles_name,r.color from ttkd_bkn.db_nguoidung db,ttkd_bkn.nguoidung nd,ttkd_bkn.roles r where db.nguoidung_id=nd.nguoidung_id(+) and nd.roles_id=r.id(+)";
            //     var data = await db.Connection.QueryAsync<Authentication.Core.DBnguoidungRoles>(qry);
            //     return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            // }
            // catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select db.*,nd.*,r.roles from ttkd_bkn.db_nguoidung db,ttkd_bkn.nguoidung nd,ttkd_bkn.roles r where db.nguoidung_id=nd.nguoidung_id(+) and nd.roles_id=r.id(+) and db.nguoidung_id={id}";
                    var data = await db.Connection.QueryFirstOrDefaultAsync<Authentication.Core.nguoidung_auth>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]/{id:int}")]
        public async Task<IActionResult> GetByDonvi(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select * from db_nguoidung where donvi_id={id}";
                    var data = await db.Connection.QueryFirstOrDefaultAsync<Authentication.Core.DBNguoidung>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpGet("[action]/{ma_nd}")]
        public async Task<IActionResult> GetPassword(string ma_nd)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle("DHSX"))
                {
                    var qry = $"select css_bkn.giaima_mk(matkhau)giaima_mk from admin_bkn.nguoidung where ma_nd='{ma_nd}'";
                    var data = await db.Connection.QueryFirstOrDefaultAsync(qry);
                    if (data == null) return Json(new { msg = TM.Core.Common.Message.not_exist.ToString() });
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPut("[action]/{id:int}")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var matkhau = "vnptbkn@123";
                    var _data = await db.Connection.GetAsync<Authentication.Core.nguoidung>(id);
                    if (_data != null)
                    {
                        // _data.app_key = data.app_key;
                        _data.matkhau = TM.Core.Encrypt.MD5.CryptoMD5TM(matkhau + _data.salt);
                        _data.change_pass_by = nd.ma_nd;
                        _data.change_pass_at = DateTime.Now;
                        await db.Connection.UpdateAsync(_data);
                    }
                    else
                    {
                        var tmp = new Authentication.Core.nguoidung();
                        tmp.nguoidung_id = id;
                        tmp.salt = Guid.NewGuid().ToString("N");
                        tmp.matkhau = TM.Core.Encrypt.MD5.CryptoMD5TM(matkhau + tmp.salt);
                        tmp.change_pass_by = nd.ma_nd;
                        tmp.change_pass_at = DateTime.Now;
                        await db.Connection.InsertOraAsync(tmp);
                    }
                    return Json(new { data = matkhau, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SetRoles([FromBody] List<Authentication.Core.nguoidung_role> data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var nguoidung = await db.Connection.GetAllAsync<Authentication.Core.nguoidung>();
                    var index = 0;
                    var qry = "BEGIN ";
                    foreach (Authentication.Core.nguoidung_role item in data)
                    {
                        // var _data = await db.Connection.GetAsync<Authentication.Core.nguoidung>(item.nguoidung_id);
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
                            await db.Connection.InsertOraAsync(tmp);
                        }
                    }
                    qry += "END;";
                    if (index > 0)
                    {
                        await db.Connection.QueryAsync(qry);
                        await db.Connection.QueryAsync("COMMIT");
                    }
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        public partial class Paging : TM.Core.Common.Paging
        {
            public List<int> donvi_id { get; set; }
        }
    }
}