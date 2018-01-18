using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DiffiHelman
{
    public static class NumberGenerator
    {
        private static Random generator = new Random();

        public static int SimpleNumber()
        {
            int result;
            do
            {
                result = generator.Next(2, 20);
            } while (!IsPrime(result));
            return result;
        }

        public static int NaturalNumber()
        {
            return generator.Next(2, 20);
        }

        public static bool IsPrime(int number)
        {
            if (number == 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int) Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }

            return true;
        }

        public static long? GetRoot(int p)
        {
            for (int i = 0; i < p; i++)
            {
                if(!IsPRoot(p, i)) continue;
                var candidat = FullRoot(i, p);
                if (candidat != null)
                    return candidat;
            }
            return null;
        }

        private static long? FullRoot(int g, int p)
        {
            var el = Eiler(p);
            var set = new List<long>();
            for (int i = 1; i < el; i += 2)
            {
                var t = Convert.ToInt64(Math.Pow(g, i)) % p;
                if(IsPrices(t, el))
                    set.Add(t);
            }
            return set.Any() ? set[generator.Next(0, set.Count - 1)] : (long?) null;
        }
        
        private static long Gcd(long a, long b) {
            if (b == 0)
                return Math.Abs(a);
            return Gcd(b, a % b);
        }

        private static bool IsPrices(long a, long b)
        {
            return Math.Abs(Gcd(Math.Abs(a), Math.Abs(b))) == 1;
        }

        private static bool IsPRoot(long p, long a)
        {
            if (a == 0 || a == 1)
                return false;
            long last = 1;
            var set = new HashSet<long>();
            for (long i = 0; i < p - 1; i++)
            {
                last = (last * a) % p;
                if (set.Contains(last)) // Если повтор
                    return false;
                set.Add(last);
            }
            
            return true;
        }

        private static int Eiler(int n)
        {
            int result = n;
            for (int i = 2; i * i <= n; ++i)
                if (n % i == 0)
                {
                    while (n % i == 0)
                        n /= i;
                    result -= result / i;
                }
            if (n > 1)
                result -= result / n;
            return result;
        }
    }
}