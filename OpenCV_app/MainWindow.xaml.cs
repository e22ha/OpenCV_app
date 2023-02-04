using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Interop;


namespace OpenCV_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture _capture;

        public MainWindow()
        {
            InitializeComponent();
            _capture = new VideoCapture();
            Application.Current.Exit += OnApplicationExit;
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            _capture.Dispose();
        }
        
        private string _usageFilterName = "None";

        private bool _cannyFilter;
        private bool _stepFilter;
        private bool cameraOn = false;

        private void SaveImage(object sender, MouseButtonEventArgs e)
        {
            string sourceFile;
            var defaultFileName = "OpenCV_" + _usageFilterName + 
                                  "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (!cameraOn)
            {
                sourceFile = ((BitmapImage)Image_sourse.Source).UriSource.AbsolutePath;
                defaultFileName = "OpenCV_" + _usageFilterName + "_"
                                  + Path.GetFileNameWithoutExtension(sourceFile) +
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
            else
            {
                var bitmap = new WriteableBitmap((BitmapSource)Image_result.Source);
                using var stream = new FileStream(defaultFileName, FileMode.Create);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }
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

        private Mat Add_Canny_filter(int threshold1, int threshold2, Mat img)
        {
            var cannyFilter = new Mat();
            CvInvoke.Canny(img, cannyFilter, threshold1, threshold2 , 3);
            return cannyFilter;
        }

        private static BitmapSource BitmapSourceFromHBitmap(Mat result)
        {
            var bitmap = result.ToBitmap();
            var bitmapSourceFromHBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                nint.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return bitmapSourceFromHBitmap;
        }

        private Mat Add_StepFilter(int threshold, int maxValue, Mat img)
        {
            var stepFilter = new Mat();
            CvInvoke.Threshold(img, stepFilter, threshold, maxValue, ThresholdType.Binary);
            return stepFilter;
        }
        
        private bool _captureInProgress;


        public int threshold1 = 0;
        public int threshold2 = 0;

        int threshold = 0;
        int maxValue = 0;
        private void Camera_Click(object sender, RoutedEventArgs e)
        {
            cameraOn = true;
            threshold1 = Convert.ToInt32(Slider_t1.Value);
            threshold2 = Convert.ToInt32(Slider_t2.Value);
            
            threshold = Convert.ToInt32(Slider_t.Value);
            maxValue = Convert.ToInt32(Slider_v.Value);
            if (_captureInProgress)
            {
                _captureInProgress = false;
                _capture.Dispose();
                return;
            }
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
                
                var canny = new Mat();
                var thresholdFilter = new Mat();
                var result = new Mat();
                if (_cannyFilter && _stepFilter)
                {
                    CvInvoke.Threshold(frame, thresholdFilter, threshold, maxValue, ThresholdType.Binary);
                    CvInvoke.Canny(frame, canny, threshold1, threshold2, 3);
                    CvInvoke.CvtColor(canny,canny, ColorConversion.Gray2Bgr);
                    CvInvoke.Subtract(thresholdFilter, canny, result);
                    
                }
                else if(_cannyFilter) 
                    CvInvoke.Canny(frame, result, threshold1, threshold2);
                else if (_stepFilter) 
                    CvInvoke.Threshold(frame, result, threshold, maxValue, ThresholdType.Binary);
                else
                {
                    result = frame;
                }
                Image_result.Dispatcher.Invoke(() =>
                {
                    Image_result.Source = BitmapSourceFromHBitmap(result);
                });
            }
        }


        private void AddFilter_Click(object sender, RoutedEventArgs e)
        {
            _usageFilterName = "";
            cameraOn = false;
            var img = CvInvoke.Imread(((BitmapImage)Image_sourse.Source).UriSource.LocalPath);
            var result = new Mat();
            if(_stepFilter && _cannyFilter)
            {
                var r1 = Add_Canny_filter(Convert.ToInt32(Slider_t1.Value), Convert.ToInt32(Slider_t2.Value), img);
                var r2 = Add_StepFilter(Convert.ToInt32(Slider_t.Value) , Convert.ToInt32(Slider_v.Value) , img);
                // Ensure both filters are of the same size by resizing the canny image
                CvInvoke.CvtColor(r1, r1, ColorConversion.Gray2Bgr);

                CvInvoke.Subtract(r2, r1, result);
                
                _usageFilterName += "_StepFilter";
                _usageFilterName += "_CannyFilter";
            }
            else if (_stepFilter)
            {
                result = Add_StepFilter(Convert.ToInt32(Slider_t.Value) , Convert.ToInt32(Slider_v.Value) , img);
                _usageFilterName += "_StepFilter";
            }
            else if (_cannyFilter)
            {
                result = Add_Canny_filter(Convert.ToInt32(Slider_t1.Value) , Convert.ToInt32(Slider_t2.Value), img);
                _usageFilterName += "_CannyFilter";
            }
            else
            {
                Image_result.Source = Image_sourse.Source;
                return;
            }

            Image_result.Source = BitmapSourceFromHBitmap(result);

        }

        private void Check_canny(object sender, RoutedEventArgs e)
        {
            _cannyFilter = !_cannyFilter;
        }

        private void Check_cellShading(object sender, RoutedEventArgs e)
        {
            _stepFilter = !_stepFilter;
        }
    }
}