using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Genetic;
using PixelSort;
using TSP;
using System.Drawing;

namespace PixelSortApp
{
    internal delegate void OnProgressUpdateEvent(double percentile,Bitmap update);
    internal delegate void OnFinishEvent(Bitmap output);
    class Sorter
    {
        public event OnProgressUpdateEvent OnProgressUpdate;
        public event OnFinishEvent OnFinish;
        static List<Color> SortBuffer(Color[] buffer, int iterations)
        {
            int citiesCount = buffer.Count();

            var map = new double[citiesCount, 4];
            int c = 0;
            foreach (var color in buffer)
            {
                var yuv = new YUV(color);

                map[c, 0] = yuv.Y;
                map[c, 1] = yuv.U;
                map[c, 2] = yuv.V;
                map[c, 3] = Array.IndexOf(buffer, color);
                c++;
            }


            // create fitness function
            TSPFitnessFunction fitnessFunction = new TSPFitnessFunction(map);
            // create population
            Population population = new Population(100,
                    new TSPChromosome(map),
                    fitnessFunction,
                    new RankSelection());

            // path
            double[,] path = new double[citiesCount + 1, 4];

            for (int i = 0; i < iterations; i++)
            {
                population.RunEpoch();
            }

            ushort[] bestValue = ((PermutationChromosome)population.BestChromosome).Value;
            var outBuffer = new List<Color>();

            for (int j = 0; j < citiesCount; j++)
            {
                path[j, 0] = map[bestValue[j], 0];
                path[j, 1] = map[bestValue[j], 1];
                path[j, 2] = map[bestValue[j], 2];

                YUV yuv = new YUV { Y = path[j, 0], U = path[j, 1], V = path[j, 2] };

                outBuffer.Add(Color.FromArgb(yuv.R, yuv.G, yuv.B));
            }
            path[citiesCount, 0] = map[bestValue[0], 0];
            path[citiesCount, 1] = map[bestValue[0], 1];
            path[citiesCount, 2] = map[bestValue[0], 2];



            return outBuffer;


        }

        public void SortVertical(Bitmap b, int iterations,int chunkNum)
        {
            Bitmap o = (Bitmap)b.Clone();

            int chunkSize = (int)Math.Ceiling((double)b.Height/chunkNum);

            for (int x = 0; x < b.Width; x++)
            {
                for (int yc = 0; yc < chunkNum; yc++)
                {
                    int yoffset = yc*chunkSize;

                    var samples = new List<Color>();

                    for (int y = 0; y < chunkSize; y++)
                    {
                        if (y + yoffset < b.Height)
                            samples.Add(b.GetPixel(x, y + yoffset));
                        else
                            break;
                    }

                    if (samples.Count > 2)
                    {
                        var buffer = samples.ToArray();

                        int y2 = 0;
                        foreach (var color in SortBuffer(buffer, iterations))
                        {
                            if (y2 + yoffset < b.Height)
                                o.SetPixel(x, y2 + yoffset, color);
                            else
                                break;
                            y2++;
                        }

                    }
                }

                OnProgressUpdate((double)x / b.Width, o);

            }
            OnFinish(o);
        }
    }
}
