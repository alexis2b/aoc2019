using System;
using System.Diagnostics.CodeAnalysis;

namespace aoc2019
{
    /// A type adding a Weight to a type T and with a Comparator
    /// Allowing to select minimum or maximum weight instances
    internal sealed class Weighted<T> : IComparable<Weighted<T>>
    {
        private readonly T _value;
        private readonly int _weight;
        public T Value => _value;
        public int Weight => _weight;
        public Weighted(T value, int weight)
        {
            _value = value;
            _weight = weight;
        }

        public int CompareTo([AllowNull] Weighted<T> other)
        {
            return _weight - other._weight;
        }
    }
}