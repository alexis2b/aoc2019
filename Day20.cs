using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day20
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day20.txt")).ToList();

            var res1 = Part1(input);
            Console.WriteLine($"Day20 - part1 - result: {res1}");

            var res2 = Part2(input);
            Console.WriteLine($"Day20 - part2 - result: {res2}");
        }

        public static int Part1(List<string> input)
        {
            var maze = Maze.FromMap(input);
            return maze.ShortestPath("AA", "ZZ");
        }

        public static int Part2(List<string> input)
        {
            var maze = Maze.FromMap(input);
            return maze.ShortestPath2("AA", "ZZ");
        }

        public sealed class Maze
        {
            private readonly List<string> _map;
            private readonly int _xmax, _ymax;
            private readonly Dictionary<(int x, int y), string> _portals;

            private Maze(IEnumerable<string> map)
            {
                _map = map.ToList();
                _xmax = _map[0].Length - 1;
                _ymax = _map.Count - 1;
                _portals = new Dictionary<(int x, int y), string>();
            }

            // Build the maze by loading the map and detecting all the portals
            public static Maze FromMap(List<string> map)
            {
                var maze = new Maze(map);
                maze.LoadPortalPositions();
                return maze;
            }

            // Perform a breadth-first search to find the shortest path between
            // two portals (they must have only one location)
            public int ShortestPath(string fromPortal, string toPortal)
            {
                var fromP = GetPortalLocation(fromPortal);
                var goalP = GetPortalLocation(toPortal);
                var visited = new HashSet<(int x, int y)> { fromP };
                var frontier = new List<(int x, int y)> { fromP };
                var steps = 0;
                while(frontier.Count > 0)
                {
                    steps++;
                    frontier = frontier.SelectMany(p => ReachablePositionsFrom(p)).Where(p => visited.Add(p)).ToList();
                    if ( frontier.Contains(goalP) )
                        return steps;
                }
                throw new Exception($"Portal {toPortal} is not reachable from portal {fromPortal}");
            }

            // Perform a breadth-first search to find the shortest path between
            // two portals (they must have only one location)
            public int ShortestPath2(string fromPortal, string toPortal)
            {
                var fromP = GetPortalLocation(fromPortal);
                var fromP2 = (fromP.x, fromP.y, 0);
                var goalP = GetPortalLocation(toPortal);
                var goalP2 = (goalP.x, goalP.y, 0);
                var visited = new HashSet<(int x, int y, int lvl)> { fromP2 };
                var frontier = new List<(int x, int y, int lvl)> { fromP2 };
                var steps = 0;
                while(frontier.Count > 0)
                {
                    steps++;
                    frontier = frontier.SelectMany(p => ReachablePositionsFrom2(p)).Where(p => visited.Add(p)).ToList();
                    if ( frontier.Contains(goalP2) )
                        return steps;
                }
                throw new Exception($"Portal {toPortal} is not reachable from portal {fromPortal}");
            }

            // find all the portals and register their positions
            private void LoadPortalPositions()
            {
                for(var y = 0; y <= _ymax - 1; y++)
                for(var x = 0; x <= _xmax - 1; x++)
                {
                    var letter1 = Map(x, y);
                    if (letter1 < 'A' || letter1 > 'Z')
                        continue;
                    // we have a potential letter1 - look for a letter2 horizontally or vertically
                    var letter2h = Map(x+1, y);
                    if (letter2h >= 'A' && letter2h <= 'Z')
                    { // horizontal match - look for a dot left or right
                        var dotl = x > 0 ? Map(x-1, y) : '#';
                        if ( dotl == '.' )
                        { // match with portal on left side
                            _portals.Add((x-1, y), ""+letter1+letter2h);
                            continue;
                        }
                        var dotr = x < _xmax ? Map(x+2, y) : '#';
                        if ( dotr == '.' )
                        { // match with portal on right side
                            _portals.Add((x+2, y), ""+letter1+letter2h);
                            continue;
                        }
                        throw new Exception($"could not find dot position of horizontal portal {letter1}{letter2h} at ({x},{y})");
                    }
                    var letter2v = Map(x, y+1);
                    if (letter2v >= 'A' && letter2v <= 'Z')
                    { // vertical match - look for a dot above or below
                        var dota = y > 0 ? Map(x, y-1) : '#';
                        if ( dota == '.' )
                        { // match with portal above
                            _portals.Add((x, y-1), ""+letter1+letter2v);
                            continue;
                        }
                        var dotb = y < _xmax ? Map(x, y+2) : '#';
                        if ( dotb == '.' )
                        { // match with portal below
                            _portals.Add((x, y+2), ""+letter1+letter2v);
                            continue;
                        }
                        throw new Exception($"could not find dot position of vertical portal {letter1}{letter2h} at ({x},{y})");
                    }
                }
            }

            private (int x, int y) GetPortalLocation(string name)
            {
                var positions = _portals.Where(kvp => kvp.Value == name).ToList();
                if ( positions.Count == 0 )
                    throw new Exception($"portal {name} not found");
                if ( positions.Count > 1 )
                    throw new Exception($"portal {name} has multiple locations");
                return positions[0].Key;
            }

            private IEnumerable<(int x, int y)> ReachablePositionsFrom((int x, int y) p)
            {
                var neighbours = new List<(int x, int y)> {
                    (p.x+1, p.y),
                    (p.x-1, p.y),
                    (p.x, p.y+1),
                    (p.x, p.y-1)
                };
                var reachableNeighbours = neighbours
                    .Where(p => p.x >= 0 && p.x <= _xmax && p.y >= 0 && p.y <= _ymax)
                    .Where(p => Map(p.x, p.y) == '.');
                // include a potential portal
                if (_portals.TryGetValue(p, out var portal))
                {
                    var destination = _portals.Where(kvp => kvp.Value == portal).Where(kvp => kvp.Key != p).ToList();
                    if (destination.Count == 1)
                        reachableNeighbours = reachableNeighbours.Append(destination[0].Key);
                }
                return reachableNeighbours;
            }

            // For Part2 - now portals connect through levels
            private IEnumerable<(int x, int y, int lvl)> ReachablePositionsFrom2((int x, int y, int lvl) p)
            {   
                var neighbours = new List<(int x, int y, int lvl)> {
                    (p.x+1, p.y, p.lvl),
                    (p.x-1, p.y, p.lvl),
                    (p.x, p.y+1, p.lvl),
                    (p.x, p.y-1, p.lvl)
                };
                var reachableNeighbours = neighbours
                    .Where(p => p.x >= 0 && p.x <= _xmax && p.y >= 0 && p.y <= _ymax)
                    .Where(p => Map(p.x, p.y) == '.');
                // include a potential portal
                if (_portals.TryGetValue((p.x, p.y), out var portal))
                {
                    var destination = _portals.Where(kvp => kvp.Value == portal).Where(kvp => kvp.Key != (p.x, p.y)).ToList();
                    if (destination.Count == 1)
                    {
                        var dp = (x: destination[0].Key.x, y: destination[0].Key.y);
                        // Inner portals lead to next level
                        if (IsInnerPortal(p.x, p.y))
                            reachableNeighbours = reachableNeighbours.Append((dp.x, dp.y, p.lvl+1));
                        // Outer portals connect back to previous level (only at lvl 1 and further)
                        else if (p.lvl > 0 && IsOuterPortal(p.x, p.y))
                            reachableNeighbours = reachableNeighbours.Append((dp.x, dp.y, p.lvl-1));
                    }
                }
                return reachableNeighbours;
            }

            private bool IsInnerPortal(int x, int y) => x > 2 && x < _xmax-2 && y > 2 && y < _ymax-2;
            private bool IsOuterPortal(int x, int y) => !IsInnerPortal(x, y);

            private char Map(int x, int y) => _map[y][x];
        }
    }


    [TestFixture]
    internal class Day20Tests
    {
        [TestCase(1, 23)]
        [TestCase(2, 58)]
        public void Test1(int i, int expected)
        {
            var actual = Day20.Part1(Maps[i-1]);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(3, 396)]
        public void Test2(int i, int expected)
        {
            var actual = Day20.Part2(Maps[i-1]);
            Assert.AreEqual(expected, actual);
        }

        public static readonly List<List<string>> Maps = new List<List<string>> {
            // Map 1
            new List<string> {
            "         A           ",
            "         A           ",
            "  #######.#########  ",
            "  #######.........#  ",
            "  #######.#######.#  ",
            "  #######.#######.#  ",
            "  #######.#######.#  ",
            "  #####  B    ###.#  ",
            "BC...##  C    ###.#  ",
            "  ##.##       ###.#  ",
            "  ##...DE  F  ###.#  ",
            "  #####    G  ###.#  ",
            "  #########.#####.#  ",
            "DE..#######...###.#  ",
            "  #.#########.###.#  ",
            "FG..#########.....#  ",
            "  ###########.#####  ",
            "             Z       ",
            "             Z       "
            },
            // Map 2
            new List<string> {
            "                   A               ",
            "                   A               ",
            "  #################.#############  ",
            "  #.#...#...................#.#.#  ",
            "  #.#.#.###.###.###.#########.#.#  ",
            "  #.#.#.......#...#.....#.#.#...#  ",
            "  #.#########.###.#####.#.#.###.#  ",
            "  #.............#.#.....#.......#  ",
            "  ###.###########.###.#####.#.#.#  ",
            "  #.....#        A   C    #.#.#.#  ",
            "  #######        S   P    #####.#  ",
            "  #.#...#                 #......VT",
            "  #.#.#.#                 #.#####  ",
            "  #...#.#               YN....#.#  ",
            "  #.###.#                 #####.#  ",
            "DI....#.#                 #.....#  ",
            "  #####.#                 #.###.#  ",
            "ZZ......#               QG....#..AS",
            "  ###.###                 #######  ",
            "JO..#.#.#                 #.....#  ",
            "  #.#.#.#                 ###.#.#  ",
            "  #...#..DI             BU....#..LF",
            "  #####.#                 #.#####  ",
            "YN......#               VT..#....QG",
            "  #.###.#                 #.###.#  ",
            "  #.#...#                 #.....#  ",
            "  ###.###    J L     J    #.#.###  ",
            "  #.....#    O F     P    #.#...#  ",
            "  #.###.#####.#.#####.#####.###.#  ",
            "  #...#.#.#...#.....#.....#.#...#  ",
            "  #.#####.###.###.#.#.#########.#  ",
            "  #...#.#.....#...#.#.#.#.....#.#  ",
            "  #.###.#####.###.###.#.#.#######  ",
            "  #.#.........#...#.............#  ",
            "  #########.###.###.#############  ",
            "           B   J   C               ",
            "           U   P   P               "
            },
            // Map 3
            new List<string> {
            "             Z L X W       C                 ",
            "             Z P Q B       K                 ",
            "  ###########.#.#.#.#######.###############  ",
            "  #...#.......#.#.......#.#.......#.#.#...#  ",
            "  ###.#.#.#.#.#.#.#.###.#.#.#######.#.#.###  ",
            "  #.#...#.#.#...#.#.#...#...#...#.#.......#  ",
            "  #.###.#######.###.###.#.###.###.#.#######  ",
            "  #...#.......#.#...#...#.............#...#  ",
            "  #.#########.#######.#.#######.#######.###  ",
            "  #...#.#    F       R I       Z    #.#.#.#  ",
            "  #.###.#    D       E C       H    #.#.#.#  ",
            "  #.#...#                           #...#.#  ",
            "  #.###.#                           #.###.#  ",
            "  #.#....OA                       WB..#.#..ZH",
            "  #.###.#                           #.#.#.#  ",
            "CJ......#                           #.....#  ",
            "  #######                           #######  ",
            "  #.#....CK                         #......IC",
            "  #.###.#                           #.###.#  ",
            "  #.....#                           #...#.#  ",
            "  ###.###                           #.#.#.#  ",
            "XF....#.#                         RF..#.#.#  ",
            "  #####.#                           #######  ",
            "  #......CJ                       NM..#...#  ",
            "  ###.#.#                           #.###.#  ",
            "RE....#.#                           #......RF",
            "  ###.###        X   X       L      #.#.#.#  ",
            "  #.....#        F   Q       P      #.#.#.#  ",
            "  ###.###########.###.#######.#########.###  ",
            "  #.....#...#.....#.......#...#.....#.#...#  ",
            "  #####.#.###.#######.#######.###.###.#.#.#  ",
            "  #.......#.......#.#.#.#.#...#...#...#.#.#  ",
            "  #####.###.#####.#.#.#.#.###.###.#.###.###  ",
            "  #.......#.....#.#...#...............#...#  ",
            "  #############.#.#.###.###################  ",
            "               A O F   N                     ",
            "               A A D   M                     "
            }
        };
    }
}
