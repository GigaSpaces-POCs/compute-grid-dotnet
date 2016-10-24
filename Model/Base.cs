using GigaSpaces.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterWorkerModel
{
    [SpaceClass]
    public class Base
    {
        [SpaceIndex(Type = SpaceIndexType.Basic)]
        public string JobID { get; set; }

        [SpaceID(AutoGenerate = false)]
        public string TaskID { get; set; }
        public string Payload { get; set; }
        public string ServiceName { get; set; }
        public string FunctionName { get; set; }
        public Dictionary<String, String> Parameters { get; set; }
    }
}
