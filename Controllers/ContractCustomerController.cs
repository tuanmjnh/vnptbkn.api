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
                if (db.isExist("contract_customer", "ma_gd", data.khachhang.ma_gd)) return Json(new { msg = "exist" });
                using(var transaction = db.Connection().BeginTransaction()) {
                    data.khachhang.cc_id = Guid.NewGuid().ToString();
                    data.khachhang.app_key = "cc_2";
                    data.khachhang.created_by = TM.Core.HttpContext.Header();
                    data.khachhang.created_at = DateTime.Now;
                    data.khachhang.flag = 1;
                    await db.Connection().InsertOraAsync(data.khachhang, transaction);

                    foreach (Models.Core.ContractCustomerThueBao item in data.thuebao) {
                        await db.Connection().InsertOraAsync(item, transaction);
                    }
                    //
                    transaction.Commit();
                    return Json(new { data = data, msg = "success" });
                }
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
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
                // var qry = $"select * from CSS_BKN.BKN_HD_THUEBAO where MA_GD='{key}' or MA_TB='{key}' or TEN_KH=N'{key}' or SO_GT='{key}' or SO_DT='{key}'";
                var qry = $@"select kh.hdkh_id,tb.hdtb_id,tb.hdtt_id 
                             from CSS_BKN.HD_KHACHHANG kh,CSS_BKN.HD_THUEBAO tb,CSS_BKN.KIEU_LD ld,CSS_BKN.TRANGTHAI_HD tt 
                             where tb.HDKH_ID=kh.HDKH_ID and tb.KIEULD_ID=ld.KIEULD_ID and tb.TTHD_ID=tt.TTHD_ID and ld.LOAIHD_ID=1 and tt.TTHD_ID=6 
                             and (kh.MA_GD='{key}' or kh.TEN_KH=N'{key}' or kh.SO_GT='{key}' or kh.SO_DT='{key}' or tb.TEN_TB=N'{key}' or tb.MA_TB='{key}')
                             order by kh.MA_GD";
                var dataCore = db.Connection("DHSX").Query<DataCoreCustomer>(qry).ToList();
                // notexist
                if (dataCore.Count() < 1) return Json(new { msg = "notexist" });
                // exist
                if (db.isExist("contract_customer", "hdkh_id", dataCore[0].hdkh_id.ToString())) return Json(new { msg = "exist" });
                // HD_KHACHHANG
                qry = $@"select kh.*,dv.ten_dv,lhd.ten_loaihd,lkh.ten_loaikh 
                         from css_bkn.hd_khachhang kh,admin_bkn.donvi dv,css_bkn.loai_hd lhd,css_bkn.loai_kh lkh 
                         where kh.donvi_id=dv.donvi_id and kh.loaihd_id=lhd.loaihd_id and kh.loaikh_id=lkh.loaikh_id and hdkh_id in({dataCore[0].hdkh_id})";
                // var khachhang = await db.Connection("DHSX").QueryFirstOrDefaultAsync<Models.Core.HD_KHACHHANG>(qry);
                var khachhang = await db.Connection("DHSX").QueryFirstOrDefaultAsync(qry);
                // HD_THUEBAO
                qry = $@"select tb.*,dv.ten_dv,lhtb.loaihinh_tb,dvvt.ten_dvvt,dttb.ten_dt 
                         from css_bkn.hd_thuebao tb,admin_bkn.donvi dv,css_bkn.loaihinh_tb lhtb,css_bkn.dichvu_vt dvvt,css_bkn.doituong dttb 
                         where tb.donvi_id=dv.donvi_id and tb.loaitb_id=lhtb.loaitb_id and tb.dichvuvt_id=dvvt.dichvuvt_id and tb.doituong_id=dttb.doituong_id and hdkh_id in({dataCore[0].hdkh_id})";
                // var thuebao = await db.Connection("DHSX").QueryAsync<Models.Core.HD_THUEBAO>(qry);
                var thuebao = await db.Connection("DHSX").QueryAsync(qry);
                // HD_THUEBAO
                // qry = $"select * from CSS_BKN.HD_THUEBAO where hdkh_id in({dataCore[0].hdkh_id})";
                // var xx = await db.Connection("DHSX").QueryAsync(qry);
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