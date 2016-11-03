using GigaSpaces.Core;
using GigaSpaces.Core.Cache;
using GigaSpaces.XAP.Events;
using GigaSpaces.XAP.Events.Polling;
using GigaSpaces.XAP.Events.Polling.Receive;
using MasterWorkerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerProject
{
    [PollingEventDriven(MinConcurrentConsumers = 1, MaxConcurrentConsumers = 1)]
    public class WorkeNIO
    {
        private ISpaceProxy proxy;

        private ILocalCache localCache;

        public WorkeNIO()
        {
            // Connect to space:
            Console.WriteLine("*** Worker started in NonBlocking IO mode.");
            Console.WriteLine();
        }

        public WorkeNIO(ISpaceProxy space)
        {
            // Connect to space:
            Console.WriteLine("*** Worker started in NonBlocking IO mode.");
            Console.WriteLine();
            proxy = space;
            localCache = GigaSpacesFactory.CreateIdBasedLocalCache(space);
        }

        [EventTemplate]
        public SqlQuery<Request> UnprocessedData
        {
            get
            {
                SqlQuery<Request> query = new SqlQuery<Request>("Priority >= 0");
                return query;
            }
        }

        [DataEventHandler]
        public Result ProcessData(Request request)
        {
            Console.WriteLine("Worker.ProcessData called for " + request.JobID + " - " + request.TaskID + "- priority=" + request.Priority);
            //process Data here and return processed data
            //Thread.Sleep(Worker.SleepTime);

            Result result = new Result();
            result.JobID = request.JobID;
            result.TaskID = request.TaskID;

            //Console.Write("Calculating NPV for trades: { ");
            //foreach (int id in request.TradeIds)
            //{
              //  Console.Write(id + " ");
            //}
            //Console.Write("}");
            try
            {
                Dictionary<String, Double> resultData = CalculateNPVUtil.execute(localCache, proxy, request.TradeIds, request.Rate);
                result.resultData = resultData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        [ReceiveHandler]
        public IReceiveOperationHandler<Request> ReceiveHandler()
        {
            TakeReceiveOperationHandler<Request> receiveHandler = new TakeReceiveOperationHandler<Request>();
            receiveHandler.NonBlocking = true;
            receiveHandler.NonBlockingFactor = 1;
            return receiveHandler;
        }
    }
}