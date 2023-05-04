using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using OpenCV_photoapp;
using Point = System.Windows.Point;

namespace recognizy;

public partial class MainWindow : Window
{
    private bool _imageLoaded;
    private bool _videoLoaded;
    private Mat _originalMat;
    private Image<Bgr, byte> _origImg;

    private List<Rectangle> listOfRectangles = new List<Rectangle>();
    private bool _selectedRegion;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void UpdateInfo(bool fromBlank, string content)
    {
        switch (fromBlank)
        {
            case false:
                InfoBlock.Text += "\n" + content;
                break;
            case true:
                InfoBlock.Text = content;
                break;
        }
    }

    private bool Load()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return false;
        var uriString = openFileDialog.FileName;
        UpdateInfo(true, "Image\nPath to image: " + uriString + ";");
        var imageSource = new BitmapImage(new Uri(uriString));

        Media.Source = null;
        Media.Visibility = Visibility.Hidden;
        Image.Visibility = Visibility.Visible;

        Image.Source = imageSource;
        _origImg = new Image<Bgr, byte>(uriString);
        _originalMat = _origImg.Mat;
        return true;
    }

    private bool Load_video()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Video files (*.mp4;*.gif;)|*.mp4;*.gif;"
        };
        if (openFileDialog.ShowDialog() != true) return false;
        var uriString = openFileDialog.FileName;
        UpdateInfo(true, "Video\nPath to video: " + uriString + ";");

        Image.Source = null;
        Image.Visibility = Visibility.Hidden;
        Media.Visibility = Visibility.Visible;

        Media.Source = new Uri(uriString);
        return true;
    }

    private void Load(object sender, RoutedEventArgs e)
    {
        var btnSender = (Button)sender;
        switch (btnSender.Name)
        {
            case "loadImageBtn":
                _imageLoaded = Load();
                break;
            case "loadVideoBtn":
                _videoLoaded = Load_video();
                break;
        }
    }

    private void FButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_imageLoaded) return;
        _selectedRegion = true;
        UpdateInfo(false, "Search contours starting...");
        var thresh = _origImg.Clone().Convert<Gray, byte>();
        thresh._ThresholdBinaryInv(new Gray(128), new Gray(255));
        thresh.Dilate(5);

        var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(thresh, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
        UpdateInfo(false, "contours count: " + contours.Size.ToString());

        var output = _origImg.Copy();
        var temp = 0;
        for (var i = 0; i < contours.Size; i++)
        {
            if (!(CvInvoke.ContourArea(contours[i]) > 50)) continue;
            var rect = CvInvoke.BoundingRectangle(contours[i]);
            listOfRectangles.Add(rect);
            output.Draw(rect, new Bgr(Color.Blue), 1);
            temp++;
        }

        UpdateInfo(false, "contoursLargeArea count: " + temp);

        Image.Source = Filter.ImageSourceFromBitmap(output.Mat);
    }

    private void Image_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_selectedRegion) return;
        RectForRecognize = GetClickRectangle(e.GetPosition(Image));
    }

    private Rectangle GetClickRectangle(Point pos)
    {
        var r = new Rectangle();
        if (listOfRectangles.Count <= 0) return r;
        r = listOfRectangles.Find(i =>
            i.Top < pos.Y &
            i.Bottom > pos.Y &
            i.Left < pos.X &
            i.Right > pos.X);
        if (r == null) UpdateInfo(true, "Not found");
        else if (r != null) UpdateInfo(true, r.Size.ToString());
        return r;
    }

    public Rectangle RectForRecognize { get; set; }
}