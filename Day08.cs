using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day08
    {
        public static void Run()
        {
            var dim   = (w: 25, h: 6);
            var input = File.ReadAllText(Path.Combine("input", "day08.txt"));

            var res1 = Part1(dim, input);
            Console.WriteLine($"Day08 - part1 - result: {res1}");

            var res2 = Part2(dim, input);
            Console.WriteLine($"Day08 - part2 - result:");
            Render(dim, res2);
        }

        private static int Part1((int w, int h) dim, string input)
        {
            var layersWithFewest0 = Layers(dim, input)
                .Select(l => new Weighted<string>(l, l.Count(c => c == '0')))
                .Min().Value;
            return layersWithFewest0.Count(c => c == '1') * layersWithFewest0.Count(c => c == '2');
        }

        public static string Part2((int w, int h) dim, string input)
        {
            var layers = Layers(dim, input).ToList();
            var image  = new StringBuilder(layers[0]);

            // progressively replace transparent pixels with the layer below
            for(var l = 1; l < layers.Count; l++)
                for(var c = 0; c < image.Length; c++)
                    if ( image[c] == '2' ) image[c] = layers[l][c];

            return image.ToString();
        }

        public static void Render((int w, int h) dim, string image)
        {
            for(var c = 0; c < image.Length; c++)
            {
                Console.Write(image[c] == '0' ? "  " : "##"); // 2-characters wide gives better proportions (more readable)
                if ((c+1)%dim.w == 0) Console.WriteLine();
            }
        }

        // Cut an input into layers
        public static IEnumerable<string> Layers((int w, int h) dim, string input)
        {
            var n = dim.w*dim.h;
            for(var i = 0; i < input.Length; i += n)
                yield return input.Substring(i, n);
        }
    }


    [TestFixture]
    internal class Day08Tests
    {
        [Test]
        public void Test1_1()
        {
            var layers  = Day08.Layers((3,2), "123456789012").ToList();
            Assert.AreEqual(2, layers.Count);
            Assert.AreEqual("123456", layers[0]);
            Assert.AreEqual("789012", layers[1]);
        }

        [Test]
        public void Test2_1()
        {
            var image = Day08.Part2((2,2), "0222112222120000");
            Assert.AreEqual("0110", image);
        }
    }
}
