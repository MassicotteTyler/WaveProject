using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace WaveProject
{
    class DFT
    {
        public Complex[] Dft(object samples, int n)
        {
            double t = 0;
            double c = 0;
            double s = 0;
            double f = 0;
            Complex[] output = new Complex[n];
            double[] real = new double[n];
            double[] ima = new double[n];

            for (f = 0; f < n; f++)
            {
                for (t = 0; t < n; t++)
                {
                    real[(int)f] += (double) Math.Cos(2 * Math.PI * t * (f / n));
                    ima[(int)f] += (double)Math.Sin(2 * Math.PI * t * (f / n));
                }
                output[(int)f] = new Complex(real[(int)f], ima[(int)f]);
            }
            return output;
        }

        //Calculates the length of the complex vector using pythag
        public double calc_length(Complex num)
        {
            double real = num.Real;
            double ima = num.Imaginary;

            double length = Math.Sqrt(Math.Pow(real, 2) + Math.Pow(ima, 2));
            return length;
        }
        public void _dft(byte[] samples, int num_samples)
        {
            int k, i, n;
            Random rnd = new Random();
            float[] x = new float[num_samples];
            float[] Xre = new float[num_samples];
            float[] Xim = new float[num_samples];
            float[] P = new float[num_samples];

            for (n = 0; n < 16; n++) x[n] = (float)(2.0 * rnd.Next(-2, 2));

            for (k = 0; k < num_samples; ++k)
            {
                Xre[k] = 0;
                Xim[k] = 0;
                for (n = 0; n < num_samples; ++n)
                {
                    Xre[k] += (float)(x[n] * Math.Cos(n * k * 6.2832 / num_samples));
                    Xim[k] -= (float)(x[n] * Math.Sin(n * k * 6.2832 / num_samples));
                }

                P[k] = Xre[k] * Xre[k] + Xim[k] * Xim[k];

            }
        }
    }
}
