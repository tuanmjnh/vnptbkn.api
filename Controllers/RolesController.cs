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
namespace VNPTBKN.API.Controllers {
    [Route("api/roles")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class RolesController : Controller {
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                var data = await db.Connection().GetAllAsync<Models.Core.ContractCustomer>();
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from contract_customer where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.ContractCustomer>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id) {
            try {
                var data = await db.Connection().GetAsync<Models.Core.ContractCustomer>(id);
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> getThuebao(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from contract_customer_thuebao where hdkh_id in({key.Trim(',')})";

                var data = await db.Connection().QueryAsync<Models.Core.ContractCustomerThueBao>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DataMainCustomer data) {
            try {
                
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) {
                return Json(new { msg = "danger" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.ContractCustomer data) {
            try {
                // var _data = await db.Connection().GetAsync<Models.Core.ContractCustomer>(data.group_id);
                // if (_data != null) {
                //     _data.customer_name = data.customer_name;
                //     _data.customer_address = data.customer_address;
                //     _data.customer_phone = data.customer_phone;
                //     _data.details = data.details;
                //     _data.attach = data.attach;
                //     _data.updated_by = TM.Core.HttpContext.Header();
                //     _data.updated_at = DateTime.Now;
                // }
                // await db.Connection().UpdateAsync(_data);
                await db.Connection().GetAllAsync<Models.Core.ContractCustomer>();
                return Json(new { data = "_data", msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<Models.Core.ContractCustomer> data) {
            try {
                // var _data = new List<Models.Core.ContractCustomer>();
                // foreach (var item in data) {
                //     var tmp = await db.Connection().GetAsync<Models.Core.ContractCustomer>(item.group_id);
                //     if (tmp != null) {
                //         tmp.flag = item.flag;
                //         _data.Add(tmp);
                //     }
                // }
                // if (_data.Count > 0) await db.Connection().UpdateAsync(_data);
                await db.Connection().GetAllAsync<Models.Core.ContractCustomer>();
                return Json(new { data = "data", msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.ContractCustomer> data) {
            try {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                await db.Connection().GetAllAsync<Models.Core.ContractCustomer>();
                return Json(new { data = id, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> getContract(string key) {
            try {
                // HD_KHACHHANG
                var param = new Dapper.Oracle.OracleDynamicParameters("returns");
                param.Add("v_key", key);
                param.Add("v_loaihd_id", 1);
                param.Add("v_tthd_id", 6);
                var khachhang = db.Connection().QueryFirstOrDefault<Models.Core.HD_KHACHHANG>("GET_DB_HD_KH", param, commandType : System.Data.CommandType.StoredProcedure);
                // exist
                if (db.Connection().isExist("contract_customer", "hdkh_id", khachhang.hdkh_id.ToString())) return Json(new { msg = "exist" });
                // HD_THUEBAO
                param = new Dapper.Oracle.OracleDynamicParameters("returns");
                param.Add("v_hdkh_id", khachhang.hdkh_id);
                var thuebao = await db.Connection().QueryAsync<Models.Core.HD_THUEBAO>("GET_DB_HD_TB", param, commandType : System.Data.CommandType.StoredProcedure);
                return Json(new { data = new { khachhang = khachhang, thuebao = thuebao }, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }
        public partial class DataCoreCustomer {
            public long hdkh_id { get; set; }
            public long hdtb_id { get; set; }
            public long hdtt_id { get; set; }
        }
        public partial class DataMainCustomer {
            public Models.Core.ContractCustomer khachhang { get; set; }
            public List<Models.Core.ContractCustomerThueBao> thuebao { get; set; }
        }
    }
}