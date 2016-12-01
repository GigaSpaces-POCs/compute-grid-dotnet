using GigaSpaces.Core;
using MasterWorkerModel;
using System;
using System.Threading;

namespace WorkerProject
{
    class WorkerHeartbeat
    {
        //int count = 0;
        int timeout = Worker.SleepTime;
        public WorkerHeartbeat(int timeout)
        {
           this.timeout = timeout;
        }
        public void DoWork()
        {
            WorkerProcess workerProcess = new WorkerProcess();
            workerProcess.ProcessID = Worker.CurrentProcess.Id;
            workerProcess.ID = workerProcess.HostName + "=" + workerProcess.ProcessID;
            while (true)
            {
                try
                {
                    workerProcess.StartDateTime = DateTime.Now;
                    IdQuery<WorkerProcess> idQuery = new IdQuery<WorkerProcess>(workerProcess.ID);
                    //IChangeResult<WorkerProcess> changeResult = Worker.SpaceProxy.Change<WorkerProcess>(idQuery, new ChangeSet().Set("LastUpdateDateTime", DateTime.Now).Lease(10000));
                    IChangeResult<WorkerProcess> changeResult = Worker.ComputeSpaceProxy.Change<WorkerProcess>(idQuery, new ChangeSet().Lease(10000));
                    if (changeResult.NumberOfChangedEntries == 0)
                    {
                         WriteHeartBeat(workerProcess);
                    } 
                    Thread.Sleep(timeout);
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }

        private void WriteHeartBeat(WorkerProcess workerProcess)
        {
            workerProcess.HostName = Worker.HostName;
            workerProcess.ProcessID = Worker.CurrentProcess.Id;
            workerProcess.ID = workerProcess.HostName + "=" + workerProcess.ProcessID;
            workerProcess.StartDateTime = DateTime.Now;
            workerProcess.LastUpdateDateTime = DateTime.Now;
            Worker.ComputeSpaceProxy.Write(workerProcess, 5000);
        }
    }
}
