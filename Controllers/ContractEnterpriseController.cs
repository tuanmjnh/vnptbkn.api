using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VNPTBKN.API.Common;
namespace VNPTBKN.API.Controllers
{
    [Route("api/contract-enterprise")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class ContractEnterpriseController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Paging paging)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle("DHSX"))
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    // Query
                    var qry = @"select dv.ten_dv ""ten_dv"",ct.group_id ""group_id"", g.title ""nhom_dv"",ct.kieuld_id ""kieuld_id"",";
                    qry += @"ct.ma_hd ""ma_hd"",ct.ten_kh ""ten_kh"",ct.diachi_kh ""diachi_kh"",ct.nguoi_dd ""nguoi_dd"",";
                    qry += @"ct.sdt ""sdt"",ct.stk ""stk"",ct.mst ""mst"",ct.sgt ""sgt"",ct.tep_dk ""tep_dk"",";
                    qry += @"to_char(ct.ngay_cap,'yyyy/MM/dd') ""ngay_cap"",ct.noi_cap ""noi_cap"",";
                    qry += @"to_char(ct.ngay_bd,'yyyy/MM/dd') ""ngay_bd"",to_char(ct.ngay_kt,'yyyy/MM/dd') ""ngay_kt"",";
                    qry += @"ct.so_luong ""so_luong"",ct.tien ""tien"",ct.thue ""thue"",ct.nguoi_gt ""nguoi_gt"",ct.nguoi_tao ""nguoi_tao"",";
                    qry += @"to_char(ct.ngay_tao,'yyyy/MM/dd') ""ngay_tao"",ct.trang_thai ""trang_thai"",ct.noi_dung ""noi_dung"",ct.ghi_chu ""ghi_chu"" ";
                    // var qry = @"select ct.*,dv.ten_dv ""ten_dv""";
                    qry += "from ttkd_bkn.contract_enterprise ct,admin_bkn.donvi dv,ttkd_bkn.groups g ";
                    qry += $"where ct.donvi_id=dv.donvi_id and ct.group_id=g.id and ct.trang_thai in({paging.flag})";
                    // Search
                    if (!string.IsNullOrEmpty(paging.search))
                        qry += $@" ct.ma_hd='{paging.search}' 
                            or CONVERTTOUNSIGN(ct.ten_kh) like CONVERTTOUNSIGN('%{paging.search}%') 
                            or ct.so_dt='{paging.search}' or stk='{paging.search}' 
                            or ct.mst='{paging.search}' or so_gt='{paging.search}' 
                            or ct.nguoi_tao like '%{paging.search}%')";
                    // Đơn vị
                    if (nd.inRoles("donvi.select"))
                    {
                        if (paging.donvi_id > 0)
                            qry += $" and ct.donvi_id in({paging.donvi_id})";
                    }
                    else qry += $" and ct.donvi_id in({nd.donvi_id})";
                    // Paging Params
                    if (paging.is_export)
                    {
                        paging.rowsPerPage = 0;
                        paging.sortBy = @"""ngay_tao""";
                    }
                    var param = new Dapper.Oracle.OracleDynamicParameters("v_data");
                    param.Add("v_sql", qry);
                    param.Add("v_offset", paging.page);
                    param.Add("v_limmit", paging.rowsPerPage);
                    param.Add("v_order", paging.sortBy);
                    param.Add("v_total", 0);
                    //var data = await db.Connection.QueryAsync("PAGING", param, commandType: System.Data.CommandType.StoredProcedure);
                    return Json(new
                    {
                        data = await db.Connection.QueryAsync("ttkd_bkn.PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                        total = param.Get<int>("v_total"),
                        msg = TM.Core.Common.Message.success.ToString()
                    });
                }
            }
            catch (Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.ContractEnterprise data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    if (db.Connection.isExist("contract_enterprise", "ma_hd", data.ma_hd))
                        return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    data.id = Guid.NewGuid().ToString("N");
                    data.nguoi_tao = nd.ma_nd;
                    data.ngay_tao = DateTime.Now;
                    data.ip_tao = TM.Core.HttpContext.Header("LocalIP");
                    data.trang_thai = 1;
                    data.donvi_id = nd.donvi_id;
                    await db.Connection.InsertOraAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.ContractEnterprise data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    // _data.customer_name = data.customer_name;
                    // _data.customer_address = data.customer_address;
                    // _data.tax_code = data.tax_code;
                    // _data.start_at = data.start_at;
                    // _data.end_at = data.end_at;
                    // _data.quantity = data.quantity;
                    // _data.price = data.price;
                    // _data.details = data.details;
                    // _data.contents = data.contents;
                    // _data.attach = data.attach;
                    data.nguoi_cn = nd.ma_nd;
                    data.ip_cn = TM.Core.HttpContext.Header("LocalIP");
                    data.ngay_cn = DateTime.Now;
                    await db.Connection.UpdateAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<Models.Core.ContractEnterprise> data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var qry = "BEGIN ";
                    foreach (var item in data)
                        qry += $"update contract_enterprise set trang_thai={item.trang_thai},nguoi_xoa='{nd.ma_nd}',ngay_xoa={DateTime.Now.ParseDateTime()} where id='{item.id}';\r\n";
                    qry += "END;";
                    await db.Connection.QueryAsync(qry);
                    await db.Connection.QueryAsync("COMMIT");
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        public partial class Paging : TM.Core.Common.Paging
        {
            public int donvi_id { get; set; }
            public int group_id { get; set; }
            public int app_key { get; set; }
        }
    }
}