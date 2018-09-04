using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
namespace VNPTBKN.API.Controllers {
    // [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController] //, Microsoft.AspNetCore.Authorization.Authorize]
    public class FileManagerController : Controller {
        string rootPath = "";
        public FileManagerController() {
            // Helpers.HttpContext.baseUrl;
            rootPath = TM.Core.HttpContext.WebRootPath;
        }

        [HttpGet]
        public IActionResult Get(string basePath = "Uploads", string subPath = "", string extension = "") {
            try {
                var data = new List<FileManagerObject>();
                var path = $"{basePath}";
                if (subPath.Length > 0) path = $"{path}/{subPath}";
                var Dir = new DirectoryInfo($"{rootPath}/{path}"); // collection["path"].ToString()
                TM.Core.IO.CreateDirectory($"{rootPath}/{path}");
                var subDir = Dir.GetDirectories();
                foreach (var item in subDir) {
                    var _file = new FileManagerObject();
                    _file.id = Guid.NewGuid().ToString("N");
                    _file.parent = "";
                    _file.root = basePath;
                    _file.sub_directory = subPath;
                    _file.levels = 0;
                    _file.name = item.Name;
                    _file.full_name = $"{path}/{item.Name}";
                    // _file.url = $"{basePath}/{path}/{item.Name}";
                    _file.extension = item.Extension.ToLower();
                    _file.extension_icon = "fa fa-folder-o";
                    _file.type = "directory";
                    _file.attributes = item.Attributes.ToString();
                    _file.attributes_id = 0;
                    _file.length = 0;
                    _file.isreadonly = false;
                    _file.creation_time = item.CreationTime;
                    _file.creation_time_utc = item.CreationTimeUtc;
                    _file.last_access_time = item.LastAccessTime;
                    _file.last_access_time_utc = item.LastAccessTimeUtc;
                    _file.last_write_time = item.LastWriteTime;
                    _file.last_write_time_utc = item.LastWriteTimeUtc;
                    _file.exists = item.Exists;
                    data.Add(_file);

                }
                var subFiles = Dir.GetFiles();
                foreach (var item in subFiles) {
                    var _file = new FileManagerObject();
                    _file.id = Guid.NewGuid().ToString("N");
                    _file.parent = "";
                    _file.root = basePath;
                    _file.sub_directory = subPath;
                    _file.levels = 0;
                    _file.name = item.Name;
                    _file.full_name = $"{path}/{item.Name}";
                    // _file.url = $"{basePath}/{path}/{item.Name}";
                    _file.extension = item.Extension;
                    _file.extension_icon = "fa fa-file-o";
                    _file.type = "file";
                    _file.attributes = item.Attributes.ToString();
                    _file.attributes_id = 0;
                    _file.length = item.Length;
                    _file.isreadonly = false;
                    _file.creation_time = item.CreationTime;
                    _file.creation_time_utc = item.CreationTimeUtc;
                    _file.last_access_time = item.LastAccessTime;
                    _file.last_access_time_utc = item.LastAccessTimeUtc;
                    _file.last_write_time = item.LastWriteTime;
                    _file.last_write_time_utc = item.LastWriteTimeUtc;
                    _file.exists = item.Exists;

                    if (extension != "" && extension == item.Extension) {
                        data.Add(_file);
                    } else {
                        data.Add(_file);
                    }
                }
                return Json(new { files = data, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPost]
        public IActionResult Upload(IFormCollection collection) {
            try {
                var rs = new List<FileManagerObject>();
                var files = collection.Files;
                var basePath = collection["basePath"].ToString();
                basePath = string.IsNullOrEmpty(basePath) ? "Uploads" : basePath;
                var subPath = collection["subPath"].ToString().Trim('/');
                if (files.Count > 0) {
                    //Create Directory Upload
                    var path = $"{basePath}";
                    TM.Core.IO.CreateDirectory($"{rootPath}/{path}", false);
                    if (path.Length > 0) {
                        path = string.IsNullOrEmpty(subPath) ? $"{path}" : $"{path}/{subPath}";
                        TM.Core.IO.CreateDirectory($"{rootPath}/{path}", false);
                    }
                    //Upload File
                    for (int i = 0; i < files.Count; i++) {
                        if (files[i].Length < 1) continue;
                        var a = ContentDispositionHeaderValue.Parse(files[i].ContentDisposition);
                        var filename = ContentDispositionHeaderValue.Parse(files[i].ContentDisposition).FileName.ToString().Trim('"');
                        string filename_full = $"{rootPath}/{path}/{filename}";
                        using(FileStream fs = System.IO.File.Create(filename_full)) {
                            files[i].CopyTo(fs);
                            fs.Flush();
                        }
                        rs.Add(new FileManagerObject {
                            id = Guid.NewGuid().ToString("N").ToUpper(),
                                name = filename,
                                full_name = $"{path}/{filename}",
                                root = basePath,
                                sub_directory = subPath,
                                length = files[i].Length,
                                extension = System.IO.Path.GetExtension(filename)
                        });
                        // Extract
                        // if (Helpers.IO.isCompress(filename_full)) {
                        //     var compress = new Helpers.SharpCompress($"{rootPath}/{path}/");
                        //     compress.Extract($"{rootPath}/{path}/{filename}");
                        // }
                        // System.IO.Compression.ZipFile.CreateFromDirectory( $"{rootPath}/{path}/test", $"{rootPath}/{path}/test.zip");
                    }
                }
                return Json(new { files = rs, message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPost("create")]
        public IActionResult Create(IFormCollection collection) {
            try {
                return Json(new { message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPost("update")]
        public IActionResult Update(IFormCollection collection) {
            try {
                return Json(new { message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }

        [HttpPost("delete")]
        public IActionResult Delete(IFormCollection collection) {
            try {
                return Json(new { message = "success" });
            } catch (System.Exception) { return Json(new { message = "danger" }); }
        }
        public class FileUpload {
            public string id { get; set; }
            public string originalName { get; set; }
            public string url { get; set; }
            public string attach { get; set; }
        }
        public class FileManagerObject {
            public string id { get; set; }
            public string parent { get; set; }
            public string root { get; set; }
            public string sub_directory { get; set; }
            public int? levels { get; set; }
            public string name { get; set; }
            public string full_name { get; set; }
            public string extension { get; set; }
            public string extension_icon { get; set; }
            public string type { get; set; }
            public string attributes { get; set; }
            public int? attributes_id { get; set; }
            public long? length { get; set; }
            public bool? isreadonly { get; set; }
            public string description { get; set; }
            public DateTime? creation_time { get; set; }
            public DateTime? creation_time_utc { get; set; }
            public DateTime? last_access_time { get; set; }
            public DateTime? last_access_time_utc { get; set; }
            public DateTime? last_write_time { get; set; }
            public DateTime? last_write_time_utc { get; set; }
            public bool? exists { get; set; }
        }
    }
}