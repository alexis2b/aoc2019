using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day19
    {
        private const int shipWidth  = 99; // off by 1 due to some error below
        private const int ShipHeight = 99; // off by 1 due to some error below


        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day19.txt")).SelectMany(l => l.Split(',')).Select(Int64.Parse).ToList();

            var res1 = Part1(input);
            Console.WriteLine($"Day19 - part1 - result: {res1}");

            var res2 = Part2(input);
            Console.WriteLine($"Day19 - part2 - result: {res2}");
        }

        public static long Part1(IEnumerable<long> program)
        {
            var beamShape = new BeamShape(program);
            var map = Utils.Range2D(0, 0, 50, 50)
                .Select(p => (p, isPulled: beamShape.IsPulled(p)))
                .ToDictionary(po => po.p, po => po.isPulled);

            for(var y = 0; y < 50; y++)
            {
                for(var x = 0; x < 50; x++)
                    Console.Write(map[(x,y)] == 1 ? '#' : '.');
                Console.WriteLine();
            }

            return map.Values.Sum();
        }

        public static int Part2(IEnumerable<long> program)
        {
            // for part2 we use the following logic
            // to fit a 100x100 square in the beam, we need a covered area that is both 100x high and 100x wide in one block
            // we can perform a guess and use that to find the next best position
            // we can see that the corner (top, right) and (bottom, left) must be exactly at the edge of the beam to work
            // so given bottom row yb, we can find the first pulled position xb and then check if (top, right) is in the border
            // (i.e. it is pulled and the next one is not)
            // we can simply perform an initial guess at yb and keep going down until we fit (we do not have many steps to evaluate)
            // we can calibrate the formula f(y) = x such that (x, y) is the first beamed position of the row y
            // calibrate the beam model as a function of y, we want - start of beam (x position) and width of beam
            // we use the last 10 rows of the 50x50 area - this model will be used for guessing initial values
            var beamShape = new BeamShape(program);
            var model = ModelCalibration(beamShape);
            Console.WriteLine($"Model calibrated: ax={model.ax} and aw={model.aw}");
            // by geometry rules, we know that the horizontal space needed to fit the square top row in the beam is of
            // 100 (the width of the square) + (ax*100) <- the "slope" applied to the height needed for the extra width
            // we can then use 'aw' to have an initial guess at y for which we get such width
            var widthRequired = shipWidth + (int) Math.Floor(model.ax * ShipHeight);
            var yGuess        = (int) Math.Ceiling(widthRequired / model.aw);
            Console.WriteLine($"Guesstimate: widthRequired={widthRequired} and yGuess={yGuess}");
            // run a gradual search around this initial guess
            var lowerLeftCorner = FitSquare(beamShape, model, yGuess+ShipHeight /* we fit from bottom left */);
            Console.WriteLine($"Lower-left corner fits at {lowerLeftCorner}");
            var upperLeftCorner = (x: lowerLeftCorner.x, y: lowerLeftCorner.y - ShipHeight);
            return upperLeftCorner.x * 10000 + upperLeftCorner.y;
        }

        // return two calibration factors ax and aw such that
        // FirstXBeamed(y) = ax * y
        // WidthBeamed(y) = aw * y
        private static (double ax, double aw) ModelCalibration(BeamShape beamShape)
        {
            var axSamples = new List<double>();
            var awSamples = new List<double>();
            for(var y = 40; y < 50; y++)
            {
                var row = new List<int>(50);
                for(var x = 0; x < 50; x++)
                    row.Add(beamShape.IsPulled((x, y)));
                axSamples.Add(row.IndexOf(1) / (double) y);
                awSamples.Add(row.Sum() / (double) y);
            }
            return (axSamples.Average(), awSamples.Average());
        }

        // we perform a simple search around the given y0
        // methodolody
        // - for a given y (initially y0), find x which is the first beamed cell on this row
        //   this (x, y) position corresponds to the lower-left corner
        // - check the corresponding upper-left corner:
        //   - if it sticks out of the beam, we need to go lower, increase y and try again
        //   - if there are more beamed cells on the same top row, we need to go higher, decrease y and try again
        //   - if we have a perfect fit, we make sure we can not fit at y-1 (rounding error)
        private static (int x, int y) FitSquare(BeamShape beamShape, (double ax, double aw) model, int y0)
        {
            // find x coordinate of first beamed square
            int FirstBeamedXAtY(int y)
            {
                var x = (int) model.ax * y;  // initial guess using the shape model
                while(true)
                {
                    var isCellPulled = beamShape.IsPulled((x, y));
                    if ( isCellPulled == 0 ) // too much on the left, push right and try again
                    {
                        x++;
                        continue;
                    }
                    var isCellBeforePulled = beamShape.IsPulled((x-1, y));
                    if ( isCellBeforePulled == 1 ) // too much on the right, push left and try again
                    {
                        x--;
                        continue;
                    }
                    return x; // found!
                }
            }

            // utility function, can we fit the square with the bottom at row y
            // if successful LowerLeftCorner contains the position of the lower-left-corner
            // deltaY tells which direction Y should move if we can not fit
            bool CanFitAtY(int lowerY, ref int deltaY, ref (int x, int y) lowerLeftCorner)
            {
                // find x coordinate of first beamed square
                var lowerX = FirstBeamedXAtY(lowerY);
                // build coordinates of top-right corner and check if it fits?
                var upperCorner    = (x: lowerX+shipWidth, y: lowerY-ShipHeight);
                var isPulledCorner = beamShape.IsPulled(upperCorner);
                if ( isPulledCorner == 0 )
                {
                    // we are sticking out, we need to go lower
                    deltaY = +1;
                    return false;
                }
                var isPulledAfterCorner = beamShape.IsPulled((upperCorner.x+1, upperCorner.y));
                if ( isPulledAfterCorner == 1 )
                {
                    // we are not fitting exactly on the border, space to go higher
                    deltaY = -1;
                    return false;
                }
                // if we are still here, we do have a match
                deltaY = 0;
                lowerLeftCorner = (lowerX, lowerY);
                return true;
            }


            var lowerY = y0;
            // find a matching Y
            var deltaY = 0;
            var lowerLeftCorner = (x: 0, y: 0);
            while(!CanFitAtY(lowerY, ref deltaY, ref lowerLeftCorner))
                lowerY += deltaY;

            // make sure we can not fit higher
            while(CanFitAtY(lowerLeftCorner.y-1, ref deltaY, ref lowerLeftCorner))
                continue; // keep looping until the row just above do not match
            
            return lowerLeftCorner;
        }

        private class BeamShape
        {
            private readonly IntComputer _computer;
            private readonly IEnumerable<long> _program;

            private readonly Dictionary<(int x, int y), int> _cache;  // cache evaluated cells

            public BeamShape(IEnumerable<long> program)
            {
                _computer = new IntComputer();
                _program = program.ToList();
                _cache = new Dictionary<(int x, int y), int>();
            }

            public int IsPulled((int x, int y) p)
            {
                if ( _cache.TryGetValue(p, out var pulled) )
                    return pulled;
                // MISS - evaluate and cache
                _computer.Run(_program, new long[] { p.x, p.y });
                return _cache[p] = (int) _computer.PopOutput();
            }
        }
    }


    [TestFixture]
    internal class Day19Tests
    {
        // Nothing today
    }
}
