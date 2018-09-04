using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
namespace VNPTBKN.API.Controllers {
    [Route("api/[controller]")]
    [ApiController, Microsoft.AspNetCore.Authorization.Authorize]
    public class GroupsController : Controller {
        private TM.Core.Connection.Oracle db;
        [HttpGet]
        public async Task<IActionResult> Get() {
            try {
                db = new TM.Core.Connection.Oracle();
                var qry = "select * from test";
                var data = await db.Connection.QueryAsync(qry);
                db.Close();
                return Json(new { data = data, message = new { type = "success", text = "Lấy dữ liệu thành công!" } });
            } catch (System.Exception ex) {
                return Json(new { danger = ex.Message });
            }
        }

        public async Task<IActionResult> Post([FromBody] Models.Core.Groups data) {
            try {
                db = new TM.Core.Connection.Oracle();
                // data.create_by = Helpers.HttpContext.Header();
                // data.create_date = DateTime.Now;
                // await db.Connection.InsertAsync<Models.Core.group>(data);
                db.Close();
                return Json(new { data = data, message = new { type = "success", text = "Cập nhật dữ liệu thành công!" } });
            } catch (System.Exception ex) {
                return Json(new { message = new { type = "danger", text = ex.Message } });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Models.Core.Groups data) {
            try {
                // List<Models.IFormFile> file_upload
                // [FromBody] Models.Test data
                db = new TM.Core.Connection.Oracle();
                // var _data = await db.Connection.GetAsync<Models.Core.group>(data.id);
                // if (_data != null) {
                    // _data.code = string.IsNullOrEmpty(data.code) ? _data.code : data.code;
                    // _data.type = string.IsNullOrEmpty(data.type) ? _data.type : data.type;
                    // _data.name = string.IsNullOrEmpty(data.name) ? _data.name : data.name;
                    // _data.descs = string.IsNullOrEmpty(data.descs) ? _data.descs : data.descs;
                    // _data.contents = string.IsNullOrEmpty(data.contents) ? _data.contents : data.contents;
                    // _data.images = data.images;
                    // _data.update_by = Helpers.HttpContext.Header();
                    // _data.last_update = DateTime.Now;
                    // _data.status = data.status;
                    // db.Connection.Update(_data);
                    // db.Close();
                // }
                return Json(new { message = new { type = "success", text = "Cập nhật dữ liệu thành công!" } });
            } catch (System.Exception ex) {
                return Json(new { message = new { type = "danger", text = ex.Message } });
            }
        }

        [HttpPut("delete")]
        public async Task<IActionResult> Delete([FromBody] List<Models.Core.Groups> data) {
            try {
                db = new TM.Core.Connection.Oracle();
                var rs = new List<Models.Core.Groups>();
                foreach (var item in data) {
                    // var _data = await db.Connection.GetAsync<Models.Core.group>(item.id);
                    // if (_data != null) {
                    //     // _data.status = item.status;
                    //     rs.Add(_data);
                    // }
                }
                // if (rs.Count > 0) await db.Connection.UpdateAsync(rs);
                return Json(new { data = rs, message = new { type = "success", text = "Cập nhật dữ liệu thành công!" } });
            } catch (System.Exception ex) {
                return Json(new { message = new { type = "danger", text = ex.Message } });
            }
        }

        [HttpDelete]
        public void Remove([FromBody] string id) {

        }

        [HttpPut("quickedit")]
        public async Task<IActionResult> QuickEdit([FromBody] Models.Core.QuickEdit data) {
            try {
                db = new TM.Core.Connection.Oracle();
                var qry = $"UPDATE Groups SET {data.column}=N'{data.value}' WHERE id='{data.id}'";
                await db.Connection.QueryAsync(qry);
                return Json(new { message = new { type = "success", text = "Cập nhật dữ liệu thành công!" } });
            } catch (System.Exception ex) {
                return Json(new { message = new { type = "danger", text = ex.Message } });
            }
        }

    }
}