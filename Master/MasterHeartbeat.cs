using GigaSpaces.Core;
using MasterWorkerModel;
using System;
using System.Threading;

namespace MasterProject
{
    class MasterHeartbeat
    {
        int timeout = 1000;
        public MasterHeartbeat(int timeout)
        {
            this.timeout = timeout;
        }
        public void DoWork()
        {
            MasterProcess masterProcess = new MasterProcess();
            masterProcess.HostName = Master.HostName;
            masterProcess.ProcessID = Master.CurrentProcess.Id;
            masterProcess.ID = masterProcess.HostName + "=" + masterProcess.ProcessID;
            while (true)
            {
                try
                {
                    masterProcess.StartDateTime = DateTime.Now;
                    IdQuery<MasterProcess> idQuery  = new IdQuery<MasterProcess>(masterProcess.ID);
                    IChangeResult<MasterProcess> changeResult = Master.SpaceProxy.Change<MasterProcess>(idQuery, new ChangeSet().Set("LastUpdateDateTime", DateTime.Now).Lease(5000));
                    if (changeResult.NumberOfChangedEntries == 0)
                    {
                        WriteHeartBeat(masterProcess);
                    }
                    Thread.Sleep(timeout);
                }
                catch (Exception)
                {
                    // do nothing
                }
                Thread.Sleep(timeout);
            }
        }

        private void WriteHeartBeat(MasterProcess masterProcess)
        {
            masterProcess.HostName = Master.HostName;
            masterProcess.ProcessID = Master.CurrentProcess.Id;
            masterProcess.ID = masterProcess.HostName + "=" + masterProcess.ProcessID;
            masterProcess.StartDateTime = DateTime.Now;
            masterProcess.LastUpdateDateTime = DateTime.Now;
            Master.SpaceProxy.Write(masterProcess, 5000);
        }
    }
}
