using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring
{
    class Scaler
    {
        private static int UpperThreashold = 50;
        private static int LowerThreashold = 10;
        private static int CoolTime = 5;
        private static String url = null;

        static void Main(string[] args)
        {
            CoolTime = args.Length == 1 ? 5 : Convert.ToInt32(args[0]);
            UpperThreashold = args.Length == 1 ? 50 : Convert.ToInt32(args[1]);
            LowerThreashold = args.Length == 2 ? 10 : Convert.ToInt32(args[2]);
            new Scaler();
        }

        public Scaler()
        {

        }
    }
}
