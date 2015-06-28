using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Genetic;
using TSP;

namespace PixelSortApp
{
    class GeneticSorter : ISorter
    {
        public Pixel[] FindPath(Pixel[] map,SortOptions options)
        {
            TSPFitnessFunction fitnessFunction = new TSPFitnessFunction(map);
            Population population = new Population(100, new TSPChromosome(map), fitnessFunction, options.GeneticMode);

            for (int i = 0; i < options.Iterations; i++)
            {
                population.RunEpoch();
            }

            ushort[] bestValue = ((PermutationChromosome)population.BestChromosome).Value;

            Pixel[] path = new Pixel[map.Length];

            for (int j = 0; j < map.Length; j++)
            {
                path[j] = map[bestValue[j]];
            }
            return path;
        }
    }
}
