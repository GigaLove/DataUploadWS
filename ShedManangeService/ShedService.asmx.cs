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
        private static string dbUser = "root";
        private static string dbPwd = "199457";

        /// <summary>
        /// 上传数据接口
        /// </summary>
        /// <param name="nID">传感器节点ID</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="dataInfo">数据信息</param>
        /// <returns>是否上传成功</returns>
        [WebMethod]
        public bool upLoadData(string nID, string dataType, string dataInfo)
        {
            //获取当前时间
            string time = DateTime.Now.ToString();
            //将数据插入到数据库中
            string sqlStr = "insert into data (nID, type, info, time) values('" + nID + "','" + dataType + "','" + dataInfo + "','" + time + "');";
            int flag = MySQLDBManager.alterData(sqlStr, dbUser, dbPwd);
            if (flag < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns>组装后的数据信息</returns>
        [WebMethod]
        public string getData()
        {
            string sqlStr = "select * from data where dID in (select max(dID) from data group by type) order by type asc;";
            DataTable table = MySQLDBManager.queryData(sqlStr, dbUser, dbPwd);
            string data = "";
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length - 1; i++)
                {
                    //记录的每一列用#进行分隔
                    data += row[i] + "#";
                }
                //每一条记录用$进行分隔
                data += row[row.ItemArray.Length - 1] + "$";
            }
            return data.Substring(0, data.Length - 1);
        }

        /// <summary>
        /// 发送操作命令
        /// </summary>
        /// <returns>是否发送成功</returns>
        [WebMethod]
        public bool sendCommand()
        {
            //置operation字段值为1
            string sqlStr = "update control set operation = 1 where cID = 1;";
            int flag = MySQLDBManager.alterData(sqlStr, dbUser, dbPwd);
            if (flag < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取operation字段值，1返回true，0返回false
        /// </summary>
        [WebMethod]
        public bool receiveCommand()
        {
            string sqlStr = "select operation from control where cID = 1;";
            DataTable table = MySQLDBManager.queryData(sqlStr, dbUser, dbPwd);
            int flag = Convert.ToInt32(table.Rows[0][0]);
            if (flag == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 置operation字段值为0，清除操作标志
        /// </summary>
        [WebMethod]
        public bool clearCommand()
        {
            string sqlStr = "update control set operation = 0 where cID = 1;";
            int flag = MySQLDBManager.alterData(sqlStr, dbUser, dbPwd);
            if (flag < 0)
            {
                return false;
            }
            return true;
        }
    }
}
