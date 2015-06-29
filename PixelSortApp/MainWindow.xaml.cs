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

        private Sorter sorter;
        private Thread sorterThread;

        private Stopwatch stopwatch = new Stopwatch();

        private ISelectionMethod geneticMode;
        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
        }

        void sorter_OnFinish(Bitmap output)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgress(1, output, true);
            });
        }

        void Sorter_OnProgressUpdate(double percentile, Bitmap updatedBitmap)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgress(percentile, updatedBitmap);
            });

        }

        private void UpdateProgress(double percentile, Bitmap updatedBitmap, bool finished = false)
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            TaskbarItemInfo.ProgressValue = percentile;
            newImage = updatedBitmap;
            NewImage.Source = Convert(newImage);

            if (finished)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                sorterThread = null;
                stopwatch.Stop();
                this.ShowMessageAsync("Elapsed Time", stopwatch.ElapsedMilliseconds + "ms");

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

                OldImage.Source = Convert(oldImage);

                ChunksTextBox.Text = oldImage.Height.ToString();

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
            RunOnePass();
        }

        void RunOnePass()
        {
            SortOptions options = new SortOptions { GeneticMode = geneticMode };

            if (sorterThread == null)
            {
                if (int.TryParse(IterationsTextBox.Text, out options.Iterations)
                    && int.TryParse(ChunksTextBox.Text, out options.ChunkSize)
                    && int.TryParse(PassesTextBox.Text, out options.Passes)
                    && double.TryParse(MoveScaleTextBox.Text, out options.MoveScale))
                {
                    if (options.ChunkSize >= 1 && options.Iterations >= 1 && options.Passes >= 1)
                    {
                        options.Mode = (SortMode)Enum.Parse(typeof(SortMode), ModeComboBox.Text);

                        options.BiDirectional = BidirectionalCheckBox.IsChecked.GetValueOrDefault();

                        options.PassesRemaining = options.Passes;

                        Bitmap b = new Bitmap(oldImage);


                        sorter = new Sorter(options);
                        sorter.OnProgressUpdate += Sorter_OnProgressUpdate;
                        sorter.OnFinish += sorter_OnFinish;

                        sorterThread = new Thread(() =>
                        {
                            sorter.Sort(b);
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

            if (!string.IsNullOrEmpty(dialog.FileName))
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
            var w = await this.ShowInputAsync("Rescale", "Width", new MetroDialogSettings { AnimateShow = true, AnimateHide = false });
            var h = await this.ShowInputAsync("Rescale", "Height", new MetroDialogSettings { AnimateShow = false, AnimateHide = true });

            int width;
            int height;
            if (w.EndsWith("%") && h.EndsWith("%"))
            {
                if (int.TryParse(w.Replace("%", ""), out width) && int.TryParse(h.Replace("%", ""), out height))
                {
                    width = (int)(oldImage.Width / (100d / width));
                    height = (int)(oldImage.Height / (100d / height));

                    oldImage = new Bitmap(oldImage, new System.Drawing.Size(width, height));

                    OldImage.Source = Convert(oldImage);
                }
            }
            else if (int.TryParse(w, out width) && int.TryParse(h, out height))
            {
                oldImage = new Bitmap(oldImage, new System.Drawing.Size(width, height));

                OldImage.Source = Convert(oldImage);
            }
            else
            {
                await this.ShowMessageAsync("Error", "Failed to parse input width/height");
                return;
            }

            ChunksTextBox.Text = oldImage.Height.ToString();
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((SortMode)Enum.Parse(typeof(SortMode), ((ComboBoxItem)e.AddedItems[0]).Content.ToString()))
            {
                case SortMode.Genetic:
                    IterationsTextBox.IsEnabled = true;
                    GeneticModeComboBox.IsEnabled = true;
                    break;
                case SortMode.NearestNeighbour:
                    IterationsTextBox.IsEnabled = false;
                    GeneticModeComboBox.IsEnabled = false;
                    break;
                case SortMode.Random:
                    IterationsTextBox.IsEnabled = false;
                    GeneticModeComboBox.IsEnabled = false;
                    break;
            }
        }


        private void ShowTmpImage(string fileName, System.Drawing.Image i)
        {
            string tmpPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);

            if (File.Exists(tmpPath))
                File.Delete(tmpPath);

            i.Save(tmpPath);

            Process.Start(tmpPath);
        }

        private void OldImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowTmpImage("oldImage.png", oldImage);
        }

        private void NewImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowTmpImage("newImage.png", newImage);
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sorterThread != null)
            {
                sorterThread.Abort();
                sorterThread = null;
            }
        }

        private void GeneticModeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBoxItem)e.AddedItems[0]).Content.ToString())
            {
                case "Roulette Wheel":
                    geneticMode = new RouletteWheelSelection();
                    break;
                case "Elite Selection":
                    geneticMode = new EliteSelection();
                    break;
                case "Rank Selection":
                    geneticMode = new RankSelection();
                    break;
            }
        }
    }
}
