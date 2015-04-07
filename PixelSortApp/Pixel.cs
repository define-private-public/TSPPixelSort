using System;
using System.Linq;

namespace PixelSortApp
{
    public struct Pixel
    {
        public double Y;
        public double U;
        public double V;

        public double OriginalLocation;

        public static double GetDistance(Pixel a,Pixel b)
        {
            //find euclidian distance between each
            double[] dimensions = {
                a.Y - b.Y,
                a.U - b.U,
                a.V - b.V,
                a.OriginalLocation - b.OriginalLocation
            };


            return Math.Sqrt(dimensions.Sum(i=>i * i));
        }
    }
}