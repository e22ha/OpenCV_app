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
using System.Threading.Tasks;
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

        private string _usageFilterName = "None";

        private void SaveImage(object sender, MouseButtonEventArgs e)
        {
            var sourceFile = ((BitmapImage)Image_sourse.Source).UriSource.AbsolutePath;
            var defaultFileName = "OpenCV_" + _usageFilterName + "_"
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

        private void Add_Canny_filter(object sender, RoutedEventArgs e)
        {
            var threshold1 = 100;
            var threshold2 = 200;
            
            var img = CvInvoke.Imread(((BitmapImage)Image_sourse.Source).UriSource.LocalPath, ImreadModes.Color);
            var result = new Mat();
            CvInvoke.Canny(img, result, threshold1, threshold2);
            var bitmap = result.ToBitmap();
            Image_result.Source = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                nint.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            _usageFilterName = "CannyFilter";
        }

        private void Add_CellShading(object sender, RoutedEventArgs e)
        {
            var threshold = 100;
            var maxValue = 255;

            var img = CvInvoke.Imread(((BitmapImage)Image_sourse.Source).UriSource.LocalPath, ImreadModes.Color);
            var result = new Mat();
            CvInvoke.Threshold(img, result, threshold, maxValue, ThresholdType.Binary);
            var bitmap = result.ToBitmap();
            Image_result.Source = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            _usageFilterName = "CellShading";
        }
        
        private VideoCapture _capture;
        private bool _captureInProgress;

        private void StartCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_capture != null) _capture.Dispose();
            _capture = new VideoCapture();
            _captureInProgress = true;
            Task.Factory.StartNew(CaptureFrame);
        }

        private void CaptureFrame()
        {
            while (_captureInProgress)
            {
                var frame = _capture.QueryFrame();
                var threshold1 = 100;
                var threshold2 = 200;
        
                var result = new Mat();
                CvInvoke.Canny(frame, result, threshold1, threshold2);
                var bitmap = result.ToBitmap();
                Image_result.Dispatcher.Invoke(() =>
                {
                    Image_result.Source = Imaging.CreateBitmapSourceFromHBitmap(
                        bitmap.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                });
            }
        }

        private void StopCapture_Click(object sender, RoutedEventArgs e)
        {
            _captureInProgress = false;
        }

    }
}