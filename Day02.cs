using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aoc2019
{
    internal class Day02
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day02.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = ExecuteAndGetPosition0(input);
            Console.WriteLine($"Day01 - part1 - result: {res1}");

            var (noun, verb) = GuessNounAndVerb(input, 19690720);
            Console.WriteLine($"Day01 - part2 - result: {100*noun + verb}");
        }

        public static long ExecuteAndGetPosition0(IEnumerable<long> input)
        {
            var modifiedInput = input.ToList();
            modifiedInput[1] = 12;
            modifiedInput[2] =  2;
            var computer = new IntComputer();
            computer.Run(modifiedInput);
            return computer.Memory[0];
        }

        public static (int, int) GuessNounAndVerb(IEnumerable<long> input, long target)
        {
            for(var noun = 0; noun < 100; noun++)
                for(var verb = 0; verb < 100; verb++)
                {
                    var modifiedInput = input.ToList();
                    modifiedInput[1] = noun;
                    modifiedInput[2] = verb;
                    var computer = new IntComputer();
                    try
                    {
                        computer.Run(modifiedInput);
                        if ( computer.Memory[0] == target )
                            return (noun, verb);

                    } catch(Exception) { continue; }
                }
            return (0, 0);
        }
    }
}
