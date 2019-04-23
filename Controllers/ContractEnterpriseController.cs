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
        public async Task<IActionResult> Get()
        {
            try
            {
                var data = await db.Connection().GetAllAsync<Models.Core.ContractEnterprise>();
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query)
        {
            try
            {
                var tmp = key.Trim(',').Split(',');
                key = "";
                foreach (var item in tmp) key += $"'{item}',";
                var qry = $"select * from contract_enterprise where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.ContractEnterprise>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                await db.Connection().GetAsync<Models.Core.ContractEnterprise>(id);
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Models.Core.ContractEnterprise data)
        {
            try
            {
                if (db.Connection().isExist("contract_enterprise", "contract_code", data.contract_code))
                    return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                data.contract_enterprise_id = Guid.NewGuid().ToString();
                data.created_by = TM.Core.HttpContext.Header();
                data.created_at = DateTime.Now;
                data.flag = 1;
                await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.ContractEnterprise data)
        {
            try
            {
                var _data = await db.Connection().GetAsync<Models.Core.ContractEnterprise>(data.group_id);
                if (_data != null)
                {
                    _data.customer_name = data.customer_name;
                    _data.customer_address = data.customer_address;
                    _data.tax_code = data.tax_code;
                    _data.start_at = data.start_at;
                    _data.end_at = data.end_at;
                    _data.quantity = data.quantity;
                    _data.price = data.price;
                    _data.details = data.details;
                    _data.contents = data.contents;
                    _data.attach = data.attach;
                    _data.updated_by = TM.Core.HttpContext.Header();
                    _data.updated_at = DateTime.Now;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<Models.Core.ContractEnterprise> data)
        {
            try
            {
                var _data = new List<Models.Core.ContractEnterprise>();
                foreach (var item in data)
                {
                    var tmp = await db.Connection().GetAsync<Models.Core.ContractEnterprise>(item.group_id);
                    if (tmp != null)
                    {
                        tmp.flag = item.flag;
                        _data.Add(tmp);
                    }
                }
                if (_data.Count > 0) await db.Connection().UpdateAsync(_data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.ContractEnterprise> data)
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
                await db.Connection().GetAsync<Models.Core.ContractEnterprise>(id);
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
    }
}