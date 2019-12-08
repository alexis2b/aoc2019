using NUnit.Framework;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace aoc2019
{
    internal class Day04
    {
        public static void Run()
        {
            var input = (min: 240920, max: 789857);

            var res1 = MatchingPasswords(input.min, input.max).ToList();
            Console.WriteLine($"Day04 - part1 - result: {res1.Count}");

            var res2 = MatchingPasswords2(res1).Count();
            Console.WriteLine($"Day04 - part2 - result: {res2}");
        }

        public static IEnumerable<int> MatchingPasswords(int min, int max)
            => Enumerable.Range(min, max-min+1).Where(IsMatch);

        public static IEnumerable<int> MatchingPasswords2(List<int> candidates)
            => candidates.Where(IsMatch2);

        public static bool IsMatch(int password)
        {
            var passwordStr = password.ToString(CultureInfo.InvariantCulture);
            return HasTwoEqualConsecutiveDigits(passwordStr)
                && HasOnlyIncreasingDigits(passwordStr);
        }

        public static bool IsMatch2(int password)
        {
            var passwordStr = password.ToString(CultureInfo.InvariantCulture);
            for(int i = 0; i < passwordStr.Length - 1; i++)
                if ( passwordStr[i] == passwordStr[i+1]
                     && (i == passwordStr.Length - 2 || passwordStr[i+2] != passwordStr[i])
                     && (i == 0 || passwordStr[i-1] != passwordStr[i]))
                    return true;
            return false;
        }

        private static bool HasTwoEqualConsecutiveDigits(string password)
        {
            for(int i = 0; i < password.Length - 1; i++)
                if ( password[i] == password[i+1] )
                    return true;
            return false;
        }

        private static bool HasOnlyIncreasingDigits(string password)
        {
            for(int i = 1; i < password.Length ; i++)
                if ( password[i] < password[i-1] ) // works also with characters
                    return false;
            return true;
        }
    }

    [TestFixture]
    internal class Day04Tests
    {
        [TestCase(111111, true)]
        [TestCase(223450, false)]
        [TestCase(123789, false)]
        public void Test1(int password, bool expected)
        {
            var actual = Day04.IsMatch(password);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(112233, true)]
        [TestCase(123444, false)]
        [TestCase(111122, true)]
        public void Test2(int password, bool expected)
        {
            var actual = Day04.IsMatch(password) && Day04.IsMatch2(password);
            Assert.AreEqual(expected, actual);
        }
    }
}
