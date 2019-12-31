using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day16
    {
        public static void Run()
        {
            var inputStr = File.ReadAllLines(Path.Combine("input", "day16.txt"))[0];

            var res1 = Part1(inputStr);
            Console.WriteLine($"Day16 - part1 - result: {res1}");

            var res2 = Part2(inputStr);
            Console.WriteLine($"Day16 - part2 - result: {res2}");
        }

        public static string Part1(string inputStr)
        {
            var input = inputStr.Select(c => c - '0').ToList();
            var fftResult = FftCalculator.RepeatFft(input, 100);
            return String.Join("", fftResult.Take(8));
        }

        public static string Part2(string inputStr)
        {
            // The trick for Part2 is to realize that given the offset being > 50% of the 
            // length of the input, the repeating pattern is basically a lot of 0s, and then all 1s
            // (so the last output digit is equal to the last input) and then it is a cumulative sum
            // with the i'th digit for xi)
            // also we only need the "end of the sequence" (after offset) and can ignore the rest

            // build the initial input sequence, starting at offset
            var offset        = Int32.Parse(inputStr.Substring(0, 7));
            var partialOffset = offset % inputStr.Length;
            var fullRemain    = 10000 - (offset / inputStr.Length) - 1; // -1 is for the partial sequence we add separately

            var fullInputStrBuilder = new StringBuilder();
            fullInputStrBuilder.Append(inputStr.Substring(partialOffset));
            for (var i = 0; i < fullRemain; i++)
                fullInputStrBuilder.Append(inputStr);
            var fullInputStr = fullInputStrBuilder.ToString();
            var input = fullInputStr.Select(c => c - '0').ToList();

            // top-level, repeat 100 times mechanism
            var lastI = input.Count - 1;
            for(var phase = 0; phase < 100; phase++)
            {
                // calculate using the backward cum-sum mechanism
                var output = input.ToList();
                var cumsum = 0;
                for(int i = lastI; i >= 0; i--)
                {
                    cumsum += input[i];
                    output[i] = cumsum % 10;  // Abs value is not necessary since we never substract
                }
                // feed back into next cycle
                input = output;
            }

            return String.Join("", input.Take(8));
        }

        public class FftCalculator
        {
            private static readonly List<int> BasePattern = new List<int> { 0, 1, 0, -1 };

            public static List<int> Fft(List<int> input)
            {
                var output = new List<int>();
                var N = input.Count;
                for (var k = 1; k <= N; k++)
                {
                    var newD = Math.Abs(
                        input.Select((v, n) =>
                            v * BasePattern[((n + 1) / k) % BasePattern.Count]
                        ).Sum()
                    ) % 10;
                    output.Add(newD);
                }
                return output;
            }

            public static List<int> RepeatFft(List<int> input, int count)
            {
                Console.WriteLine($"Running {count} fft:");
                var current = input.ToList();
                for (var i = 0; i < count; i++)
                {
                    current = Fft(current);
                    Console.Write('.');
                }
                Console.WriteLine();
                return current;
            }
        }
    }


    [TestFixture]
    internal class Day16Tests
    {
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 1, new[] { 4, 8, 2, 2, 6, 1, 5, 8 })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 2, new[] { 3, 4, 0, 4, 0, 4, 3, 8 })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 3, new[] { 0, 3, 4, 1, 5, 5, 1, 8 })]
        [TestCase(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 4, new[] { 0, 1, 0, 2, 9, 4, 9, 8 })]
        [TestCase(new[] { 8, 0, 8, 7, 1, 2, 2, 4, 5, 8, 5, 9, 1, 4, 5, 4, 6, 6, 1, 9, 0, 8, 3, 2, 1, 8, 6, 4, 5, 5, 9, 5 }, 100, new[] { 2, 4, 1, 7, 6, 1, 7, 6 })]
        [TestCase(new[] { 1, 9, 6, 1, 7, 8, 0, 4, 2, 0, 7, 2, 0, 2, 2, 0, 9, 1, 4, 4, 9, 1, 6, 0, 4, 4, 1, 8, 9, 9, 1, 7 }, 100, new[] { 7, 3, 7, 4, 5, 4, 1, 8 })]
        [TestCase(new[] { 6, 9, 3, 1, 7, 1, 6, 3, 4, 9, 2, 9, 4, 8, 6, 0, 6, 3, 3, 5, 9, 9, 5, 9, 2, 4, 3, 1, 9, 8, 7, 3 }, 100, new[] { 5, 2, 4, 3, 2, 1, 3, 3 })]
        public void TestFftCalculator(IEnumerable<int> input, int count, int[] first8Expected)
        {
            var input1 = input.ToList();
            var result = Day16.FftCalculator.RepeatFft(input1, count);
            var actual = result.Take(8).ToArray();
            Assert.AreEqual(first8Expected, actual);
        }

        [TestCase("80871224585914546619083218645595", "24176176")]
        [TestCase("19617804207202209144916044189917", "73745418")]
        [TestCase("69317163492948606335995924319873", "52432133")]
        public void TestPart1(string input, string expected)
        {
            var actual = Day16.Part1(input);
            Assert.AreEqual(expected, actual);
        }


        [TestCase("03036732577212944063491565474664", "84462026")]
        [TestCase("02935109699940807407585447034323", "78725270")]
        [TestCase("03081770884921959731165446850517", "53553731")]
        public void TestPart2(string input, string expected)
        {
            var actual = Day16.Part2(input);
            Assert.AreEqual(expected, actual);
        }
    }
}
