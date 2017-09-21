using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coder
{
    internal class NormalRandom : Random
    {
        private double prevSample = double.NaN;

        protected override double Sample()
        {
            // есть предыдущее значение? возвращаем его
            if (!double.IsNaN(prevSample))
            {
                double result = (double)prevSample;
                prevSample = double.NaN;
                return result;
            }

            //polar method из википедии
            double u, v, s;
            do
            {
                u = 2 * base.Sample() - 1;
                v = 2 * base.Sample() - 1; // [-1, 1)
                s = u * u + v * v;
            }
            while (u <= -1 || v <= -1 || s >= 1 || s == 0);
            double r = Math.Sqrt(-2 * Math.Log(s) / s);

            prevSample = r * v;
            return r * u;
        }
    }
}
