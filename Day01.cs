using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aoc2019
{
    internal class Day01
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day01.txt")).Select(Int32.Parse).ToList();

            var res1 = ComputeFuelRequirements(input);
            Console.WriteLine($"Day01 - part1 - result: {res1}");

            var res2 = ComputeTotalFuelRequirements(input);
            Console.WriteLine($"Day01 - part2 - result: {res2}");
        }

        public static int ComputeFuelRequirements(IEnumerable<int> input)
            => input.Select(ModuleFuelRequirement).Sum();

        public static int ModuleFuelRequirement(int mass) => mass / 3 - 2;

        public static int ComputeTotalFuelRequirements(IEnumerable<int> input)
            => input.Select(ModuleFuelTotalRequirement).Sum();

        public static int ModuleFuelTotalRequirement(int mass)
        {
            var totalFuel = 0;
            var newFuel   = ModuleFuelRequirement(mass);
            while(newFuel > 0)
            {
                totalFuel += newFuel;
                newFuel   = ModuleFuelRequirement(newFuel);
            }
            return totalFuel;
        }
    }


    [TestFixture]
    internal class Day01Tests
    {
        [TestCase(12, 2)]
        [TestCase(14, 2)]
        [TestCase(1969, 654)]
        [TestCase(100756, 33583)]
        public void Test1(int mass, int fuel)
        {
            var res = Day01.ModuleFuelRequirement(mass);
            Assert.AreEqual(fuel, res);
        }

        [TestCase(14, 2)]
        [TestCase(1969, 966)]
        [TestCase(100756, 50346)]
        public void Test2(int mass, int fuel)
        {
            var res = Day01.ModuleFuelTotalRequirement(mass);
            Assert.AreEqual(fuel, res);
        }
    }
}
