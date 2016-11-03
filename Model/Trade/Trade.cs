using GigaSpaces.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterWorkerModel
{
    [Serializable]
    [SpaceClass]
    public class Trade
    {
        [SpaceID(AutoGenerate = false)]
        public int? id { get; set; }
        public Double NPV { get; set; }
        public CacheFlowData cacheFlowData { get; set; }

        public Trade() { }

        public String getBook() {
        return "Book" + this.id % 4;
        }

    }

}
