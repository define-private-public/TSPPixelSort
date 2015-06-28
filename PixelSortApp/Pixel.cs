using System;
using System.Linq;
using System.Drawing;
using PixelSort;

namespace PixelSortApp
{
    public struct Pixel
    {
        public Pixel(Color rgb,YUV yuv,int originalLocation)
        {
            Y = yuv.Y;
            U = yuv.U;
            V = yuv.V;

            R = rgb.R;
            G = rgb.G;
            B = rgb.B;

            OriginalLocation = originalLocation;
        }
        public double Y;
        public double U;
        public double V;

        public byte R;
        public byte G;
        public byte B;

        public double OriginalLocation;

        public static double GetDistance(Pixel a,Pixel b)
        {
            //find euclidian distance between each
            var dY = a.Y - b.Y;
            var dU = a.U - b.U;
            var dV = a.V - b.V;
            var dL = a.OriginalLocation - b.OriginalLocation;
           


            return Math.Sqrt(dY*dY + dU*dU + dV*dV +dL*dL);
        }
    }
}