using System;

namespace TestAICore.DataObjects
{
    public class DailyReturn
    {
        public DateTime Date { get; set; }

        public decimal DayReturn { get; set; }

        public decimal BenchmarkReturn { get; set; }

    }
}
