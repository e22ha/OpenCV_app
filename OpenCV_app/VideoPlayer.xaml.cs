using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenCV_app;

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

    private void PlayVideo()
    {
        mediaElement.Play();
        _isMediaPlaying = true;
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

}