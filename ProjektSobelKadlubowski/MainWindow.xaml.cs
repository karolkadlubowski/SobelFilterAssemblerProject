using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;


namespace ProjektSobelKadlubowski
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Bitmap imageBefore = null;
        public Bitmap imageAfter = null;

        public MainWindow()
        {
            InitializeComponent();
            threadsBox.Text = Environment.ProcessorCount.ToString();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                imageOrigin.Source = new BitmapImage(new Uri(op.FileName));
                var bitmapImage = new BitmapImage(new Uri(op.FileName));
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                    imageBefore = new Bitmap(bitmap);
                }        
            }
        }

        private void applyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (imageBefore == null)
            {
                MessageBox.Show("Select image to process");
                return;
            }
            int threadAmount;
            try
            {
               threadAmount = int.Parse(threadsBox.Text.ToString());
                if (threadAmount < 1 || threadAmount > 64)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Threads number should be between 1-64");
                return;
            }

            if (masmCheckBox.IsChecked==true)
            {
                
                var ticks =SobelC.SobelAssembly.ConvolutionFilter2Version(imageBefore, SobelC.SobelAssembly.xSobel, SobelC.SobelAssembly.ySobel,out imageAfter,int.Parse(threadsBox.Text.ToString()));
                TimeBlock.Text = ticks.ToString();
                using (MemoryStream memory = new MemoryStream())
                {
                    imageAfter.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    imageProcessed.Source = bitmapimage;
                }
                
            }
            else
            {
                var ticks = SobelC.SobelC.ConvolutionFilter2Version(imageBefore, SobelC.SobelAssembly.xSobel, SobelC.SobelAssembly.ySobel, out imageAfter, int.Parse(threadsBox.Text.ToString()));
                TimeBlock.Text = ticks.ToString();
                using (MemoryStream memory = new MemoryStream())
                {
                    imageAfter.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    imageProcessed.Source = bitmapimage;
                }
            }
               
        }
    }
}
