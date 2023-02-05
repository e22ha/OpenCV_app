using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Win32;


namespace OpenCV_app;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }


    private string _usageFilterName = "None";

    private bool _cannyFilter;
    private bool _stepFilter;

    private void SaveImage(object sender, MouseButtonEventArgs e)
    {
        var sourceFile = ((BitmapImage)Image_sourse.Source).UriSource.AbsolutePath;
        var defaultFileName = "OpenCV_" + _usageFilterName + "_"
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

    private void LoadImage(object sender, MouseButtonEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return;
        Image_sourse.Source = new BitmapImage(new Uri(openFileDialog.FileName));
    }

    private void Camera_Click(object sender, RoutedEventArgs e)
    {
        var cv = new CameraView();
        cv.Show();
    }


    private void AddFilter_Click(object sender, RoutedEventArgs e)
    {
        _usageFilterName = "";
        var img = CvInvoke.Imread(((BitmapImage)Image_sourse.Source).UriSource.LocalPath);
        var result = new Mat();
        var stepFilter = new Mat();
        var cannyFilter = new Mat();
        if (_stepFilter && _cannyFilter)
        {
            CvInvoke.Canny(img, cannyFilter, Filter.t1, Filter.t2);
            CvInvoke.Threshold(img, stepFilter, Filter.t, Filter.v, ThresholdType.Binary);
            CvInvoke.CvtColor(cannyFilter, cannyFilter, ColorConversion.Gray2Bgr);

            CvInvoke.Subtract(stepFilter, cannyFilter, result);

            _usageFilterName += "_StepFilter";
            _usageFilterName += "_CannyFilter";
        }
        else if (_stepFilter)
        {
            CvInvoke.Threshold(img, stepFilter, Filter.t, Filter.v, ThresholdType.Binary);
            _usageFilterName += "_StepFilter";
        }
        else if (_cannyFilter)
        {
            CvInvoke.Canny(img, cannyFilter, Filter.t1, Filter.t2);
            _usageFilterName += "_CannyFilter";
        }
        else
        {
            Image_result.Source = Image_sourse.Source;
            return;
        }

        Image_result.Source = Filter.BitmapSourceFromHBitmap(result);
    }

    private void Check_canny(object sender, RoutedEventArgs e)
    {
        _cannyFilter = !_cannyFilter;
    }

    private void Check_cellShading(object sender, RoutedEventArgs e)
    {
        _stepFilter = !_stepFilter;
    }

    private void Slider_t1_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Filter.t1 = Convert.ToInt32(Slider_t1.Value);
    }

    private void Slider_t2_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Filter.t2 = Convert.ToInt32(Slider_t2.Value);
    }

    private void Slider_v_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Filter.v = Convert.ToInt32(Slider_v.Value);
    }

    private void Slider_t_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Filter.t = Convert.ToInt32(Slider_t.Value);
    }

    private void VVideoView(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Video files (*.mp4;*.avi;*.mkv;*.mov)|*.mp4;*.avi;*.mkv;*.mov"
        };
        if (openFileDialog.ShowDialog() != true) return;
        var vp = new VideoPlayer();
        vp.Show();
        vp.LoadVideo(openFileDialog.FileName);
    }
}