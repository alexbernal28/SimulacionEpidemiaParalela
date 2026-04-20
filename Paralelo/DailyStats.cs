using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paralelo
{
    public class DailyStats
    {
        public int Day { get; set; }
        public int CurrentInfected { get; set; }
        public int AccumulatedDead { get; set; }
        public int AccumulatedRecovered { get; set; }
        public int NewInfectionsToday { get; set; }
    }
}
