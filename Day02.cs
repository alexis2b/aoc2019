using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace aoc2019
{
    internal class Day02
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day02.txt")).SelectMany(l => l.Split(',')).Select(Int32.Parse).ToList();

            var res1 = ExecuteAndGetPosition0(input);
            Console.WriteLine($"Day01 - part1 - result: {res1}");

            var (noun, verb) = GuessNounAndVerb(input, 19690720);
            Console.WriteLine($"Day01 - part2 - result: {100*noun + verb}");
        }

        public static int ExecuteAndGetPosition0(IEnumerable<int> input)
        {
            var modifiedInput = input.ToList();
            modifiedInput[1] = 12;
            modifiedInput[2] =  2;
            var computer = new IntComputer();
            computer.Run(modifiedInput);
            return computer.Memory[0];
        }

        public static (int, int) GuessNounAndVerb(IEnumerable<int> input, int target)
        {
            for(var noun = 0; noun < 100; noun++)
                for(var verb = 0; verb < 100; verb++)
                {
                    var modifiedInput = input.ToList();
                    modifiedInput[1] = noun;
                    modifiedInput[2] = verb;
                    var computer = new IntComputer();
                    try
                    {
                        computer.Run(modifiedInput);
                        if ( computer.Memory[0] == target )
                            return (noun, verb);

                    } catch(Exception) { continue; }
                }
            return (0, 0);
        }
    }



    internal sealed class IntComputer
    {
        const int OP_ADD = 1;
        const int OP_MUL = 2;
        const int OP_END = 99;

        private List<int> _mem;
        private int _ip;

        public List<int> Memory => _mem.ToList();

        public void Run(IEnumerable<int> program)
        {
            _mem = program.ToList();
            _ip = 0;
            while(NextOp());
        }

        // Execute the next operation, returns false when exit is requested (opcode 99)
        private bool NextOp()
        {
            switch(_mem[_ip])
            {
                case OP_ADD: _mem[_mem[_ip+3]] = _mem[_mem[_ip+1]] + _mem[_mem[_ip+2]]; _ip = _ip + 4; return true;
                case OP_MUL: _mem[_mem[_ip+3]] = _mem[_mem[_ip+1]] * _mem[_mem[_ip+2]]; _ip = _ip + 4; return true;
                case OP_END: return false;
                default:
                    throw new NotImplementedException($"unknown opcode {_mem[_ip]}");
            }
        }
    }

    [TestFixture]
    internal class Day02Tests
    {
        [TestCase(new[] {1,9,10,3,2,3,11,0,99,30,40,50}, 0, 3500)]
        [TestCase(new[] {1,0,0,0,99}, 0, 2)]
        [TestCase(new[] {2,3,0,3,99}, 3, 6)]
        [TestCase(new[] {2,4,4,5,99,0}, 5, 9801)]
        [TestCase(new[] {1,1,1,4,99,5,6,0,99}, 0, 30)]
        public void Test1(int[] program, int position, int value)
        {
            var computer = new IntComputer();
            computer.Run(program);
            Assert.AreEqual(value, computer.Memory[position]);
        }
    }
}
