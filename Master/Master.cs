using System;

using GigaSpaces.Core;
using System.Threading;
using MasterWorkerModel;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MasterProject
{
    public class Master
    {
        public static ISpaceProxy SpaceProxy;
        public static ITransactionManager TxnManager = null;
        public static String HostName;
        public static Process CurrentProcess = null;
        static int JobId = 0;
        private static String url = null;
        public static int NumberOfJobs = 100;
        public static int NumberOfTask = 5;
        public static int Timeout = 1000;
        static int MapIndex = 0;
        static int tradesPerTask = 10;
        static Double Rate = 0.05;
        static Dictionary<String, Double> aggResults = new Dictionary<String, Double>();
        static Dictionary<int, object[]> IdsMap = new Dictionary<int, object[]>();
        static Stopwatch stopWatch = new Stopwatch();

        public static void Main(string[] args)
        {
            HostName = Dns.GetHostName();
            CurrentProcess = Process.GetCurrentProcess();
            if (args.Length < 1)
            {
                url = "jini://*/*/AIGDemo?locators=RajivPC&groups=Rajiv";
                NumberOfJobs = 200;
                NumberOfTask = 5;
            }
            else
            {
                url = args[0];
                tradesPerTask = args.Length == 1 ? 10 : Convert.ToInt32(args[1]);
                NumberOfTask = args.Length == 2 ? 10 : Convert.ToInt32(args[2]);
            }
            Console.WriteLine("*** Connecting to remote space named '" + url + "' From Master...");
            SpaceProxy = GigaSpacesFactory.FindSpace(url);

            MasterHeartbeat masterHeartbeat = new MasterHeartbeat(Timeout);
            Thread workerThread = new Thread(masterHeartbeat.DoWork);
            workerThread.Start();
            new Master();
        }

        public Master()
        {
            Console.WriteLine(Environment.NewLine + "Welcome to XAP.NET 12 Master!" + Environment.NewLine);
            try
            {
                TxnManager = GigaSpacesFactory.CreateDistributedTransactionManager();
                Console.WriteLine("*** Connected to space  From Master.");
                int numOfTrades = SpaceProxy.Count(new Trade());
                NumberOfJobs = (int)(numOfTrades / (NumberOfTask * tradesPerTask));
                Console.WriteLine("*** NumberOfJobs=" + NumberOfJobs);
                Console.WriteLine("Rate is: " + (int)(Rate * 100) + "%");
                Console.WriteLine();
                InitIdsMap(numOfTrades);
                stopWatch.Start();
                GenenateJobs();
                stopWatch.Stop();
                Console.WriteLine();
                DisplayResults();

                Console.WriteLine(Environment.NewLine + "NPV Calculation Example finished successfully!" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine + "NPV Calculation Example failed: " + ex);
            }
        }

        public static void GenenateJobs()
        {
            int parallelThread = 10;
            for (int i = 0; i < NumberOfJobs / parallelThread; i++)
            {
                GridService[] gridServiceA = new GridService[parallelThread];
                for (int j = 0; j < parallelThread; j++)
                {
                    
                    GridService gridService = GridAPI.createService(i + "");
                    Dictionary<String, String> parameters = new Dictionary<String, String>();
                    parameters.Add("param1", ((i*10) + j) + "");
                    Console.WriteLine("Submiting Job");
                    Job job = gridService.Submit("Function1", parameters);
                    gridServiceA[j] = gridService;
                }
                Task<ServiceData>[] taskArray = {
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[0].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[1].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[2].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[3].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[4].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[5].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[6].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[7].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[8].CollectNext(1000)),
                                                    Task<ServiceData>.Factory.StartNew(() => gridServiceA[9].CollectNext(1000))
                                                };
                ServiceData[] serviceDataA = new ServiceData[taskArray.Length];
                for (int k = 0; k < taskArray.Length; k++)
                {
                    serviceDataA[k] = taskArray[k].Result;
                    for (int l = 0; l < serviceDataA[k].Data.Length; l++ )
                        if (serviceDataA[k].Data[l] != null)
                        {
                            CalculateNPVUtil.subreducer(aggResults, serviceDataA[k].Data[l].resultData); 
                        } 
                        Console.WriteLine("From GenenateJobs using Task now Results found for k=" + k);
                }
            }
        }

        public static void DisplayResults() {
                Console.WriteLine("Calculation Time: " + stopWatch.Elapsed);
                Console.WriteLine("Book0 " + "NPV: " + aggResults["Book0"]);
                Console.WriteLine("Book1 " + "NPV: " + aggResults["Book1"]);
                Console.WriteLine("Book2 " + "NPV: " + aggResults["Book2"]);
                Console.WriteLine("Book3 " + "NPV: " + aggResults["Book3"]);
        }

        public static void InitIdsMap(int nTrades){
            int acc = 1;
            for (int i = 0; i < nTrades / tradesPerTask; i++)
            {
                Object[] ids = new Object[tradesPerTask];
                for (int j = 0; j < tradesPerTask; j++)
                {
                    ids[j] = acc;
                    acc++;
                }
                IdsMap.Add(i, ids);
            }

        }

        public static Job submitJob(int tasks, String serviceName, String functionName, Dictionary<String, String> paramters)
        {
            JobId++;
            Console.WriteLine(" - Executing Job " + JobId);
            JobResult jobResult = new JobResult();
            Request[] requests = new Request[tasks];
            
                for (int i = 0; i < tasks; i++)
                {
                    requests[i] = new Request();
                    requests[i].JobID = JobId + "";
                    requests[i].TaskID = ((JobId) + "_" + (i));
                    requests[i].Payload = "PayloadData_" + requests[i].TaskID;
                    requests[i].ServiceName = serviceName;
                    requests[i].FunctionName = functionName;
                    requests[i].Parameters = paramters;
                    requests[i].Priority =  i % 4 + 1;
                    requests[i].Rate = Rate;
                    requests[i].TradeIds = IdsMap[MapIndex];
                    MapIndex++;   
                }
            try
            {
                SpaceProxy.WriteMultiple(requests);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw e;
            }

            Job job = new Job();
            job.JobID = JobId + "";
            job.NumberOfTask = tasks;
            SpaceProxy.Write(job);
            return job;
        }
    }
}