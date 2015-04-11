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

        private Color[][] inputArray;
        private Color[][] outputArray;
        private int progress;
        private int bWidth;
        private int bHeight;

        private int iterations;
        private int chunkSize;
        private SortMode mode;
        private double moveScale;
        private bool biDirectional;
        private ISelectionMethod geneticMode;


        private int lastProgress = 0;
        private bool updating = false;

        public Sorter(int iterations, int chunkSize, SortMode mode, double moveScale, bool biDirectional, ISelectionMethod geneticMode)
        {
            this.iterations = iterations;
            this.chunkSize = chunkSize;
            this.mode = mode;
            this.moveScale = moveScale;
            this.biDirectional = biDirectional;
            this.geneticMode = geneticMode;
        }
        private List<Color> SortBuffer(Color[] buffer)
        {
            int citiesCount = buffer.Count();

            var map = new Pixel[citiesCount];
            int c = 0;
            foreach (var color in buffer)
            {
                map[c].R = color.R;
                map[c].G = color.G;
                map[c].B = color.B;

                //generate yuv colors for sorting purposes
                var yuv = new YUV(color);

                map[c].Y = yuv.Y;
                map[c].U = yuv.U;
                map[c].V = yuv.V;
                map[c].OriginalLocation = c * moveScale;
                c++;
            }

            Pixel[] path = new Pixel[citiesCount];

            switch (mode)
            {
                case SortMode.Genetic:
                    TSPFitnessFunction fitnessFunction = new TSPFitnessFunction(map);
                    Population population = new Population(100, new TSPChromosome(map), fitnessFunction, geneticMode);

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
                case SortMode.Downsample:
                case SortMode.NearestNeighbour:
                    var nn = new NearestNeighbour(map);

                    path = nn.FindPath();
                    break;
            }


            var outBuffer = new List<Color>();
            for (int i = 0; i < citiesCount; i++)
            {
                outBuffer.Add(Color.FromArgb(path[i].R, path[i].G, path[i].B));
            }
            return outBuffer;
        }


        public void Sort(Bitmap b)
        {
            var bitmap = SortVertical(b);

            if (biDirectional)
            {
                //rotate chunkSize too if neccessary
                if (chunkSize == bHeight)
                    chunkSize = bWidth;

                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                bitmap = SortVertical(bitmap);
                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }

            OnFinish(bitmap);
        }

        private Bitmap SortVertical(Bitmap b)
        {
            //allow updater to continue
            updating = false;

            //cant sort such a small chunk
            if (chunkSize <= 2)
                return b;

            //determine new height, downsample mode will decrease it.
            bHeight = (mode == SortMode.Downsample) ? (bHeight / chunkSize * 2) : (b.Height);
            bWidth = b.Width;

            int chunkNum = bHeight / chunkSize;


            //create arrays to store bitmap data, allow for locking multithreading.
            inputArray = bitmapToArray(b);
            outputArray = bitmapToArray(b);

            //create updater timer
            Timer updater = new Timer(OnUpdate, null, 1000, 1000);

            //go through each vertical line
            Parallel.For(0, b.Width, x =>
            {
                //reduce locking by taking the column we are going to be using at the start
                var column = inputArray[x];

                //go through each chunk
                for (int yc = 0; yc < chunkNum; yc++)
                {
                    int yoffset = yc * chunkSize;

                    var samples = new Color[chunkSize];

                    //create samples
                    for (int y = 0; y < chunkSize; y++)
                    {
                        if (y + yoffset < bHeight)
                            samples[y] = column[y + yoffset];
                        else
                            break;
                    }

                    //sort the samples
                    var sorted = SortBuffer(samples);

                    //enumerate through sorted samples and move them onto output
                    for (int y = 0; y < sorted.Count; y++)
                    {
                        var color = sorted[y];
                        if (mode == SortMode.Downsample)
                        {
                            //only care about first and last pixels
                            int newYOffset = yc*2;
                            if (y == 0)
                            {
                                column[newYOffset] = color;
                            }
                            else if (y == chunkSize - 1)
                            {
                                column[newYOffset + 1] = color;
                            }
                        }
                        else
                        {
                            if (y + yoffset < bHeight)
                                column[y + yoffset] = color;
                        }
                    }
                }
                
                outputArray[x] = column;

                //update progress so updater recognises change
                progress++;
            });


            //stop updater from trying to do things
            updater.Dispose();
            updating = true;

            //return the array to bitmap form
            var outputBitmap = arrayToBitmap(outputArray);

            return outputBitmap;

        }


        private void OnUpdate(object data)
        {
            //check that progress has been made and updating isnt currently occuring
            if (progress > lastProgress && !updating)
            {
                updating = true;

                double percentile = biDirectional ? ((double) progress / (bWidth+bHeight)) : ((double) progress/bWidth);

                OnProgressUpdate(percentile, arrayToBitmap(outputArray));

                lastProgress = progress;
                updating = false;
            }
        }

        static Bitmap arrayToBitmap(Color[][] array)
        {
            Bitmap b = new Bitmap(array.Length, array[0].Length);


            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    b.SetPixel(x, y, array[x][y]);
                }
            }

            return b;
        }
        static Color[][] bitmapToArray(Bitmap b)
        {
            Color[][] array = new Color[b.Width][];

            for (int x = 0; x < b.Width; x++)
            {
                array[x] = new Color[b.Width];
                for (int y = 0; y < b.Height; y++)
                {
                    array[x][y] = b.GetPixel(x, y);
                }
            }

            return array;
        }

    }

    public enum SortMode
    {
        Genetic,
        NearestNeighbour,
        Downsample
    }


}
