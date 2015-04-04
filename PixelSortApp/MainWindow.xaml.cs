using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using System.Net.Mime;
using System.Threading;
using System.Windows.Shell;
using AForge.Genetic;
using MahApps.Metro.Controls.Dialogs;
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

        private Stopwatch stopwatch=new Stopwatch();

        private int numPasses;

        private int passesToComplete;
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

                if (numPasses < passesToComplete)
                {
                    LoopImages();
                    RunOnePass();
                }
                else
                {
                    stopwatch.Stop();
                    this.ShowMessageAsync("Elapsed Time", stopwatch.ElapsedMilliseconds + "ms");
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
            stopwatch.Reset();
            stopwatch.Start();
            numPasses = 0;
            RunOnePass();
        }

        void RunOnePass()
        {
            int iterations;
            int chunks;

            SortMode mode;

            if (sorterThread == null)
            {
                if (int.TryParse(IterationsTextBox.Text, out iterations)
                    && int.TryParse(ChunksTextBox.Text, out chunks) 
                    && int.TryParse(PassesTextBox.Text,out passesToComplete))
                {
                    if (chunks >= 1 && iterations >= 1)
                    {
                        mode = ModeComboBox.Text == "Genetic" ? SortMode.Genetic : SortMode.NearestNeighbour;


                        Bitmap b = new Bitmap(oldImage);

                        sorterThread = new Thread(() =>
                        {
                            sorter.SortVertical(b, iterations, chunks,mode);
                        });
                        sorterThread.Start();
                    }
                    else
                    {
                        this.ShowMessageAsync("Invalid input", "Input parameters are not within bounds.");
                    }
                }
                else
                {
                    this.ShowMessageAsync("Invalid input", "Input parameters failed to parse.");
                }
            }
        }

        private void SaveImage(System.Drawing.Image i)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.DefaultExt = ".png";
            dialog.Filter = "PNG File (*.png)|*.png";

            dialog.ShowDialog();

            i.Save(dialog.FileName);
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

        private void RotateMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            oldImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

            OldImage.Source = Convert(oldImage);
        }

        private void SaveOldMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SaveImage(oldImage);
        }

        private void SaveNewMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SaveImage(newImage);
        }

        private async void RescaleMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var w = await this.ShowInputAsync("Rescale", "Width",new MetroDialogSettings{AnimateShow = true,AnimateHide = false});
            var h = await this.ShowInputAsync("Rescale", "Height", new MetroDialogSettings { AnimateShow = false, AnimateHide = true });

            int width;
            int height;

            if (int.TryParse(w, out width) && int.TryParse(h, out height))
            {
                oldImage = new Bitmap(oldImage, new System.Drawing.Size(width, height));

                OldImage.Source = Convert(oldImage);
            }
            else
            {
                await this.ShowMessageAsync("Error", "Failed to parse input width/height");
            }
        }
    }
}
