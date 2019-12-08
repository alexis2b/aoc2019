using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aoc2019
{
    internal class Day03
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day03.txt")).ToList();

            var res1 = FindClosestIntersectionDistance(input[0], input[1]);
            Console.WriteLine($"Day03 - part1 - result: {res1}");

            var res2 = FindShortestIntersectionDistance(input[0], input[1]);
            Console.WriteLine($"Day03 - part2 - result: {res2}");
        }

        public static int FindClosestIntersectionDistance(string wire1, string wire2)
        {
            var points1 = GetWirePoints(wire1.Split(','));
            var points2 = GetWirePoints(wire2.Split(','));
            var intersections = points1.Intersect(points2).Where(p => p != (0, 0));
            return intersections.Select(ManhattanDistanceToOrigin).Min();
        }

        public static int FindShortestIntersectionDistance(string wire1, string wire2)
        {
            var points1 = GetWirePoints(wire1.Split(',')).ToList();
            var points2 = GetWirePoints(wire2.Split(',')).ToList();
            var intersections = points1.Intersect(points2).Where(p => p != (0, 0));
            return intersections.Select(p => ShortestDistanceTo(points1, p) + ShortestDistanceTo(points2, p)).Min();
        }

        public static IEnumerable<(int x, int y)> GetWirePoints(IEnumerable<string> input)
        {
            var segments = input.Select(ParseSegment).ToList();
            var points   = new List<(int x, int y)>();
            
            var start = (x: 0, y: 0);
            points.Add(start);

            foreach(var segment in segments)
            {
                var end = start;
                foreach(var p in SegmentPoints(segment, start))
                {
                    end = p;
                    points.Add(p);
                }
                start = end;
            }

            return points;
        }

        private static (char d, int c) ParseSegment(string s)
        {
            var d = s[0];
            var c = Int32.Parse(s.Substring(1));
            return (d, c);
        }

        private static IEnumerable<(int x, int y)> SegmentPoints((char d, int c) segment, (int x, int y) start)
        {
            (int dx, int dy) =
                segment.d == 'U' ? (0, 1) :
                segment.d == 'R' ? (1, 0) :
                segment.d == 'D' ? (0, -1) :
                segment.d == 'L' ? (-1, 0) : (0, 0);
            for(int i = 1; i <= segment.c; i++)
                yield return (start.x + i*dx, start.y+i*dy);
        }

        private static int ManhattanDistance((int x, int y) p1, (int x, int y) p2)
            => Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);

        private static int ManhattanDistanceToOrigin((int x, int y) p)
            => ManhattanDistance(p, (0, 0));

        private static int ShortestDistanceTo(List<(int x, int y)> points, (int x, int y) point)
            => points.IndexOf(point);
    }

    [TestFixture]
    internal class Day03Tests
    {
        [TestCase("R8,U5,L5,D3", "U7,R6,D4,L4", 6)]
        [TestCase("R75,D30,R83,U83,L12,D49,R71,U7,L72", "U62,R66,U55,R34,D71,R55,D58,R83", 159)]
        [TestCase("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51", "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7", 135)]
        public void Test1(string wire1, string wire2, int expected)
        {
            var actual = Day03.FindClosestIntersectionDistance(wire1, wire2);
            Assert.AreEqual(expected, actual);
        }

        [TestCase("R8,U5,L5,D3", "U7,R6,D4,L4", 30)]
        [TestCase("R75,D30,R83,U83,L12,D49,R71,U7,L72", "U62,R66,U55,R34,D71,R55,D58,R83", 610)]
        [TestCase("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51", "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7", 410)]
        public void Test2(string wire1, string wire2, int expected)
        {
            var actual = Day03.FindShortestIntersectionDistance(wire1, wire2);
            Assert.AreEqual(expected, actual);
        }
    }
}
