using GigaSpaces.Core.Metadata;
using System;

namespace MasterWorkerModel
{
    [SpaceClass]
    public class BaseProcess
    {
        [SpaceIndex(Type = SpaceIndexType.Basic)]
        [SpaceID(AutoGenerate = false)]
        public string ID { get; set; }
        public string HostName { get; set; }
        public int ProcessID { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
    }
}
