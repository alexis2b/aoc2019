using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day22
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day22.txt")).ToList();

            var res1 = Part1(input);
            var res1b = FindFinalPosition(input, 2019, 10007, 1);
            Console.WriteLine($"Day22 - part1 - result: {res1}");

            var res2 = Part2(input);
            Console.WriteLine($"Day22 - part2 - result: {res2}");
        }

        public static int Part1(List<string> input)
        {
            var shuffler = new Shuffler(10007);
            shuffler.Shuffle(input);
            return shuffler.IndexOf(2019);
        }


        // for Part 2 we obviously can not actually simulate a deck of this size like we did in Part1
        // however, the question only asks for a single card - so the idea is to track the position of that
        // card throughout the shuffle - the trick of part2 also compared to part1 is that they ask to find
        // the initial position of the card (which corresponds to the card number) given it's final position
        // this means we need to apply the shuffle in reverse!
        // given the high number of repetitions we need to find a periodic pattern - it is unlikely that this
        // actual number of iterations has to be done
        // the following formulas gives the new position of a card after a single shuffle operation:
        // i    is the index (position) of the card in the deck, i(n+1) is the new index after applying operation n
        // i0   is the initial index
        // s    is the size of the deck
        // Case of:
        // - "deal into new stack"
        //   - shuffling:         i(n+1) = s-i(n)-1
        //   - reverse shuffling: i(n)   = s-i(n+1)-1
        // - "cut N cards" with N>0
        //   - shuffling:         i(n+1) = if i(n) < N then i(n)+(s-N) else i(n)-N
        //   - reverse shuffling: i(n)   = if i(n+1) >= s-N then i(n+1)-(s-N) else i(n+1)+N  - also equivalent to "cut -N cards"
        // - "cut N cards" with N<0
        //   - shuffling:         i(n+1) = if i(n) >= s+N then i(n)-(s+N) else i(n)-N
        //   - reverse shuffling: i(n)   = if i(n+1) < -N then i(n+1)+(s+N) else i(n+1)+N  - also equivalent to "cut -N cards"
        // - "deal with increment N"
        //   - shuffling:         i(n+1) = ( i(n)*N ) % s   means i(n)*N = i(n+1) + s*x   where x in 0 - inf.
        //   - reverse shuffling: i(n)   = i(n+1)/N + ((i(n+1)%N)*((s/N-1)*N+s%N))%s
        //
        // a % b = a - a/b * b
        // i*N % s = i*N - (i*N)/s * s
        // i(n+1) = i*N - (i*N)/s * s
        //
        // with N = 7 and s = 10    -> s = 1*N + 3    -> s/N = 1 and s%N = 3
        // ---------------------
        // i(n+1)   -> i(n)
        // 0           0   --         0/N + 0   the second term is (i*N)%s for i [0 - N-1])  -> 0 * 3 + 3  -> step=(s/N-1)*N+s%N -> i(n+1)%N*step
        // 1           3   -->        0/N + 3
        // 2           6   --->       0/N + 6
        // 3           9              0/N + 9
        // 4           2              0/N + 12%10
        // 5           5              0/N + 15%10
        // 6           8              0/N + 18%10
        // 7           1              1/N + 0
        // 8           4              1/N + 3
        // 9           7              1/N + 6
        // serie of second terms is 0, 7, 4
        //
        // with N = 3 and s = 10   -> s = 3*N + 1    -> s/N = 3 and s%N = 1
        // ---------------------
        // i(n+1)   -> i(n)
        // 0           0   --         0/N + 0   -> 0  augmente de 7 en 7  -> 2 * 3 + 1
        // 1           7   -->        0/N + 7%10
        // 2           4   --->       0/N + 14%10
        // 3           1              3/N + 0   -> 1
        // 4           8              3/N + 7
        // 5           5              3/N + 4
        // 6           2              6/N + 0   -> 2
        // 7           9              6/N + 7
        // 8           6              6/N + 4
        // 9           3              9/N + 0   -> 3
        // serie of second terms is 0, 7, 4
        //
        // first dealing, get every N card
        // we can get (s/N) cards, and we are left with s%N cards after the last pick
        // so the next pick has the first one at N-s%N and then every N again
        //
        // how to solve that?
        // - what is my offset? -> my position - last clean pick position  (i/N)*N
        // - which row does this make me?
        // 
        // 1   2   3   4
        //   5   6   7   8
        //     9   10  11 12
        //
        // my position was given by i(n+1) = i(n)*N % s
        // meaning that i(n)*N = x*s + i(n+1)        where x is unknown from 0 to inf...
        // meaning that i(n) = x*s/N + i(n+1)/N





        public static long Part2(List<string> input)
        {
            var s      = 119315717514047L; // size of deck (number of cards)
            var repeat = 101741582076661L; // repetitions of shuffle process
            var finalI = 2020L;
            
            return FindInitialPosition(input, finalI, s, repeat);
        }
        
        public static long FindFinalPosition(List<string> instructions, long initialPosition, long sizeOfDeck, long repeat)
        {
            var s        = sizeOfDeck; // size of deck (number of cards)
            var initialI = initialPosition;

            // Formulas
            long DealIntoNewStack(long i) => s - i - 1;
            long ReverseDealIntoNewStack(long i) => DealIntoNewStack(i);
            long CutNCards(long i, long N) => N > 0 ? CutNCardsPositive(i, N) : CutNCardsNegative(i, N);
            long ReverseCutNCards(long i, long N) => CutNCards(i, -N);
            long CutNCardsPositive(long i, long N) => i < N ? i + s - N : i - N;
            long CutNCardsNegative(long i, long N) => i >= s+N ? i - (s+N) : i - N;
            long DealWithIncrementN(long i, long N) => (i*N)%s;
            //long ReverseDealWithIncrementN(long i, long N) => i/N + ((i%N)*((s/N-1)*N+s%N))%s;
            long ReverseDealWithIncrementN(long i, long N)
            {
                for (var c = 0; c < N; c++)
                    i = DealWithIncrementN(i, N);
                return i;
            };

            // 4/7 + ((4%7)*((10/7-1)*7+10%7))%7
            // 0   + ((4)*((1-1)*7+3))%7
            // 0   + ((4)*(3))%7


            var i = initialI;
            var iSeq = new List<long>() { i };
            var gSeq = new List<long>() { i };
            for(var r = 1; r <= repeat; r++)
            {
                //gSeq.Clear();
                //Console.WriteLine($"Repetition#{r}");
                foreach(var instruction in instructions)
                {
                    // Match possible instructions
                    if (instruction.StartsWith("cut"))
                    {
                        var N = Int32.Parse(instruction.Split(' ')[1]);
                        i = CutNCards(i, N);
                    }
                    else if (instruction.StartsWith("deal with"))
                    {
                        var N = Int32.Parse(instruction.Split(' ')[3]);
                        i = DealWithIncrementN(i, N);
                    }
                    else if (instruction.StartsWith("deal into"))
                    {
                        i = DealIntoNewStack(i);
                    }
                    else
                        throw new Exception("Unrecognized instruction: " + instruction);

                    gSeq.Add(i);
                }
                iSeq.Add(i);
                Console.WriteLine(i);

                // detect a 5x seq, starting from the end
                if ( r > 8 && iSeq[4] == i && iSeq[3] == iSeq[r-1] && iSeq[2] == iSeq[r-2] && iSeq[1] == iSeq[r-3] && iSeq[0] == iSeq[r-4])
                    Console.WriteLine($"!! Potential cycle detected starting at {r-4}");
            }

            Console.WriteLine("FindFinalPosition:   sequence is " + string.Join(" -> ", gSeq));
            return i;
        }

        public static long FindInitialPosition(List<string> instructions, long finalPosition, long sizeOfDeck, long repeat)
        {
            var s      = sizeOfDeck; // size of deck (number of cards)
            var finalI = finalPosition;

            // Formulas
            long DealIntoNewStack(long i) => s - i - 1;
            long ReverseDealIntoNewStack(long i) => DealIntoNewStack(i);
            long CutNCards(long i, long N) => N > 0 ? CutNCardsPositive(i, N) : CutNCardsNegative(i, N);
            long ReverseCutNCards(long i, long N) => CutNCards(i, -N);
            long CutNCardsPositive(long i, long N) => i < N ? i + s - N : i - N;
            long CutNCardsNegative(long i, long N) => i >= s+N ? i - (s+N) : i - N;
            long DealWithIncrementN(long i, long N) => (i*N)%s;
            //long ReverseDealWithIncrementN(long i, long N) => i/N + ((i%N)*((s/N-1)*N+s%N))%s;
            long ReverseDealWithIncrementN(long i, long N)
            {
                for (var c = 0; c < N; c++)
                    i = DealWithIncrementN(i, N);
                return i;
            };

            var i = finalI;
            var iSeq = new List<long>() { i };
            var gSeq = new List<long>() { i };
            for(var r = 1; r <= repeat; r++)
            {
                //gSeq.Clear();
                //Console.WriteLine($"Repetition#{r}");
                foreach(var instruction in instructions.Reverse<string>())
                {
                    // Match possible instructions
                    if (instruction.StartsWith("cut"))
                    {
                        var N = Int32.Parse(instruction.Split(' ')[1]);
                        i = ReverseCutNCards(i, N);
                    }
                    else if (instruction.StartsWith("deal with"))
                    {
                        var N = Int32.Parse(instruction.Split(' ')[3]);
                        i = ReverseDealWithIncrementN(i, N);
                    }
                    else if (instruction.StartsWith("deal into"))
                    {
                        i = ReverseDealIntoNewStack(i);
                    }
                    else
                        throw new Exception("Unrecognized instruction: " + instruction);
                    
                    gSeq.Add(i);
                }
                iSeq.Add(i);
                Console.WriteLine(i);

                // detect a 5x seq, starting from the end
                if ( r > 8 && iSeq[4] == i && iSeq[3] == iSeq[r-1] && iSeq[2] == iSeq[r-2] && iSeq[1] == iSeq[r-3] && iSeq[0] == iSeq[r-4])
                    Console.WriteLine($"!! Potential cycle detected starting at {r-4}");
            }

            Console.WriteLine("FindInitialPosition: sequence is " + string.Join(" -> ", gSeq.Reverse<long>()));
            return i;
        }

        public sealed class Shuffler
        {
            private List<int> _deck;

            public Shuffler(int cardsCount)
            {
                _deck = Enumerable.Range(0, cardsCount).ToList();
            }

            public IEnumerable<int> Deck => _deck;

            public int IndexOf(int cardNum) => _deck.IndexOf(cardNum);

            public void Shuffle(List<string> instructions)
            {
                foreach(var instruction in instructions)
                {
                    // Match possible instructions
                    if (instruction.StartsWith("cut"))
                    {
                        var cutN = Int32.Parse(instruction.Split(' ')[1]);
                        CutN(cutN);
                    }
                    else if (instruction.StartsWith("deal with"))
                    {
                        var dealN = Int32.Parse(instruction.Split(' ')[3]);
                        DealWithIncrement(dealN);
                    }
                    else if (instruction.StartsWith("deal into"))
                    {
                        DealIntoNewStack();
                    }
                    else
                        throw new Exception("Unrecognized instruction: " + instruction);
                }
            }

            private void DealIntoNewStack()
            {
                _deck.Reverse();
            }

            private void CutN(int n)
            {
                if (n>0)
                { // cut from beginning
                    for(var i = 0; i < n; i++)
                    {
                        var c = _deck[0];
                        _deck.RemoveAt(0);
                        _deck.Add(c);
                    }
                }
                else
                { // cut from end
                    var cs = _deck.Take(_deck.Count + n).ToList();
                    _deck.RemoveRange(0, _deck.Count + n);
                    _deck.AddRange(cs);
                }
            }

            private void DealWithIncrement(int n)
            {
                var s = _deck.Count;
                var newDeck = Enumerable.Repeat(-1, s).ToList();
                for(var i = 0; i < s; i++)
                    newDeck[(i*n)%s] = _deck[i];
                _deck = newDeck;
            }
        }
    }


    [TestFixture]
    internal class Day22Tests
    {
        [TestCase(1, new[] {0,3,6,9,2,5,8,1,4,7})]
        [TestCase(2, new[] {3,0,7,4,1,8,5,2,9,6})]
        [TestCase(3, new[] {6,3,0,7,4,1,8,5,2,9})]
        [TestCase(4, new[] {9,2,5,8,1,4,7,0,3,6})]
        public void Test1_1(int i, int[] expected)
        {
            var shuffler = new Day22.Shuffler(10);
            shuffler.Shuffle(Shuffles[i-1]);
            var actual = shuffler.Deck;
            Assert.AreEqual(expected, actual);
            // using position-based calculation
            foreach(var p in expected.OrderBy(p => p))
            {
                Console.WriteLine();

                // verify FindFinalPosition (part 1 fast alternative)
                var expected2 = shuffler.IndexOf(p);
                var actual2   = Day22.FindFinalPosition(Shuffles[i-1], p, expected.Length, 1L);
                Assert.AreEqual(expected2, actual2);

                // verify FindInitialPosition (part 2)
                var actual3 = Day22.FindInitialPosition(Shuffles[i-1], actual2, expected.Length, 1L);
                Assert.AreEqual(p, actual3);
            }
        }

        [Test]
        public void Test2()
        {
            // check the reverse algorithm on the 10,007 deck with the input instructions
            var instructions = File.ReadAllLines(Path.Combine("..", "..", "..", "input", "day22.txt")).ToList();
            for(var p = 0; p < 10007; p++)
            {
                var final   = Day22.FindFinalPosition(instructions, p, 10007, 1);
                var initial = Day22.FindInitialPosition(instructions, final, 10007, 1);
                Assert.AreEqual(p, initial);
            }
        }

        private readonly List<List<string>> Shuffles = new List<List<string>> {
            // Shuffle 1
            new List<string> {
                "deal with increment 7",
                "deal into new stack",
                "deal into new stack"
            },
            // Shuffle 2
            new List<string> {
                "cut 6",
                "deal with increment 7",
                "deal into new stack"
            },
            // Shuffle 3
            new List<string> {
                "deal with increment 7",
                "deal with increment 9",
                "cut -2"
            },
            // Shuffle 4
            new List<string> {
                "deal into new stack",
                "cut -2",
                "deal with increment 7",
                "cut 8",
                "cut -4",
                "deal with increment 7",
                "cut 3",
                "deal with increment 9",
                "deal with increment 3",
                "cut -1"
            }
        };
    }
}
