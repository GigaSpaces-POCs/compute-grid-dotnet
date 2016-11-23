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
    public class Result : Base
    {
        public Dictionary<String, Double> resultData { get; set; }
        public int? Processingtime { get; set; }
    }
}
