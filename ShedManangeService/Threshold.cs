using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShedManangeService
{
    public class Threshold
    {
        private string type;                //数据类型
        private double lowThreshold;        //数据下界
        private double highThreshold;       //数据上界

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public double LowThreshold
        {
            get { return lowThreshold; }
            set { lowThreshold = value; }
        }
        

        public double HighThreshold
        {
            get { return highThreshold; }
            set { highThreshold = value; }
        }
    }
}