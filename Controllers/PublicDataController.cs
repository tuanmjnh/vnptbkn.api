using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TM.Core.Helpers.TMString;
using VNPTBKN.API.Common;
namespace VNPTBKN.API.Controllers {
    [Route("api/public_data")]
    [ApiController]
    public class PublicDataController : Controller {

        [HttpGet("[action]")]
        public async Task<IActionResult> GetLanguages() {
            try {
                var qry = $"select * from Languages";
                var data = await db.Connection().QueryAsync(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]/{key}")]
        public async Task<IActionResult> GetDictionary(string key) {
            try {
                var qry = $"select module_code,key,value from Dictionary where lower(lang_code)='{key.ToLower()}' order by lang_code,module_code,key";
                var data = await db.Connection().QueryAsync<Dictionary>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetModules() {
            try {
                var qry = $"select * from Modules";
                var data = await db.Connection().QueryAsync(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetNavigation() {
            try {
                var qry = $"select * from Navigation";
                var data = await db.Connection().QueryAsync<Navigation>(qry);
                return Json(new { data = data, msg = "success" });
            } catch (System.Exception) { return Json(new { msg = "danger" }); }
        }
        private class Dictionary {
            public string module_code { get; set; }
            public string key { get; set; }
            public string value { get; set; }
        }
        private class Navigation {
            public string module_code { get; set; }
            public string key { get; set; }
            public string value { get; set; }
        }
    }
}