using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day18
    {
        public static void Run()
        {
            var map1 = File.ReadAllLines(Path.Combine("input", "day18.txt")).ToList();
            var res1 = Part1(map1);
            Console.WriteLine($"Day18 - part1 - result: {res1}");

            var map2 = File.ReadAllLines(Path.Combine("input", "day18b.txt")).ToList();
            var res2 = Part1(map2); // code is similar, only the input changes
            Console.WriteLine($"Day18 - part2 - result: {res2}");
        }

        public static int Part1(List<string> map)
        {
            // we do a TreeSearch:
            // - first, a breadth-first search to find all possible options (i.e: keys we can recover at this point)
            // - this involves -> the key name, the key positions, the number of steps to reach it
            // - we iterate depth-first through all the options, each decision point is a TreeSearchNode which has the Options (branches)
            // - when we have found all the keys, we have a "solution", with the total number of steps to reach it
            // - we cut branches before having a solution if: we are stuck, or we have already more steps than the best solution
            //
            // another optimisation: if I get in the same position with the same set of keys, only consider if the
            // number of steps required to get there was smaller than the last time)
            //
            // Part2 is similar to Part1 except that the area is divided into four zones
            // and we cycle through the zones sequentially until we find all keys
            var world = World.FromMap(map);
            var treeSearch = new TreeSearch(world);
            return treeSearch.SearchOptimalSolution();
        }

        // Our World: the map and some general useful properties
        public class World
        {
            private readonly List<string> _map;
            private readonly HashSet<char> _allKeys;
            private readonly (int x, int y) _maxP;
            private readonly List<(int x, int y)> _startPs;

            private World(List<string> map, (int x, int y) maxP, IEnumerable<(int x, int y)> startPs, HashSet<char> allKeys)
            {
                _map     = map;
                _maxP    = maxP;
                _startPs = startPs.ToList();
                _allKeys = allKeys;
            }

            public static World FromMap(List<string> map)
            {
                var width  = map[0].Length;
                var height = map.Count;

                // startPosition(s) (multiple added in part2)
                var startPs = Utils.Range2D(0, 0, width, height).Where(p => map[p.y][p.x] == '@').ToList();

                // allKeys
                var allKeys = new HashSet<char>(
                    map.SelectMany(s => s).Where(c => c >= 'a' && c <= 'z')
                );

                return new World(map.ToList(), (width-1, height-1), startPs, allKeys);
            }

            // Exposed properties and utility functions
            public bool InMap((int x, int y) p ) => (p.x >=0 && p.x <= _maxP.x && p.y >= 0 && p.y <= _maxP.y);
            public char Map((int x, int y) p) => _map[p.y][p.x];
            public List<(int x, int y)> StartPs => _startPs;
            public int KeyCount => _allKeys.Count;
            public int RobotCount => _startPs.Count;

            // Cells around a position which are in the map and not walls
            public IEnumerable<(int x, int y)> ReachableNeighbours((int x, int y) p)
            => new List<(int x, int y)> {
                    (p.x-1, p.y), (p.x+1, p.y), (p.x, p.y-1), (p.x, p.y+1)
                }
                .Where(p => InMap(p) && Map(p) != '#').ToList();

            // find all the (not yet owned) reachable keys from the given positions
            public IEnumerable<ReachableKey> ReachableKeys((int x, int y) fromP, List<char> alreadyOwned)
            {
                var visited  = new HashSet<(int x, int y)> { fromP };
                var frontier = new List<(int x, int y)> { fromP };
                var steps    = 0;
                while(frontier.Count > 0)
                {
                    steps++;
                    // all the new positions we can reach
                    frontier = frontier.SelectMany(p => ReachableNeighbours(p)).Where(p => visited.Add(p)).ToList();
                    // remove the doors for which we do not have the keys (convert Door to Key, equivalent of ToLower)
                    var closedDoorPositions = frontier.Where(p => Map(p) >= 'A' && Map(p) <= 'Z' && ! alreadyOwned.Contains(Char.ToLower(Map(p)))).ToList();
                    foreach(var closedDoorPosition in closedDoorPositions)
                        frontier.Remove(closedDoorPosition); // we can not go further
                    // find the keys we have reached, and remove those positions (we only aim to reach one key)
                    var reachableKeyPositions = frontier.Where(p => Map(p) >= 'a' && Map(p) <= 'z' && ! alreadyOwned.Contains(Map(p))).ToList();
                    foreach(var reachableKeyPosition in reachableKeyPositions)
                    {
                        yield return new ReachableKey {
                            Name     = Map(reachableKeyPosition),
                            Position = reachableKeyPosition,
                            Steps    = steps
                        };
                        frontier.Remove(reachableKeyPosition); // stop there
                    }
                } 
            }
        }

        public class ReachableKey
        {
            public char Name { get; set; }
            public (int x, int y) Position { get; set; }
            public int Steps { get; set; }
        }

        public class TreeSearch
        {
            private readonly World _world;
            private readonly Dictionary<string, int> _cache; // node cacheKey -> known min steps
            private int _smallestStepCount;

            public TreeSearch(World world)
            {
                _world = world;
                _cache = new Dictionary<string, int>();
                _smallestStepCount = Int32.MaxValue;
            }

            internal int SearchOptimalSolution()
            {
                var rootNode = new TreeSearchNode {
                    Positions  = _world.StartPs.ToArray(),
                    OwnedKeys  = new List<char>(),
                    TotalSteps = 0
                };
                // Recursive loop
                SearchLoop(rootNode, 0);

                return _smallestStepCount;
            }

            private void SearchLoop(TreeSearchNode fromNode, int depth)
            {
                // start by performing one cycle through all the searchers
                // to consider all their reachable keys
                var reachableKeys = fromNode.Positions.Select(p => _world.ReachableKeys(p, fromNode.OwnedKeys).ToList()).ToList();
                // attempt all the possible choices at this point
                // only 1 robot moves, 2 robot moves, etc. by combining all options - not moving is always an option
                foreach(var nextNode in AllPossibleNextNodes(fromNode, reachableKeys))
                {
                    // optimization: if already went through the same position with same set of keys with a same or bigger
                    // number of steps, then we can skip
                    var cacheKey = nextNode.CacheKey();
                    var previousSteps = _cache.GetValueOrDefault(cacheKey);
                    if ( previousSteps > 0 && nextNode.TotalSteps >= previousSteps)
                        continue;
                    _cache[cacheKey] = nextNode.TotalSteps;
                    // abort if over the current optimal solution already
                    if (nextNode.TotalSteps >= _smallestStepCount)
                        continue;
                    // do we have a successful solution?
                    if (nextNode.OwnedKeys.Count == _world.KeyCount)
                    {
                        _smallestStepCount = nextNode.TotalSteps;
                        continue;
                    }
                    // Iterate depth first
                    SearchLoop(nextNode, depth+1);
                }
            }

            // Create a list of all the possible move combinations
            // Not moving is always an option
            // We can have: only 1 robot moving, 2 robot movings, 3 robot movings, etc.
            private IEnumerable<TreeSearchNode> AllPossibleNextNodes(TreeSearchNode fromNode, List<List<ReachableKey>> reachableKeys)
            {
                // only 1 robot moving
                for(var i = 0; i < _world.RobotCount; i++)
                {
                    foreach(var reachableKey in reachableKeys[i])
                    {
                        var nextNode = new TreeSearchNode {
                            Positions  = Utils.MutateArray(fromNode.Positions, i, reachableKey.Position),
                            OwnedKeys  = fromNode.OwnedKeys.Append(reachableKey.Name).ToList(),
                            TotalSteps = fromNode.TotalSteps + reachableKey.Steps
                        };
                        yield return nextNode;
                    }
                }
            }
        }

        public class TreeSearchNode
        {
            public (int x, int y)[] Positions { get; set; }
            public List<char> OwnedKeys { get; set; }
            public int TotalSteps { get; set; }

            // we take only Position and owned keys into account
            public string CacheKey() => ""+String.Join("", Positions)+String.Join("", OwnedKeys.OrderBy(k => k));
        }
    }


    [TestFixture]
    internal class Day18Tests
    {
        [TestCase(1, 8)]
        [TestCase(2, 86)]
        [TestCase(3, 132)]
        [TestCase(4, 136)]
        [TestCase(5, 81)]
        [TestCase(6, 8)]       // start of Part2 tests
        [TestCase(7, 24)]
        [TestCase(8, 32)]
        [TestCase(9, 72)]
        public void Test1(int i, int expected)
        {
            var actual = Day18.Part1(Maps[i-1]);
            Assert.AreEqual(expected, actual);
        }

        public static readonly List<List<string>> Maps = new List<List<string>> {
            // Map 1
            new List<string> {
            "#########",
            "#b.A.@.a#",
            "#########"
            },
            // Map 2
            new List<string> {
            "########################",
            "#f.D.E.e.C.b.A.@.a.B.c.#",
            "######################.#",
            "#d.....................#",
            "########################"
            },
            // Map 3
            new List<string> {
            "########################",
            "#...............b.C.D.f#",
            "#.######################",
            "#.....@.a.B.c.d.A.e.F.g#",
            "########################"
            },
            // Map 4
            new List<string> {
            "#################",
            "#i.G..c...e..H.p#",
            "########.########",
            "#j.A..b...f..D.o#",
            "########@########",
            "#k.E..a...g..B.n#",
            "########.########",
            "#l.F..d...h..C.m#",
            "#################"
            },
            // Map 5
            new List<string> {
            "########################",
            "#@..............ac.GI.b#",
            "###d#e#f################",
            "###A#B#C################",
            "###g#h#i################",
            "########################"
            },
            // Map 6
            new List<string> {
            "#######",
            "#a.#Cd#",
            "##@#@##",
            "#######",
            "##@#@##",
            "#cB#Ab#",
            "#######"
            },
            // Map 7
            new List<string> {
            "###############",
            "#d.ABC.#.....a#",
            "######@#@######",
            "###############",
            "######@#@######",
            "#b.....#.....c#",
            "###############"
            },
            // Map 8
            new List<string> {
            "#############",
            "#DcBa.#.GhKl#",
            "#.###@#@#I###",
            "#e#d#####j#k#",
            "###C#@#@###J#",
            "#fEbA.#.FgHi#",
            "#############"
            },
            // Map 9
            new List<string> {
            "#############",
            "#g#f.D#..h#l#",
            "#F###e#E###.#",
            "#dCba@#@BcIJ#",
            "#############",
            "#nK.L@#@G...#",
            "#M###N#H###.#",
            "#o#m..#i#jk.#",
            "#############"
            },
        };
    }
}
