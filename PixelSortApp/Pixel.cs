using System;
using System.Linq;

namespace PixelSortApp
{
    public struct Pixel
    {
        public double Y;
        public double U;
        public double V;

        public int R;
        public int G;
        public int B;

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