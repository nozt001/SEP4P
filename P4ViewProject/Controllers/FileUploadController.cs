using CsvHelper;
using P4ViewProject.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using P4ViewProject.Models;

namespace P4ViewProject.Controllers
{
    public class FileUploadController : Controller
    {
        public SQLServerConnClass sqlConn = new SQLServerConnClass();

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
           // if (file != null) {}
                
            //Creaate the file path on the server
            var path = Path.Combine(Server.MapPath("~/App_Data/UploadedFiles//"), file.FileName);
            
            //Reading the file with the stream
            var data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);

            //writing the data to file
            using (var sw = new FileStream(path, FileMode.Create))
            {
                sw.Write(data, 0, data.Length);
            }

            return RedirectToAction("Index");
        }


        public ActionResult ViewCsvData(string filename)
        {
            //Creaate the file path on the server
            var csvFile = Server.MapPath("~/App_Data/UploadedFiles/" + filename);
            return View(new ViewDataViewModel(filename));
        }

        
        public ActionResult Delete(string filename)
        {
            // delete the file 
            deleteCSVFile(filename, string.Empty);

            return RedirectToAction("Index");
        }

        private string deleteCSVFile(string filename, string caller)
        {
            //Creaate the file "DONE_" 's path on the server
            string donefile = Request.MapPath("~/App_Data/UploadedFiles/" + "DONE_" + filename);

            var delFilePath = Request.MapPath("~/App_Data/UploadedFiles/" + filename);

            if (System.IO.File.Exists(delFilePath))
            {
                try
                {
                    if (caller == "insertdb")
                    {
                        // If this method is called after insert into DB, copy the file with DONE_ prefix
                        // and delete the original one
                        System.IO.File.Copy(delFilePath, donefile, true);
                    }
                    System.IO.File.Delete(delFilePath);
                    return "File Deleted";
                }
                catch (IOException e)
                {
                    return "Exception: " + e;
                }
                
            }

            return "File does not exist!";
        }

        public ActionResult ShowGraph()
        {
            return View();
        }

        /**
        * this method first designed to insert a csv file into DB 
        * later was changed to show the csv file structure and corresponding db table
        * the insert procedure defined to be called from two screen to allow for 
        * a single insert and whole tables insert
        **/
        public ActionResult ImportCsvToDb(string filename)
        {

            SqlCommand cmd = new SqlCommand();
            SqlConnection SqlConnection = new SqlConnection();
            DataTable schemaTable = new DataTable();
            SqlDataReader myReader;
            List<TableInfo> tableInfos = new List<TableInfo>();


            SqlConnection.ConnectionString = ConfigurationManager.ConnectionStrings["ViewSimulation"].ConnectionString;
            SqlConnection.Open();
            string fname = filename.Substring(0, filename.LastIndexOf(".csv"));

            cmd.Connection = SqlConnection;
            cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE , CHARACTER_MAXIMUM_LENGTH" 
                             + " FROM INFORMATION_SCHEMA.COLUMNS "
                             + " WHERE TABLE_NAME= ViewSimulation." + "PATIENT"
                             + " ORDER BY ORDINAL_POSITION";

            myReader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
            schemaTable = myReader.GetSchemaTable();

            if (myReader.HasRows)
            {
                string maxLength = "";
                while (myReader.Read())
                {

                    try
                    {
                        maxLength = myReader.GetInt32(2).ToString();
                    }
                    catch (System.Data.SqlTypes.SqlNullValueException e)
                    {
                        maxLength = "NULL";
                    }
                                        
                    tableInfos.Add(new TableInfo(){ 
                        ColumnName = myReader.GetString(0),
                        DataType = myReader.GetString(1),
                        MaxLength = maxLength});

                    System.Diagnostics.Debug.WriteLine("{0} \t {1} \t {2}",
                         myReader.GetString(0), myReader.GetString(1), maxLength);
                }
            }

            myReader.Close();
            SqlConnection.Close();
            ViewBag.TableInfo = tableInfos;

            return View(new ViewDataViewModel(filename));

        }

        public string InsertOneFile(string filename)
        {
            InsertCsvToDb(filename) ;
            deleteCSVFile(filename, "insertdb");

            return "Return the FeedBack";

            /* TODO Add appropriate feedback
             * 
             */
        }

        /* TODO (After having a discussion with Jim )
         * TODO implement the insertion of all of the files at once into the DB
         * **/
        public string InsertListOfFiles(List<string> filenamesList)
        {
            foreach (var fname in filenamesList)
            {
                InsertCsvToDb(fname);
            }

            /* ToDo Add appropriate feedback
             */
            return "Return Feedback!";
        }

        public string InsertCsvToDb(string filename)
        { 
            var csvFilePath = Request.MapPath("~/App_Data/UploadedFiles/" + filename);
            string fileName = Path.GetFileName(filename);

            // Set up DataTable place holder
            DataTable dt = new DataTable();

            try
            {
                //Process the CSV file and capture the results to our DataTable place holder
                dt = ProcessCSV(csvFilePath);

                //Process the DataTable and capture the results to our SQL Bulk copy
                ViewData["Feedback"] = ProcessBulkCopy(dt, filename);
            }
            catch (Exception ex)
            {
                //Catch errors
                ViewData["Feedback"] = ex.Message;
            }

            //Tidy up
            dt.Dispose();

            return "The result of operation";
            //return View("ImportCsvToDb", ViewData["Feedback"]);

        }

        private static DataTable ProcessCSV(string fileName)
        {
            //Set up our variables
            string Feedback = string.Empty;
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            DataRow row;

            // work out where we should split on comma, but not in a sentence
            Regex r = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            //Set the filename in to our stream
            StreamReader sr = new StreamReader(fileName);

            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            //we add the temporal dates here before dynamically adding the columns
            line = line + ",ValidFrom" + ",ValidTo";

            //splite the line
            strArray = r.Split(line);

            //For each item in the new split array, dynamically builds our Data columns. Save us having to worry about it.
            Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));

            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                row = dt.NewRow();
                //we add the temporal dates here before dynamically adding the columns
                line = line + "," + DateTime.Today.ToString() + "," + "01/01/9999";

                //add our current value to our data row
                row.ItemArray = r.Split(line);
                dt.Rows.Add(row);
            }

            //Tidy Streameader up
            sr.Dispose();

            //return a the new DataTable
            return dt;
        }

        private  String ProcessBulkCopy(DataTable dt, string filename)
        {   

            sqlConn.commandExecution("Insert ");
            string Feedback = string.Empty;
            string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            //make our connection and dispose at the end
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // make our command and dispose at the end
                using (var copy = new SqlBulkCopy(conn))
                {

                    // Open our connection
                    conn.Open();

                    // Set target table and tell the number of rows
                    // Cutting the file extension out from the file name to match the table name
                    string fname = filename.Substring(0, filename.LastIndexOf(".csv"));
                    copy.DestinationTableName = fname;
                    copy.BatchSize = dt.Rows.Count;
                    try
                    {
                        // Send it to the server
                        copy.WriteToServer(dt);
                        Feedback = "Upload complete";
                    }
                    catch (Exception ex)
                    {
                        Feedback = ex.Message;
                    }
                }
            }

            return Feedback;
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

