using System;
using System.Text;

namespace Chela.Compiler
{
    public class Benchmark
    {
        private static bool enabled = false;
        private static System.DateTime benchmarkStart;
        private static StringBuilder builder = null;

        public static bool Enabled {
            get {
                return enabled;
            }
            set {
                enabled = value;
            }
        }

        public static void Begin()
        {
            if(!enabled)
                return;

            benchmarkStart = System.DateTime.Now;
        }

        public static void End(string what)
        {
            if(!enabled)
                return;

            // Compute the benchmark time.
            System.TimeSpan benchmarkTime = System.DateTime.Now - benchmarkStart;

            // Store the message.
            if(builder == null)
                builder = new StringBuilder();
            builder.Append(string.Format("{0}: {1} ms\n", what, benchmarkTime.TotalMilliseconds));
        }

        public static void Resume()
        {
            if(!enabled)
                return;

            System.Console.WriteLine("Benchmarks resume:");
            System.Console.WriteLine("-----------------------------------------");
            System.Console.WriteLine(builder.ToString());
        }
    }
}

