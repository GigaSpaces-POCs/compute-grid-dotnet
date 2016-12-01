using System;
using GigaSpaces.Core;
using GigaSpaces.XAP.Events;
using MasterWorkerModel;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace WorkerProject
{
    class Worker
    {
        public static ISpaceProxy ComputeSpaceProxy;
        public static ISpaceProxy TradeSpaceProxy;
        public static int SleepTime;
        public static String HostName;
        public static Process CurrentProcess = null;
        public static int Timeout = 1000;
        public static int Priority = 0;

        public static void Main(string[] args)
        {
            HostName = Dns.GetHostName();
            CurrentProcess = Process.GetCurrentProcess();
            String url = args[0];
            String tradeUrl = args[1];
           
            Console.WriteLine("*** Connecting to remote space named '" + url + "' from Worker...");
            ComputeSpaceProxy = GigaSpacesFactory.FindSpace(url);
            TradeSpaceProxy = GigaSpacesFactory.FindSpace(tradeUrl);
            WorkerHeartbeat masterHeartbeat = new WorkerHeartbeat(Timeout);
            Thread workerThread = new Thread(masterHeartbeat.DoWork);
            workerThread.Start();
            new Worker(args);
        }

        public Worker(string[] args)
        {
            Console.WriteLine(Environment.NewLine + "Welcome to XAP.NET 11 Worker!" + Environment.NewLine + " hostName=" + HostName + "currentProcess.Id="+CurrentProcess.Id);
            
            WorkerProcess workerProcess = new WorkerProcess();
            workerProcess.HostName = HostName;
            workerProcess.ProcessID = CurrentProcess.Id;
            workerProcess.ID = HostName + "=" + CurrentProcess.Id;
            workerProcess.StartDateTime = DateTime.Now;
            ComputeSpaceProxy.Write(workerProcess, 5000);

            String ioType = args.Length == 1 ? "NIO" : args[2];
            SleepTime = args.Length == 2 ? 2000 : Convert.ToInt32(args[3]);
            if (ioType.Equals("IO"))
            {
                IEventListenerContainer<Request> eventListenerContainer = EventListenerContainerFactory.CreateContainer<Request>(ComputeSpaceProxy, new WorkeIO(ComputeSpaceProxy,TradeSpaceProxy));
                eventListenerContainer.Start();
            } else if (ioType.Equals("NIO"))
            {
                IEventListenerContainer<Request> eventListenerContainer = EventListenerContainerFactory.CreateContainer<Request>(ComputeSpaceProxy, new WorkeNIO(ComputeSpaceProxy, TradeSpaceProxy));
                eventListenerContainer.Start();
            } else
            {
                WorkerThread workerThread = new WorkerThread();
                Thread t = new Thread(workerThread.DoWork);
                t.Start();
            }
        }
    }
}
