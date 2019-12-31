using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day17
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day17.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = Part1(input);
            Console.WriteLine($"Day17 - part1 - result: {res1}");

            Part2_Step1(input); // step1 output must be manually processed
            var res2 = Part2_Step2(input);
            Console.WriteLine($"Day17 - part2 - result: {res2}");
        }

        public static int Part1(IEnumerable<long> program)
        {
            var map = GetMap(program);

            // Render the map on screen
            map.ForEach(Console.WriteLine);

            // detect all intersections
            var intersections = 
                Enumerable.Range(1, map.Count-2)                                                // y from 1 to n-2
                    .SelectMany(y => Enumerable.Range(1, map[0].Length-2).Select(x => (x, y)))  // x from 1 to n-2
                    .Where(p => map[p.y][p.x]=='#'                                              // Detect cross (+) intersection pattern
                                && map[p.y-1][p.x]=='#'
                                && map[p.y+1][p.x]=='#'
                                && map[p.y][p.x-1]=='#'
                                && map[p.y][p.x+1]=='#')
                    .ToList();

            var result = intersections.Select(p => p.x * p.y).Sum();
            
            return result;
        }

        public static void Part2_Step1(IEnumerable<long> program)
        {
            // Record the path needed to complete the scaffolding
            var map           = GetMap(program);
            var xmax          = map[0].Length - 1;
            var ymax          = map.Count - 1;

            // utility function, position is in map boundaries and to get a map character
            bool InMap((int x, int y) p) => p.x >=0 && p.x <= xmax && p.y >= 0 && p.y <= ymax;
            char Map((int x, int y) p) => map[p.y][p.x];

            var startPosition = Utils.Range2D(0, 0, map[0].Length, map.Count).Where(p => Map(p) == '^').First();
            var pathRecorder  = new PathRecorder(startPosition);
            while(true)
            {
                var nextForwardPosition = pathRecorder.NextForwardPosition();
                // can move forward?
                if (InMap(nextForwardPosition) && Map(nextForwardPosition) == '#')
                {
                    pathRecorder.Forward();
                    continue;
                }
                // attempt a right turn
                var rightTurnPosition = pathRecorder.RightTurnPosition();
                if (InMap(rightTurnPosition) && Map(rightTurnPosition) == '#')
                {
                    pathRecorder.TurnRight();
                    pathRecorder.Forward();
                    continue;
                }
                // attempt a left turn
                var leftTurnPosition = pathRecorder.LeftTurnPosition();
                if (InMap(leftTurnPosition) && Map(leftTurnPosition) == '#')
                {
                    pathRecorder.TurnLeft();
                    pathRecorder.Forward();
                    continue;
                }
                // no way to go, we are done
                pathRecorder.Stop();
                break;
            }
            var steps = pathRecorder.Steps;

            // print the steps which have to be factorized (manually for now)
            Console.WriteLine("Steps:\n" + steps);
        }

        // In step1 we (manually) found the instructions
        // Step2 applies the program and gets the result
        public static long Part2_Step2(IEnumerable<long> program)
        {
            var instructions = "A,B,B,C,C,A,A,B,B,C\n" +
                               "L,12,R,4,R,4\n" +
                               "R,12,R,4,L,12\n" +
                               "R,12,R,4,L,6,L,8,L,8\n" +
                               "n\n";
            // execute and get results
            var modifiedProgram = program.ToList();
            modifiedProgram[0] = 2;
            var computer = new IntComputer();
            var ec = computer.Run(modifiedProgram, instructions.Select(c => (long) c));
            Console.WriteLine("Exit code: " + ec);
            Console.WriteLine("Outputs: " + String.Join(',', computer.Outputs));
            return computer.Outputs.Last();
        }

        private static List<string> GetMap(IEnumerable<long> program)
        {
            // execute the program
            var computer = new IntComputer();
            var ec = computer.Run(program);
            var outputs = computer.Outputs.ToList();
            Console.WriteLine("Exit code: " + ec);

            // map to a list of strings
            var sb = new StringBuilder();
            foreach(var output in outputs)
                sb.Append((char) output);
            var map = sb.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            return map;
        }

        public enum Direction {Up, Right, Down, Left}

        public class PathRecorder
        {
            private (int x, int y) _position;
            private Direction _direction;
            private int _forwardCount;
            private readonly List<string> _steps = new List<string>();

            public PathRecorder((int x, int y) start)
            {
                _position = start;
                _direction = Direction.Up;
                _forwardCount = 0;
            }

            public (int x, int y) NextForwardPosition() => NextPosition(_position, _direction);
            public (int x, int y) RightTurnPosition() => NextPosition(_position, RightTurnDirection(_direction));
            public (int x, int y) LeftTurnPosition() => NextPosition(_position, LeftTurnDirection(_direction));

            public void Forward()
            {
                _position = NextPosition(_position, _direction);
                _forwardCount++;
            }
            public void TurnRight()
            {
                if ( _forwardCount > 0 )
                    _steps.Add(_forwardCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                _forwardCount = 0;
                _direction = RightTurnDirection(_direction);
                _steps.Add("R");
            }
            public void TurnLeft()
            {
                if ( _forwardCount > 0 )
                    _steps.Add(_forwardCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                _forwardCount = 0;
                _direction = LeftTurnDirection(_direction);
                _steps.Add("L");
            }
            public void Stop()
            {
                _steps.Add(_forwardCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            public string Steps => String.Join(',', _steps);

            private static (int x, int y) NextPosition((int x, int y) p, Direction d)
            {
                switch(d)
                {
                    case Direction.Up: return (p.x, p.y-1);
                    case Direction.Right: return (p.x+1, p.y);
                    case Direction.Down: return (p.x, p.y+1);
                    case Direction.Left: return (p.x-1, p.y);
                    default: throw new NotImplementedException($"unknown direction: {d}");
                }
            }
            private static Direction RightTurnDirection(Direction d) =>  (Direction) (((int) d + 1) % 4);
            private static Direction LeftTurnDirection(Direction d) =>  (Direction) (((int) d + 3) % 4); // L is equivalent to 3x R
        }
    }


    [TestFixture]
    internal class Day17Tests
    {
        // Nothing today
    }
}
