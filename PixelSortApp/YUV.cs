using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelSort
{
    public class YUV
    {
        public double Y;
        public double U;
        public double V;

        double Wr = 0.299;
        double Wg = 0.587;
        double Wb = 0.114;

        double UMax = 0.436;
        double VMax = 0.615;

        public int R
        {
            get
            {
                return Clamp((int)(Y + 1.14 * V), 255, 0);
            }
        }

        public int G
        {
            get
            {
                return Clamp((int)(Y - 0.395 * U - 0.581 * V), 255, 0);
            }
        }
        public int B
        {
            get
            {
                return Clamp((int)(Y + 2.033 * U), 255, 0);
            }
        }

        public YUV(Color c)
        {
            double R = c.R;
            double G = c.G;
            double B = c.B;


            Y = Wr * R + Wg * G + Wb * B;

            U = UMax * (B - Y) / (1 - Wr);

            V = VMax * (R - Y) / (1 - Wr);
        }

        private int Clamp(int input, int max, int min)
        {
            if (input > max)
                return max;
            if (input < min)
                return min;

            return input;
        }

    }
}
