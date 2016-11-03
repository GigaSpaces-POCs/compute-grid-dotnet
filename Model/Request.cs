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
    public class Request : Base
    {
        [SpaceIndex(Type = SpaceIndexType.Extended)]
        public int Priority { get; set; }
        public Object[] TradeIds { get; set; }
        public Double Rate { get; set; }
    }
}
