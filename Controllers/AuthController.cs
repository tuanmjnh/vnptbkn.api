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
                return Json(new { message = new { type = "success", text = "Success!" } });
            }
            catch (System.Exception)
            {
                return Json(new { message = new { type = "danger", text = "Fail!" } });
            }
        }

        [HttpPost, Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] Authentication.Core.DBNguoidung data)
        {
            try
            {
                var Authorization = TM.Core.HttpContext.Http.Request.Headers.TryGetValue("Authorization", out authorizationToken); ;
                var Author = TM.Core.HttpContext.Http.Request.Headers["Author"].ToString();
                // db_nguoidung
                var qry = $"select * from db_nguoidung where ma_nd='{data.ma_nd}'";
                data = await db.Connection().QueryFirstOrDefaultAsync<Authentication.Core.DBNguoidung>(qry);
                if (data == null) return Json(new { message = "null" });
                // nguoidung
                qry = $"select db.*,nd.*,r.roles from ttkd_bkn.db_nguoidung db,ttkd_bkn.nguoidung nd,ttkd_bkn.roles r where db.nguoidung_id=nd.nguoidung_id(+) and nd.roles_id=r.id(+) and db.nguoidung_id={data.nguoidung_id}";
                var user = await db.Connection().QueryFirstOrDefaultAsync<Authentication.Core.nguoidung_auth>(qry);
                if (user == null) return Json(new { message = "null" });

                //Password wrong
                // data.matkhau = TM.Core.Encrypt.MD5.CryptoMD5TM(data.matkhau + user.salt);
                // if (user.matkhau != data.matkhau)
                //     return Json(new { message = "false" });

                //Account is locked
                if (data.trangthai < 1)
                    return Json(new { message = "locked" });
                // Roles
                // qry = $"select * from user_role where user_id='{user.user_id}'";
                // var roles = await db.Connection().QueryAsync(qry);
                // Token
                user.token = BuildToken(user);
                //Update last login
                user.last_login = DateTime.Now;
                qry = $"update ttkd_bkn.nguoidung set last_login={user.last_login.Value.ParseDateTime()},token='{user.token}' where nguoidung_id={user.nguoidung_id}";
                await db.Connection().QueryAsync(qry);
                //await db.Connection().UpdateAsync(user);
                return Json(new { data = user, message = "success" });
            }
            catch (System.Exception)
            {
                return Json(new { message = "danger" });
            }
        }
        private string BuildToken(Authentication.Core.nguoidung_auth user)
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

        [HttpGet]
        public IActionResult Get(string id)
        {
            try
            {
                var _data = "tuanmjnh";
                return Json(new { data = _data, message = new { type = "success", text = "Cập nhật dữ liệu thành công!" } });
            }
            catch (System.Exception ex)
            {
                return Json(new { message = new { type = "danger", text = ex.Message } });
            }
        }
    }
}