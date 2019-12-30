using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day15
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day15.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            // Initialize the world (to keep it between part 1 and part 2)
            var ship = new Ship();

            var res1 = Part1(ship, input);
            Console.WriteLine($"Day15 - part1 - result: {res1}");

            var res2 = Part2(ship);
            Console.WriteLine($"Day15 - part2 - result: {res2}");
        }


        // Find the shortest path to the Oxygen Tank
        // 1. use an (enhanced) random walk to explore the map and find the O2 tank
        // -- we now have a partially explored map and the target location --
        // 2. start again from start (reinitialize the computer)
        // 3. use a path search algorithm (A*) taking unknown cells as free-to-pass
        //    but adapting once they are discovered
        public static int Part1(Ship ship, IEnumerable<long> program)
        {
            ship.InitializeNavigation(program);

            // Step 1 - explore and find the tank
            Part1_Step1(ship);

            // Step 2 - reinitialize the computer and perform an A* search
            return Part1_Step2(ship, program);
        }

        // For part2, we use the following strategy:
        // - discover the entire map, by repeating the following actions:
        //   - find closest reachable unknown position
        //   - get the path to find it (using A*) from current position
        //   - go there and repeat until there is no reachable unknown cell left
        // - then perform a breadth-first search counting the iterations until there
        //   are no cell left to explore -> this is the answer
        public static int Part2(Ship ship)
        {
            // Step 1 - discover the entire map
            Part2_Step1(ship);

            // Step 2 - calculate the oxygen diffusion time
            return ship.ComputeOxygenDiffusionTime();
        }

        private static void Part1_Step1(Ship ship)
        {
            var rnd  = new System.Random(); // new expert AI navigation system ;-)
            // "Enhanced" random walk to explore and discover the O2 position
            Console.Clear();
            do
            {
                // Draw the current state
                Console.SetCursorPosition(0, 0);
                RenderShipMap(ship);
                // to observe logic, remove comments
                //System.Threading.Thread.Sleep(200);
                // find next position
                // 1st prio: unknown, then empty and finally wall
                var possibleMoves = ship.Surroundings().Where(dp => dp.place != Place.Wall).OrderBy(dp => dp.place).ToList();
                if ( possibleMoves[0].place == Place.Unknown )
                    ship.NextMove(possibleMoves[0].direction);
                else
                    ship.NextMove(possibleMoves[rnd.Next() % possibleMoves.Count].direction);
            } while(ship.Oxygen == null);
            Console.WriteLine($"Oxygen System found at {ship.Oxygen}");
        }

        private static int Part1_Step2(Ship ship, IEnumerable<long> program)
        {
            // do an A* to find the best path to target
            // run the path with the robot, if we hit a wall, abort and do A* again from scratch
            // we succeed when we have a matching A* without hitting a wall
            List<Direction> path;
            bool success;
            do
            {
                path = ship.ShortestPathToOxygen();
                ship.InitializeNavigation(program);
                success = ship.RunPath(path);
            } while(!success);

            return path.Count;
        }

        // Fully discover the map
        private static void Part2_Step1(Ship ship)
        {
            while(true)
            {
                var p = ship.FindClosestUnknownPosition();
                if (p == Ship.Start)
                    break;
                var steps = ship.ShortestPath(ship.Robot, p);
                ship.RunPath(steps);

                // Draw the current state
                Console.SetCursorPosition(0, 0);
                RenderShipMap(ship);
            }
        }

        private static void RenderShipMap(Ship ship)
        {
            var minX = Math.Min(ship.Map.Keys.Select(p => p.x).Min(), ship.Robot.x-3);
            var maxX = Math.Max(ship.Map.Keys.Select(p => p.x).Max(), ship.Robot.x+3);
            var minY = Math.Min(ship.Map.Keys.Select(p => p.y).Min(), ship.Robot.y-3);
            var maxY = Math.Max(ship.Map.Keys.Select(p => p.y).Max(), ship.Robot.y+3);
            for(var y = maxY; y >= minY; y--)
            {
                for(var x = minX; x <= maxX; x++)
                {
                    var p = (x, y);
                    if ( ship.Robot == p )
                        Console.Write('+');
                    else if ( p == Ship.Start )
                        Console.Write('S');
                    else if ( ship.Oxygen == p )
                        Console.Write('O');
                    else if ( ! ship.Map.TryGetValue((x, y), out Place place) )
                        Console.Write('?');
                    else if ( place == Place.Empty )
                        Console.Write(' ');
                    else if ( place == Place.Wall )
                        Console.Write('W');
                }
                Console.WriteLine();
            }
        }

        public enum Place {Unknown, Empty, Wall} // order is key! Unknown is Default and from most interesting to less (for navigation)
        public enum Direction {North = 1, South, West, East}

        // Our world! it contains a map, a robot and an oxygen system
        // the robot moves according to a navigation system
        public sealed class Ship
        {
            public static readonly (int x, int y) Start = (0, 0);
            private readonly Dictionary<(int x, int y), Place> _map = new Dictionary<(int x, int y), Place>();
            private (int x, int y) _robot = Start;
            private (int x, int y)? _oxygen = null;  // null until we know where it is!
            private IntComputer _robotComputer;

            public Ship()
            {
                _map.Add(_robot, Place.Empty); // initial robot position is assumed empty
            }

            public void InitializeNavigation(IEnumerable<long> program)
            {
                _robotComputer = new IntComputer();
                _robotComputer.Run(program);
                _robot = Start;
            }

            public IDictionary<(int x, int y), Place> Map => _map;
            public (int x, int y) Robot => _robot;
            public (int x, int y)? Oxygen => _oxygen;

            /// Executes the list of directions given in path
            /// returns false if the path can not be completed (by hitting a wall for example)
            public bool RunPath(List<Direction> path)
            {
                foreach(var step in path)
                    if (!NextMove(step))
                        return false;
                return true;
            }

            public bool NextMove(Direction direction)
            {
                var targetPosition = NextPosition(_robot, direction);
                if ( _map.GetValueOrDefault(targetPosition) == Place.Wall)
                    return false; // quickly exit illegal moves (i.e. into known walls)
                var ec = _robotComputer.Continue(new[] { (long) direction});
                Debug.Assert(ec == IntComputer.ExitCode.NeedInput);
                Debug.Assert(_robotComputer.OutputCount == 1);
                switch(_robotComputer.PopOutput())
                {
                    case 0 /* WALL  */: _map[targetPosition] = Place.Wall; break;
                    case 1 /* EMPTY */: _map[targetPosition] = Place.Empty; _robot = targetPosition; break;
                    case 2 /* GOAL  */: _map[targetPosition] = Place.Empty; _robot = targetPosition; _oxygen = targetPosition; break;
                    default: throw new Exception($"unexpected computer output!");
                }
                return _map[targetPosition] != Place.Wall;
            }

            // Returns the surroundings of the current bot position
            // Each possible direction is scanned and the type of Place located
            // in that direction is returned
            public IEnumerable<((int x, int y) position, Direction direction, Place place)> Surroundings()
                => Surroundings(_robot);

            public IEnumerable<((int x, int y) position, Direction direction, Place place)> Surroundings((int x, int y) p)
            {
                foreach(Direction d in Enum.GetValues(typeof(Direction)))
                {
                    var position = NextPosition(p, d);
                    var place    = _map.GetValueOrDefault(position);
                    yield return (position, d, place);
                }
            }

            // given a position, and a move in a given direction, returns the next position
            private static (int x, int y) NextPosition((int x, int y) current, Direction moveDirection)
            {
                switch(moveDirection)
                {
                    case Direction.North: return (current.x, current.y+1);
                    case Direction.South: return (current.x, current.y-1);
                    case Direction.West:  return (current.x-1, current.y);
                    case Direction.East:  return (current.x+1, current.y);
                    default: throw new NotImplementedException($"direction {moveDirection}");
                }
            }

            // given two adjacent positions, return the Direction to go from -> to
            private static Direction GetDirection((int x, int y) from, (int x, int y) to)
            {
                var v = (to.x - from.x, to.y - from.y);
                switch(v)
                {
                    case (0,  1): return Direction.North;
                    case (0, -1): return Direction.South;
                    case (-1, 0): return Direction.West;
                    case ( 1, 0): return Direction.East;
                    default: throw new Exception($"not adjacent: from {from} tp {to}");
                }
            }


            /// Performs an A* search to find the shortest path to the (known) Oxygen position
            public List<Direction> ShortestPathToOxygen() => ShortestPath(Start, _oxygen.Value);
            public List<Direction> ShortestPath((int x, int y) start, (int x, int y) goal)
            {
                int h((int x, int y) p) => Math.Abs(goal.x - p.x) + Math.Abs(goal.y - p.y);

                var openSet  = new HashSet<(int x, int y)>() { start };
                var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
                var gScore   = new Dictionary<(int x, int y), int>() { [start] = 0 };
                var fScore   = new Dictionary<(int x, int y), int>() { [start] = h(start) };

                while(openSet.Count > 0)
                {
                    var current = openSet.OrderBy(p => fScore[p]).First();
                    if (current == goal)
                    {
                        // Goal hit - reconstruct the path
                        var path = new List<Direction>();
                        var to   = current;
                        var from = current;
                        do
                        {
                            from = cameFrom[to];
                            path.Add(GetDirection(from, to));
                            to = from;
                        } while(from != start);
                        path.Reverse();
                        return path;
                    }
                    // keep searching
                    openSet.Remove(current);
                    var neighbours = Surroundings(current).Where(s => s.place != Place.Wall).Select(s => s.position).ToList();
                    foreach(var neighbour in neighbours)
                    {
                        var tentative_gScore = gScore[current] + 1;
                        if (!gScore.ContainsKey(neighbour) || tentative_gScore < gScore[neighbour])
                        {
                            // This path to neighbor is better than any previous one. Record it!
                            cameFrom[neighbour] = current;
                            gScore[neighbour] = tentative_gScore;
                            fScore[neighbour] = tentative_gScore + h(neighbour);
                            openSet.Add(neighbour);
                        }
                    }
                }
                throw new Exception("no path found to goal");
            }

            // Search the closest reachable unknown place from the current robot position
            // return Start (0, 0) if no unknown positions exists
            public (int x, int y) FindClosestUnknownPosition()
            {
                var visited  = new HashSet<(int x, int y)>() { _robot };
                var frontier = new List<(int x, int y)>() { _robot };
                while(frontier.Count > 0)
                {
                    frontier = frontier
                        .SelectMany(p => Surroundings(p))
                        .Where(s => s.place != Place.Wall)
                        .Select(s => s.position)
                        .Where(p => visited.Add(p)) // filter out the already visited ones
                        .ToList();
                    var unknownPosition = frontier.FirstOrDefault(p => _map.GetValueOrDefault(p) == Place.Unknown);
                    if ( unknownPosition != Start )
                        return unknownPosition;
                }
                return Start; // nothing found
            }

            // Perform a breadth-first search to calculate the diffusion time
            internal int ComputeOxygenDiffusionTime()
            {
                var visited  = new HashSet<(int x, int y)>() { _oxygen.Value };
                var frontier = new List<(int x, int y)>() { _oxygen.Value };
                var time     = 0;
                while(frontier.Count > 0)
                {
                    frontier = frontier
                        .SelectMany(p => Surroundings(p))
                        .Where(s => s.place != Place.Wall)
                        .Select(s => s.position)
                        .Where(p => visited.Add(p)) // filter out the already visited ones
                        .ToList();
                    time++;
                }
                return time-1;
            }
        }
    }


    [TestFixture]
    internal class Day15Tests
    {
        // Nothing today
    }
}
