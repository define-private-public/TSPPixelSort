using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelSortApp
{
    class RandomSorter : ISorter
    {
        static Random r = new Random();
        public Pixel[] FindPath(Pixel[] map, SortOptions options)
        {
            lock(r)
            Shuffle(map);

            return map;
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
