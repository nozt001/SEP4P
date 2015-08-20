using CsvHelper;
using P4ViewProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P4ViewProject.Controllers
{
    public class FileUploadController : Controller
    {
        public ActionResult Index()
        {
            var path = Server.MapPath("~/App_Data/UploadedFiles/");

            var dir = new DirectoryInfo(path);

            var files = dir.EnumerateFiles().Select(f => f.Name);
            //List<Filecsv> files = dir.EnumerateFiles().Select(f => f.Name);

            return View(files);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            var path = Path.Combine(Server.MapPath("~/App_Data/UploadedFiles//"), file.FileName);

            var data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);

            using (var sw = new FileStream(path, FileMode.Create))
            {
                sw.Write(data, 0, data.Length);
            }

            return RedirectToAction("Index");
        }


        public ActionResult ViewCsvData(string filename)
        {
            var csvFile = Server.MapPath("~/App_Data/UploadedFiles/" + filename);
            return View(new ViewDataViewModel(filename));
        }

        
        public ActionResult Delete(string filename)
        {

            var delFilePath = Request.MapPath("~/App_Data/UploadedFiles/" + filename);

            if (System.IO.File.Exists(delFilePath))
            {
                System.IO.File.Delete(delFilePath);
            }
            return RedirectToAction("Index");
        }

        public ActionResult ShowGraph()
        {
            return View();
        }

    }

    public class ViewDataViewModel
    {
        public string Filename { get; set; }

        public ViewDataViewModel(string filename)
        {
            Filename = filename;
        }
    }
}

