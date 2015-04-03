using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Windows.Shell;
using AForge.Genetic;
using Microsoft.Win32;
using PixelSort;
using TSP;

namespace PixelSortApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private System.Drawing.Image oldImage;
        private System.Drawing.Image newImage;

        Sorter sorter = new Sorter();
        private Thread sorterThread;

        private int numPasses;
        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            sorter.OnProgressUpdate += Sorter_OnProgressUpdate;
            sorter.OnFinish += sorter_OnFinish;
        }

        void sorter_OnFinish(Bitmap output)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgress(1, output);
            }); 
        }

        void Sorter_OnProgressUpdate(double percentile, Bitmap updatedBitmap)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgress(percentile, updatedBitmap);
            });

        }

        private void UpdateProgress(double percentile, Bitmap updatedBitmap)
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            TaskbarItemInfo.ProgressValue = percentile;
            newImage = updatedBitmap;
            NewImage.Source = Convert(newImage);

            if (percentile == 1)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                sorterThread = null;
                numPasses++;

                if (numPasses < int.Parse(PassesTextBox.Text))
                {
                    LoopImages();
                    RunOnePass();
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Image Files (*.png, *.gif, *.bmp, *.jpg)|*.png;*.gif;*.bmp;*.jpg";
            o.ShowDialog();

            if (File.Exists(o.FileName))
            {
                oldImage = System.Drawing.Image.FromFile(o.FileName);

                var oldWpfImage = Convert(oldImage);


                OldImage.Source = oldWpfImage;

            }
        }


        public ImageSource Convert(System.Drawing.Image i)
        {
            using (MemoryStream drawingStream = new MemoryStream())
            {
                drawingStream.Seek(0, SeekOrigin.Begin);
                i.Save(drawingStream, ImageFormat.Bmp);
                drawingStream.Seek(0, SeekOrigin.Begin);


                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = drawingStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }


        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            numPasses = 0;
            RunOnePass();
        }

        void RunOnePass()
        {
            if (sorterThread == null)
            {
                Bitmap b = new Bitmap(oldImage);
                int iterations = int.Parse(IterationsTextBox.Text);
                int chunks = int.Parse(ChunksTextBox.Text);

                sorterThread = new Thread(() =>
                {
                    sorter.SortVertical(b, iterations,chunks);
                });
                sorterThread.Start();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.DefaultExt = ".png";
            dialog.Filter = "PNG File (*.png)|*.png";

            dialog.ShowDialog();



            var img = NewImage.Source;
            using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)img));
                encoder.Save(fileStream);
            }
        }

        private void LoopButton_Click(object sender, RoutedEventArgs e)
        {
            LoopImages();
        }

        private void LoopImages()
        {
            oldImage = newImage;
            OldImage.Source = Convert(oldImage);
        }
    }
}
