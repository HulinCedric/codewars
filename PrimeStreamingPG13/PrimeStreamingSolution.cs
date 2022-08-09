using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Codewars.PrimeStreamingPG13;

public class PrimesTest
{
    [Fact]
    public void Test_0_10()
        => Test(0, 10, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29);

    [Fact]
    public void Test_10_10()
        => Test(10, 10, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71);

    [Fact]
    public void Test_100_10()
        => Test(100, 10, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601);

    [Fact]
    public void Test_1000_10()
        => Test(1000, 10, 7927, 7933, 7937, 7949, 7951, 7963, 7993, 8009, 8011, 8017);

    [Fact]
    public void Test_10000_10()
        => Test(10000, 10, 104743, 104759, 104761, 104773, 104779, 104789, 104801, 104803, 104827, 104831);

    [Fact]
    public void Test_100000_10()
        => Test(100000, 10, 1299721, 1299743, 1299763, 1299791, 1299811, 1299817, 1299821, 1299827, 1299833, 1299841);

    [Fact]
    public void Test_1000000_10()
        => Test(
            1_000_000,
            10,
            15485867,
            15485917,
            15485927,
            15485933,
            15485941,
            15485959,
            15485989,
            15485993,
            15486013,
            15486041);
    
    [Fact]
    public void Test_10000000_10()
        => Test(
            5_000_000,
            10,
            15485867,
            15485917,
            15485927,
            15485933,
            15485941,
            15485959,
            15485989,
            15485993,
            15486013,
            15486041);

    private void Test(int skip, int limit, params int[] expect)
    {
        var found = Primes.Stream().Skip(skip).Take(limit).ToArray();
        found.Should().BeEquivalentTo(expect);
    }
}

public class Primes
{
    public static IEnumerable<int> Stream2()
    {
        var primes = new List<int> { 2 };

        yield return 2;

        var nextPrime = 3;
        while (true)
        {
            var sqrt = (int)Math.Sqrt(nextPrime);
            var isPrime = true;
            for (var i = 0; primes[i] <= sqrt; i++)
                if (nextPrime % primes[i] == 0)
                {
                    isPrime = false;
                    break;
                }

            if (isPrime)
            {
                yield return nextPrime;
                primes.Add(nextPrime);
            }

            nextPrime += 2;
        }
    }

    public static IEnumerable<int> Stream()
    {
        var sieve = new BitArray(1<<28);
        for (var p = 2; p < sieve.Length; p++)
        {
            if (sieve[p]) continue;
            yield return p;
            for (int i = p * 2; i < sieve.Length; i += p)
                sieve.Set(i, true);
        }
    }
    
    public static IEnumerable<int> Stream3()
        => new PrimeEnumerator();
}

internal class PrimeEnumerator : IEnumerable<int>
{
    private int max = 2;
    private int next = 2;
    private BitArray sieve = new(100);

    public IEnumerator<int> GetEnumerator()
    {
        while (true)
        {
            var p = NextClearBit(next);
            if (p > max)
            {
                var m = max * 2;
                for (var i = 2; i <= max; i++)
                    if (!sieve[i])
                        for (var j = 2 * i; j <= m; j += i)
                            SetWithAutoGrow(j);
                max = m;
                p = NextClearBit(next);
            }

            next = p + 1;
            yield return p;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private int NextClearBit(int next)
    {
        for (var i = next; i < sieve.Length; i++)
            if (!sieve[i])
                return i;
        return -1;
    }

    private void SetWithAutoGrow(int index)
    {
        if (index < sieve.Length)
        {
            sieve[index] = true;
        }
        else
        {
            var tmp = new BitArray(index + index / 2);
            for (var i = 0; i < sieve.Length; i++)
                tmp[i] = sieve[i];
            sieve = tmp;
            sieve[index] = true;
        }
    }
}