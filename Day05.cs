using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aoc2019
{
    internal class Day05
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day05.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = CheckTestsAndGetDiagnosticCode(input, 1);
            Console.WriteLine($"Day05 - part1 - result: {res1}");

            var res2 = CheckTestsAndGetDiagnosticCode(input, 5);
            Console.WriteLine($"Day05 - part2 - result: {res2}");
        }

        public static long CheckTestsAndGetDiagnosticCode(List<long> program, long input)
        {
            var computer = new IntComputer();
            computer.Run(program, new[] {input});
            var outputs = computer.Outputs;
            for(var i = 0; i < outputs.Count - 1; i++)
            {
                if ( outputs[i] != 0 )
                    throw new Exception($"test {i} failed");
            }
            return outputs.Last();
        }
    }
}
