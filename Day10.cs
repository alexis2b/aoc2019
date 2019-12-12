using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day10
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day10.txt")).ToArray();

            var res1 = BestPosition(input);
            Console.WriteLine($"Day10 - part1 - result: {res1.Value} with {res1.Weight} asteroids");

            var res2 = VaporizedPosition(input, res1.Value, 200);
            Console.WriteLine($"Day10 - part2 - result: {res2.x*100+res2.y}");
        }

        // We can use the concept of ray tracing
        // Given a Start Position p0 and a Target Asteroid position pt:
        // Calculate the f(x)=ax+b that goes from p0 to pt
        // for each in between x or y value (manhattan style) check if we hit that position
        // if we hit that position precisely, and there is an asteroid there, we do not have line of sight
        public static Weighted<(int x, int y)> BestPosition(string[] input)
        {
            var positions = AsteroidPositions(input);
            var weighted  = new List<Weighted<(int x, int y)>>();
            // check each potential base position (each asteroid)
            foreach(var p0 in positions)
            {
                // check if we have line of sight to each other asteroid
                var others = positions.Where(p => p != p0);
                var lineOfSightCount = others.Count(p1 => HasLineOfSight(p0, p1, positions));
                weighted.Add(new Weighted<(int x, int y)>(p0, lineOfSightCount));
            }
            return weighted.Max();
        }

        // idea for part2
        // transform into polar coordinates
        // evaluate each other asteroid by increasing angle and decreasing distance
        // sort by angle of line of sight between p0 and p1 then by distance
        // execute each point in order - if clear line of sight > add to list and *remove position*
        // list contains vaporized asteroids in the right order
        public static (int x, int y) VaporizedPosition(string[] map, (int x, int y) p0, int which)
        {
            var others = AsteroidPositions(map).Where(p => p != p0);
            var withPolarCoordinates = others.Select(p => new {
                Cartesian = p,
                Polar = StandardPolarToMapPolar(CartesianToPolar(Vector(p0, p)))
            });
            // we sort clockwise then by furthest to closest
            var sorted = withPolarCoordinates.OrderBy(o => o.Polar.a).ThenByDescending(o => o.Polar.r).ToList();
            var vaporized = new List<(int x, int y)>(); // filled up in order of vaporization
            while(sorted.Count > 0)
            {
                for(var i = 0; i < sorted.Count; i++)
                {
                    var p = sorted[i].Cartesian;
                    if ( ! HasLineOfSight(p0, p, sorted.Select(s => s.Cartesian).ToList()) )
                        continue;
                    // vaporize
                    sorted.RemoveAt(i--); // decrease i because we want to hit the same index on next loop
                    vaporized.Add(p);
                    //Console.WriteLine($"{vaporized.Count}: {p}");
                }
            }
            return vaporized[which-1];
        }

        public static HashSet<(int x, int y)> AsteroidPositions(string[] map)
        {
            var positions = new HashSet<(int x, int y)>();
            for(var x = 0; x < map[0].Length; x++)
                for(var y = 0; y < map.Length; y++)
                    if (map[y][x] == '#')
                        positions.Add((x, y));
            return positions;
        }

        // check line of sight using ray-tracing method from p0 to p1
        public static bool HasLineOfSight((int x, int y) p0, (int x, int y) p1, IEnumerable<(int x, int y)> positions)
        {
            var v = (x: p1.x-p0.x, y: p1.y-p0.y); // vector p0, p1
            var d = Math.Abs(v.x) + Math.Abs(v.y); // we use manhattan distance to simplify as step count
            for(var i = 1; i <= d; i++)
            {
                // rough scan of hit squares
                var rx = p0.x + i * v.x / d;
                var ry = p0.y + i * v.y / d;
                var rp = (rx, ry);
                if ( rp == p0 || rp == p1 ) // do not check start and end positions
                    continue;
                // center hit check
                // cf. https://en.wikipedia.org/wiki/Linear_equation Equation of a Line / Two points form
                var check = (p1.x-p0.x)*(ry-p1.y) - (p1.y-p0.y)*(rx-p1.x);
                //Console.WriteLine($"{rx}, {ry} -> check {check}");
                if (check == 0 && positions.Contains(rp))
                    return false; // we hit
            }
            return true;
        }

        /// calculates the AB vector given A and B
        private static (int vx, int vy) Vector((int x, int y) a, (int x, int y) b)
            => ((b.x - a.x), (b.y - a.y));

        /// Transform cartesian coordinates into Polar coordinates
        private static (double r, double a) CartesianToPolar((int x, int y) v)
        {
            var r = Math.Sqrt(v.x*v.x+v.y*v.y);
            var a = Math.Atan2(v.y, v.x);
            return (r, a);
        }

        // our map referential is different since (x, y) are not Euclidean (y is inverted)
        // also we want an always positive angle clockwise starting at x and pointing upward (to negative y)
        // distance is not changed
        private static (double r, double a) StandardPolarToMapPolar((double r, double a) p)
            => (p.r, p.a >= 0.0 ? p.a + Math.PI/2.0 :
                     p.a >= -Math.PI/2.0 ? Math.PI/2.0+p.a : 2.0 * Math.PI + Math.PI/2.0 + p.a );
    }


    [TestFixture]
    internal class Day10Tests
    {
        private static readonly string[] Map1 = new[] {
            "......#.#.",
            "#..#.#....",
            "..#######.",
            ".#.#.###..",
            ".#..#.....",
            "..#....#.#",
            "#..#....#.",
            ".##.#..###",
            "##...#..#.",
            ".#....####",
        };

        private static readonly string[] Map2 = new[] {
            "#.#...#.#.",
            ".###....#.",
            ".#....#...",
            "##.#.#.#.#",
            "....#.#.#.",
            ".##..###.#",
            "..#...##..",
            "..##....##",
            "......#...",
            ".####.###."
        };

        private static readonly string[] Map3 = new[] {
            ".#..#..###",
            "####.###.#",
            "....###.#.",
            "..###.##.#",
            "##.##.#.#.",
            "....###..#",
            "..#.#..#.#",
            "#..#.#.###",
            ".##...##.#",
            ".....#.#.."
        };

        private static readonly string[] Map4 = new[] {
            ".#..##.###...#######",
            "##.############..##.",
            ".#.######.########.#",
            ".###.#######.####.#.",
            "#####.##.#.##.###.##",
            "..#####..#.#########",
            "####################",
            "#.####....###.#.#.##",
            "##.#################",
            "#####.##.###..####..",
            "..######..##.#######",
            "####.##.####...##..#",
            ".#####..#.######.###",
            "##...#.##########...",
            "#.##########.#######",
            ".####.#.###.###.#.##",
            "....##.##.###..#####",
            ".#.#.###########.###",
            "#.#.#.#####.####.###",
            "###.##.####.##.#..##"
        };

        private static readonly string[] Map5 = new[] {
            ".#....#####...#..",
            "##...##.#####..##",
            "##...#...#.#####.",
            "..#.....#...###..",
            "..#.#.....#....##",
        };

        [TestCase(1, 5, 8, 33)]
        [TestCase(2, 1, 2, 35)]
        [TestCase(3, 6, 3, 41)]
        [TestCase(4,11,13,210)]
        public void Test1(int mapI, int x, int y, int weight)
        {
            var map = mapI == 1 ? Map1 :
                      mapI == 2 ? Map2 :
                      mapI == 3 ? Map3 :
                      mapI == 4 ? Map4 : Enumerable.Empty<string>().ToArray();
            var res = Day10.BestPosition(map);
            Assert.AreEqual(x, res.Value.x);
            Assert.AreEqual(y, res.Value.y);
            Assert.AreEqual(weight, res.Weight);
        }

        [Test]
        public void Test2_1()
        {
            var res = Day10.VaporizedPosition(Map5, (8, 3), 1);
            Assert.AreEqual(8, res.x);
            Assert.AreEqual(1, res.y);
        }

        [Test]
        public void Test2_2()
        {
            var res = Day10.VaporizedPosition(Map4, (11, 13), 200);
            Assert.AreEqual(8, res.x);
            Assert.AreEqual(2, res.y);
        }
    }
}
