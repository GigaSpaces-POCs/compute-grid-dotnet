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

        private ISpaceProxy tradeProxy;

        private ILocalCache localCache;

        private int spaceSize = 5000000;

        private int cacheBatchSize = 1000;

        

        public WorkeIO()
        {
            Console.WriteLine("*** Worker started in Blocking IO mode.");
            Console.WriteLine();
        }

        public WorkeIO(ISpaceProxy space,ISpaceProxy tradeSpace)
        {
            int cacheSize = spaceSize / 5;
            Random rng = new Random();
            Console.WriteLine("*** Worker started in Blocking IO mode.");
            Console.WriteLine();
            proxy = space;
            tradeProxy = tradeSpace;
            TimeSpan ts = new TimeSpan(10, 0, 0, 0);
            IdBasedLocalCacheConfig cacheConfig = new IdBasedLocalCacheConfig();
            cacheConfig.EvictionStrategyBuilder = new FifoSegmentEvictionStrategyBuilder(cacheSize, 1000, ts);
            localCache = GigaSpacesFactory.CreateIdBasedLocalCache(tradeSpace, cacheConfig);
            HashSet<Object> ids;
            for (int i = 0; i < cacheSize / cacheBatchSize; i++)
            {
                ids = new HashSet<Object>();
                for (int j = 0; j < cacheBatchSize; j++)
                {
                    int rn = rng.Next(1, spaceSize);
                    ids.Add(RngRecursive(rn,rng,ids));
                }
                
                try {
                    //Console.WriteLine("Reading batch: " + i);
                    localCache.ReadByIds<Trade>(ids.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("*** Cache initializaded.");
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
            Worker.Priority = request.Priority;
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
                Dictionary<String, Double[]> resData = CalculateUtil.execute(tradeProxy, localCache, request.TradeIds, request.Rate);
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
        private Object RngRecursive(int n,Random rng ,HashSet<Object> hs)
        {
            if (hs.Contains<Object>(n))
            {
              n = (int)RngRecursive(rng.Next(1, spaceSize), rng, hs);
            }
            return (Object)n;
        }
    }
}