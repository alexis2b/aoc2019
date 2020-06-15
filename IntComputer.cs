using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    public sealed class IntComputer
    {
        public enum ExitCode { NotStarted, Continue, NeedInput, Ended };
        private readonly bool _trace;
        private readonly Dictionary<int, Func<int, int, int, ExitCode>> _ops;

        private List<long> _mem;
        private Queue<long> _inputs;
        private readonly Queue<long> _outputs = new Queue<long>();
        private int _ip;
        private int _relativeBase; // for relative mode

        public List<long> Memory => _mem.ToList();
        public List<long> Outputs => _outputs.ToList();

        public ExitCode LastExitCode { get; private set; } = ExitCode.NotStarted;

        public IntComputer(bool trace = false)
        {
            _trace = trace; // enable tracemode

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
                [ 9] = OpAddToRelativeBase,
                [99] = OpEnd
            };
        }

        // compatibility with Day 2
        public ExitCode Run(IEnumerable<long> program) => Run(program, Enumerable.Empty<long>());

        public ExitCode Run(IEnumerable<long> program, IEnumerable<long> inputs)
        {
            // Load the program and initialize the state
            _mem = program.ToList();
            _inputs = new Queue<long>();
            _ip = 0;
            _relativeBase = 0;
            return Continue(inputs);
        }

        public ExitCode Continue(IEnumerable<long> inputs)
        {
            // add the new inputs
            foreach(var input in inputs)
                _inputs.Enqueue(input);
            ExitCode exitCode;
            while((exitCode = NextOp()) == ExitCode.Continue);
            return LastExitCode = exitCode;
        }

        public int InputCount => _inputs.Count;
        public void AddInput(long input) => _inputs.Enqueue(input);

        public int OutputCount => _outputs.Count;
        public long PopOutput() => _outputs.Dequeue();
        public void ClearOutputs()
        {
            _outputs.Clear();
        }

        // Execute the next operation, returns false when exit is requested (opcode 99)
        private ExitCode NextOp()
        {
            var op     = (int) _mem[_ip]; // opcode can not be larger than an int
            var code   = op % 100;
            var mode1  = ( op /   100 ) % 10;
            var mode2  = ( op /  1000 ) % 10;
            var mode3  = ( op / 10000 ) % 10;

            return _ops[code](mode1, mode2, mode3);
        }

        private long ReadAt(int valuePosition, int mode)
        {
            var value = ReadAtPosition(valuePosition);
            switch(mode)
            {
                case 0 /* POSITION  */: return ReadAtPosition((int) value); // position can not be long
                case 1 /* IMMEDIATE */: return value;
                case 2 /* RELATIVE  */: return ReadAtPosition(_relativeBase + (int) value); // position can not be long
                default: throw new InvalidOperationException($"ReadAt: mode {mode} is not valid");
            }
        }

        private long ReadAtPosition(int position) => _mem.ElementAtOrDefault(position);

        private void WriteAt(int targetPosition, int mode, long value)
        {
            var target = ReadAtPosition(targetPosition);
            switch(mode)
            {
                case 0 /* POSITION */: WriteAtPosition((int) target, value); return;
                case 2 /* RELATIVE */: WriteAtPosition(_relativeBase + (int) target, value); return; // position can not be long
                default: throw new InvalidOperationException($"WriteAt: mode {mode} is not valid");
            }
        }

        private void WriteAtPosition(int position, long value)
        {
            if ( position >= _mem.Count )
            {
                // Resize to support the new address
                var newMem = new List<long>(Enumerable.Repeat(0L, position+1));
                for(var i = 0; i < _mem.Count; i++) newMem[i] = _mem[i];
                _mem = newMem;
            }
            _mem[position] = value;
        }

        private ExitCode OpAdd(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            var a2 = ReadAt(_ip+2, mode2);
            var r  = a1 + a2;
            WriteAt(_ip+3, mode3, r);
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpMultiply(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            var a2 = ReadAt(_ip+2, mode2);
            var r  = a1 * a2;
            WriteAt(_ip+3, mode3, r);
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpInput(int mode1, int mode2, int mode3)
        {
            if ( _inputs.Count == 0 ) return ExitCode.NeedInput; // preserve the state, but need input
            var r = _inputs.Dequeue();
            WriteAt(_ip+1, mode1, r);
            _ip = _ip + 2;
            return ExitCode.Continue;
        }

        private ExitCode OpOutput(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            _outputs.Enqueue(a1);
            Trace($"OpOutput: a1={a1}");
            _ip = _ip + 2;
            return ExitCode.Continue;
        }

        private ExitCode OpJumpIfTrue(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            var a2 = ReadAt(_ip+2, mode2);
            Trace($"OpJumpIfTrue: a1={a1}, a2={a2}");
            _ip = ( a1 != 0 ) ? (int) a2 : _ip + 3; // position can not be long
            return ExitCode.Continue;
        }

        private ExitCode OpJumpIfFalse(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            var a2 = ReadAt(_ip+2, mode2);
            Trace($"OpJumpIfFalse: a1={a1}, a2={a2}");
            _ip = ( a1 == 0 ) ? (int) a2 : _ip + 3; // position can not be long
            return ExitCode.Continue;
        }

        private ExitCode OpLessThan(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            var a2 = ReadAt(_ip+2, mode2);
            var r  = (a1 < a2) ? 1 : 0;
            WriteAt(_ip+3, mode3, r);
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpEquals(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            var a2 = ReadAt(_ip+2, mode2);
            var r  = (a1 == a2) ? 1 : 0;
            WriteAt(_ip+3, mode3, r);
            Trace($"OpEquals: a1={a1}, a2={a2}, r={r}");
            _ip = _ip + 4;
            return ExitCode.Continue;
        }

        private ExitCode OpAddToRelativeBase(int mode1, int mode2, int mode3)
        {
            var a1 = ReadAt(_ip+1, mode1);
            _relativeBase += (int) a1; // position can not be long
            _ip = _ip + 2;
            return ExitCode.Continue;
        }

        private ExitCode OpEnd(int mode1, int mode2, int mode3) => ExitCode.Ended;

        private void Trace(string message)
        {
            if ( ! _trace ) return;
            var ts   = DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture);
            var text = $"{ts} {_ip,04} {message}\n";
            System.IO.File.AppendAllText("trace.log", text);
        }
    }


    /// specific tests for new behavious which are not necessary
    /// covered in a specific day test
    [TestFixture]
    internal class IntComputerTests
    {
        [Test]
        public void TestRequestInputAndContinue()
        {
            var program  = new long[] {3,5,4,5,99,0};
            var computer = new IntComputer();
            Assert.AreEqual(IntComputer.ExitCode.NotStarted, computer.LastExitCode);
            var ec1 = computer.Run(program);
            Assert.AreEqual(IntComputer.ExitCode.NeedInput, ec1);
            Assert.AreEqual(IntComputer.ExitCode.NeedInput, computer.LastExitCode);

            const long newInput = 321;
            var ec2 = computer.Continue(new[] {newInput});
            Assert.AreEqual(IntComputer.ExitCode.Ended, ec2);
            Assert.AreEqual(IntComputer.ExitCode.Ended, computer.LastExitCode);
            Assert.AreEqual(newInput, computer.Memory[5]);
            Assert.AreEqual(1, computer.OutputCount);
            Assert.AreEqual(newInput, computer.PopOutput());
            Assert.AreEqual(0, computer.OutputCount);
        }

        [Test]
        public void TestReadOutOfBounds()
        {
            var program  = new long[] {1,10,11,7,4,7,99,321}; // add[10]+[11]->[7], output [7] -> expect [7] to be zero (0+0)
            var computer = new IntComputer();
            computer.Run(program);
            Assert.AreEqual(0, computer.PopOutput());
        }

        [Test]
        public void TestWriteOutOfBounds()
        {
            var program  = new long[] {1,7,8,1024,4,1024,99,321,123}; // add[7]+[8]->[1024], output [1024] -> expect [1024] to be 444 (321+123)
            var computer = new IntComputer();
            computer.Run(program);
            Assert.AreEqual(444, computer.PopOutput());
        }
    }



    [TestFixture]
    internal class Day02Tests
    {
        [TestCase(new long[] {1,9,10,3,2,3,11,0,99,30,40,50}, 0, 3500)]
        [TestCase(new long[] {1,0,0,0,99}, 0, 2)]
        [TestCase(new long[] {2,3,0,3,99}, 3, 6)]
        [TestCase(new long[] {2,4,4,5,99,0}, 5, 9801)]
        [TestCase(new long[] {1,1,1,4,99,5,6,0,99}, 0, 30)]
        public void Test1(long[] program, int position, long value)
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
            computer.Run(new long[] {3,0,4,0,99}, new long[] {input});
            Assert.AreEqual(input, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 0)]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        public void TestEqual8PositionMode(long input, long expected)
        {
            var computer = new IntComputer();
            computer.Run(new long[] {3,9,8,9,10,9,4,9,99,-1,8}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 0)]
        [TestCase(7, 1)]
        [TestCase(0, 1)]
        [TestCase(10, 0)]
        public void TestLessThan8PositionMode(long input, long expected)
        {
            var computer = new IntComputer();
            computer.Run(new long[] {3,9,7,9,10,9,4,9,99,-1,8}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 0)]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        public void TestEqual8ImmediateMode(long input, long expected)
        {
            var computer = new IntComputer();
            computer.Run(new long[] {3,3,1108,-1,8,3,4,3,99}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 0)]
        [TestCase(7, 1)]
        [TestCase(0, 1)]
        [TestCase(10, 0)]
        public void TestLessThan8ImmediateMode(long input, long expected)
        {
            var computer = new IntComputer();
            computer.Run(new long[] {3,3,1107,-1,8,3,4,3,99}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 0)]
        [TestCase(10, 1)]
        public void JumpTestPositionMode(long input, long expected)
        {
            var computer = new IntComputer();
            computer.Run(new long[] {3,12,6,12,15,1,13,14,13,4,13,99,-1,0,1,9}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 0)]
        [TestCase(10, 1)]
        public void JumpTestImmediateMode(long input, long expected)
        {
            var computer = new IntComputer();
            computer.Run(new long[] {3,3,1105,-1,9,1101,0,0,12,4,12,99,1}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }

        [TestCase(8, 1000)]
        [TestCase(7, 999)]
        [TestCase(0, 999)]
        [TestCase(10, 1001)]
        public void TestComparisonTo8(long input, long expected)
        {
            var computer = new IntComputer();
            computer.Run(new long[] {3,21,1008,21,8,20,1005,20,22,107,8,21,20,1006,20,31,
                                    1106,0,36,98,0,0,1002,21,125,20,4,20,1105,1,46,104,
                                    999,1105,1,46,1101,1000,1,20,4,20,1105,1,46,98,99}, new[] {input});
            Assert.AreEqual(expected, computer.Outputs[0]);
        }
    }

    internal class Day09Texts
    {
        [Test]
        public void Test1_1()
        {
            var program = new long[] {109,1,204,-1,1001,100,1,100,1008,100,16,101,1006,101,0,99};
            var computer = new IntComputer();
            computer.Run(program, new long[] {});
            Assert.AreEqual(program, computer.Outputs.ToArray());
        }

        [Test]
        public void Test1_2()
        {
            var program = new long[] {1102,34915192,34915192,7,4,7,99,0};
            var computer = new IntComputer();
            computer.Run(program, new long[] {});
            var outputStr = computer.PopOutput().ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(16, outputStr.Length);
        }

        [Test]
        public void Test1_3()
        {
            var program = new long[] {104,1125899906842624,99};
            var computer = new IntComputer();
            computer.Run(program, new long[] {});
            Assert.AreEqual(1125899906842624L, computer.PopOutput());
        }
    }
}