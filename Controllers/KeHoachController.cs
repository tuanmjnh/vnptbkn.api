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
    [Route("api/kehoach")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class KeHoachController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Paging paging)
        {
            try
            {
                // var data = await db.Connection().GetAllAsync<Models.Core.Items>();
                // return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString()});
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                // Query
                var qry = "";
                if (paging.isExport)
                {
                    qry = "select dv.ten_dv,gr.title,tb.ma_tb,tb.diachi_tb,tb.so_dt,tb.ma_nv,tb.de_xuat,tb.thang_bd,tb.thang_kt,tb.ghichu";
                    qry += " from kehoach_tb tb,db_donvi dv,groups gr";
                    qry += $" where tb.donvi_id=dv.donvi_id and tb.nhom_kh=gr.id and tb.trang_thai in({paging.flag})";
                }
                else qry = $"select * from kehoach_tb tb where tb.trang_thai in({paging.flag})";
                // Đơn vị
                if (nd.donvi_id > 0)
                    qry += $" and donvi_id in({nd.donvi_id})";
                else
                   if (paging.donvi_id > 0) qry += $" and tb.donvi_id in({paging.donvi_id})";
                // Nhóm kế hoạch
                if (paging.nhomkh_id > 0) qry += $" and tb.nhom_kh in({paging.nhomkh_id})";
                // Search
                if (!string.IsNullOrEmpty(paging.search))
                {
                    qry += $@" and (CONVERTTOUNSIGN(tb.ma_tb) like CONVERTTOUNSIGN('%{paging.search}%')";
                    qry += $@" or CONVERTTOUNSIGN(tb.diachi_tb) like CONVERTTOUNSIGN('%{paging.search}%')";
                    qry += $@" or CONVERTTOUNSIGN(tb.so_dt) like CONVERTTOUNSIGN('%{paging.search}%')";
                    qry += $@" or CONVERTTOUNSIGN(tb.ma_nv) like CONVERTTOUNSIGN('%{paging.search}%'))";
                }
                // Paging Params
                if (paging.isExport)
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
                if (paging.isExport) // Export data
                    return Json(new
                    {
                        data = await db.Connection().QueryAsync("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                        total = param.Get<int>("v_total"),
                        msg = TM.Core.Common.Message.success.ToString()
                    });
                else // View data
                    return Json(new
                    {
                        data = await db.Connection().QueryAsync<Models.Core.Kehoach_TB>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                        total = param.Get<int>("v_total"),
                        msg = TM.Core.Common.Message.success.ToString()
                    });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("[action]/{key:int}")]
        public async Task<IActionResult> GetByFlag(int key)
        {
            try
            {
                var qry = $"select * from Items where flag in({key})";
                var data = await db.Connection().QueryAsync<Models.Core.Items>(qry);
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
                var qry = $"select * from Items where app_key in({key.Trim(',')}) and flag={query.flag}";
                var data = await db.Connection().QueryAsync<Models.Core.Items>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var data = await db.Connection().GetAsync<Models.Core.Items>(id);
                return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        [HttpGet("[action]/{id:int}")]
        public async Task<IActionResult> GetNguoidung(int id)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var qry = $"select * from db_nguoidung";
                if (nd.donvi_id > 0)
                    qry += $" where donvi_id in({nd.donvi_id})";
                else
                   if (id > 0) qry += $" where donvi_id in({id})";

                var data = await db.Connection().QueryAsync<Authentication.Core.DBNguoidung>(qry);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        [HttpGet("[action]/{code}")]
        public IActionResult ExistCode(string code)
        {
            try
            {
                if (db.Connection().isExist("Items", "code", code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                return Json(new { msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] request_import req)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var qry = $@"update kehoach_tb set trang_thai=2,nguoi_huy='{nd.ma_nd}',
                        ip_huy='{TM.Core.HttpContext.Header("LocalIP")}',ngay_huy={DateTime.Now.ParseDateTime()}
                        where nhom_kh={req.nhomkh_id} and thang_bd={req.thang_bd} and donvi_id={req.donvi_id} and trang_thai=1";
                await db.Connection().QueryAsync(qry);
                var data = new Models.Core.Kehoach_TB();
                var csv = TM.Core.IO.ReadFile(req.file_upload, '\t');
                var error = new List<template_import>();
                var success = 0;
                var index = 0;
                for (int i = 0; i < csv.Count; i++)
                {
                    try
                    {
                        index = i;
                        if (i < 1) continue;
                        data.id = Guid.NewGuid().ToString("N");
                        data.nhom_kh = req.nhomkh_id;
                        data.donvi_id = req.donvi_id;
                        data.ma_tb = csv[i][0];
                        data.ten_tb = csv[i][1];
                        data.diachi_tb = csv[i][2];
                        data.so_dt = csv[i][3];
                        data.thang_bd = req.thang_bd;
                        data.thang_kt = req.thang_bd;
                        data.ma_nv = csv[i][4];
                        data.ghichu = csv[i][5];
                        data.nguoi_nhap = nd.ma_nd;
                        data.ngay_nhap = DateTime.Now;
                        data.ip_nhap = TM.Core.HttpContext.Header("LocalIP");
                        data.trang_thai = 1;
                        await db.Connection().InsertOraAsync(data);
                        success++;
                    }
                    catch (System.Exception)
                    {
                        var tmp = new template_import();
                        tmp.ma_tb = csv[index][0];
                        tmp.ten_tb = csv[index][1];
                        tmp.diachi_tb = csv[index][2];
                        tmp.so_dt = csv[index][3];
                        tmp.ma_nv = csv[index][4];
                        tmp.ghichu = csv[index][5];
                        tmp.error = "Sai định dạng";
                        error.Add(tmp);
                        continue;
                    }
                }
                return Json(new { success = success, error = error, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception)
            {
                return Json(new { msg = TM.Core.Common.Message.danger.ToString() });
            }
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Post([FromBody] Models.Core.Items data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                if (db.Connection().isExist("Items", "code", data.code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                data.created_by = nd.ma_nd;
                data.created_at = DateTime.Now;
                data.created_ip = TM.Core.HttpContext.Header("LocalIP");
                data.flag = 1;
                await db.Connection().InsertOraAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception)
            {
                return Json(new { msg = TM.Core.Common.Message.danger.ToString() });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.Items data)
        {
            try
            {
                var nd = db.Connection().getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                var _data = await db.Connection().GetAsync<Models.Core.Items>(data.id);
                if (_data != null)
                {
                    _data.app_key = data.app_key;
                    _data.code = data.code;
                    _data.title = data.title;
                    _data.icon = data.icon;
                    _data.image = data.image;
                    _data.url = data.url;
                    _data.orders = data.orders;
                    _data.quantity = data.quantity;
                    _data.descs = data.descs;
                    _data.content = data.content;
                    _data.attach = data.attach;
                    _data.tags = data.tags;
                    _data.updated_by = nd.ma_nd;
                    _data.updated_at = DateTime.Now;
                    _data.updated_ip = TM.Core.HttpContext.Header("LocalIP");
                    _data.flag = data.flag;
                }
                await db.Connection().UpdateAsync(_data);
                return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
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
                    qry += $"update Items set flag={item.flag},deleted_ip='{TM.Core.HttpContext.Header("LocalIP")}',deleted_by='{nd.ma_nd}',deleted_at={now.ParseDateTime()} where id='{item.id}';\r\n";
                qry += "END;";
                await db.Connection().QueryAsync(qry);
                await db.Connection().QueryAsync("COMMIT");
                return Json(new { msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Items> data)
        {
            try
            {
                if (data.Count > 0) await db.Connection().DeleteAsync(data);
                return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        public partial class Paging : TM.Core.Common.Paging
        {
            public int donvi_id { get; set; }
            public int nhomkh_id { get; set; }
        }
        public partial class request_import
        {
            public int donvi_id { get; set; }
            public int nhomkh_id { get; set; }
            public int thang_bd { get; set; }
            public int thang_kt { get; set; }
            public string file_name { get; set; }
            public string file_upload { get; set; }
        }
        public partial class template_import
        {
            public string ma_tb { get; set; }
            public string ten_tb { get; set; }
            public string diachi_tb { get; set; }
            public string so_dt { get; set; }
            public string ma_nv { get; set; }
            public string ghichu { get; set; }
            public string error { get; set; }
        }
    }
}