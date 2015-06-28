using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelSortApp
{
    class NearestNeighbour : ISorter
    {
        private Pixel[] map;
        private bool[] visited;
        private List<int> path = new List<int>();
        private int numVisited = 1;
        private int cityCount;
        //Find a path along a pixel array using nearest-neighbour TSP solution
        public Pixel[] FindPath(Pixel[] _map,SortOptions options)
        {
            map = _map;
            visited = new bool[_map.Length];

            //register the first city as visited
            visited[0] = true;
            path.Add(0);

            cityCount = map.Length;

            //find a path
            recursivePath(0);

            //construct pixel array from path
            Pixel[] citiesPath = new Pixel[map.Length];

            int i = 0;
            foreach (var p in path)
            {
                citiesPath[i] = map[p];
                i++;
            }

            return citiesPath;
        }

        public void recursivePath(int cityNum)
        {
            if (numVisited >= cityCount)
                return;

            int bestCity = 0;
            double bestDistance = double.PositiveInfinity;

            for (int cityB = 0; cityB < map.Length; cityB++)
            {
                if (!visited[cityB])
                {
                    double pathLength = Pixel.GetDistance(map[cityNum], map[cityB]);

                    if (pathLength < bestDistance)
                    {
                        bestCity = cityB;
                        bestDistance = pathLength;
                    }

                }
            }

            path.Add(bestCity);
            visited[bestCity] = true;

            numVisited++;

            recursivePath(bestCity);
        }
    }
}
