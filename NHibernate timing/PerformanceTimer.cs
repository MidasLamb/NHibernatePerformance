using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NHibernate_timing
{
    public class PerformanceTimer
    {
        private string name;
        private IDictionary<string, long> elapsedTicksDictionary = new Dictionary<string, long>();
        private long amountOfIterations;

        private string currentSection;
        private Stopwatch watch;
        public PerformanceTimer(string name)
        {
            this.name = name;
            watch = new Stopwatch();
        }

        public void NewIteration()
        {
            amountOfIterations++;
        }

        public void Start(string name)
        {
            currentSection = name;
            watch.Restart();
        }

        public void Stop()
        {
            long elapsedTicks = watch.ElapsedTicks;
            watch.Reset();
            if (elapsedTicksDictionary.ContainsKey(currentSection))
            {
                elapsedTicksDictionary[currentSection] += elapsedTicks;
            } else
            {
                elapsedTicksDictionary[currentSection] = elapsedTicks;
            }

            currentSection = null;
        }

        public void Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Timings for: {this.name}");
            foreach(var kvp in elapsedTicksDictionary)
            {
                decimal elapsedMs = (decimal)kvp.Value / Stopwatch.Frequency * 1000M / amountOfIterations;
                sb.AppendLine($"\t{kvp.Key}: \t{elapsedMs}ms");
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
