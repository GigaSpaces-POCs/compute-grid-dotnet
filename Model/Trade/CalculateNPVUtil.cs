using GigaSpaces.Core;
using GigaSpaces.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterWorkerModel
{
    public class CalculateNPVUtil
    {

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
            }
        }

        public static Trade[] getMissingTradesFromSpace(List<Object> missingIds, ISpaceProxy tradeDataSpace)
        {

            IReadByIdsResult<Trade> result = tradeDataSpace.ReadByIds<Trade>(missingIds.ToArray());
            return result.ResultsArray;
        }

        public static Dictionary<String, Double> execute(ILocalCache localCache, ISpaceProxy tradeDataSpace, Object[] tradeIds, Double rate)
        {
            Dictionary<String, Double> rtnVal = new Dictionary<string, Double>();
            IReadByIdsResult<Trade> result = localCache.ReadByIds<Trade>(tradeIds);
            List<Trade> tlist = new List<Trade>();
            foreach (Trade t in result) {
                tlist.Add(t);
            }
            runAnalysis(tlist, rate);
            foreach (Trade t in tlist) {
                String key = t.getBook();
                if (rtnVal.ContainsKey(key)) {
                    rtnVal[key] = rtnVal[key] + t.NPV;
                }
                else {
                    rtnVal.Add(key, t.NPV);
                }
            }
            return rtnVal;
        }

        public static Trade generateTrade(int id)
        {
            Trade trade = new Trade();
            trade.id = id;
            CacheFlowData cf = new CacheFlowData();
            if (id < 10000) {
                cf.cacheFlowYear0 = ((Double)(id * -100));
                cf.cacheFlowYear1 = ((Double)(id * 20));
                cf.cacheFlowYear2 = ((Double)(id * 40));
                cf.cacheFlowYear3 = ((Double)(id * 60));
                cf.cacheFlowYear4 = ((Double)(id * 80));
                cf.cacheFlowYear5 = ((Double)(id * 100));
            }
            else {
                cf.cacheFlowYear0 = ((Double)(id * -1));
                cf.cacheFlowYear1 = ((Double)(id * 0.2));
                cf.cacheFlowYear2 = ((Double)(id * 0.4));
                cf.cacheFlowYear3 = ((Double)(id * 0.6));
                cf.cacheFlowYear4 = ((Double)(id * 0.8));
                cf.cacheFlowYear5 = ((Double)(id));
            }
            trade.cacheFlowData = cf;
            trade.NPV = 0.0;
            return trade;
        }


        public static void subreducer(Dictionary<String, Double> aggregatedNPVCalc, Dictionary<String, Double> incPositions)
        {
            foreach (String key in incPositions.Keys)
            {
                if (aggregatedNPVCalc.ContainsKey(key))
                {
                    Double currentNPV = aggregatedNPVCalc[key];
                    aggregatedNPVCalc[key] = currentNPV + incPositions[key];
                }
                else 
                {
                    aggregatedNPVCalc.Add(key, incPositions[key]);
                }
            }
        }
    }
}
