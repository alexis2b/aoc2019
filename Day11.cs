using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day11
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day11.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = PaintedSquares(input);
            Console.WriteLine($"Day11 - part1 - result: {res1.Count()}");

            Console.WriteLine($"Day11 - part2 - result:");
            RenderPainting(input);
        }

        public static IEnumerable<(int x, int y)> PaintedSquares(IEnumerable<long> program)
        => PaintSquares(program, 0L).uniqueTiles;

        public static void RenderPainting(IEnumerable<long> program)
        {
            var colors = PaintSquares(program, 1L).colors;
            // find the white square boundaries
            var minX = colors.Where(kvp => kvp.Value == 1L).Min(kvp => kvp.Key.x);
            var maxX = colors.Where(kvp => kvp.Value == 1L).Max(kvp => kvp.Key.x);
            var minY = colors.Where(kvp => kvp.Value == 1L).Min(kvp => kvp.Key.y);
            var maxY = colors.Where(kvp => kvp.Value == 1L).Max(kvp => kvp.Key.y);
            for(var y = maxY; y >= minY; y--)
            {
                for(var x = minX; x <= maxX; x++ )
                    Console.Write(colors.GetValueOrDefault((x, y)) == 1L ? "##" : "  ");
                Console.WriteLine();
            }
        }

        public static (IEnumerable<(int x, int y)> uniqueTiles, Dictionary<(int x, int y), long> colors) PaintSquares(
            IEnumerable<long> program, long startingPanelColor)
        {
            var computer  = new IntComputer();
            var visited   = new HashSet<(int x, int y)>();
            var position  = (x:0, y:0);
            var colors    = new Dictionary<(int x, int y), long>() { [position] = startingPanelColor };
            var direction = 0; // up
            var ec = computer.Run(program);
            do
            {
                visited.Add(position);
                var currentColor = colors.GetValueOrDefault(position);
                ec = computer.Continue(new[] { currentColor });

                var paintColor = computer.PopOutput();
                //Console.WriteLine($"Painting {position} in {paintColor}");
                colors[position] = paintColor;

                var nextTurn  = 2 * (int) computer.PopOutput() - 1; // map (0 or 1) to (-1 or 1)
                direction = (direction + nextTurn + 4) % 4;
                switch(direction)
                {
                    case 0 /* up    */: position = (position.x, position.y+1); break;
                    case 1 /* right */: position = (position.x+1, position.y); break;
                    case 2 /* down  */: position = (position.x, position.y-1); break;
                    case 3 /* left  */: position = (position.x-1, position.y); break;
                    default: throw new Exception($"Invalid Direction {direction}");
                }
            } while(ec != IntComputer.ExitCode.Ended || computer.OutputCount > 0);
            return (visited, colors);
        }
    }


    [TestFixture]
    internal class Day11Tests
    {
        // Nothing today
    }
}
