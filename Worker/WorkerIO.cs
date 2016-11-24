using GigaSpaces.Core;
using GigaSpaces.Core.Cache;
using GigaSpaces.Core.Cache.Eviction;
using GigaSpaces.XAP.Events;
using GigaSpaces.XAP.Events.Polling;
using GigaSpaces.XAP.Events.Polling.Receive;
using MasterWorkerModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerProject
{
    [PollingEventDriven(MinConcurrentConsumers = 1, MaxConcurrentConsumers = 1)]
    public class WorkeIO
    {
        private ISpaceProxy proxy;
    
        private ILocalCache localCache;

        public WorkeIO()
        {
            Console.WriteLine("*** Worker started in Blocking IO mode.");
            Console.WriteLine();
        }

        public WorkeIO(ISpaceProxy space,ISpaceProxy tradeSpace)
        {
            Console.WriteLine("*** Worker started in Blocking IO mode.");
            Console.WriteLine();
            proxy = space;
            TimeSpan ts = new TimeSpan(10,0,0,0);
            IdBasedLocalCacheConfig cacheConfig = new IdBasedLocalCacheConfig();
            cacheConfig.EvictionStrategyBuilder = new FifoSegmentEvictionStrategyBuilder(1000000, 1000,ts);
            localCache = GigaSpacesFactory.CreateIdBasedLocalCache(tradeSpace,cacheConfig);
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
            DateTime start;
            TimeSpan time;
            start = DateTime.Now;
            Console.WriteLine("Worker.ProcessData called for " + request.JobID + " - " + request.TaskID + "- priority=" + request.Priority);
            //process Data here and return processed data
            //Thread.Sleep(Worker.SleepTime);

            Result result = new Result();
            result.JobID = request.JobID;
            result.TaskID = request.TaskID;
           // Console.Write("Calculating NPV for trades: { ");
          //  foreach (int id in request.TradeIds)
           // {
             //   Console.Write(id + " ");
            //}
            //Console.Write("}");
            try
            {
                Dictionary<String, Double> resData = CalculateNPVUtil.execute(localCache, proxy, request.TradeIds, request.Rate);
                result.resultData = resData;
                time = DateTime.Now - start;
                result.Processingtime = time.Milliseconds;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return result;
        }

        [ReceiveHandler]
        public IReceiveOperationHandler<Request> ReceiveHandler()
        {
            TakeReceiveOperationHandler<Request> receiveHandler = new TakeReceiveOperationHandler<Request>();
            receiveHandler.NonBlocking = false;
            return receiveHandler;
        }
    }
}