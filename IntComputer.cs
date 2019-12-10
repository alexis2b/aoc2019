using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    public sealed class IntComputer
    {
        public enum ExitCode { NotStarted, Continue, NeedInput, Ended };
        private readonly Dictionary<int, Func<int, int, int, ExitCode>> _ops;

        private List<int> _mem;
        private Queue<int> _inputs;
        private readonly Queue<int> _outputs = new Queue<int>();
        private int _ip;

        public List<int> Memory => _mem.ToList();
        public List<int> Outputs => _outputs.ToList();

        public ExitCode LastExitCode { get; private set; } = ExitCode.NotStarted;

        public IntComputer()
        {
            // Build the instruction list
            _ops = new Dictionary<int, Func<int, int, int, ExitCode>>
            {
                [ 1] = OpAdd,
                [ 2] = OpMultiply,
                [ 3] = OpInput,
                [ 4] = OpOutput,
                [ 5] = OpJumpIfTrue,
                [ 6] = OpJumpIfFalse,
                [ 7] = OpLessThan,
                [ 8] = OpEquals,
                [99] = OpEnd
            };
        }

        // compatibility with Day 2
        public ExitCode Run(IEnumerable<int> program) => Run(program, Enumerable.Empty<int>());

        public ExitCode Run(IEnumerable<int> program, IEnumerable<int> inputs)
        {
            // Load the program and initialize the state
            _mem = program.ToList();
            _inputs = new Queue<int>();
            _ip = 0;
            return Continue(inputs);
        }

        public ExitCode Continue(IEnumerable<int> inputs)
        {
            // add the new inputs
            foreach(var input in inputs)
                _inputs.Enqueue(input);
            ExitCode exitCode;
            while((exitCode = NextOp()) == ExitCode.Continue);
            return LastExitCode = exitCode;
        }

        public void AddInput(int input) => _inputs.Enqueue(input);

        public int OutputCount => _outputs.Count;
        public int PopOutput() => _outputs.Dequeue();

        // Execute the next operation, returns false when exit is requested (opcode 99)
        private ExitCode NextOp()
        {
            var op     = _mem[_ip];
            var code   = op % 100;
            var mode1  = ( op /   100 ) % 10;
            var mode2  = ( op /  1000 ) % 10;
            var mode3  = ( op / 10000 ) % 10;

            return _ops[code](mode1, mode2, mode3);
        }

        private ExitCode OpAdd(int mode1, int mode2, int mode3)
        {
            var a1 = mode1 == 1 ? _mem[_ip+1] : _mem[_mem[_ip+1]];
            var a2 = mode2 == 1 ? _mem[_ip+2] : _mem[_mem[_ip+2]];
            if ( mode3 == 1 ) throw new Exception($"OpAdd: mode3 can not be 1");
            _mem[_mem[_ip+3]] = a1 + a2;
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpMultiply(int mode1, int mode2, int mode3)
        {
            var a1 = mode1 == 1 ? _mem[_ip+1] : _mem[_mem[_ip+1]];
            var a2 = mode2 == 1 ? _mem[_ip+2] : _mem[_mem[_ip+2]];
            if ( mode3 == 1 ) throw new Exception($"OpMultiply: mode3 can not be 1");
            _mem[_mem[_ip+3]] = a1 * a2;
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpInput(int mode1, int mode2, int mode3)
        {
            if ( mode1 == 1 ) throw new Exception($"OpInput: mode1 can not be 1");
            if ( _inputs.Count == 0 ) return ExitCode.NeedInput; // preserve the state, but need input
            _mem[_mem[_ip+1]] = _inputs.Dequeue();
            _ip = _ip + 2;
            return ExitCode.Continue;
        }

        private ExitCode OpOutput(int mode1, int mode2, int mode3)
        {
            var a1 = mode1 == 1 ? _mem[_ip+1] : _mem[_mem[_ip+1]];
            _outputs.Enqueue(a1);
            _ip = _ip + 2;
            return ExitCode.Continue;
        }

        private ExitCode OpJumpIfTrue(int mode1, int mode2, int mode3)
        {
            var a1 = mode1 == 1 ? _mem[_ip+1] : _mem[_mem[_ip+1]];
            var a2 = mode2 == 1 ? _mem[_ip+2] : _mem[_mem[_ip+2]];
            _ip = ( a1 != 0 ) ? a2 : _ip + 3;
            return ExitCode.Continue;
        }

        private ExitCode OpJumpIfFalse(int mode1, int mode2, int mode3)
        {
            var a1 = mode1 == 1 ? _mem[_ip+1] : _mem[_mem[_ip+1]];
            var a2 = mode2 == 1 ? _mem[_ip+2] : _mem[_mem[_ip+2]];
            _ip = ( a1 == 0 ) ? a2 : _ip + 3;
            return ExitCode.Continue;
        }

        private ExitCode OpLessThan(int mode1, int mode2, int mode3)
        {
            var a1 = mode1 == 1 ? _mem[_ip+1] : _mem[_mem[_ip+1]];
            var a2 = mode2 == 1 ? _mem[_ip+2] : _mem[_mem[_ip+2]];
            if ( mode3 == 1 ) throw new Exception($"OpLessThan: mode3 can not be 1");
            _mem[_mem[_ip+3]] = (a1 < a2) ? 1 : 0;
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpEquals(int mode1, int mode2, int mode3)
        {
            var a1 = mode1 == 1 ? _mem[_ip+1] : _mem[_mem[_ip+1]];
            var a2 = mode2 == 1 ? _mem[_ip+2] : _mem[_mem[_ip+2]];
            if ( mode3 == 1 ) throw new Exception($"OpLessThan: mode3 can not be 1");
            _mem[_mem[_ip+3]] = (a1 == a2) ? 1 : 0;
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpEnd(int mode1, int mode2, int mode3) => ExitCode.Ended;
    }


    /// specific tests for new behavious which are not necessary
    /// covered in a specific day test
    [TestFixture]
    internal class IntComputerTests
    {
        [Test]
        public void TestRequestInputAndContinue()
        {
            var program  = new[] {3,5,4,5,99,0};
            var computer = new IntComputer();
            Assert.AreEqual(IntComputer.ExitCode.NotStarted, computer.LastExitCode);
            var ec1 = computer.Run(program);
            Assert.AreEqual(IntComputer.ExitCode.NeedInput, ec1);
            Assert.AreEqual(IntComputer.ExitCode.NeedInput, computer.LastExitCode);

            const int newInput = 321;
            var ec2 = computer.Continue(new[] {newInput});
            Assert.AreEqual(IntComputer.ExitCode.Ended, ec2);
            Assert.AreEqual(IntComputer.ExitCode.Ended, computer.LastExitCode);
            Assert.AreEqual(newInput, computer.Memory[5]);
            Assert.AreEqual(1, computer.OutputCount);
            Assert.AreEqual(newInput, computer.PopOutput());
            Assert.AreEqual(0, computer.OutputCount);
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

    internal class Day05Texts
    {
        [Test]
        public void Test1()
        {
            var input = 54321;
            var computer = new IntComputer();
            computer.Run(new[] {3,0,4,0,99}, new[] {input});
            Assert.AreEqual(input, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 0)]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        public void TestEqual8PositionMode(int input, int expected)
        {
            var computer = new IntComputer();
            computer.Run(new[] {3,9,8,9,10,9,4,9,99,-1,8}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 0)]
        [TestCase(7, 1)]
        [TestCase(0, 1)]
        [TestCase(10, 0)]
        public void TestLessThan8PositionMode(int input, int expected)
        {
            var computer = new IntComputer();
            computer.Run(new[] {3,9,7,9,10,9,4,9,99,-1,8}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 0)]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        public void TestEqual8ImmediateMode(int input, int expected)
        {
            var computer = new IntComputer();
            computer.Run(new[] {3,3,1108,-1,8,3,4,3,99}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 0)]
        [TestCase(7, 1)]
        [TestCase(0, 1)]
        [TestCase(10, 0)]
        public void TestLessThan8ImmediateMode(int input, int expected)
        {
            var computer = new IntComputer();
            computer.Run(new[] {3,3,1107,-1,8,3,4,3,99}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 0)]
        [TestCase(10, 1)]
        public void JumpTestPositionMode(int input, int expected)
        {
            var computer = new IntComputer();
            computer.Run(new[] {3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 0)]
        [TestCase(10, 1)]
        public void JumpTestImmediateMode(int input, int expected)
        {
            var computer = new IntComputer();
            computer.Run(new[] {3,3,1105,-1,9,1101,0,0,12,4,12,99,1}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1000)]
        [TestCase(7, 999)]
        [TestCase(0, 999)]
        [TestCase(10, 1001)]
        public void TestComparisonTo8(int input, int expected)
        {
            var computer = new IntComputer();
            computer.Run(new[] {3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,
                                1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,
                                999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }
    }
}