using GigaSpaces.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterWorkerModel
{
    [Serializable]
    public class CacheFlowData
    {
        public Double cacheFlowYear0 { get; set; }
        public Double cacheFlowYear1 { get; set; }
        public Double cacheFlowYear2 { get; set; }
        public Double cacheFlowYear3 { get; set; }
        public Double cacheFlowYear4 { get; set; }
        public Double cacheFlowYear5 { get; set; }

        public override String ToString()
        {
            return "cf0:" + this.cacheFlowYear0 + " cf1:" + cacheFlowYear1;
        }
    }
}
