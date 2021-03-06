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
    public class AuthController_old : Controller
    {
        private IConfiguration _config;
        private Microsoft.Extensions.Primitives.StringValues authorizationToken;
        public AuthController_old(IConfiguration config)
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
        public async Task<IActionResult> Post([FromBody] Authentication.Core.Users data)
        {
            try
            {
                // var Authorization = TM.Core.HttpContext.Http.Request.Headers.TryGetValue("Authorization", out authorizationToken); ;
                // var Author = TM.Core.HttpContext.Http.Request.Headers["Author"].ToString();
                var qry = $"select * from users where username='{data.username}'";
                //AuthDB
                var user = await db.Connection().QueryFirstOrDefaultAsync<Authentication.Core.Users>(qry);

                //Account not Exist
                if (user == null)
                    return Json(new { msg = TM.Core.Common.Message.exist.ToString() });

                //Password wrong
                data.password = TM.Core.Encrypt.MD5.CryptoMD5TM(data.password + user.salt);
                if (user.password != data.password)
                    return Json(new { msg = TM.Core.Common.Message.wrong.ToString() });

                //Account is locked
                if (user.flag != 1)
                    return Json(new { msg = TM.Core.Common.Message.locked.ToString() });
                // Roles
                qry = $"select * from user_role where user_id='{user.id}'";
                var roles = await db.Connection().QueryAsync(qry);
                // Token
                var tokenString = BuildToken(user);

                //Update last login
                user.last_login = DateTime.Now;
                await db.Connection().UpdateAsync(user);
                return Json(new { data = user, token = tokenString, roles = roles, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception) { return Json(new { msg = TM.Core.Common.Message.danger.ToString() }); }
        }
        private string BuildToken(Authentication.Core.Users user)
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
                return Json(new { data = _data, msg = TM.Core.Common.Message.success.ToString() });
            }
            catch (System.Exception)
            {
                return Json(new { msg = TM.Core.Common.Message.danger.ToString() });
            }
        }
    }
}