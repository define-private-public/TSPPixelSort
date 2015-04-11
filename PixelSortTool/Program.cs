using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelSortApp;

namespace PixelSortTool
{
    class Program
    {
        private static int currentI = 0;
        static void Main(string[] args)
        {
            //make frames of an animation
            for (int i = 0; i <= 20; i++)
            {
                //currentI = i;
                //Sorter t = new Sorter(1, 8, SortMode.NearestNeighbour, i,false,null);

                //t.OnFinish += t_OnFinish;
                //t.OnProgressUpdate += t_OnProgressUpdate;

                //t.Sort(new Bitmap("input.jpg"));
            }
        }

        static void t_OnProgressUpdate(double progress, Bitmap update)
        {
        }

        static void t_OnFinish(Bitmap output)
        {
            output.Save((20 - currentI) + ".png");
        }
    }
}
