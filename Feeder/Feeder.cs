using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GigaSpaces.Core;
using MasterWorkerModel;

namespace Feeder
{
    public class Feeder
    {
        public static String url;
        public static int numOfTrades;
        public static ISpaceProxy SpaceProxy;
        public static void Main(string[] args)
        {
            url = args[0];
            Int32 batchSize = 1000000;
            numOfTrades = Convert.ToInt32(args[1]);
            Console.WriteLine("Connecting to Space:" + url);
            SpaceProxy = GigaSpacesFactory.FindSpace(url);
            Console.WriteLine("Inserting " + numOfTrades + " Trades in the space");
            
            Int32 k=0;
            if (numOfTrades <= batchSize)
            {
                Trade[] trades = new Trade[numOfTrades];
                for (int i = 0; i < numOfTrades; i++)
                {
                    trades[i] = CalculateNPVUtil.generateTrade(i + 1);
                }
                SpaceProxy.WriteMultiple(trades);
            }
            else
            {
                for (int i = 0; i < numOfTrades / batchSize; i++)
                {
                    Trade[] trades = new Trade[batchSize];
                    for (int j = 0; j < numOfTrades; j++)
                    {
                        trades[k] = CalculateNPVUtil.generateTrade(k + 1);
                        k++;
                    }
                    SpaceProxy.WriteMultiple(trades);
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
