using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VNPTBKN.API.Common;
namespace VNPTBKN.API.Controllers
{
    // [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private IConfiguration _config;
        private Microsoft.Extensions.Primitives.StringValues authorizationToken;
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("[action]"), Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult CheckToken()
        {
            try
            {
                return Json(new { msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception)
            {
                return Json(new { msg = TM.Core.Common.Message.danger.ToString() });
            }
        }

        [HttpPost, Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] Authentication.Core.nguoidung_login data)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle("DHSX"))
                {
                    // var Authorization = TM.Core.HttpContext.Http.Request.Headers.TryGetValue("Authorization", out authorizationToken);
                    // var Author = TM.Core.HttpContext.Http.Request.Headers["Author"].ToString();
                    // db_nguoidung
                    var qry = "select nd.nguoidung_id \"nguoidung_id\",";
                    qry += "nd.ma_nd \"ma_nd\",";
                    qry += "nd.matkhau \"matkhau\",";
                    qry += "css_bkn.giaima_mk(nd.matkhau) \"giaima_mk\",";
                    qry += "nd.trangthai \"trangthai\" ";
                    qry += "from admin_bkn.nguoidung nd,";
                    qry += "ttkd_bkn.nguoidung tnd,";
                    qry += "ttkd_bkn.roles r ";
                    qry += "where nd.nguoidung_id=tnd.nguoidung_id(+) ";
                    qry += "and tnd.roles_id=r.id(+) ";
                    qry += $"and nd.ma_nd=:ma_nd";
                    var nguoidung = await db.Connection.QueryFirstOrDefaultAsync(qry, new { ma_nd = data.ma_nd });
                    if (nguoidung == null) return Json(new { msg = TM.Core.Common.Message.exist.ToString() });

                    // Password wrong
                    // if (user.matkhau != TM.Core.Encrypt.MD5.CryptoMD5TM(data.matkhau + user.salt))
                    //     return Json(new { msg = TM.Core.Common.Message.wrong.ToString() });
                    if (nguoidung.giaima_mk != data.matkhau)
                        return Json(new { msg = TM.Core.Common.Message.wrong.ToString() });

                    //Account is locked
                    if (nguoidung.trangthai < 1)
                        return Json(new { msg = TM.Core.Common.Message.locked.ToString() });
                    // Roles
                    // qry = $"select * from user_role where user_id='{user.user_id}'";
                    // var roles = await db.Connection().QueryAsync(qry);
                    // Token
                    nguoidung.token = BuildToken();
                    //Update last login
                    // nguoidung.last_login = DateTime.Now;
                    qry = $"update ttkd_bkn.nguoidung set last_login={DateTime.Now.ParseDateTime()},token='{nguoidung.token}' where nguoidung_id={nguoidung.nguoidung_id}";
                    await db.Connection.QueryAsync(qry);
                    var user = await db.Connection.QueryFirstOrDefaultAsync(userQuery(), new { nguoidung_id = nguoidung.nguoidung_id });
                    //await db.Connection().UpdateAsync(user);
                    return Json(user);
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }
        private string BuildToken()
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("{id:int}"), Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                using (var db = new TM.Core.Connection.Oracle("DHSX"))
                {
                    var user = await db.Connection.QueryFirstOrDefaultAsync(userQuery(), new { nguoidung_id = id });
                    return Json(user);
                }
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
            finally { }
        }

        private string userQuery()
        {
            var qry = "select nd.nguoidung_id \"nguoidung_id\",nd.ma_nd \"ma_nd\",nd.ten_nd \"ten_nd\",nd.quantri \"quantri\",";
            qry += "nd.nhanvien_id \"nhanvien_id\",nd.nhom_nd_id \"nhom_nd_id\",nd.trangthai \"trangthai\",nd.ngoaile \"ngoaile\",";
            qry += "nv.ma_nv \"ma_nv\",nv.ten_nv \"ten_nv\",nv.diachi_nv \"diachi_nv\",nv.so_dt \"so_dt\",nv.gioitinh \"gioitinh\",";
            qry += "nv.chucdanh \"chucdanh\",nv.ngay_sn \"ngay_sn\",nv.ten_tn \"ten_tn\",tnd.token \"token\",r.name \"ten_quyen\",r.roles \"quyen\" ";
            qry += "from admin_bkn.nguoidung nd,admin_bkn.nhanvien nv,ttkd_bkn.nguoidung tnd,ttkd_bkn.roles r ";
            qry += $"where nd.nguoidung_id=tnd.nguoidung_id(+) and nd.nhanvien_id=nv.nhanvien_id and tnd.roles_id=r.id(+) and nd.nguoidung_id=:nguoidung_id";
            return qry;
        }
    }
}