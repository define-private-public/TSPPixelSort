using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public SortOptions Options;

        public Sorter(SortOptions options)
        {
            Options = options;
        }


        private int lastProgress = 0;
        private bool updating = false;


        private List<Color> SortBuffer(Color[] buffer)
        {
            int citiesCount = buffer.Count();

            var map = new Pixel[citiesCount];
            int c = 0;
            foreach (var color in buffer)
            {
                var yuv = new YUV(color);

                map[c] = new Pixel(color, yuv,(int) (c*Options.MoveScale));
                c++;
            }

            Pixel[] path = new Pixel[citiesCount];

            ISorter sorter = null;

            switch (Options.Mode)
            {
                case SortMode.Genetic:
                    sorter = new GeneticSorter();
                    break;
                case SortMode.NearestNeighbour:
                    sorter = new NearestNeighbour();
                    break;
            }

            path = sorter.FindPath(map, Options);

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

            if (Options.BiDirectional)
            {
                //rotate chunkSize too if neccessary
                if (Options.ChunkSize == bHeight)
                    Options.ChunkSize = bWidth;

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
            if (Options.ChunkSize <= 2)
                return b;

            //determine new height, downsample mode will decrease it.
            bHeight = b.Height;
            bWidth = b.Width;

            int chunkNum = bHeight / Options.ChunkSize;


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
                    int yoffset = yc * Options.ChunkSize;

                    var samples = new Color[Options.ChunkSize];

                    //create samples
                    for (int y = 0; y < Options.ChunkSize; y++)
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
                        if (y + yoffset < bHeight)
                            column[y + yoffset] = color;
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

                double percentile = Options.BiDirectional ? ((double)progress / (bWidth + bHeight)) : ((double)progress / bWidth);

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
}
