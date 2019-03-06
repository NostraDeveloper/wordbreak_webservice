using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Configuration;
using System.Text.RegularExpressions;
namespace WB_WebService_GT
{
    /// <summary>
    /// Summary description for WB_Service
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WB_Service : System.Web.Services.WebService
    {
        static DataTable Outter_S = new DataTable();
        static DataTable Inner_S = new DataTable();
        static DataTable InAndOut_S = new DataTable();
        static DataTable Back_Remove_S = new DataTable();
        static DataTable Dic_Uni = new DataTable();
        static DataTable SPSumWrong_S = new DataTable();
        static DataTable SubLong_S = new DataTable();
        string FileDataWBFolder = WebConfigurationManager.AppSettings["FileDataWBFolder"];
        ServiceReference_secure.nostra_serviceSoapClient SECURE = new ServiceReference_secure.nostra_serviceSoapClient("nostra_serviceSoap");

        public WB_Service()
        {
            if (!(Dic_Uni.Rows.Count > 0))
            {
                LoaddataFormfile();
            }
        }

        [WebMethod]
        public string Do_WordsBK(string BK_Str)
        {
            if (!SECURE.Text(GetIP()))
            {
                Context.Response.Status = "403 Forbidden";
                Context.Response.StatusCode = 403;
                Context.ApplicationInstance.CompleteRequest();
                Context.Response.End();
                return "";
            }

            string result = "";
            
            string outdata = "";
            string[] tmp = RemoveMoreSpace(BK_Str.Trim()).Split(' ');
            for (int i = 0; i < tmp.Length; i++)
            {
                TSPR.cl_WB_Main CLSM = new TSPR.cl_WB_Main();
                string resd = tmp[i].Replace("\r\n", "⬆");
                resd = resd.Replace("\r", "⬆");
                resd = CLSM.Main_WB(resd.Replace("\n", "⬆"), Outter_S, Inner_S, InAndOut_S, Back_Remove_S, Dic_Uni, SPSumWrong_S, SubLong_S, 1, true);
                resd = resd.Replace("|", "⬥");
                outdata = resd.Replace("⬆⬆", "\r\n");
                outdata = outdata.Replace("⬆", "\r\n");
                outdata = outdata.Replace("]", "");
                outdata = outdata.Replace("[", "");
                outdata = outdata.Replace("|", "⬥");
                outdata = outdata.Replace("⬥\r\n", "\r\n");
                outdata = outdata.Replace("\r\n\r\n", "\r\n");
                outdata = outdata.Replace("⬥", "^");

                result += outdata + "^";
            }
            return RemoveMoreSpace(result.Replace("^", " ")).Trim().Replace(" ", "^");
        }

        private void LoaddataFormfile()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assemblyFolder = FileDataWBFolder;
            SeRead(assemblyFolder, "Outter_S", out Outter_S);
            SeRead(assemblyFolder, "Inner_S", out Inner_S);
            SeRead(assemblyFolder, "InAndOut_S", out InAndOut_S);
            SeRead(assemblyFolder, "Back_Remove_S", out Back_Remove_S);
            SeRead(assemblyFolder, "SPSumWrong_S", out SPSumWrong_S);
            SeRead(assemblyFolder, "Dic_Uni", out Dic_Uni);
            SeRead(assemblyFolder, "SubLong_S", out SubLong_S);
        }
        private void SeRead(string Filepath, string NameFile, out DataTable DataOut)
        {
            //  s=serialize, r=read 
            using (Stream stream = File.Open(Filepath + "\\" + NameFile, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                DataOut = bin.Deserialize(stream) as DataTable;
            }
        }

        public string GetIP()
        {
            String ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }

        public string RemoveMoreSpace(string input)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            input = regex.Replace(input, " ");

            return input;
        }

        public string RemoveMoreSpilter(string input)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[\^]{2,}", options);
            input = regex.Replace(input, "^");

            return input;
        }

        public string RemoveLastSpilter(string input)
        {
            //RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[\^]$");
            input = regex.Replace(input, "");

            return input;
        }
    }
}