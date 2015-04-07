using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Genetic;
using PixelSort;
using TSP;
using System.Drawing;
using System.Threading;

namespace PixelSortApp
{
    public delegate void OnProgressUpdateEvent(double progress, Bitmap update);
    public delegate void OnFinishEvent(Bitmap output);
    public class Sorter
    {
        public event OnProgressUpdateEvent OnProgressUpdate;
        public event OnFinishEvent OnFinish;
        static List<Color> SortBuffer(Color[] buffer, int iterations, SortMode mode,int movementScale = 1)
        {
            int citiesCount = buffer.Count();

            var map = new Pixel[citiesCount];
            int c = 0;
            foreach (var color in buffer)
            {
                var yuv = new YUV(color);

                map[c].Y = yuv.Y;
                map[c].U = yuv.U;
                map[c].V = yuv.V;
                map[c].OriginalLocation = c * movementScale;
                c++;
            }

            // path
            Pixel[] path = new Pixel[citiesCount];

            switch (mode)
            {
                case SortMode.Genetic:
                    // create fitness function
                    TSPFitnessFunction fitnessFunction = new TSPFitnessFunction(map);
                    // create population
                    Population population = new Population(100,
                        new TSPChromosome(map),
                        fitnessFunction,
                        new RankSelection());


                    for (int i = 0; i < iterations; i++)
                    {
                        population.RunEpoch();
                    }

                    ushort[] bestValue = ((PermutationChromosome)population.BestChromosome).Value;

                    for (int j = 0; j < citiesCount; j++)
                    {
                        path[j] = map[bestValue[j]];
                        path[j] = map[bestValue[j]];
                        path[j] = map[bestValue[j]];
                    }
                    break;
                case SortMode.NearestNeighbour:
                    var nn = new NearestNeighbour(map);

                    path = nn.FindPath();


                    break;
            }


            var outBuffer = new List<Color>();
            for (int i = 0; i < citiesCount; i++)
            {
                YUV yuv = new YUV { Y = path[i].Y, U = path[i].U, V = path[i].V };

                outBuffer.Add(Color.FromArgb(yuv.R, yuv.G, yuv.B));
            }
            return outBuffer;


        }

        private Color[,] inputArray;
        private Color[,] outputArray;
        private int progress;
        private int bWidth;
        private int bHeight;

        public void SortVertical(Bitmap b, int iterations, int chunkNum, SortMode mode,int moveScale)
        {
            inputArray = bitmapToArray(b);
            outputArray = bitmapToArray(b);

            bHeight = b.Height;
            bWidth = b.Width;

            int chunkSize = (int)Math.Ceiling((double)b.Height / chunkNum);

            Timer updater = new Timer(OnUpdate,null,1000,1000);

            Parallel.For(0, b.Width, x =>
            {
                for (int yc = 0; yc < chunkNum; yc++)
                {
                    int yoffset = yc * chunkSize;

                    var samples = new List<Color>();

                    for (int y = 0; y < chunkSize; y++)
                    {
                        if (y + yoffset < bHeight)
                            samples.Add(inputArray[x, y + yoffset]);//locking
                        else
                            break;
                    }
                    
                    if (samples.Count > 2)
                    {
                        var buffer = samples.ToArray();

                        int y2 = 0;
                        foreach (var color in SortBuffer(buffer, iterations, mode, moveScale))
                        {
                            if (y2 + yoffset < bHeight)
                                outputArray[x, y2 + yoffset] = color;//lots of locking
                            else
                                break;
                            y2++;
                        }

                    }
                }
                progress++;


            });

            updating = true;//stop updater from trying to do things

            OnFinish(arrayToBitmap(outputArray));
        }

        private int lastProgress = 0;
        private bool updating = false;

        private void OnUpdate(object data)
        {
            if (progress > lastProgress && !updating)
            {
                updating = true;
                OnProgressUpdate((double) progress/bWidth, arrayToBitmap(outputArray));
                lastProgress = progress;
                updating = false;
            }
        }

        Bitmap arrayToBitmap(Color[,] array)
        {
            Bitmap b = new Bitmap(array.GetLength(0), array.GetLength(1));


            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    b.SetPixel(x, y, array[x, y]);
                }
            }

            return b;
        }
        Color[,] bitmapToArray(Bitmap b)
        {
            var array = new Color[b.Width, b.Height];

            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    array[x, y] = b.GetPixel(x, y);
                }
            }

            return array;
        }

    }

    public enum SortMode
    {
        Genetic,
        NearestNeighbour
    }

    
}
