using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day25
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day25.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = Part1(input);
            Console.WriteLine($"Day25 - part1 - result: {res1}");

            // var res2 = Part2(input);
            // Console.WriteLine($"Day25 - part2 - result: {res2}");
        }

        // get to the checkpoint with all the possible items on the floor
        // once we are there, we run phase 2 - an automatic search algorithm
        private static readonly List<string> _initInstructions = new List<string>() {
            "north",
            "north",
            "north",
            "take mutex",
            "south",
            "south",
            "east",
            "north",
            "take loom",
            "south",
            "west",
            "south",
            "east",
            "take semiconductor",
            "east",
            "north",
            // in Navigation - do not take the photons!
            "west",
            // in Storage - do not take the infinite loop!
            "west",
            // in Hot Chocolate Fountain - do not take the giant electromagnet!
            "east",
            "east",
            "south", // to Arcade
            "take ornament",
            "west", // to Kitchen
            "west", // to Hull
            "west", // to Engineering
            "west", // to Hallway
            "take sand",
            "south", // to Sick Bay - do not take the molten lava!
            "east", // to Warp Drive Maintenance
            "take asterisk",
            "north", // to Stables
            "take wreath",
            "south", // to Warp Drive Maintenance
            "west", // to Sick Bay
            "north", // to Hallway
            "north", // to Science Lab
            "take dark matter",
            "east", // to Checkpoint
            // now start dropping stuff to find the right weight
            "drop semiconductor",
            "drop loom",
            "drop mutex",
            "drop sand",
            "drop asterisk",
            "drop wreath",
            "drop dark matter",
            "drop ornament",
            // try
            //"east"
        };

        // Attempts
        // semiconductor X       XX
        // loom           X      XX
        // mutex           X      X
        // sand             X
        // asterisk          X
        // wreath             X
        // dark matter         X
        // ornament             X
        //               LLLLLLLHLL

        public static long Part1(IEnumerable<long> program)
        {
            var computer = new IntComputer(false);

            // helpers
            void ContinueWith(string input)
            {
                computer.Continue(input.Select(c => (long) c));
                computer.Continue(new[] {10L});
            }

            string OutputsToString()
            {
                var s = new string(computer.Outputs.Select(o => (char) o).ToArray());
                computer.ClearOutputs();
                return s;
            }

            var ec = computer.Run(program);
            var i = 0;
            while(true)
            {
                // render output
                Console.Write(OutputsToString());

                // take and feed input
                string input;
                if(i < _initInstructions.Count)
                {
                    input = _initInstructions[i];
                    Console.WriteLine(input);
                } else
                    break; // move to phase 2
                    //input = Console.ReadLine();
                ContinueWith(input);
                i++;
            }
            // Phase 2 - search matching combination
            // we have 8 items, so it is a search from 0 to 255 using binary rep.
            var items = new[] {"semiconductor", "loom", "mutex", "sand", "asterisk", "wreath", "dark matter", "ornament"};
            for(var c = 0; c < 256; c++)
            {
                var picks = items.Where((s, p) => (c & (1 << p)) != 0).ToList();
                // pick up
                picks.ForEach(item => ContinueWith($"take {item}"));
                var o1 = OutputsToString();
                Console.WriteLine($"{c,3} picked " + string.Join(", ", picks));
                // test
                ContinueWith("east");
                var output = OutputsToString();
                if ( output.Contains("heavier") )
                    Console.WriteLine("too light");
                else if ( output.Contains("lighter"))
                    Console.WriteLine("too heavy");
                else
                {
                    // win?
                    Console.WriteLine(output);
                    return 0;
                }

                // drop / reset
                picks.ForEach(item => ContinueWith($"drop {item}"));
            }
            return 1;
        }
    }


    [TestFixture]
    internal class Day25Tests
    {
        // Nothing today
    }
}
