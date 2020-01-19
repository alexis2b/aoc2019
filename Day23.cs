using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day23
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day23.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = Part1(input);
            Console.WriteLine($"Day23 - part1 - result: {res1}");

            var res2 = Part2(input);
            Console.WriteLine($"Day23 - part2 - result: {res2}");
        }

        public static long Part1(IEnumerable<long> program)
        {
            // create 50 computers instances, all loaded up
            const int nc = 50;
            var computers = new IntComputer[nc];
            for(var i = 0; i < nc; i++)
            {
                computers[i] = new IntComputer();
                computers[i].Run(program, new[] { (long) i});
            }

            // Run until a packet is sent to interface address 255
            while(true)
            {
                for(var i = 0; i < nc; i++)
                {
                    // feed an input to computer [i], at least -1
                    if(computers[i].InputCount == 0)
                        computers[i].AddInput(-1L);

                    // continue
                    computers[i].Continue(new long[]{});

                    // get the outputs of the computer [i]
                    while(computers[i].OutputCount > 0)
                    {
                        var destinationAddress = (int) computers[i].PopOutput();
                        var x = computers[i].PopOutput();
                        var y = computers[i].PopOutput();

                        if(destinationAddress == 255)
                            return y; // Part 1 solution

                        // push the packet to its destination
                        Debug.Assert(destinationAddress < nc);
                        computers[destinationAddress].AddInput(x);
                        computers[destinationAddress].AddInput(y);
                    }
                }
            }
        }

        public static long Part2(IEnumerable<long> program)
        {
            // create 50 computers instances, all loaded up
            const int nc = 50;
            var computers = new IntComputer[nc];
            for(var i = 0; i < nc; i++)
            {
                computers[i] = new IntComputer();
                computers[i].Run(program, new[] { (long) i});
            }

            // NAT last packet
            long natX = 0L, natY = 0L, lastNatYSent = Int64.MaxValue;

            // Run until a packet is sent to interface address 255
            while(true)
            {
                var idleCycle = true;
                for(var i = 0; i < nc; i++)
                {
                    // feed an input to computer [i], at least -1
                    if(computers[i].InputCount == 0)
                        computers[i].AddInput(-1L);
                    else
                        idleCycle = false;

                    // continue
                    computers[i].Continue(new long[]{});

                    // get the outputs of the computer [i]
                    while(computers[i].OutputCount > 0)
                    {
                        var destinationAddress = (int) computers[i].PopOutput();
                        var x = computers[i].PopOutput();
                        var y = computers[i].PopOutput();

                        if(destinationAddress == 255)
                        {
                            natX = x;
                            natY = y;
                        }
                        else
                        {
                            // push the packet to its destination
                            Debug.Assert(destinationAddress < nc);
                            computers[destinationAddress].AddInput(x);
                            computers[destinationAddress].AddInput(y);
                        }
                    }
                }
                if(idleCycle)
                {
                    // check end of Part 2 condition
                    if(natY == lastNatYSent)
                        return natY;
                    
                    // push NAT packet to computer 0
                    computers[0].AddInput(natX);
                    computers[0].AddInput(natY);
                    lastNatYSent = natY;
                }
            }
        }
    }


    [TestFixture]
    internal class Day23Tests
    {
        // Nothing today
    }
}
