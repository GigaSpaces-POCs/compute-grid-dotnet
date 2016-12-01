using GigaSpaces.Core.Metadata;
using System;

namespace MasterWorkerModel
{
    [Serializable]
    [SpaceClass]
    public class WorkerProcess : BaseProcess
    {
        public int Priority { get; set; }
    }
}
