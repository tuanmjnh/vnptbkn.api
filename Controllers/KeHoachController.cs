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
                        qry = "select dv.ten_dv,gr.title nhom_kh,tb.ma_tb,tb.ten_tb,tb.diachi_tb,tb.so_dt,tb.ma_nd,tb.ghichu,tb.nguoi_nhap,to_char(tb.ngay_nhap,'MM/DD/YYYY')ngay_nhap,";
                        qry += "decode(th.ket_qua,1,'Thành công',2,'Không thành công','Chưa thực hiện')ket_qua,th.de_xuat,goicuoc.title goicuoc,lydo.title lydo,th.ghichu ghichu_th,to_char(th.ngay_th,'MM/DD/YYYY')ngay_th,th.nguoi_cn nguoi_th,th.ip_cn ip_th ";
                        qry += "from kehoach_tb tb,kehoach_th th,items lydo,items goicuoc,groups gr,admin_bkn.donvi dv ";
                        qry += $"where tb.id=th.kehoachtb_id(+) and th.lydo=lydo.id(+) and th.goicuoc=goicuoc.id(+) and tb.nhom_kh=gr.id and tb.donvi_id=dv.donvi_id and tb.trang_thai in({paging.flag})";
                    }
                    else
                    {
                        qry = "select tb.*,th.ngay_th,th.ket_qua,th.de_xuat,goicuoc.title goicuoc,lydo.title lydo,th.ghichu ghichu_th,th.nguoi_cn nguoi_cn_th,th.ip_cn ip_cn_th,th.ngay_cn ngay_cn_th ";
                        qry += "from kehoach_tb tb,kehoach_th th,items lydo,items goicuoc ";
                        qry += $"where tb.id=th.kehoachtb_id(+) and th.lydo=lydo.id(+) and th.goicuoc=goicuoc.id(+) and tb.trang_thai in({paging.flag})";
                    }
                    if (paging.flag == 2 && paging.ket_qua != null)
                        qry += $" and th.ket_qua in({paging.ket_qua})";
                    // Đơn vị
                    if (nd.inRoles("donvi.select") && paging.donvi_id != null && paging.donvi_id.Count > 0)
                        qry += $" and tb.donvi_id in({String.Join(",", paging.donvi_id)})";
                    else
                        qry += $" and tb.donvi_id in({nd.donvi_id})";

                    if (nd.inRoles("nguoidung.select"))
                    {
                        if (paging.ma_nd != null && paging.ma_nd.Count > 0) // if (!string.IsNullOrEmpty(paging.ma_nd))
                        {
                            if (paging.ma_nd[0] != "$all") qry += $" and tb.ma_nd in('{String.Join("','", paging.ma_nd)}')";
                        }
                        else qry += $" and tb.ma_nd is null";
                    }
                    else
                        qry += $" and tb.ma_nd='{nd.ma_nd}'";

                    // Nhóm kế hoạch
                    if (paging.nhomkh_id != null && paging.nhomkh_id.Count > 0)
                        qry += $" and tb.nhom_kh in({String.Join(",", paging.nhomkh_id)})";
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
                            data = await db.Connection.QueryAsync<kehoach_TH>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                            total = param.Get<int>("v_total"),
                            msg = TM.Core.Common.Message.success.ToString()
                        });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetThucHienTB([FromQuery] Paging paging)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    // Query
                    var qry = "";
                    if (paging.is_export)
                    {
                        qry = "select dv.ten_dv,gr.title,tb.ma_tb,tb.diachi_tb,tb.so_dt,tb.de_xuat,tb.thang_bd,tb.thang_kt,";
                        qry += "th.ma_nd,th.ngay_th,decode(th.ket_qua,1,'Chưa hoàn thành','Đã hoàn thành')ket_qua,th.ghichu ";
                        qry += "from kehoach_th th,kehoach_tb tb,db_donvi dv,groups gr ";
                        qry += $"where th.kehoachtb_id=tb.id and tb.donvi_id=dv.donvi_id and tb.nhom_kh=gr.id and th.ket_qua in({paging.flag})";
                    }
                    else
                    {
                        qry = "select th.*,tb.nhom_kh,tb.thuebao_id,tb.ma_tb,tb.ten_tb,tb.diachi_tb,tb.so_dt,tb.de_xuat,";
                        qry += "tb.thang_bd,tb.thang_kt,tb.nguoi_nhap,tb.ngay_nhap,tb.ip_nhap,tb.trang_thai,tb.donvi_id ";
                        qry += "from kehoach_th th,kehoach_tb tb ";
                        qry += $"where th.kehoachtb_id=tb.id and th.ket_qua in({paging.flag})";
                    }
                    // Đơn vị
                    if (nd.cap_quyen > 1)
                    {
                        qry += $" and tb.donvi_id in({nd.donvi_id})";
                        if (nd.cap_quyen > 2)
                            qry += $" and th.ma_nd in('{nd.ma_nd}')";
                        else
                          if (paging.ma_nd != null && paging.ma_nd.Count > 0) // if (!string.IsNullOrEmpty(paging.ma_nd))
                            qry += $" and th.ma_nd in('{String.Join("','", paging.ma_nd)}')";
                    }
                    if (nd.inRoles("donvi.select") && paging.donvi_id != null && paging.donvi_id.Count > 0)
                        qry += $" and tb.donvi_id in({String.Join(",", paging.donvi_id)})";
                    if (nd.inRoles("nguoidung.select") && paging.ma_nd != null && paging.ma_nd.Count > 0)
                        qry += $" and th.ma_nd in('{String.Join("','", paging.ma_nd)}')";
                    // Nhóm kế hoạch
                    if (paging.nhomkh_id != null && paging.nhomkh_id.Count > 0)
                        qry += $" and tb.nhom_kh in({String.Join(",", paging.nhomkh_id)})";
                    // Search
                    if (!string.IsNullOrEmpty(paging.search))
                    {
                        qry += $@" and (CONVERTTOUNSIGN(tb.ma_tb) like CONVERTTOUNSIGN('%{paging.search}%')";
                        qry += $@" or CONVERTTOUNSIGN(tb.diachi_tb) like CONVERTTOUNSIGN('%{paging.search}%')";
                        qry += $@" or CONVERTTOUNSIGN(tb.so_dt) like CONVERTTOUNSIGN('%{paging.search}%')";
                        qry += $@" or CONVERTTOUNSIGN(th.ma_nd) like CONVERTTOUNSIGN('%{paging.search}%'))";
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
                            data = await db.Connection.QueryAsync<kehoach_TH>("PAGING", param, commandType: System.Data.CommandType.StoredProcedure),
                            total = param.Get<int>("v_total"),
                            msg = TM.Core.Common.Message.success.ToString()
                        });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]/{key:int}")]
        public async Task<IActionResult> GetByFlag(int key)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var qry = $"select * from Items where flag in({key})";
                    var data = await db.Connection.QueryAsync<Models.Core.Items>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] Models.Core.QueryString query)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var tmp = key.Trim(',').Split(',');
                    key = "";
                    foreach (var item in tmp) key += $"'{item}',";
                    var qry = $"select * from Items where app_key in({key.Trim(',')}) and flag={query.flag}";
                    var data = await db.Connection.QueryAsync<Models.Core.Items>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var data = await db.Connection.GetAsync<Models.Core.Items>(id);
                    return Json(new { data = id, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetNguoidung([FromQuery] Paging paging)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var qry = "select dnd.donvi_id,dnd.ma_nd,dnd.ten_nd,dnd.ma_nd,dnd.ten_nv,dnd.ten_nd||' - '||dnd.ma_nd||' - '||dv.ma_dv ten_nd_dv,dnd.gioitinh,dnd.so_dt,dv.ma_dv,dv.ten_dv,r.name ten_quyen ";
                    qry += "from db_nguoidung dnd,nguoidung nd,db_donvi dv,roles r ";
                    qry += "where dnd.nguoidung_id=nd.nguoidung_id and dnd.donvi_id=dv.donvi_id and nd.roles_id=r.id and r.levels=4 ";
                    if (!nd.inRoles("donvi.select"))
                        qry += $" and dv.donvi_id in({nd.donvi_id})";
                    else
                       if (paging.donvi_id != null && paging.donvi_id.Count > 0)
                        qry += $" and dv.donvi_id in({String.Join(",", paging.donvi_id)})";
                    qry += "order by dnd.donvi_id,dnd.ma_nd";
                    var data = await db.Connection.QueryAsync<nguoi_dung>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetLyDo(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var qry = $"select * from items where app_key in('lydo','goicuoc') and code='{id}' order by orders";
                    var data = await db.Connection.QueryAsync<Models.Core.Items>(qry);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpGet("[action]/{code}")]
        public IActionResult ExistCode(string code)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    if (db.Connection.isExist("Items", "code", code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] request_import req)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    if (req.nhomkh_id < 1) return Json(new { msg = TM.Core.Common.Message.danger.ToString() });
                    if (req.donvi_id < 1) return Json(new { msg = TM.Core.Common.Message.danger.ToString() });
                    // Bỏ danh sách thuê bao cũ
                    // var qry = $@"update kehoach_tb set trang_thai=2,nguoi_huy='{nd.ma_nd}',
                    //         ip_huy='{TM.Core.HttpContext.Header("LocalIP")}',ngay_huy={DateTime.Now.ParseDateTime()}
                    //         where nhom_kh={req.nhomkh_id} and thang_bd={req.thang_bd} and donvi_id={req.donvi_id} and trang_thai=1";
                    // await db.Connection.QueryAsync(qry);
                    // Get kehoach_tb check
                    var qry = $"select ma_tb from kehoach_tb where nhom_kh={req.nhomkh_id} and donvi_id={req.donvi_id}";
                    var kehoach_tb = await db.Connection.QueryAsync<Models.Core.Kehoach_TB>(qry);
                    var data = new Models.Core.Kehoach_TB();
                    var csv = TM.Core.IO.ReadFile(req.file_upload, '\t');
                    var error = new List<template_import>();
                    var success = 0;
                    var index = 0;
                    for (index = 0; index < csv.Count; index++)
                    {
                        try
                        {
                            if (index < 1) continue;
                            data.id = Guid.NewGuid().ToString("N");
                            data.nhom_kh = req.nhomkh_id;
                            data.donvi_id = req.donvi_id;
                            data.ma_tb = csv[index][0].Trim();
                            data.ten_tb = csv[index][1].Trim();
                            data.diachi_tb = csv[index][2].Trim();
                            data.so_dt = csv[index][3].Trim();
                            data.thang_bd = req.thang_bd;
                            data.thang_kt = req.thang_bd;
                            data.ma_nd = csv[index][4].Trim().ToLower();
                            data.ghichu = csv[index][5].Trim();
                            data.nguoi_nhap = nd.ma_nd;
                            data.ngay_nhap = DateTime.Now;
                            data.ip_nhap = TM.Core.HttpContext.Header("LocalIP");
                            data.trang_thai = 1;
                            // Nếu đã có thuê bao thì bỏ qua
                            //var tmp = kehoach_tb.Any(b => b.ma_tb==data.ma_tb);
                            if (kehoach_tb.Any(b => b.ma_tb == data.ma_tb))
                            {
                                ImportData(error, csv, index, "Trùng thuê bao");
                                continue;
                            }
                            // Chưa có thực hiện nhập vào csdl
                            await db.Connection.InsertOraAsync(data);
                            // qry = $"select ma_tb from kehoach_tb where nhom_kh={req.nhomkh_id} and donvi_id={req.donvi_id} and ma_tb='{data.ma_tb}' and id!='{data.id}'";
                            // var update = await db.Connection.QueryFirstOrDefaultAsync(qry);
                            // if (update != null)
                            // {
                            //     await db.Connection.QueryAsync(qry.Replace("select ma_tb from", "delete"));
                            //     ImportData(error, csv, index, "Cập nhật lại");
                            // }
                            success++;
                        }
                        catch (System.Exception)
                        {
                            ImportData(error, csv, index, "Sai định dạng");
                            // var tmp = new template_import();
                            // tmp.ma_tb = csv[index][0];
                            // tmp.ten_tb = csv[index][1];
                            // tmp.diachi_tb = csv[index][2];
                            // tmp.so_dt = csv[index][3];
                            // tmp.ma_nd = csv[index][4];
                            // tmp.ghichu = csv[index][5];
                            // tmp.error = "Sai định dạng";
                            // error.Add(tmp);
                            continue;
                        }
                    }
                    return Json(new
                    {
                        total = index - 1,
                        success = success,
                        error = error,
                        msg = TM.Core.Common.Message.success.ToString()
                    });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPost("[action]/{ma_nd}")]
        public async Task<IActionResult> updateNVTB(string ma_nd, [FromBody]  List<Models.Core.Kehoach_TB> tb)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var now = DateTime.Now;
                    var qry = "BEGIN ";
                    foreach (var item in tb)
                        qry += $"update kehoach_tb set ma_nd='{ma_nd}',ip_cn='{TM.Core.HttpContext.Header("LocalIP")}',nguoi_cn='{nd.ma_nd}',ngay_cn={now.ParseDateTime()} where id='{item.id}';\r\n";
                    qry += "END;";
                    await db.Connection.QueryAsync(qry);
                    await db.Connection.QueryAsync("COMMIT");
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateND([FromBody] request_import req)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var qry = "";
                    var csv = TM.Core.IO.ReadFile(req.file_upload, '\t');
                    var error = new List<template_update_import>();
                    var success = 0;
                    var index = 0;
                    for (index = 0; index < csv.Count; index++)
                    {
                        try
                        {
                            if (index < 1) continue;
                            // Kiểm tra thuê bao
                            qry = $"select ma_tb from kehoach_tb where donvi_id={nd.donvi_id} and ma_tb='{csv[index][0]}'";
                            var tmp = await db.Connection.QueryFirstOrDefaultAsync<Models.Core.Kehoach_TB>(qry);
                            if (tmp == null)
                            {
                                var _tmp = new template_update_import();
                                _tmp.ma_tb = csv[index][0].Trim();
                                _tmp.ma_nd = csv[index][1].Trim();
                                _tmp.ct_loi = "Không tồn tại thuê bao";
                                error.Add(_tmp);
                                continue;
                            }
                            qry = $"update kehoach_tb set ma_nd='{csv[index][1].Trim()}' where ma_tb='{csv[index][0]}' and donvi_id={nd.donvi_id}";
                            await db.Connection.QueryAsync(qry);
                            success++;
                        }
                        catch (System.Exception)
                        {
                            var _tmp = new template_update_import();
                            _tmp.ma_tb = csv[index][0].Trim();
                            _tmp.ma_nd = csv[index][1].Trim();
                            _tmp.ct_loi = "Sai định dạng";
                            continue;
                        }
                    }
                    return Json(new
                    {
                        total = index - 1,
                        success = success,
                        error = error,
                        msg = TM.Core.Common.Message.success.ToString()
                    });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPost("[action]/{ma_nd}")]
        public async Task<IActionResult> GanThueBaoTH(string ma_nd, [FromBody] List<request_thuebao_th> req)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    if (req.Count < 1) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    var thuebao_th = new List<Models.Core.Kehoach_TH>();
                    var qry = "";
                    foreach (var item in req)
                    {
                        qry += $"'{item.id}',";
                        thuebao_th.Add(new Models.Core.Kehoach_TH()
                        {
                            id = Guid.NewGuid().ToString("N"),
                            kehoachtb_id = item.id,
                            ma_nd = ma_nd,
                            ket_qua = 1,
                            ip_cn = TM.Core.HttpContext.Header("LocalIP"),
                            nguoi_cn = nd.ma_nd,
                            ngay_cn = DateTime.Now
                        });
                    }
                    // Nhập danh sách thuê bao vào danh sách thực hiện kế hoạch
                    await db.Connection.InsertOraAsync(thuebao_th);
                    // Cập nhật trạng thái thuê bao kế hoạch - 2: Đã được cho phép thực hiện
                    qry = $"update kehoach_tb set trang_thai=2 where id in({qry.Trim(',')})";
                    await db.Connection.QueryAsync(qry);
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Post([FromBody] Models.Core.Items data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    if (db.Connection.isExist("Items", "code", data.code)) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    data.created_by = nd.ma_nd;
                    data.created_at = DateTime.Now;
                    data.created_ip = TM.Core.HttpContext.Header("LocalIP");
                    data.flag = 1;
                    await db.Connection.InsertOraAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Thuchien([FromBody] Models.Core.Kehoach_TH data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var qry = $"select * from kehoach_tb where id='{data.kehoachtb_id}'";
                    var _data = await db.Connection.QueryFirstOrDefaultAsync<Models.Core.Kehoach_TB>(qry);
                    if (_data == null) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });
                    _data.trang_thai = 2;
                    data.id = Guid.NewGuid().ToString("N");
                    data.ma_nd = _data.ma_nd;
                    data.ngay_th = DateTime.Now;
                    data.nguoi_cn = nd.ma_nd;
                    data.ngay_cn = DateTime.Now;
                    data.ip_cn = TM.Core.HttpContext.Header("LocalIP");
                    await db.Connection.InsertOraAsync(data);
                    await db.Connection.UpdateAsync(_data);
                    return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Models.Core.Items data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var _data = await db.Connection.GetAsync<Models.Core.Items>(data.id);
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
                    await db.Connection.UpdateAsync(_data);
                    return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Delete([FromBody] List<dynamic> data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    var nd = db.Connection.getUserFromToken(TM.Core.HttpContext.Header("Authorization"));
                    if (nd == null) return Json(new { msg = TM.Core.Common.Message.error_token.ToString() });
                    var now = DateTime.Now;
                    var qry = "BEGIN ";
                    foreach (var item in data)
                        qry += $"update Items set flag={item.flag},deleted_ip='{TM.Core.HttpContext.Header("LocalIP")}',deleted_by='{nd.ma_nd}',deleted_at={now.ParseDateTime()} where id='{item.id}';\r\n";
                    qry += "END;";
                    await db.Connection.QueryAsync(qry);
                    await db.Connection.QueryAsync("COMMIT");
                    return Json(new { msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Remove([FromBody] List<Models.Core.Items> data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle())
                {
                    if (data.Count > 0) await db.Connection.DeleteAsync(data);
                    return Json(new { data = data, msg = TM.Core.Common.Message.success.ToString() });
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        public void ImportData(List<template_import> data, List<string[]> csv, int index, string error)
        {
            var tmp = new template_import();
            var length = csv[index].Length;
            tmp.ma_tb = length > 0 ? csv[index][0].Trim() : "";
            tmp.ten_tb = length > 1 ? csv[index][1].Trim() : "";
            tmp.diachi_tb = length > 2 ? csv[index][2].Trim() : "";
            tmp.so_dt = length > 3 ? csv[index][3].Trim() : "";
            tmp.ma_nd = length > 4 ? csv[index][4].Trim() : "";
            tmp.ghichu = length > 5 ? csv[index][5].Trim() : "";
            tmp.ct_loi = error;
            data.Add(tmp);
        }
        public partial class Paging : TM.Core.Common.Paging
        {
            public List<int> donvi_id { get; set; }
            public List<int> nhomkh_id { get; set; }
            public List<string> ma_nd { get; set; }
            public int? ket_qua { get; set; }
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
            public string ma_nd { get; set; }
            public string ghichu { get; set; }
            public string ct_loi { get; set; }
        }
        public partial class template_update_import
        {
            public string ma_tb { get; set; }
            public string ma_nd { get; set; }
            public string ct_loi { get; set; }
        }
        public partial class nguoi_dung
        {
            public int donvi_id { get; set; }
            public string ma_nd { get; set; }
            public string ten_nd { get; set; }
            public string ma_nv { get; set; }
            public string ten_nv { get; set; }
            public string ten_nd_dv { get; set; }
            public string ma_dv { get; set; }
            public string ten_dv { get; set; }
            public string ten_quyen { get; set; }
        }
        public partial class request_thuebao_th
        {
            public string id { get; set; }
            public string ma_tb { get; set; }
        }
        public partial class kehoach_TH : Models.Core.Kehoach_TB
        {

            // public string kehoachtb_id { get; set; }
            public DateTime? ngay_th { get; set; }
            public int ket_qua { get; set; }
            public string de_xuat { get; set; }
            public string goicuoc { get; set; }
            public string lydo { get; set; }
            public string ghichu_th { get; set; }
            public string nguoi_cn_th { get; set; }
            public string ip_cn_th { get; set; }
            public string ngay_cn_th { get; set; }
        }
    }
}