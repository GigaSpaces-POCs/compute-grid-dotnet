using GigaSpaces.Core;
using GigaSpaces.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterWorkerModel
{
    public class CalculateUtil
    {
        static Random random = new Random();

        public static void calculateNPV(Double rate, Trade trade)
        {
            double disc = 1.0 / (1.0 + (double)(rate / 100));
            CacheFlowData cf = trade.cacheFlowData;
            double NPV = (cf.cacheFlowYear0 +
                    disc * (cf.cacheFlowYear1 +
                    disc * (cf.cacheFlowYear2 +
                    disc * (cf.cacheFlowYear3 +
                    disc * (cf.cacheFlowYear4 +
                    disc * cf.cacheFlowYear5)))));
            trade.NPV = NPV;
        }

        public static void runAnalysis(List<Trade> trades, Double rate)
        {
            foreach (Trade t in trades)
            {
                calculateNPV(rate, t);
                calculateIRR(t);
            }
        }

        public static Dictionary<String, Double[]> execute(ISpaceProxy tradeDataSpace, ILocalCache workerCache, Object[] tradeIds, Double rate)
        {
            Dictionary<String, Double[]> rtnVal = new Dictionary<string, Double[]>();
            IReadByIdsResult<Trade> result = workerCache.ReadByIds<Trade>(tradeIds);
            List<Trade> tlist = new List<Trade>();
            foreach (Trade t in result) {
                tlist.Add(t);
            }
            runAnalysis(tlist, rate);
            foreach (Trade t in tlist) {
                String key = t.getBook();
                if (rtnVal.ContainsKey(key)) {
                    rtnVal[key][0] = rtnVal[key][0] + t.NPV;
                    rtnVal[key][1] = rtnVal[key][1] + t.IRR;
                }
                else {
                    rtnVal.Add(key, new Double[] {t.NPV , t.IRR});
                }
            }
            return rtnVal;
        }

        public static Trade generateTrade(int id)
        {
            Trade trade = new Trade();
            trade.id = id;
            CacheFlowData cf = new CacheFlowData();
            cf.cacheFlowYear0 = trunc(random.NextDouble() * -10000);
            cf.cacheFlowYear1 = trunc(random.NextDouble() * 10000);
            cf.cacheFlowYear2 = trunc(random.NextDouble() * 10000);
            cf.cacheFlowYear3 = trunc(random.NextDouble() * 10000);
            cf.cacheFlowYear4 = trunc(random.NextDouble() * 10000);
            cf.cacheFlowYear5 = trunc(random.NextDouble() * 10000);
            trade.cacheFlowData = cf;
            trade.NPV = 0.0;
            return trade;
        }

        private static Double trunc(Double d)
        {
            return Math.Truncate(d * 1000) / 1000;
         }


        public static void subreducer(Dictionary<String, Double[]> aggregatedNPVCalc, Dictionary<String, Double[]> incPositions)
        {
            if(incPositions != null)
            foreach (String key in incPositions.Keys)
            {
                if (aggregatedNPVCalc.ContainsKey(key))
                {
                    Double currNPV = aggregatedNPVCalc[key][0];
                    aggregatedNPVCalc[key][0] = currNPV + incPositions[key][0];
                    Double currIRR = aggregatedNPVCalc[key][1];
                    aggregatedNPVCalc[key][1] = currIRR + incPositions[key][1];
                    }
                else 
                {
                    aggregatedNPVCalc.Add(key, incPositions[key]);
                }
            }
        }

        public static Trade calculateIRR(Trade trade)
        {
            int MAX_ITER = 20;
            double EXCEL_EPSILON = 0.00000001;
            double[] cashFlows = { trade.cacheFlowData.cacheFlowYear0, trade.cacheFlowData.cacheFlowYear1, trade.cacheFlowData.cacheFlowYear2, trade.cacheFlowData.cacheFlowYear3, trade.cacheFlowData.cacheFlowYear4, trade.cacheFlowData.cacheFlowYear5 };
            double x = 0.1;
            int iter = 0;
            while (iter < MAX_ITER)
            {   
                double x1 = 1.0 + x;
                double fx = 0.0;
                double dfx = 0.0;
                for (int i = 0; i < cashFlows.Length; i++)
                {
                    double v = cashFlows[i];
                    double x1_i = Math.Pow(x1, i);
                    fx += v / x1_i;
                    double x1_i1 = x1_i * x1;
                    dfx += -i * v / x1_i1;
                }
                double new_x = x - fx / dfx;
                double epsilon = Math.Abs(new_x - x);
               
                if (epsilon <= EXCEL_EPSILON)
                {
                    //Console.WriteLine("iter=" + iter);
                    if (x == 0.0 && Math.Abs(new_x) <= EXCEL_EPSILON)
                    {
                        trade.IRR = 0.0;
                        return trade; // OpenOffice calc does this
                    }
                    else
                    {
                        trade.IRR = new_x * 100;
                        return trade;
                    }
                    
                }
                
                x = new_x;
                trade.IRR = x;
                iter++;
            }
            return trade;
        }

    }
}
