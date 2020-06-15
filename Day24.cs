using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day24
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day24.txt")).ToList();

            var res1 = Part1(input);
            Console.WriteLine($"Day24 - part1 - result: {res1}");

            // var res2 = Part2(input);
            // Console.WriteLine($"Day24 - part2 - result: {res2}");
        }

        public static long Part1(List<string> input)
        {
            var map = Map.FromInput(input);
            var knownMaps = new HashSet<long>() { map.BiodiversityRating() }; // we use biodiversityrating as a hash
            while(true)
            {
                map = map.NextIteration();
                if(!knownMaps.Add(map.BiodiversityRating()))
                    return map.BiodiversityRating(); // part 1 solution
            }
        }

        // public static int Part2(List<string> input)
        // {
        //     var maze = Maze.FromMap(input);
        //     return maze.ShortestPath2("AA", "ZZ");
        // }


        public sealed class Map
        {
            private readonly int _width;
            private readonly List<int> _map;
            private int _iteration;

            public Map(int width, List<int> map, int iteration)
            {
                _width = width;
                _map = map;
                _iteration = iteration;
            }

            public static Map FromInput(List<string> input)
            {
                var map = input.SelectMany(s => s.Select(c => c == '#'? 1 : 0)).ToList();
                return new Map(input[0].Length, map, 0);
            }

            public Map NextIteration()
            {
                var nextMap = _map.Select((b, i) => NextCellIteration(b, i)).ToList();
                return new Map(_width, nextMap, _iteration+1);
            }

            public long BiodiversityRating() => _map.Select((b, i) => b == 1 ? 1L << i : 0).Sum();

            private int NextCellIteration(int current, int position)
            {
                // count adjacent cells
                var row   = position % _width;
                var count = 0;
                if (row > 0       ) count += _map[position-1];
                if (row < _width-1) count += _map[position+1];
                if ( position - _width >= 0) count += _map[position-_width];
                if ( position + _width < _map.Count) count += _map[position+_width];
                if ( current == 1 ) return count == 1 ? 1 : 0;
                return count == 1 || count == 2 ? 1 : 0;
            }
        }
    }


    [TestFixture]
    internal class Day24Tests
    {
        private readonly List<string> _map0 = new List<string>() {
            "....#",
            "#..#.",
            "#..##",
            "..#..",
            "#...."
        };
        private readonly List<string> _map1 = new List<string>() {
            "#..#.",
            "####.",
            "###.#",
            "##.##",
            ".##.."
        };
        private readonly List<string> _map2 = new List<string>() {
            "#####",
            "....#",
            "....#",
            "...#.",
            "#.###"
        };
        private readonly List<string> _map3 = new List<string>() {
            "#....",
            "####.",
            "...##",
            "#.##.",
            ".##.#"
        };
        private readonly List<string> _map4 = new List<string>() {
            "####.",
            "....#",
            "##..#",
            ".....",
            "##..."
        };

        [Test]
        public void TestMapIterations()
        {
            var map0 = Day24.Map.FromInput(_map0);
            var map1 = map0.NextIteration();
            Assert.AreEqual(Day24.Map.FromInput(_map1).BiodiversityRating(), map1.BiodiversityRating());
            var map2 = map1.NextIteration();
            Assert.AreEqual(Day24.Map.FromInput(_map2).BiodiversityRating(), map2.BiodiversityRating());
            var map3 = map2.NextIteration();
            Assert.AreEqual(Day24.Map.FromInput(_map3).BiodiversityRating(), map3.BiodiversityRating());
            var map4 = map3.NextIteration();
            Assert.AreEqual(Day24.Map.FromInput(_map4).BiodiversityRating(), map4.BiodiversityRating());
        }

        [Test]
        public void TestBiodiversityRating()
        {
            var map = new Day24.Map(5, new List<int> {0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 1,0,0,0,0, 0,1,0,0,0,}, 0);
            Assert.AreEqual(2129920, (int) map.BiodiversityRating());
        }

        [Test]
        public void TestPart1()
        {
            var actual = Day24.Part1(_map0);
            Assert.AreEqual(2129920, (int) actual);
        }
    }
}
