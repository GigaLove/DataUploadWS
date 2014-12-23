using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShedManangeService
{
    public class Mode
    {
        private string tMode;           //温度状态
        private string hMode;           //湿度状态
        private string pMode;           //气压状态
        private string dMode;           //门磁状态
        private string sMode;           //烟雾状态
        private string resultMode;      //当前大棚状态

        public string TMode
        {
            get { return tMode; }
            set { tMode = value; }
        }

        public string HMode
        {
            get { return hMode; }
            set { hMode = value; }
        }

        public string PMode
        {
            get { return pMode; }
            set { pMode = value; }
        } 

        public string DMode
        {
            get { return dMode; }
            set { dMode = value; }
        }
      

        public string SMode
        {
            get { return sMode; }
            set { sMode = value; }
        }
        

        public string ResultMode
        {
            get { return resultMode; }
            set { resultMode = value; }
        }

    }
}