using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace P4ViewProject.Models
{
    public class SQLServerConnClass
    {
        SqlConnection SqlConnection = new SqlConnection();
        public DataTable SqlTable = new DataTable();

        public SQLServerConnClass()
        {
            SqlConnection.ConnectionString = ConfigurationManager.ConnectionStrings["ViewSimulation"].ConnectionString;
        }

        public Dictionary <string, List<string>> getTables() {

            Dictionary <string, List<string>> tableDict = new Dictionary<string, List<string>>();
            String[] tableRestrictions = new String[4];

            try
            {
                SqlConnection.Open();
                DataTable t = SqlConnection.GetSchema("Tables");

                foreach (DataRow row in t.Rows)
                {
                    string tableName = (string)row[2];
                    tableRestrictions[2] = tableName;
                    List<string> cols = new List<string>();

                    DataTable c = SqlConnection.GetSchema("Columns", tableRestrictions);
                    foreach (DataRow rowCol in c.Rows)
                    {
                        cols.Add((string)rowCol[3]);

                    }
                    tableDict.Add(tableName, cols);

                }
                return tableDict;

            }
            catch (Exception e)
            {
                return tableDict;
            }
            finally {
                SqlConnection.Close();
            }
        }

        public Tuple<string, Dictionary<string, List<string>>> getTablesWithDatabaseName()
        {

            Dictionary<string, List<string>> tableDict = new Dictionary<string, List<string>>();
            String[] tableRestrictions = new String[4];
            Tuple<string, Dictionary<string, List<string>>> tableNameAndData = null;

            try
            {
                SqlConnection.Open();
                DataTable t = SqlConnection.GetSchema("Tables");
                string databaseName ="";

                foreach (DataRow row in t.Rows)
                {
                    databaseName = (string)row[0];
                    string tableName = (string)row[2];
                    tableRestrictions[2] = tableName;
                    List<string> cols = new List<string>();

                    DataTable c = SqlConnection.GetSchema("Columns", tableRestrictions);
                    foreach (DataRow rowCol in c.Rows)
                    {
//                        cols.Add((string)rowCol[3] + "  Type: " + (string)rowCol[7]);  // Uncomment to add type information, comment next line
                        cols.Add((string)rowCol[3]);

                    }
                    tableDict.Add(tableName, cols);

                }

                tableNameAndData = new Tuple<string, Dictionary<string, List<string>>>(databaseName, tableDict);
                return tableNameAndData;

            }
            catch (Exception e)
            {
                return tableNameAndData;
            }
            finally
            {
                SqlConnection.Close();
            }
        }

        public void retrieveData(string sql) 
        {
            try 
            {
                SqlConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(sql, SqlConnection);
                da.Fill(SqlTable);
            }
            catch (Exception e)
            {
                HttpContext.Current.Response.Write("<script>alert('Invalid sql:  "+ e.Message +" ');</script>");
            }
            finally
            {
                SqlConnection.Close();
            }
        
        }

        public void commandExecution(string sql) 
        {
            try
            {
                SqlConnection.Open();
                SqlCommand comm = new SqlCommand(sql, SqlConnection);

                int rowsAlterated = comm.ExecuteNonQuery();
            }
            catch
            { }
            finally 
            {
                SqlConnection.Close();
            }

        }

    }
}