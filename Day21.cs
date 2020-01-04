using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day21
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day21.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = RunCode(input, _part1Code);
            Console.WriteLine($"Day21 - part1 - result: {res1}");

            var res2 = RunCode(input, _part2Code);
            Console.WriteLine($"Day21 - part2 - result: {res2}");
        }

// SpringScript Code - 15 instructions max
// All values are booleans: T means there is ground, F means there is a hole
// T - temporary value register
// J - jump register -> if true, droid will jump
// A - ground one tile away
// B - ground two tiles away
// C - ground three tiles away
// D - ground four tiles away
// AND X Y -> Y = (X && Y)
// OR X Y  -> Y = (X || Y)
// NOT X Y -> Y = !X


        // Part 1 - we basically have a function (bool A, bool B, bool C, bool D) -> bool J
        // Decision table:
        // ##  A B C D -> J  Notes              Logic Result
        //  0  0 0 0 0 -> ?  N/A Game Over      JUMP - ok
        //  1  0 0 0 1 -> 1  MUST jump          JUMP - ok
        //  2  0 0 1 0 -> ?  N/A Game Over      JUMP - ok
        //  3  0 0 1 1 -> 1  MUST jump          JUMP - ok
        //  4  0 1 0 0 -> ?  N/A Game Over      JUMP - ok
        //  5  0 1 0 1 -> 1  MUST jump          JUMP - ok
        //  6  0 1 1 0 -> ?  N/A Game over      JUMP - ok
        //  7  0 1 1 1 -> 1  MUST jump          JUMP - ok
        //  8  1 0 0 0 -> 0  MUST NOT jump      walk - ok
        //  9  1 0 0 1 -> ?  COULD jump         JUMP - ok
        // 10  1 0 1 0 -> 0  MUST NOT jump      walk - ok
        // 11  1 0 1 1 -> ?  COULD jump         JUMP - ok
        // 12  1 1 0 0 -> 0  MUST NOT jump      walk - ok
        // 13  1 1 0 1 -> ?  COULD jump         walk - ok  ---> Must Jump in that case!
        // 14  1 1 1 0 -> 0  MUST NOT jump      walk - ok
        // 15  1 1 1 1 -> ?  COULD jump         walk - ok
        //
        // Observations:
        // A is 0 -> MUST JUMP in any case
        // if A is 1
        // - D is 0 -> MUST NOT JUMP in any case
        // - if D is 1
        //   - could jump if B is 0
        //
        // Logic: !A || (D && !B) || (A & B & !C & D)
        // NOT A J    // first case, !A
        // NOT B T    // second term
        // AND D T
        // OR T J     // first or second term
        // NOT C T    // third term
        // AND A T
        // AND B T
        // AND D T
        // OR T J     // OR third term
    private const string _part1Code =
@"NOT A J
NOT B T
AND D T
OR T J
NOT C T
AND A T
AND B T
AND D T
OR T J
WALK
";

        // Part 2 - same as part 1 with a twist
        // Decision table:
        // ##  A B C D -> J  Notes              Logic Result
        //  0  0 0 0 0 -> ?  N/A Game Over      JUMP - ok
        //  1  0 0 0 1 -> 1  MUST jump          JUMP - ok
        //  2  0 0 1 0 -> ?  N/A Game Over      JUMP - ok
        //  3  0 0 1 1 -> 1  MUST jump          JUMP - ok
        //  4  0 1 0 0 -> ?  N/A Game Over      JUMP - ok
        //  5  0 1 0 1 -> 1  MUST jump          JUMP - ok
        //  6  0 1 1 0 -> ?  N/A Game over      JUMP - ok
        //  7  0 1 1 1 -> 1  MUST jump          JUMP - ok
        //  8  1 0 0 0 -> 0  MUST NOT jump      walk - ok
        //  9  1 0 0 1 -> ?  COULD jump         JUMP - ok
        // 10  1 0 1 0 -> 0  MUST NOT jump      walk - ok
        // 11  1 0 1 1 -> ?  COULD jump         JUMP - ok
        // 12  1 1 0 0 -> 0  MUST NOT jump      walk - ok
        // 13  1 1 0 1 -> ?  COULD jump         walk - ok  ---> Must Jump in that case! but only if E or H is available
        // 14  1 1 1 0 -> 0  MUST NOT jump      walk - ok
        // 15  1 1 1 1 -> ?  COULD jump         walk - ok
        //
        // Observations:
        // A is 0 -> MUST JUMP in any case
        // if A is 1
        // - D is 0 -> MUST NOT JUMP in any case
        // - if D is 1
        //   - could jump if B is 0
        //
        // Logic: !A || (D && !B) || (A & B & !C & D & (E || H))
        // Start with last term assuming J is false at beginning
        // OR E J
        // OR H J
        // AND A J
        // AND B J
        // AND D J
        // NOT C T
        // AND T J
        //
        // NOT A T    // first case, !A
        // OR T J
        //
        // NOT B T    // second term
        // AND D T
        // OR T J     // first or second term
    private const string _part2Code =
@"OR E J
OR H J
AND A J
AND B J
AND D J
NOT C T
AND T J
NOT A T
OR T J
NOT B T
AND D T
OR T J
RUN
";

        public static long RunCode(IEnumerable<long> program, string code)
        {
            // execute script and get results
            var computer = new IntComputer();
            var ec = computer.Run(program, code.Select(c => (long) c));
            Console.WriteLine("Exit code: " + ec);
            Console.WriteLine("Outputs: " + String.Join(',', computer.Outputs));
            Render(computer.Outputs);
            return computer.Outputs.Last();
        }

        private static void Render(List<long> outputs)
        {
            foreach(var c in outputs)
            if ( c <= 255)
                Console.Write((char) c);
        }
    }


    [TestFixture]
    internal class Day21Tests
    {
        // Nothing today
    }
}
