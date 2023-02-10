using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace OpenCV1_lern;

public partial class CameraView : Window
{
    private VideoCapture _capture;

    public CameraView()
    {
        InitializeComponent();
        _capture = new VideoCapture();
        CameraOn();
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _captureInProgress = false;
        _capture.Dispose();
    }

    private bool _captureInProgress;
    

    private void CameraOn()
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

            var canny = new Mat();
            var thresholdFilter = new Mat();
            var result = new Mat();

            CvInvoke.Threshold(frame, thresholdFilter, Filter.t, Filter.v, ThresholdType.Binary);
            CvInvoke.Canny(frame, canny, Filter.t1, Filter.t2, 3);
            CvInvoke.CvtColor(canny, canny, ColorConversion.Gray2Bgr);
            CvInvoke.Subtract(thresholdFilter, canny, result);
            
            Camera_view.Dispatcher.Invoke(() => { Camera_view.Source = Filter.BitmapSourceFromHBitmap(result); });
        }
    }
}