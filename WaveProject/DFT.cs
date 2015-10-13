using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Numerics;

namespace WaveProject
{
    class DFT
    {
        public class Complex
        {
            public double real;
            public double ima;

            public Complex()
            {
                real = 0;
                ima = 0;
            }

            public Complex(double r, double i)
            {
                real = r;
                ima = i;
            }

            public static Complex Polar(double r, double i)
            {
                double pythag = Math.Sqrt(Math.Pow(r, 2) + (Math.Pow(i, 2)));
                double angle = Math.Atan(i / r);
                return new Complex(pythag * Math.Cos(angle), pythag * Math.Sin(angle));
            }
        }

        public Complex[] Dft(Complex[] samples)
        {
            int N = samples.Length;

            Complex[] output = new Complex[N];

            double arg = -2.0 * Math.PI / (double)N;

            for (int k = 0; k < N; k++)
            {
                double sumreal = 0;
                double sumimag = 0;
                for (int t = 0; t < N; t++)
                {
                    double angle = 2 * Math.PI * t * k / N;
                    sumreal += samples[t].real * Math.Cos(angle) + samples[t].ima * Math.Sin(angle);
                    sumimag += -samples[t].real * Math.Sin(angle) + samples[t].ima * Math.Cos(angle);
                }
                output[k].real = sumreal;
                output[k].ima = sumimag;
            }
            return output;
            
        }

        //Calculates the length of the complex vector using pythag
        public double calc_length(Complex num)
        {
            double real = num.real;
            double ima = num.ima;

            double length = Math.Sqrt(Math.Pow(real, 2) + Math.Pow(ima, 2));
            return length;
        }
        public double[] _dft(double[] samples, int num_samples, out double[] power)
        {
            int k, n;
            double[] x = samples; 
            double[] Xre = new double[num_samples];
            double[] Xim = new double[num_samples];
            double[] P = new double[num_samples];


            for (k = 0; k < num_samples; ++k)
            {
                Xre[k] = 0;
                Xim[k] = 0;
                for (n = 0; n < num_samples; ++n)
                {
                    Xre[k] += (x[n] * Math.Cos(n * k * 6.2832 / num_samples));
                    Xim[k] -= (x[n] * Math.Sin(n * k * 6.2832 / num_samples));
                }

                P[k] = (Xre[k] * Xre[k]) + (Xim[k] * Xim[k]);

            }
            power = P;
            return Xre;
        }
    }
}
