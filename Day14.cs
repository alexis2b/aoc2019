using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day14
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day14.txt")).ToList();

            var res1 = Part1(input);
            Console.WriteLine($"Day14 - part1 - result: {res1}");

            var res2 = Part2(input);
            Console.WriteLine($"Day14 - part2 - result: {res2}");
        }

        public static int Part1(IEnumerable<string> input)
        {
            var reactions = input.Select(Reaction.FromString).ToList();
            var system    = new System(reactions);
            return system.OreNeededFor(1, "FUEL");
        }

        // we look for cycles in the reactions - the state is simply how much stock you have
        // each time we produce 1 more fuel we count how many ores we consumed and what is the
        // resulting state (stocks)
        public static long Part2(IEnumerable<string> input)
        {
            var totalOreAvailable = 1000000000000L;
            var reactions = input.Select(Reaction.FromString).ToList();
            var system    = new System(reactions);
            var stocks    = new Dictionary<string, int>();
            var oreNeededPerCycle = 0L;
            var fuelProducedPerCycle = 0L;
            var oreNeededAtEachStep = new List<int>(); // how much ore we consume at each step
            var stocksH = new HashSet<string>();
            // Search for the cycle (i.e. come back to 0 stocks)
            do
            {
                var stocksHStr = String.Join('+', stocks);
                if ( ! stocksH.Add(stocksHStr) )
                {
                    Console.WriteLine($"Cycle found at {fuelProducedPerCycle} FUEL produced: {stocksHStr}");
                    break;
                }
                var marginalOreNeeded = system.OreNeededFor(1, "FUEL", stocks);

                // sometimes we do not have cycles
                if (oreNeededPerCycle+marginalOreNeeded > totalOreAvailable)
                    return fuelProducedPerCycle;

                oreNeededAtEachStep.Add(marginalOreNeeded);
                oreNeededPerCycle += marginalOreNeeded;
                fuelProducedPerCycle++;
            } while(stocks.Values.Any(v => v!=0));

            // how many full cycles do we need to be at or below 1T ore
            var fullCyclesCount = totalOreAvailable / oreNeededPerCycle;
            var leftOverOre     = totalOreAvailable % oreNeededPerCycle;

            // add up a partial cycle until we reach (but not overflow) 1T ore
            var extraFuelProduced = 0L;
            var extraOreConsumed  = 0L;
            for(var i = 0; i < fuelProducedPerCycle; i++)
            {
                var oreNeededAtNextStep = oreNeededAtEachStep[i];
                if ( extraOreConsumed + oreNeededAtNextStep > leftOverOre )
                    break;
                extraOreConsumed += oreNeededAtNextStep;
                extraFuelProduced++;
            }

            return fullCyclesCount*fuelProducedPerCycle+extraFuelProduced;
        }


        public sealed class Reaction
        {
            private readonly List<(int n, string c)> _inputs;
            private readonly (int n, string c) _output;

            public (int n, string c) Output => _output;
            public IEnumerable<(int n, string c)> Inputs => _inputs;

            private Reaction(IEnumerable<(int n, string c)> inputs, (int n, string c) output)
            {
                _inputs = inputs.ToList();
                _output = output;
            }

            public static Reaction FromString(string reactionStr)
            {
                (int n, string c) ParseToken(string s) => (Int32.Parse(s.Split(' ')[0]), s.Split(' ')[1]);

                var splitIO = reactionStr.Split(" => ");
                var splitI  = splitIO[0].Split(", ");
                return new Reaction(splitI.Select(ParseToken), ParseToken(splitIO[1]));
            }

            public override string ToString()
            {
                var inputs = String.Join('+', _inputs);
                return $"{inputs} => {_output}";
            }
        }

        // A system is defined by its possible reactions
        public sealed class System
        {
            private readonly List<Reaction> _reactions;

            public System(IEnumerable<Reaction> reactions)
            {
                _reactions = reactions.ToList();
            }

            // Part 1 does not care about stocks
            public int OreNeededFor(int count, string compound)
                => OreNeededFor(count, compound, new Dictionary<string, int>());

            // Part 2 - we need to track stocks accross calls (passed dictionary will mutate)
            public int OreNeededFor(int count, string compound, Dictionary<string, int> stocks)
            {
                var oreConsumed = 0;
                var current     = new List<(int n, string c)>() { (count, compound) };
                while(current.Count > 0)
                {
                    var nextCurrent = new List<(int n, string c)>();
                    foreach(var chemical in current)
                    {
                        // Console.WriteLine($"Need {chemical}");

                        // ORE is always an input compound
                        if (chemical.c == "ORE")
                        {
                            oreConsumed += chemical.n;
                            // Console.WriteLine($"- adding to required quantity of ORE");
                            continue;
                        }

                        // Check in stocks
                        var needed = chemical;
                        var stock = stocks.GetValueOrDefault(needed.c); // how much do we have in stock?
                        if (stock >= needed.n) // we have enough in stock
                        {
                            stocks[needed.c] = stock - needed.n;
                            // Console.WriteLine($"- stock is {stock}, we have enough -> new stock is {stocks[needed.c]}");
                            continue;
                        }

                        // adjust the actual needed quantity for what we have in stock
                        needed = (needed.n - stock, needed.c);
                        // Console.WriteLine($"- stock is {stock}, so we only need {needed}");

                        // we need to make some, and we will store the extra, if any
                        var reaction   = _reactions.FirstOrDefault(r => r.Output.c == chemical.c);
                        if (reaction == null) // no reaction, we simply need this chemical, no impact on stock
                            throw new Exception($"No reaction found to produce {chemical.c}");

                        var multiplier = (int) Math.Ceiling((decimal) needed.n / reaction.Output.n);
                        // Console.WriteLine($"- performing {multiplier} times reaction {reaction} to get it");
                        foreach(var input in reaction.Inputs)
                            nextCurrent.Add((input.n*multiplier, input.c));
                        var extra = reaction.Output.n * multiplier - needed.n;
                        stocks[needed.c] = extra;
                    }
                    current = nextCurrent;
                }
                // Console.WriteLine($"Consumed {oreConsumed} ORE to produce {count} {compound}");
                return oreConsumed;
            }
        }
    }


    [TestFixture]
    internal class Day14Tests
    {
        [Test]
        public void Test1()
        {
            var reactions = new[] {
                "10 ORE => 10 A",
                "1 ORE => 1 B",
                "7 A, 1 B => 1 C",
                "7 A, 1 C => 1 D",
                "7 A, 1 D => 1 E",
                "7 A, 1 E => 1 FUEL"
            };
            var actual = Day14.Part1(reactions);
            Assert.AreEqual(31, actual);
        }

        [Test]
        public void Test2()
        {
            var reactions = new[] {
                "9 ORE => 2 A",
                "8 ORE => 3 B",
                "7 ORE => 5 C",
                "3 A, 4 B => 1 AB",
                "5 B, 7 C => 1 BC",
                "4 C, 1 A => 1 CA",
                "2 AB, 3 BC, 4 CA => 1 FUEL"
            };
            var actual = Day14.Part1(reactions);
            Assert.AreEqual(165, actual);
        }

        [Test]
        public void Test3()
        {
            var reactions = new[] {
                "157 ORE => 5 NZVS",
                "165 ORE => 6 DCFZ",
                "44 XJWVT, 5 KHKGT, 1 QDVJ, 29 NZVS, 9 GPVTF, 48 HKGWZ => 1 FUEL",
                "12 HKGWZ, 1 GPVTF, 8 PSHF => 9 QDVJ",
                "179 ORE => 7 PSHF",
                "177 ORE => 5 HKGWZ",
                "7 DCFZ, 7 PSHF => 2 XJWVT",
                "165 ORE => 2 GPVTF",
                "3 DCFZ, 7 NZVS, 5 HKGWZ, 10 PSHF => 8 KHKGT"
            };
            var actual = Day14.Part1(reactions);
            Assert.AreEqual(13312, actual);

            var actual2 = Day14.Part2(reactions);
            Assert.AreEqual(82892753, actual2);
        }

        [Test]
        public void Test4()
        {
            var reactions = new[] {
                "2 VPVL, 7 FWMGM, 2 CXFTF, 11 MNCFX => 1 STKFG",
                "17 NVRVD, 3 JNWZP => 8 VPVL",
                "53 STKFG, 6 MNCFX, 46 VJHF, 81 HVMC, 68 CXFTF, 25 GNMV => 1 FUEL",
                "22 VJHF, 37 MNCFX => 5 FWMGM",
                "139 ORE => 4 NVRVD",
                "144 ORE => 7 JNWZP",
                "5 MNCFX, 7 RFSQX, 2 FWMGM, 2 VPVL, 19 CXFTF => 3 HVMC",
                "5 VJHF, 7 MNCFX, 9 VPVL, 37 CXFTF => 6 GNMV",
                "145 ORE => 6 MNCFX",
                "1 NVRVD => 8 CXFTF",
                "1 VJHF, 6 MNCFX => 4 RFSQX",
                "176 ORE => 6 VJHF"
            };
            var actual = Day14.Part1(reactions);
            Assert.AreEqual(180697, actual);

            var actual2 = Day14.Part2(reactions);
            Assert.AreEqual(5586022, actual2);
        }

        [Test]
        public void Test5()
        {
            var reactions = new[] {
                "171 ORE => 8 CNZTR",
                "7 ZLQW, 3 BMBT, 9 XCVML, 26 XMNCP, 1 WPTQ, 2 MZWV, 1 RJRHP => 4 PLWSL",
                "114 ORE => 4 BHXH",
                "14 VRPVC => 6 BMBT",
                "6 BHXH, 18 KTJDG, 12 WPTQ, 7 PLWSL, 31 FHTLT, 37 ZDVW => 1 FUEL",
                "6 WPTQ, 2 BMBT, 8 ZLQW, 18 KTJDG, 1 XMNCP, 6 MZWV, 1 RJRHP => 6 FHTLT",
                "15 XDBXC, 2 LTCX, 1 VRPVC => 6 ZLQW",
                "13 WPTQ, 10 LTCX, 3 RJRHP, 14 XMNCP, 2 MZWV, 1 ZLQW => 1 ZDVW",
                "5 BMBT => 4 WPTQ",
                "189 ORE => 9 KTJDG",
                "1 MZWV, 17 XDBXC, 3 XCVML => 2 XMNCP",
                "12 VRPVC, 27 CNZTR => 2 XDBXC",
                "15 KTJDG, 12 BHXH => 5 XCVML",
                "3 BHXH, 2 VRPVC => 7 MZWV",
                "121 ORE => 7 VRPVC",
                "7 XCVML => 6 RJRHP",
                "5 BHXH, 4 VRPVC => 5 LTCX"
            };
            var actual = Day14.Part1(reactions);
            Assert.AreEqual(2210736, actual);

            var actual2 = Day14.Part2(reactions);
            Assert.AreEqual(460664, actual2);
        }
    }
}
