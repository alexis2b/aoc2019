using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day13
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day13.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = CountBlockTiles(input);
            Console.WriteLine($"Day13 - part1 - result: {res1}");

            var res2 = GetScoreAfterBreakingAllBlocks(input);
            Console.WriteLine($"Day13 - part2 - result: {res2}");

        }

        public static int CountBlockTiles(IEnumerable<long> program)
        {
            var computer = new IntComputer();
            computer.Run(program);
            var blockTilesCount = 0;
            for(var i = 2; i < computer.OutputCount; i+=3)
                if (computer.Outputs[i] == 2 /*Block*/)
                    blockTilesCount++;
            return blockTilesCount;
        }

        public static long GetScoreAfterBreakingAllBlocks(IEnumerable<long> program)
        {
            var modifiedProgram = program.ToList();
            modifiedProgram[0] = 2; // 2 quarters inserted
            var computer = new IntComputer();
            long score = 0L;
            var bx = 0; // ball x-position, for autoplay
            var px = 0; // paddle x-position, for autoplay
            computer.Run(modifiedProgram);
            var screen = Enumerable.Range(0,21).Select(i => new StringBuilder(new String('X', 44))).ToList();
            Console.Clear();
            do
            {
                while(computer.OutputCount >= 3)
                {
                    var x = (int) computer.PopOutput();
                    var y = (int) computer.PopOutput();
                    long v = computer.PopOutput();
                    if ( x == -1 && y == 0 )
                        score = v;
                    else
                        screen[y][x] = v==0 ? ' ' : v==1 ? 'W' : v==2 ? '#' : v==3 ? '=' : v==4 ? 'O' : '?';
                    if ( v == 3 && x >= 0 ) px = x;
                    if ( v == 4 && x >= 0 ) bx = x;
                }

                // display
//                Console.SetCursorPosition(0, 0);
//                screen.ForEach(r => Console.WriteLine(r));
//                Console.WriteLine("Score: " + score);

                //System.Threading.Thread.Sleep(150);

                // input joystick
                var joystick = Math.Sign(bx - px);

                // if ( Console.KeyAvailable )
                // switch(Console.ReadKey(true).Key)
                // {
                //     case ConsoleKey.LeftArrow:  joystick = -1; break;
                //     case ConsoleKey.RightArrow: joystick =  1; break;
                // }
                computer.AddInput(joystick);

            } while(computer.Continue(Enumerable.Empty<long>()) != IntComputer.ExitCode.Ended);

            // final score
            while(computer.OutputCount >= 3)
            {
                var x = (int) computer.PopOutput();
                var y = (int) computer.PopOutput();
                long v = computer.PopOutput();
                if ( x == -1 && y == 0 )
                    score = v;
            }

            return score;
        }
    }


    [TestFixture]
    internal class Day13Tests
    {
        // Nothing today
    }
}
