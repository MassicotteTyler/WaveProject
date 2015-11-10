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

        public double[] apply_filter(double[] samples, double[] filter)
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
    }
}
