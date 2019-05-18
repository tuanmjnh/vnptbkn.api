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
        public async Task<IActionResult> GetByDonVi([FromQuery] Paging paging)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    // Query
                    var qry = "ten_dv,group_id,kieu_ld,ma_hd,ma_kh,ten_kh,diachi_kh,nguoi_dd,so_dt,stk,mst,";
                    qry += "so_gt,ngay_cap,noi_cap,to_char(ngay_bd,'yyyy/MM/dd')ngay_bd,to_char(ngay_kt,'yyyy/MM/dd')ngay_kt,";
                    qry += "so_luong,don_gia,vat,ghichu,created_by nguoi_nhap,to_char(created_at,'yyyy/MM/dd')ngay_nhap";//,deleted_by,to_char(deleted_at,'dd/MM/yyyy')";
                    qry = $"select {(paging.is_export ? qry : "*")} from contract_enterprise where flag in({paging.flag})";
                    // Search
                    if (!string.IsNullOrEmpty(paging.search))
                        qry += $@" ma_hd='{paging.search}' 
                            or CONVERTTOUNSIGN(ten_kh) like CONVERTTOUNSIGN('%{paging.search}%') 
                            or so_dt='{paging.search}' or stk='{paging.search}' 
                            or mst='{paging.search}' or so_gt='{paging.search}' 
                            or created_by like '%{paging.search}%')";
                    // Extras
                    if (nd.donvi_id > 0)
                        qry += $" and donvi_id in({nd.donvi_id})";
                    else
                        if (paging.donvi_id > 0) qry += $" and donvi_id in({paging.donvi_id})";
                    // Paging Params
                    if (paging.is_export)
                    {
                        paging.rowsPerPage = 0;
                        paging.sortBy = "ngay_nhap";
                    }
                    var param = new Dapper.Oracle.OracleDynamicParameters("v_data");
                    param.Add("v_sql", qry);
                    param.Add("v_offset", paging.page);
                    param.Add("v_limmit", paging.rowsPerPage);
                    param.Add("v_order", paging.sortBy);
                    param.Add("v_total", 0);
                    // var data = await db.Connection.QueryAsync<ContractCustomerKH>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure);
                    // var data = await db.Connection.QueryAsync<ContractCustomerKH>(qry);
                    if (paging.is_export)// Export data
                        return Json(new
                        {
                            data = await db.Connection.QueryAsync("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                            total = param.Get<int>("v_total"),
                            msg = TM.Core.Common.Message.success.ToString()
                        });
                    else  // View data
                        return Json(new
                        {
                            data = await db.Connection.QueryAsync<Models.Core.ContractEnterprise>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                            total = param.Get<int>("v_total"),
                            msg = TM.Core.Common.Message.success.ToString()
                        });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
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
                    data.created_by = nd.ma_nd;
                    data.created_at = DateTime.Now;
                    data.flag = 1;
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
                    data.updated_by = nd.ma_nd;
                    data.updated_at = DateTime.Now;
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
                        qry += $"update contract_enterprise set flag={item.flag},deleted_by='{nd.ma_nd}',deleted_at={DateTime.Now.ParseDateTime()} where id='{item.id}';\r\n";
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