using GigaSpaces.Core;
using MasterWorkerModel;
using System;
using System.Threading;

namespace WorkerProject
{
    class WorkerThread
    {
        int timeout = Worker.SleepTime;
        public WorkerThread()
        {
            Console.WriteLine("Worker started as Thread instead of polling container 3, Worker.SleepTime=" + Worker.SleepTime);
        }
        public void DoWork()
        {
            Request template = new Request();
            template.FunctionName = null;
            template.JobID = null;
            template.Parameters = null;
            template.Payload = null;
            template.ServiceName = null;
            template.TaskID = null;
            while (true)
            {
                try
                {
                    Request request = Worker.SpaceProxy.Read<Request>(template,1000);
                    Console.WriteLine("WorkerThread.DoWork called for " + request.JobID + " - " + request.TaskID);
                    //process Data here and return processed data
                    Result result = new Result();
                    result.JobID = request.JobID;
                    result.TaskID = request.TaskID;
                    Worker.SpaceProxy.Write(result);

                    Thread.Sleep(Worker.SleepTime);
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }
    }
}
