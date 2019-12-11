using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aoc2019
{
    internal class Day09
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day09.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = RunBoostDiagnostics(input);
            Console.WriteLine($"Day09 - part1 - result: {res1}");

            var res2 = RunSensorBoost(input);
            Console.WriteLine($"Day09 - part2 - result: {res2}");
        }

        public static long RunBoostDiagnostics(IEnumerable<long> program)
        {
            var computer = new IntComputer();
            computer.Run(program, new long[]{1}); // 1 is for Test Mode
            if ( computer.OutputCount > 1 )
                throw new Exception("self-diagnostic failed, error: " + string.Join(",", computer.Outputs));
            return computer.PopOutput();
        }

        public static long RunSensorBoost(IEnumerable<long> program)
        {
            var computer = new IntComputer();
            computer.Run(program, new long[]{2}); // 2 is for Actual Boost Mode
            if ( computer.OutputCount > 1 )
                throw new Exception("there should be only one output");
            return computer.PopOutput();
        }
    }
}
