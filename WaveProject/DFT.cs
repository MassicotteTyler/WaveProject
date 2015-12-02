using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Numerics;

namespace WaveProject
{
    class DFT
    {


        public Complex[] Dft(double[] samples)
        {
            int N = samples.Length;
            Thread t1, t2;
            //Setup threadpool que
            Complex[] output = new Complex[N];
            Complex[] temp1 = new Complex[N / 2];
            Complex[] temp2 = new Complex[N / 2];

            var subDft1 = new Action<double[]>(samp => 
            {
                for (int k = 0; k < N / 2 + 1; k++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t = 0; t < N / 2 + 1; t++)
                    {
                        sumreal += samples[t] * Math.Cos(t * k * -6.2832 / samples.Length);
                        sumimag -= samples[t] * Math.Sin(t * k * -6.2832 / samples.Length);
                    }
                    output[k] = new Complex(sumreal, sumimag);
                }
            });
            
            var subDft2 = new Action<double[]>(samp =>
            {
                for (int k = N / 2 + 1; k < N; k++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t = N / 2 + 1; t < N; t++)
                    {
                        sumreal += samples[t] * Math.Cos(t * k * -6.2832 / samples.Length);
                        sumimag -= samples[t] * Math.Sin(t * k * -6.2832 / samples.Length);
                    }
                    output[k] = new Complex(sumreal, sumimag);
                }
            });


            t1 = new Thread(() => subDft1(samples), 1024 * 1024);
            t2 = new Thread(() => subDft2(samples), 1024 * 1024);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            return output;

        }


        public double[] iDft(Complex[] input)
        {
            int N = input.Length;

            double[] output = new double[N];

            double arg = 2.0 * Math.PI / (double)N;

            var subDft1 = new Action(() =>
            {
                for (int k = 0; k < N / 2 + 1; k++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t = 0; t < N / 2 + 1; t++)
                    {
                        sumreal += input[t].real * Math.Cos(t * k * -6.2832 / N);
                        sumimag += input[t].ima * Math.Sin(t * k * -6.2832 / N);
                    }
                    output[k] = (sumreal - sumimag) / N;
                }
            });

            var subDft2 = new Action(() =>
            {
                for (int k = N / 2 + 1; k < N; k++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t = N / 2 + 1; t < N; t++)
                    {
                        sumreal += input[t].real * Math.Cos(t * k * -6.2832 / N);
                        sumimag += input[t].ima * Math.Sin(t * k * -6.2832 / N);
                    }
                    output[k] = (sumreal - sumimag) / N;
                }
            });

            Thread t1 = new Thread(() => subDft1(), 1024 * 1024);
            Thread t2 = new Thread(() => subDft2(), 1024 * 1024);

            t1.Start();
            t2.Start();



            t1.Join();
            t2.Join();

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
                    Xre[k] += (x[n] * Math.Cos(n * k * -6.2832 / num_samples));
                    Xim[k] -= (x[n] * Math.Sin(n * k * -6.2832 / num_samples));
                }

                P[k] = Math.Sqrt((Xre[k] * Xre[k]) + (Xim[k] * Xim[k]));

            }
            power = P;
            return Xre;
        }

        //remove 
        public double[] createFilter(int selection, double[] samples)
        {
            double[] filter;
            Complex[] temp = new Complex[samples.Length];

            temp[0] = new Complex(1, 1);
            int i, k;
            for (i = 1, k = temp.Length - 1; i < selection; i++, k--)
            {
                temp[i] = new Complex(1, 1);
                temp[k] = new Complex(1, 1) ;
            }
            for (i = selection; i < temp.Length / 2 + 1; i++, k--)
            {
                temp[i] = new Complex(0, 0);
                temp[k] = new Complex(0, 0) ;
            }

            filter = iDft(temp);

            return filter;
        }

    }
}
