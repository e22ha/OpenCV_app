using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;


namespace OpenCV_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Go_CLick(object sender, RoutedEventArgs e)
        {
            var img = CvInvoke.Imread(((BitmapImage)Image_sourse.Source).UriSource.LocalPath, ImreadModes.Color);
            var result = new Mat();
            CvInvoke.Canny(img, result, 100, 200);
            var bitmap = result.ToBitmap();
            Image_result.Source = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        

        private void SaveImage(object sender, MouseButtonEventArgs e)
        {
            var sourceFile = ((BitmapImage)Image_sourse.Source).UriSource.AbsolutePath;
            var defaultFileName = "OpenCV_" //потом можно будет добавить какой метод использовался для обработки
                                  +Path.GetFileNameWithoutExtension(sourceFile) +
                                  "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
    
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif",
                FileName = defaultFileName
            };
            if (saveFileDialog.ShowDialog() != true) return;
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)Image_result.Source));
            using var stream = new FileStream(saveFileDialog.FileName, FileMode.Create);
            encoder.Save(stream);
        }

        private void LoadImage(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };
            if (openFileDialog.ShowDialog() != true) return;
            Image_sourse.Source = new BitmapImage(new Uri(openFileDialog.FileName));
        }
    }
}