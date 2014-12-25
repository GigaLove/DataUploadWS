using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Data;

namespace ShedManangeService
{
    public class DataAnalyse
    {
        public static double temperature = 25;           //温度信息
        public static string door = "closed";           //门磁信息 
        public static double humidity = 20;             //湿度信息
        public static double pressure = 25;             //气压信息
        public static string smog = "normal";           //烟雾状态
        public static Hashtable thresholdTable;         //阈值表
        public static List<Mode> modeList;              //模式表

        /// <summary>
        /// 根据数据库信息初始化各项指标的阈值
        /// </summary>
        public static void initThresHold()
        {
            thresholdTable = new Hashtable();
            string sqlStr = "select * from threshold;";
            DataTable table = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);

            //将阈值存储到哈希表中
            foreach (DataRow row in table.Rows)
            {
                Threshold ts = new Threshold();
                ts.Type = row[0].ToString();
                ts.LowThreshold = Convert.ToDouble(row[1]);
                ts.HighThreshold = Convert.ToDouble(row[2]);
                thresholdTable.Add(row[0].ToString(), ts);
            }
        }

        /// <summary>
        /// 根据数据库信息，初始化系统模式信息
        /// </summary>
        public static void initMode()
        {
            modeList = new List<Mode>();
            string sqlStr = "select * from mode;";
            DataTable table = MySQLDBManager.queryData(sqlStr, MySQLDBManager.dbUser, MySQLDBManager.dbPwd);

            //将模式信息添加到list中
            foreach (DataRow row in table.Rows)
            {
                Mode mode = new Mode();
                mode.TMode = row[1].ToString();
                mode.HMode = row[2].ToString();
                mode.PMode = row[3].ToString();
                mode.DMode = row[4].ToString();
                mode.SMode = row[5].ToString();
                mode.ResultMode = row[6].ToString();
                modeList.Add(mode);
            }
        }

        /// <summary>
        /// 数据分析函数
        /// </summary>
        /// <returns>当前大棚所处的状态</returns>
        public static string analyse()
        {
            string analyseRes = "normal";

            string tMode = analyseMode("T", temperature);           //获取温度的状态
            string hMode = analyseMode("H", humidity);              //获取湿度的状态
            string pMode = analyseMode("P", pressure);              //获取气压的状态

            //匹配模式过程中，遇到none表示该项数据信息对于模式匹配没有影响
            foreach (Mode mode in modeList)
            {
                if (!tMode.Equals(mode.TMode) && !mode.TMode.Equals("none"))
                {
                    continue;
                }
                if (!hMode.Equals(mode.HMode) && !mode.HMode.Equals("none"))
                {
                    continue;
                }
                if (!pMode.Equals(mode.PMode) && !mode.PMode.Equals("none"))
                {
                    continue;
                }
                if (!door.Equals(mode.DMode) && !mode.DMode.Equals("none"))
                {
                    continue;
                }
                if (!smog.Equals(mode.SMode) && !mode.SMode.Equals("none"))
                {
                    continue;
                }
                analyseRes = mode.ResultMode;
            }
            return analyseRes;
        }

        /// <summary>
        /// 对比数值与阈值，分析数据的状态
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <param name="data">数据信息</param>
        /// <returns>数据是否正常</returns>
        private static string analyseMode(string type, double data)
        {
            string mode;
            if (thresholdTable.Contains(type))
            {
                //大于阈值，为high，小于阈值为low
                if (data > (thresholdTable[type] as Threshold).HighThreshold)
                {
                    mode = "high";
                }
                else if (data < (thresholdTable[type] as Threshold).LowThreshold)
                {
                    mode = "low";
                }
                else
                {
                    mode = "normal";
                }
            }
            else
            {
                mode = "none";
            }
            return mode;
        }
    }
}