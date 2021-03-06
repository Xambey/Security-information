﻿using System;
using System.Collections.Generic;

namespace RsaEncryption
{
    internal class Primes
    {
        private const int MaxValue = 25000;
        private readonly bool[] isPrime = new bool[MaxValue + 1];
        private readonly List<int> primes = new List<int>();

        internal Primes()
        {
            for (var i = 2; i <= MaxValue; i++)
            {
                if (!isPrime[i])
                {
                    primes.Add(i);
                    for (var j = i * i; j <= MaxValue; j += i)
                    {
                        isPrime[j] = true;
                    }
                }
            }
        }

        internal Key GetKey()
        {
            var end = primes.Count - 1;
            var start = end / 4;
            var random = new Random();
            var primeOne = primes[random.Next(start, end)];
            var primeTwo = primes[random.Next(start, end)];
            while (primeTwo == primeOne)
            {
                primeTwo = primes[random.Next(start, end)];
            }
            return new Key(primeOne, primeTwo, primes);
        }
    }
}
