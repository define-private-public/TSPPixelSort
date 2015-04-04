using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelSortApp
{
    class NearestNeighbour
    {
        private double[,] map;
        private bool[] visited;
        private List<int> path = new List<int>();
        private int numVisited = 1;

        public NearestNeighbour(double[,] _map)
        {
            map = _map;
            visited = new bool[_map.Length];
        }

        public double[,] FindPath()
        {
            visited[0] = true;
            recursivePath(0);

            double[,] citiesPath = new double[map.Length,4];

            int i = 0;
            foreach (var p in path)
            {
                citiesPath[i, 0] = map[p, 0];
                citiesPath[i, 1] = map[p, 1];
                citiesPath[i, 2] = map[p, 2];
                citiesPath[i, 3] = map[p, 3];
                i++;
            }

            return citiesPath;

        }

        public void recursivePath(int cityNum)
        {
            if (numVisited >= map.Length / 4)
                return;

            int bestCity = 0;
            double bestDistance = double.PositiveInfinity;

            for (int cityB = 0; cityB < map.Length / 4; cityB++)
            {
                if (!visited[cityB])
                {
                    double dx = map[cityNum, 0] - map[cityB, 0];
                    double dy = map[cityNum, 1] - map[cityB, 1];
                    double dz = map[cityNum, 2] - map[cityB, 2];
                    double dn = map[cityNum, 3] - map[cityB, 3];
                    double pathLength = Math.Sqrt(dx*dx + dy*dy + dz*dz + dn*dn);

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
