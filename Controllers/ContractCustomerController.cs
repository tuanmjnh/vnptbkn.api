using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VNPTBKN.API.Common;
namespace VNPTBKN.API.Controllers {
    [Route("api/contract-customer")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class ContractCustomerController : Controller {
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                var data = await db.Connection().GetAllAsync<Models.Core.ContractCustomer>();
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query) {
            try {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from contract_customer where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.ContractCustomer>(qry);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id) {
            try {
                var data = await db.Connection().GetAsync<Models.Core.ContractCustomer>(id);
                return Json(new { data = id, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.ContractCustomer data) {
            try {
                if (db.isExist("contract_customer", "contract_code", data.ma_gd))
                    return Json(new { message = "exist" });
                data.contract_customer_id = Guid.NewGuid().ToString();
                data.created_by = TM.Core.HttpContext.Header();
                data.created_at = DateTime.Now;
                data.flag = 1;
                await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
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
                return Json(new { data = "_data", message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
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
                return Json(new { data = "data", message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.ContractCustomer> data) {
            try {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveOne(int id) {
            try {
                await db.Connection().GetAllAsync<Models.Core.ContractCustomer>();
                return Json(new { data = id, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpGet("getContract")]
        public async Task<IActionResult> getContract(string str) {
            try {
                var qry = $"select * from CSS_BKN.BKN_HD_THUEBAO where MA_GD='{str}' or MA_TB='{str}' or TEN_KH=N'{str}' or SO_GT='{str}' or SO_DT='{str}'";
                var data = await db.Connection("DHSX").QueryAsync<Models.Core.BKN_HD_THUEBAO>(qry);
                return Json(new { data = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }
    }
}