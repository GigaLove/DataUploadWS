using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;

namespace ShedManangeService
{
    /// <summary>
    /// WebService1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class ShedService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public bool upLoadData(string dataStr, string user, string pwd)
        {
            string time = DateTime.Now.ToString();
            string sqlStr = "insert into data values('01', '01', '10.01', '" + time + "');";
            MySQLDBManager.alterData(sqlStr, "root", "199457");
            return true;
        }

        [WebMethod]
        public string getData()
        {
            string sqlStr = "select * from data;";
            DataTable table = MySQLDBManager.queryData(sqlStr, "root", "199457");
            string data = table.Rows[0][0].ToString() + table.Rows[0][1].ToString() + table.Rows[0][2].ToString() + table.Rows[0][3].ToString();
            return data;
        }
    }
}
