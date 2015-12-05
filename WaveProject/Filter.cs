using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveProject
{
    /********************************************************
    * The Filtering class houses the methods for creating and 
    * applying filters. As well as the methods that apply window
    * functions to samples.
    **********************************************************/
    class Filter
    {

        /********************************************************
        * Create filter takes in a double array of samples and an
        * int to specify where to make the cut off point for the
        * lowpass filter. It then creates an array of complex numbers
        * that have values of 1's or 0's. One to specify to keep that
        * corresponding frequency. 0 to remove the frequency. The 
        * array of complex numbers are then passed through iDft to get
        * the weights of the filter. The filter is then returned.
        **********************************************************/
        public static double[] createFilter(int selection, double[] samples)
        {
            double[] filter;
            Complex[] temp = new Complex[samples.Length];

            //Always keep the DC component
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

        public static double[] rectangle_window(double[] samples)
        {
            double[] output = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                output[i] = samples[i] * 1;
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
