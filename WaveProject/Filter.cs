using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveProject
{
    class Filter
    {

        public Filter()
        {

        }

        public static double[] createFilter(int selection, double[] samples)
        {
            double[] filter;
            Complex[] temp = new Complex[samples.Length];

            temp[0] = new Complex(1, 1);
            int i, k;
            for (i = 1, k = temp.Length - 1; i < selection; i++, k--)
            {
                temp[i] = new Complex(1, 1);
                temp[k] = new Complex(1, 1);
            }
            for (i = selection; i < temp.Length / 2 + 1; i++, k--)
            {
                temp[i] = new Complex(0, 0);
                temp[k] = new Complex(0, 0);
            }

            filter = DFT.iDft(temp);

            return filter;
        }

        public static double[] apply_filter(double[] samples, double[] filter)
        {
            double[] result = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                double temp = 0;
                for (int j = 0; j < filter.Length; j++)
                {
                    if ((i + j) < (samples.Length - 1))
                        temp += samples[i + j] * filter[j];
                    else
                        temp += 0;
                }
                result[i] = temp;
            }
            return result;
        }

        public static double[] hanning_window(double[] samples)
        {
            double[] output = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                output[i] = 0.54 + (0.46 * Math.Cos(Math.PI * samples[i]));
            }

            return output;
        }

        public static double[] triangle_window(double[] samples)
        {
            double[] output = new double[samples.Length];
            int N = samples.Length;
            for (int i = 0; i < samples.Length; i++)
            {
                output[i] = 1 - Math.Abs(((samples[i] - ((N - 1) / 2)) /
                                 (N / 2)));
            }

            return output;
        }
    }
}
