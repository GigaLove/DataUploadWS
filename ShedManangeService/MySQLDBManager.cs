using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;

namespace ShedManangeService
{
    public class MySQLDBManager
    {
        private static MySqlConnection con;
        private static MySqlCommand cmd;
        public static string dbUser = "root";
        public static string dbPwd = "199457";

        /// <summary>
        /// 建立数据库连接
        /// </summary>
        /// <param name="user">数据库用户名</param>
        /// <param name="pwd">数据库密码</param>
        /// <returns>连接是否打开</returns>
        private static bool openConnection(string user, string pwd)
        {
            bool flag = false;
            try
            {
                string connectionStr = "Data Source = 127.0.0.1;Initial Catalog = shedInfo;User ID = " + user + ";Password = " + pwd;
                //根据连接字符串打开数据库连接
                con = new MySqlConnection(connectionStr);
                con.Open();
                flag = true;
            }
            catch
            {
                con = null;
            }
            return flag;
        }

        /// <summary>
        /// 根据sql语句，查询数据库中相关信息
        /// </summary>
        /// <param name="querySQL">查询SQL</param>
        /// <param name="user">数据库用户名</param>
        /// <param name="pwd">数据库密码</param>
        /// <returns>查询结果</returns>
        public static DataTable queryData(string querySQL, string user, string pwd)
        {
            if (openConnection(user, pwd))
            {
                cmd = new MySqlCommand(querySQL, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet dataSet = new DataSet();
                //填充数据集
                adapter.Fill(dataSet, "table");
                cmd = null;
                closeConnection();
                //返回数据集中的第一张表
                return dataSet.Tables[0];
            }
            return null;
        }

        /// <summary>
        /// insert, update, delete等sql操作
        /// </summary>
        /// <param name="alterSQL">sql语句</param>
        /// <param name="user">数据库用户名</param>
        /// <param name="pwd">数据库密码</param>
        /// <returns>影响的记录数</returns>
        public static int alterData(string alterSQL, string user, string pwd)
        {
            int count = -1;
            if (openConnection(user, pwd))
            {
                cmd = new MySqlCommand(alterSQL, con);
                count = cmd.ExecuteNonQuery();
                cmd = null;
                closeConnection();
            }
            return count;
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        private static void closeConnection()
        {
            con.Close();
            con.Dispose();
        }
    }
}