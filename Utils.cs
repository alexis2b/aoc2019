using System.Collections.Generic;
using System.Linq;

namespace aoc2019
{
    // Special utility functions
    public static class Utils
    {
        // Returns a set of all coordinations in a 2D zone starting at (x0, y0) and covering
        // the whole width and height
        public static IEnumerable<(int x, int y)> Range2D(int x0, int y0, int width, int height)
            => Enumerable.Range(y0, height).SelectMany(y => Enumerable.Range(x0, width).Select(x => (x, y)));

        // Create a new copy of an array where the value at index i is replace with the given value
        public static T[] MutateArray<T>(T[] array, int i, T value)
        {
            var result = new T[array.Length];
            array.CopyTo(result, 0);
            result[i] = value;
            return result;
        }
    }
}