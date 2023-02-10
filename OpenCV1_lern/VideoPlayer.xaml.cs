using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Cuda;

namespace OpenCV1_lern;

public partial class VideoPlayer : Window
{
    private string _currentVideoPath;
    private bool _isMediaPlaying;
    private double _volume;

    public VideoPlayer()
    {
        InitializeComponent();

        mediaElement.MediaEnded += MediaElement_MediaEnded;
        volumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        seekSlider.ValueChanged += SeekSlider_ValueChanged;
    }

    public void LoadVideo(string videoPath)
    {
        _currentVideoPath = videoPath;
        mediaElement.Source = new Uri(_currentVideoPath);
        PlayVideo();
    }

    // private void PlayVideo()
    // {
    //     mediaElement.Play();
    //     _isMediaPlaying = true;
    // }
    
    private void PlayVideo()
    {
        mediaElement.Play();
        _isMediaPlaying = true;
        Task.Factory.StartNew(PlayVideoWithFilter);
    }

    private void PlayVideoWithFilter()
    {
        var videoCapture = new VideoCapture(_currentVideoPath);
        while (_isMediaPlaying)
        {
            Mat frame = new Mat();
            videoCapture.Read(frame);
    
            if (frame.IsEmpty)
                break;
    
            var canny = new Mat();
            var thresholdFilter = new Mat();
            var result = new Mat();
            CvInvoke.Threshold(frame, thresholdFilter, Filter.t, Filter.v, ThresholdType.Binary);
            CvInvoke.Canny(frame, canny, Filter.t1, Filter.t2, 3);
            CvInvoke.CvtColor(canny, canny, ColorConversion.Gray2Bgr);
            CvInvoke.Subtract(thresholdFilter, canny, result);
    
            Image.Dispatcher.Invoke(() => { Image.Source = Filter.BitmapSourceFromHBitmap(result); });
        }
    }
    
    


    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isMediaPlaying)
        {
            PauseVideo();
        }
        else
        {
            PlayVideo();
        }
    }

    private void PauseVideo()
    {
        mediaElement.Pause();
        _isMediaPlaying = false;
    }

    private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
    {
        PauseVideo();
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _volume = volumeSlider.Value / 100;
        mediaElement.Volume = _volume;
    }

    private bool _isDragged;
    private bool _filterInProgress;
    private bool _filter = false;

    private void SeekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isMediaPlaying && !_isDragged)
        {
            PauseVideo();
            mediaElement.Position = TimeSpan.FromSeconds(seekSlider.Value / 100 * mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
            PlayVideo();
        }
        else
        {
            mediaElement.Position = TimeSpan.FromSeconds(seekSlider.Value / 100 * mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
        }
    }

    private void SeekSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
    {
        _isDragged = true;
        if (_isMediaPlaying)
        {
            PauseVideo();
        }
    }

    private void SeekSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        _isDragged = false;
        mediaElement.Position = TimeSpan.FromSeconds(seekSlider.Value / 100 * mediaElement.NaturalDuration.TimeSpan.TotalSeconds);
    }

    private void Box_OnClick(object sender, RoutedEventArgs e)
    {
        _filter = !_filter;
    }
}