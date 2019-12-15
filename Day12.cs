using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day12
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day12.txt")).ToList();

            var res1 = SystemEnergyAfter(input, 1000);
            Console.WriteLine($"Day12 - part1 - result: {res1}");

            // for part2, we need to optimize the simulation since playing all ticks is not possible
            // by observing that the Moon movement on an axis is independant from all other axis, we
            // can simulate each axis independantly and calculate the Axis period. The total system
            // period is then smallest period divisible by all axis periods
            var res2 = SystemPeriod(input);
            Console.WriteLine($"Day12 - part2 - result: {res2}");
        }

        public static 
        int SystemEnergyAfter(IEnumerable<string> input, int steps)
        {
            var system = SkySimulation.InitialiseFrom(input);
            for(var t = 1; t <= steps; t++)
                system.Tick();
            return system.TotalEnergy;
        }

        public static long SystemPeriod(IEnumerable<string> input)
        {
            var system  = SkySimulation.InitialiseFrom(input);
            var xPeriod = system.GetAxisPeriod(Axis.X);
            var yPeriod = system.GetAxisPeriod(Axis.Y);
            var zPeriod = system.GetAxisPeriod(Axis.Z);
            return lcm(xPeriod, lcm(yPeriod, zPeriod));
        }

        // Greatest Common Factor
        private static long gcf(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        // Least Common Multiple
        private static long lcm(long a, long b)
        {
            return (a / gcf(a, b)) * b;
        }

        // useful class to manage vectors mathematically
        public sealed class Vector
        {
            public static readonly Vector Zero = new Vector((0, 0, 0));
            public readonly (int x, int y, int z) _v;

            public Vector((int x, int y, int z) v)
            {
                _v = v;
            }

            public int X => _v.x;
            public int Y => _v.y;
            public int Z => _v.z;

            // Norm of the Vector (i.e. sum of absolute coordinates values)
            public int Norm => Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);

            // Returns the vector where positive values are set to +1, negative to -1 and zero stays zero
            public Vector Normalized() => new Vector((Math.Sign(_v.x), Math.Sign(_v.y), Math.Sign(_v.z)));

            public static Vector operator +(Vector v1, Vector v2)
                => new Vector((v1.X+v2.X, v1.Y+v2.Y, v1.Z+v2.Z));
            
            public static Vector operator -(Vector v1, Vector v2)
                => new Vector((v1.X-v2.X, v1.Y-v2.Y, v1.Z-v2.Z));

            public static Vector operator -(Vector v)
                => new Vector((-v.X, -v.Y, -v.Z));

            // for indexed retrieval of dimensions (0 => X, 1 => Y, 2 => Z)
            public int this[int i] { get => i == 0 ? X : i == 1 ? Y : Z; }
        }


        public enum Axis { X, Y, Z };

        public sealed class SkySimulation
        {   
            private readonly List<Moon> _moons;
            private int _t;

            private SkySimulation(IEnumerable<Moon> moons)
            {
                _moons = moons.ToList();
                _t     = 0;
            }

            public static SkySimulation InitialiseFrom(IEnumerable<string> moonCoordinates)
                => new SkySimulation(moonCoordinates.Select(c => Moon.FromCoordinates(c)));

            public int T => _t;
            public int TotalEnergy => _moons.Sum(m => m.Energy);

            // increase simulated time by one tick
            public int Tick()
            {
                _t++;
                // build every possible pairs of moon
                // - for each moon, calculate the gravity vector from the others, and sum them
                Pairs(_moons)
                    .SelectMany(mp => new[] {
                        (moon: mp.p1, gravityVector: GravityPull(mp.p1, mp.p2)),
                        (moon: mp.p2, gravityVector: GravityPull(mp.p2, mp.p1))
                    })
                    .GroupBy(mv => mv.moon)
                    .Select(mvg => (moon: mvg.Key, totalGravityVector: mvg.Aggregate(Vector.Zero, (v, mv) => v+mv.gravityVector)))
                    .ToList()
                    .ForEach(mv => mv.moon.UpdateVelocityAndPosition(mv.totalGravityVector));

                return _t;
            }

            // Calculate the number of ticks needed for the system projection along one single axis
            // to repeat itself
            public long GetAxisPeriod(Axis axis)
            {
                var p0 = _moons.Select(m => m.P[(int)axis]).ToArray(); // all positions
                var v0 = _moons.Select(m => m.V[(int)axis]).ToArray(); // all velocities
                var p = p0.ToArray();
                var v = v0.ToArray();
                var t = 0L;
                var cycleFound = true;
                do
                {
                    cycleFound = true;
                    // update each moon with the others
                    for(var m1 = 0; m1 < p0.Length; m1++)
                    {
                        for(var m2 = m1+1; m2 < p0.Length; m2++)
                        {
                            // acceleration impacts the velocity of each moons, in opposite directions
                            var a = Math.Sign(p[m2] - p[m1]);
                            v[m1] += a;
                            v[m2] -= a; 
                        }
                        p[m1] += v[m1];
                        if (p[m1] != p0[m1] || v[m1] != v0[m1])
                            cycleFound = false;
                    }
                    t++;
                    if ( t >= 2770L && t <= 2773L )
                        t = t + 0L; // noop
                } while(!cycleFound);
                return t;
            }

            private static IEnumerable<(T p1, T p2)> Pairs<T>(IEnumerable<T> input)
            {
                var list = input.ToList();
                for(var i1 = 0; i1 < list.Count-1; i1++)
                    for(var i2 = i1+1; i2 < list.Count; i2++)
                        yield return (list[i1], list[i2]);
            }

            private static Vector GravityPull(Moon on, Moon from)
            => (from.P - on.P).Normalized();

            public void PrintState()
            {
                foreach(var moon in _moons)
                    moon.PrintState();
            }
        }

        public sealed class Moon
        {
            private static readonly Regex CoordsRegex = new Regex(@"^<x=(?<x>-?\d+), y=(?<y>-?\d+), z=(?<z>-?\d+)>$");

            private Vector _p;  // position
            private Vector _v;  // speed

            public Vector P => _p;
            public Vector V => _v;

            public int PotentialEnergy => _p.Norm;
            public int KineticEnergy => _v.Norm;
            public int Energy => PotentialEnergy * KineticEnergy;

            private Moon(Vector p0)
            {
                _p = p0;
                _v = Vector.Zero;
            }

            public static Moon FromCoordinates(string coordinates)
            {
                var match = CoordsRegex.Match(coordinates);
                if ( ! match.Success ) throw new Exception($"coordinates '{coordinates}' could not be parsed");
                return new Moon(new Vector((
                    Int32.Parse(match.Groups["x"].Value),
                    Int32.Parse(match.Groups["y"].Value),
                    Int32.Parse(match.Groups["z"].Value)
                )));
            }

            public void UpdateVelocityAndPosition(Vector a)
            {
                _v += a;
                _p += _v;
            }

            public void PrintState()
            {
                Console.WriteLine($"pos=<x={_p.X,3}, y={_p.Y,3}, z={_p.Z,3}>, vel=<x={_v.X,3}, y={_v.Y,3}, z={_v.Z,3}>");
            }
        }
    }

    [TestFixture]
    internal class Day12Tests
    {
        [Test]
        public void Test1_1()
        {
            var input = new string[] {
                "<x=-1, y=0, z=2>",
                "<x=2, y=-10, z=-7>",
                "<x=4, y=-8, z=8>",
                "<x=3, y=5, z=-1>"
            };
            var system = Day12.SkySimulation.InitialiseFrom(input);
            for(var t = 0; t <= 10; t++)
            {
                Console.WriteLine($"After {t} steps:");
                system.PrintState();
            }
        }

        [Test]
        public void Test2_1()
        {
            var input = new string[] {
                "<x=-1, y=0, z=2>",
                "<x=2, y=-10, z=-7>",
                "<x=4, y=-8, z=8>",
                "<x=3, y=5, z=-1>"
            };
            var actual = Day12.SystemPeriod(input);
            Assert.AreEqual(2772L, actual);
        }

        [Test]
        public void Test2_2()
        {
            var input = new string[] {
                "<x=-8, y=-10, z=0>",
                "<x=5, y=5, z=10>",
                "<x=2, y=-7, z=3>",
                "<x=9, y=-8, z=-3>"
            };
            var actual = Day12.SystemPeriod(input);
            Assert.AreEqual(4686774924L, actual);
        }
    }
}
