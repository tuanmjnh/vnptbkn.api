using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using VNPTBKN.API.Common;
namespace VNPTBKN.API.Controllers
{
    [Route("api/contract-customer")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class ContractCustomerController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var qry = $"select * from CONTRACT_CUSTOMER_KH";
                var data = await db.Connection().QueryAsync<ContractCustomerKH>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetByDonVi([FromQuery] Paging paging)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                // Query
                var qry = "ten_dv,ma_gd,ma_hd,ma_kh,ten_kh,so_dt,ten_loaihd loaihd,nguoi_cn,to_char(ngay_cn,'yyyy/MM/dd')ngay_cn,";
                qry += "REPLACE(attach,'Uploads/HopDong/','')file_hd,created_by nguoi_nhap,to_char(created_at,'yyyy/MM/dd')ngay_nhap,descs ghichu";//,deleted_by,to_char(deleted_at,'dd/MM/yyyy')";
                qry = $"select {(paging.isExport ? qry : "*")} from CONTRACT_CUSTOMER_KH where flag in({paging.flag})";
                // Search
                if (!string.IsNullOrEmpty(paging.search))
                    qry += $@" and (ma_gd='{paging.search}' 
                            or ma_hd='{paging.search}' 
                            or ma_kh='{paging.search}' 
                            or CONVERTTOUNSIGN(ten_kh) like CONVERTTOUNSIGN('%{paging.search}%') 
                            or so_dt='{paging.search}' or so_gt='{paging.search}' 
                            or created_by like '%{paging.search}%')";
                // Extras
                if (nd.donvi_id > 0)
                    qry += $" and donvi_id in({nd.donvi_id})";
                else
                    if (paging.donvi_id > 0) qry += $" and donvi_id in({paging.donvi_id})";
                if (paging.start_at.HasValue)
                    qry += $" and created_at>={paging.start_at.Value.ParseDateTime()}";
                if (paging.end_at.HasValue)
                    qry += $" and created_at<={TM.Core.Format.Formating.AbsoluteEnd(paging.start_at.Value).ParseDateTime()}";
                // Paging Params
                if (paging.isExport)
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
                // var data = await db.Connection().QueryAsync<ContractCustomerKH>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure);
                // var data = await db.Connection().QueryAsync<ContractCustomerKH>(qry);
                if (paging.isExport)// Export data
                    return Json(new
                    {
                        data = await db.Connection().QueryAsync("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                        total = param.Get<int>("v_total"),
                        msg = TM.Core.Common.Message.success.ToString()
                    });
                else  // View data
                    return Json(new
                    {
                        data = await db.Connection().QueryAsync<ContractCustomerKH>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                        total = param.Get<int>("v_total"),
                        msg = TM.Core.Common.Message.success.ToString()
                    });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key)
        {
            try
            {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from contract_customer where app_key in({key.Trim(',')})";
                var data = await db.Connection().QueryAsync<Models.Core.ContractCustomer>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var data = await db.Connection().GetAsync<Models.Core.ContractCustomer>(id);
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> getThuebao(string key, [FromQuery] Models.Core.QueryString query)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"{item},";
                var qry = $"select * from CONTRACT_CUSTOMER_TB where hdkh_id in({key.Trim(',')})";
                var data = await db.Connection().QueryAsync<Models.Core.HD_THUEBAO>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.ContractCustomer data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                //if (db.Connection().isExist("contract_customer", "ma_gd", data.khachhang.ma_gd)) return Json(new { msg = "exist" });
                //data.khachhang.cc_id = Guid.NewGuid().ToString();
                //data.khachhang.app_key = "cc_2";
                data.id = Guid.NewGuid().ToString("N");
                data.created_by = nd.ma_nd;
                data.created_at = DateTime.Now;
                data.flag = 1;
                await db.Connection().InsertOraAsync(data);
                // 
                var qry = $"select * from CONTRACT_CUSTOMER_KH where hdkh_id='{data.hdkh_id}' and flag=1";
                var rs = db.Connection().QueryFirstOrDefault<ContractCustomerKH>(qry);
                return Json(new { data = rs, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.ContractCustomer data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                data.updated_by = nd.ma_nd;
                data.updated_at = DateTime.Now;
                await db.Connection().UpdateAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var now = DateTime.Now;
                var qry = "BEGIN ";
                foreach (var item in data)
                    qry += $"update contract_customer set flag={item.flag},deleted_by='{nd.ma_nd}',deleted_at={now.ParseDateTime()} where id='{item.id}';\r\n";
                qry += "END;";
                await db.Connection().QueryAsync(qry);
                await db.Connection().QueryAsync("COMMIT");
                return Json(new { msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.ContractCustomer> data)
        {
            try
            {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOne(int id)
        {
            try
            {
                await db.Connection().GetAllAsync<Models.Core.ContractCustomer>();
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> getContract(string key)
        {
            try
            {
                // HD_KHACHHANG
                var param = new Dapper.Oracle.OracleDynamicParameters("returns");
                param.Add("v_key", key);
                param.Add("v_loaihd_id", 1);
                param.Add("v_tthd_id", 6);
                var khachhang = db.Connection().QueryFirstOrDefault<Models.Core.HD_KHACHHANG>("GET_DB_HD_KH", param, commandType: System.Data.CommandType.StoredProcedure);
                // not exist
                if (khachhang == null) return Json(new { msg = TM.Core.Common.Message.not_exist.ToString() });
                // exist
                if (db.Connection().isExist("contract_customer", "hdkh_id", khachhang.hdkh_id.ToString(), "flag=1")) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                // HD_THUEBAO
                param = new Dapper.Oracle.OracleDynamicParameters("returns");
                param.Add("v_hdkh_id", khachhang.hdkh_id);
                var thuebao = await db.Connection().QueryAsync<Models.Core.HD_THUEBAO>("GET_DB_HD_TB", param, commandType: System.Data.CommandType.StoredProcedure);
                // not exist
                if (thuebao == null) return Json(new { msg = TM.Core.Common.Message.not_exist.ToString() });
                return Json(new { data = new { khachhang = khachhang, thuebao = thuebao }, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        public partial class Paging : TM.Core.Common.Paging
        {
            public int donvi_id { get; set; }
        }
        public partial class DataCoreCustomer
        {
            public long hdkh_id { get; set; }
            public long hdtb_id { get; set; }
            public long hdtt_id { get; set; }
        }
        public partial class ContractCustomerKH : Models.Core.HD_KHACHHANG
        {
            // public string hdkh_id { get; set; }
            public string id { get; set; }
            public string attach { get; set; }
            public string descs { get; set; }
            public string created_by { get; set; }
            public DateTime? created_at { get; set; }
            public string updated_by { get; set; }
            public DateTime? updated_at { get; set; }
            public string deleted_by { get; set; }
            public DateTime? deleted_at { get; set; }
            public int flag { get; set; }
        }
    }
}