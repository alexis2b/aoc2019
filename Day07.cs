using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day07
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day07.txt")).SelectMany(l => l.Split(',')).Select(Int32.Parse).ToList();

            var res1 = MaxThrusterSignal(input);
            Console.WriteLine($"Day07 - part1 - result: {res1}");

            var res2 = MaxThrusterSignalWithFeedbackLoop(input);
            Console.WriteLine($"Day07 - part2 - result: {res2}");
        }

        public static int MaxThrusterSignal(List<int> program)
        => Permutations(new[]{0,1,2,3,4}).Select(p => ThrusterSignal(program, p.ToList())).Max();

        public static int MaxThrusterSignalWithFeedbackLoop(List<int> program)
        => Permutations(new[]{5,6,7,8,9}).Select(p => ThrusterSignalWithFeedbackLoop(program, p.ToList())).Max();

        public static IEnumerable<int[]> Permutations(int[] input)
        {
            var permutations = new List<int[]>();

            int Loop(List<int> b, List<int> t)
            {
                if ( t.Count == 0 )
                    permutations.Add(b.ToArray());
                else
                    t.Select((v, i) => Loop(b.Append(v).ToList(), t.Where((v2, i2) => i2 != i).ToList())).ToList();
                return 0;
            }

            Loop(new List<int>(), input.ToList());
            return permutations;
        }

        public static int ThrusterSignal(List<int> program, List<int> phases)
        {
            var signal = 0;
            for(var i = 0; i < phases.Count; i++)
            {
                var phase = phases[i];
                var computer = new IntComputer();
                computer.Run(program, new[] { phase, signal });
                signal = computer.Outputs[0];
            }
            return signal;
        }

        public static int ThrusterSignalWithFeedbackLoop(List<int> program, List<int> phases)
        {
            var computers = phases.Select(p => new IntComputer()).ToList();
            
            // initial run - inject phase as single input
            for(var i = 0; i < computers.Count; i++)
                computers[i].Run(program, new[] {phases[i]});

            // inject 0 into 1st computer to kickoff the process
            computers.First().AddInput(0);

            // keep going, feeding output of computers(n-1) into computers(n), until all computers have finished
            do
            {
                for(var i = 0; i < computers.Count; i++)
                {
                    var previousComputer = i > 0 ? computers[i-1] : computers.Last();
                    while(previousComputer.OutputCount > 0)
                        computers[i].AddInput(previousComputer.PopOutput());
                    computers[i].Continue(Enumerable.Empty<int>());
                }
            } while(computers.Last().LastExitCode != IntComputer.ExitCode.Ended);

            return computers.Last().Outputs.Last();
        }
    }


    [TestFixture]
    internal class Day07Tests
    {
        [TestCase(new[] {3,15,3,16,1002,16,10,16,1,16,15,15,4,15,99,0,0}, new[] {4,3,2,1,0}, 43210)]
        [TestCase(new[] {3,23,3,24,1002,24,10,24,1002,23,-1,23,101,5,23,23,1,24,23,23,4,23,99,0,0}, new[] {0,1,2,3,4}, 54321)]
        [TestCase(new[] {3,31,3,32,1002,32,10,32,1001,31,-2,31,1007,31,0,33,1002,33,7,33,1,33,31,31,1,32,31,31,4,31,99,0,0,0}, new[] {1,0,4,3,2}, 65210)]
        public void Test1(int[] program, int[] phases, int expected)
        {
            var actual  = Day07.ThrusterSignal(program.ToList(), phases.ToList());
            Assert.AreEqual(expected, actual);
        }

        [TestCase(new[] {3,26,1001,26,-4,26,3,27,1002,27,2,27,1,27,26,27,4,27,1001,28,-1,28,1005,28,6,99,0,0,5}, new[] {9,8,7,6,5}, 139629729)]
        [TestCase(new[] {3,52,1001,52,-5,52,3,53,1,52,56,54,1007,54,5,55,1005,55,26,1001,54,-5,54,1105,1,12,1,53,54,53,1008,54,0,55,1001,55,1,55,2,53,55,53,4,53,1001,56,-1,56,1005,56,6,99,0,0,0,0,10}, new[] {9,7,8,5,6}, 18216)]
        public void Test2(int[] program, int[] phases, int expected)
        {
            var actual  = Day07.ThrusterSignalWithFeedbackLoop(program.ToList(), phases.ToList());
            Assert.AreEqual(expected, actual);
        }
    }
}
