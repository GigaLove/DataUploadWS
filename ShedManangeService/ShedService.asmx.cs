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

        public ShedService()
        {
            DataAnalyse.initThresHold();
            DataAnalyse.initMode();
        }

        /// <summary>
        /// 上传数据接口
        /// </summary>
        /// <param name="nID">传感器节点ID</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="dataInfo">数据信息</param>
        /// <returns>是否上传成功</returns>
        [WebMethod]
        public string upLoadData(string nID, string dataType, string dataInfo)
        {
            //获取当前时间
            string time = DateTime.Now.ToString();


            //数据类型为R，表示考勤信息
            if (dataType.Equals("R"))
            {
                //查询用户表，验证用户是否有效
                string sqlStr = "select * from user where uID = '" + dataInfo + "';";
                DataTable table = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
                if (table.Rows.Count > 0)
                {
                    string uName = table.Rows[0][1].ToString();
                    //搜索记录中当前人员的最近状态
                    sqlStr = "select uName,status from userlog where lID in (select max(lID) from userlog where uID='" + dataInfo + "');";
                    table = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);

                    //有相应记录则根据原有记录翻转状态，否则插入新的记录
                    if (table.Rows.Count > 0)
                    {
                        string status = table.Rows[0][1].ToString();
                        //翻转用户状态
                        if (status.Equals("arrival"))
                        {
                            status = "leaving";
                        }
                        else
                        {
                            status = "arrival";
                        }
                        sqlStr = "insert into userlog (uID, uName, time, status) values ('" + dataInfo + "','" +
                        uName + "','" + DateTime.Now + "','" + status + "');";
                        MySQLDBManager.alterData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
                        return "User " + uName + " " + status;
                    }
                    else
                    {
                        sqlStr = "insert into userlog(uID, uName, time, status) values ('" + dataInfo + "','" + uName + "','" + DateTime.Now.ToString() + "','arrival');";
                        MySQLDBManager.alterData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
                        return "User " + uName + " " + "arrival";
                    }
                }
                else
                {
                    return "Invalid User";
                }
            }
            else
            {
                //将传感器数据插入到数据库中
                string sqlStr = "insert into data (nID, type, info, time) values('" + nID + "','" + dataType + "','" + dataInfo + "','" + time + "');";
                MySQLDBManager.alterData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);

                //根据不同的数据类型，分别记录最新数据
                switch (dataType)
                {
                    case "T":
                        DataAnalyse.temperature = Convert.ToDouble(dataInfo);
                        break;
                    case "D":
                        DataAnalyse.door = dataInfo;
                        break;
                    case "S":
                        DataAnalyse.smog = dataInfo;
                        break;
                    case "P":
                        DataAnalyse.pressure = Convert.ToDouble(dataInfo);
                        break;
                    case "H":
                        DataAnalyse.humidity = Convert.ToDouble(dataInfo);
                        break;
                    default:
                        break;
                }

                string mode = DataAnalyse.analyse();
                if (!mode.Equals("normal"))
                {
                    insertExceptionData(mode);
                    //置operation字段值为1
                    sqlStr = "update control set operation = 1 where cID = 1;";
                    MySQLDBManager.alterData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
                }
                return mode;
            }           
        }


        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns>组装后的数据信息</returns>
        [WebMethod]
        public string getData()
        {
            string sqlStr = "select * from data where dID in (select max(dID) from data group by type) order by type asc;";
            DataTable table = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
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

            string mode = DataAnalyse.analyse();
            //if (!mode.Equals("normal"))
            //{
            //    insertExceptionData(mode);
            //}
            return data + mode;
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
            int flag = MySQLDBManager.alterData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
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
            DataTable table = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
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
            int flag = MySQLDBManager.alterData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
            if (flag < 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取最近的5条异常信息
        /// </summary>
        /// <returns>异常信息</returns>
        [WebMethod]
        public string getExceptionRecord()
        {
            return getRecentData("dID", "exceptiondata", 5);
        }

        [WebMethod]
        public string getLogInfo()
        {
            return getRecentData("lID", "userlog", 3);
        }

        /// <summary>
        /// 插入异常数据
        /// </summary>
        /// <param name="mode">异常模式</param>
        private void insertExceptionData(string mode)
        {
            string sqlStr = "insert into exceptiondata(temperatrue, humidity, pressure, door, smog, status, time) values ('" +
                    DataAnalyse.temperature + "','" + DataAnalyse.humidity + "','" + DataAnalyse.pressure + "','" + DataAnalyse.door + "','" +
                    DataAnalyse.smog + "','" + mode + "','" + DateTime.Now.ToString() + "');";
            MySQLDBManager.alterData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
        }

        /// <summary>
        /// 获取特定表格中标号最大的数据项
        /// </summary>
        /// <param name="attribute">标号属性</param>
        /// <param name="table">表名</param>
        /// <returns>最大的ID</returns>
        private int getMaxID(string attribute, string table)
        {
            string sqlStr = "select max(" + attribute + ") from " + table + ";";
            DataTable dataTable = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
            if (dataTable.Rows.Count > 0)
            {
                return Convert.ToInt32(dataTable.Rows[0][0]);
            }
            return 0;
        }

        /// <summary>
        /// 获取table中最新的数据信息
        /// </summary>
        /// <param name="IDAttribute">ID属性</param>
        /// <param name="table">表名</param>
        /// <param name="count">请求的最新数据的个数</param>
        /// <returns>table中的最新信息</returns>
        private string getRecentData(string IDAttribute, string table, int count)
        {
            int maxID = getMaxID(IDAttribute, table);
            if (maxID > 0)
            {
                string sqlStr = " select * from " + table + " where " + IDAttribute + " between " + (maxID - count + 1) + " and " + maxID + " order by " + IDAttribute + " desc;";
                DataTable dataTable = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);
                string data = "";
                foreach (DataRow row in dataTable.Rows)
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
            return "";
        }
    }
}
