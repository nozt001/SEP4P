using CsvHelper;
using P4ViewProject.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P4ViewProject.Controllers
{
    // [Authorize]
    public class FileUploadController : Controller
    {

        private static string path = Constants.CsvFilesPath;

        public ActionResult Index()
        {

            var dir = new DirectoryInfo(Server.MapPath(path));

            var files = dir.EnumerateFiles().Select(f => f.Name);
            //List<Filecsv> files = dir.EnumerateFiles().Select(f => f.Name);

            return View(files);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
           // if (file != null) {}
                
            //Reading the file with the stream
            var data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);

            //Creaate the file path on the server
            //writing the data to file
            using (var sw = new FileStream(Path.Combine(Server.MapPath(path), file.FileName), FileMode.Create))
            {
                sw.Write(data, 0, data.Length);
            }

            return RedirectToAction("Index");
        }

        public string GetFileRowsNumber(string filename)
        {
            CsvFileController cnt = new CsvFileController();
            return cnt.GetCsvRowsNumber(filename).ToString();
        }

        public ActionResult ViewCsvData(string filename)
        {
            //Creaate the file path on the server
            var csvFile = Server.MapPath(Constants.CsvFilesPath + filename);
            return View(new ViewDataViewModel(filename));
        }

        public string ReadCsvFile(string filename, int start = 0, int end = 0)
        {
            CsvFileController cnt = new CsvFileController();
            return cnt.GetCsvAsJson(filename, start, end);
        }

        
        public ActionResult Delete(string filename)
        {
            var delFilePath = Request.MapPath(Constants.CsvFilesPath + filename);

            if (System.IO.File.Exists(delFilePath))
            {
                try
                {
                    System.IO.File.Delete(delFilePath);
                    //return "File Deleted";
                }
                catch (IOException e)
                {
                    //return "Exception: " + e;
                }

            }


            return RedirectToAction("Index");
        }


        /**
        * this method first designed to insert a csv file into DB 
        * later was changed to show the csv file structure and corresponding db table
        * the insert procedure defined to be called from two screen to allow for 
        * a single insert and whole tables insert
        **/
        public ActionResult ImportCsvToDb(string filename)
        {

            SqlConnection SqlConnection = new SqlConnection();
            DataTable schemaTable = new DataTable();
            List<TableInfo> tableInfos = new List<TableInfo>();
            String[] tableRestriction = new string[4];
            string fname = filename.Substring(0, filename.LastIndexOf(".csv"));

            // For the array -
            // 0-represents Catalog, 1-Schema, 2-TableName, 3-table type
            // restrict to get our filename schema not all
            tableRestriction[2] = fname;
            SqlConnection.ConnectionString =
                 ConfigurationManager.ConnectionStrings[Constants.dbName].ConnectionString;

            SqlConnection.Open();
            DataTable fnameTable = SqlConnection.GetSchema("Columns", tableRestriction);

            var selectedRows = from info in fnameTable.AsEnumerable()
                select new
                {
                    TableCatalog = info["TABLE_CATALOG"],
                    TableSchema = info["TABLE_SCHEMA"],
                    TableName = info["TABLE_NAME"],
                    ColumnName = info["COLUMN_NAME"],
                    DataType = info["DATA_TYPE"],
                    CharacterMax = info["CHARACTER_MAXIMUM_LENGTH"]
                };
            foreach (var row in selectedRows)
            {
                tableInfos.Add( new TableInfo
                {
                    ColumnName = row.ColumnName.ToString(),
                    DataType = row.DataType.ToString(),
                    MaxLength = row.CharacterMax.ToString()
                });

            }

            SqlConnection.Close();
            ViewBag.TableInfo = tableInfos;//tableInfos(fname);

            return View(new ViewDataViewModel(filename));

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

