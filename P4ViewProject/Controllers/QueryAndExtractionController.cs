using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P4ViewProject.Models;
using System.Data;
using System.Text;
using System.IO;
using P4ViewProject.DAL;
using System.Web.Script.Serialization;

namespace P4ViewProject.Controllers
{
    public class QueryAndExtractionController : Controller
    {

        private SQLServerConnClass sqlConn = new SQLServerConnClass();
        public static ViewExtractedDataWrapperModel data = new ViewExtractedDataWrapperModel();
        DataExtractionContext context = new DataExtractionContext();
        public static string select, from, where, otherClauses, queryDate;

        // GET: QueryAndExtraction
        [HttpGet]
        public ActionResult Index()
        {
            Tuple<string, Dictionary<string, List<string>>> dbNameAndTableData = sqlConn.getTablesWithDatabaseName();
            Dictionary<string, List<string>> tableDict = dbNameAndTableData.Item2;
            ViewBag.DatabaseName = dbNameAndTableData.Item1;
            ViewBag.TableDict = tableDict;

            return View();
        }

        [HttpGet]
        public ActionResult AnalyseResults() {

            if (data.Data == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                Dictionary<string, List<string>> resultDict = new Dictionary<string, List<string>>();

                List<string> listOfAttributes = new List<string>();

                foreach (DataColumn col in data.Data.Columns) {
                    listOfAttributes.Add(col.ColumnName);
                }

                resultDict.Add("Result_Table", listOfAttributes);
                ViewBag.resultTableDict = resultDict;

                return View();
            }
        }

        [HttpPost]
        public PartialViewResult ViewData(string selectText, string fromText, string whereText, string otherClauseText, string startDate) {

            if (startDate.Equals("")) {
                startDate = DateTime.Now.ToString("M/d/yyyy");
            }

            string query = "Select " + selectText;
            query += " From " + fromText;
            query += " Where " + whereText + " ";

            List<string> fromTables = fromText.Split(',').ToList();
            bool whereTextExists = (whereText.Equals("")?false:true);
            bool flagWhereText = false;

            foreach (String tableName in fromTables) {
                if (whereTextExists || flagWhereText)
                {
                    query += "AND ";
                }
                query += getTableTemporalString(tableName.Trim(), startDate);
                flagWhereText = true;
            }

            query += " " + otherClauseText;
            query += ";";

            sqlConn.retrieveData(query);
            data.Data = sqlConn.SqlTable;

            //Temp save extract code

            select = selectText;
            from = fromText;
            where = whereText;
            queryDate = startDate;
            otherClauses = otherClauseText;



            //

            //int numRows = 0;
            //ViewBag.numResultRows = 0;
            //if (data.Data != null) {
            //    ViewBag.numResultRows = data.Data.Rows.Count; 
            //    numRows = data.Data.Rows.Count;
            //}


            //Tuple<PartialViewResult, int> myResult = new Tuple<PartialViewResult, int>(PartialView("ViewDataTable", data), numRows);

            return PartialView("ViewDataTable", data);
            //return sqlConn.SqlTable.Rows.Count.ToString();
        }

        [HttpPost]
        public string CsvData(string csvData, string title, string description) {

            title = title.Trim();

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = "myCsv.csv";

            StringBuilder sb = new StringBuilder();
            sb.Append(csvData);
            // Write the stream contents to a new file named "AllTxtFiles.txt".
            using (StreamWriter outfile = new StreamWriter(path+ @"\myCsv.csv"))
            {
                outfile.Write(sb.ToString());
            }

            ExtractionData extraction = new ExtractionData();
            extraction.Name = title;
            extraction.Description = description;
            extraction.SelectBox = select;
            extraction.FromBox = from;
            extraction.WhereBox = where;
            extraction.QueryDate = queryDate;
            extraction.OtherClausesBox = otherClauses;

            context.DataExtracts.Add(extraction);
            context.SaveChanges();

            return path+fileName;
        }

        private String getTableTemporalString(string tableName, string startDate) {
            string tableTempString = "((" + tableName + ".Transaction_Date_Start <= '" + startDate + "') AND ((" +
                                      tableName + ".Transaction_Date_End IS NULL) OR (" + tableName + ".Transaction_Date_End > '" +
                                      startDate + "'))) ";
            return tableTempString;
        }

        [HttpGet]
        public int getRows() {
            try
            {
                return data.Data.Rows.Count;
            }
            catch (Exception e) {
                return 0;
            }
        }

        [HttpPost]
        public string RequestData(string colName) {

            try
            {
                List<string> resultColumn = new List<string>();
                string result = "";

                foreach (DataRow row in data.Data.Rows) {
                    resultColumn.Add(row[colName].ToString());
                }

                result = String.Join(",", resultColumn.ToArray());
                return result;
            }
            catch {
                return "";
            }
        }

        [HttpGet]
        public PartialViewResult Extractions() {
            List<ExtractionData> extractionsList = context.DataExtracts.ToList();

            //ExtractionData ed1 = new ExtractionData();
            //ed1.Name = "Sam";
            //ed1.QueryDate = "1/1/2014";

            //ExtractionData ed2 = new ExtractionData();
            //ed2.Name = "SamK";
            //ed2.QueryDate = "2/1/2014";

            //extractionsList.Add(ed1);
            //extractionsList.Add(ed2);

            

            return PartialView("Extractions", extractionsList);
        }

        [HttpPost]
        public string GetExtractionInfo(string extractionName) {

            var extraction = context.DataExtracts.Where(b => b.Name == extractionName).FirstOrDefault();
            
            ExtractionData extractionInfo = (ExtractionData)extraction;

            return new JavaScriptSerializer().Serialize(extractionInfo);
        }


        [HttpGet]
        public PartialViewResult getResultsTable() {
            
            if (data.Data != null){
                return (PartialView("ViewDataTable", data));
            }

            return null;
        }

    }
}