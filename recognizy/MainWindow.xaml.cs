using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using OpenCV_photoapp;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

namespace recognizy;

public partial class MainWindow : Window
{
    private bool _imageLoaded;
    private bool _videoLoaded;
    private Mat _originalMat;
    private Image<Bgr, byte> _origImg;

    private List<Rectangle> listOfRectangles = new List<Rectangle>();
    private Image<Bgr, byte> _origImg_BoundingBox;

    private List<Rectangle> listOfFaces = new List<Rectangle>();
    private bool _selectionMode;

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
        _imageLoaded = _videoLoaded = false;
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
        _selectionMode = true;
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
        if (!_selectionMode) return;
        RectForRecognize = GetClickRectangle(e.GetPosition(Image));
        var image = _origImg_BoundingBox.Copy();
        image.Draw(RectForRecognize, new Bgr(Color.GreenYellow), 1);
        Image.Source = Filter.ImageSourceFromBitmap(image.Mat);
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
        return r;
    }

    private Rectangle RectForRecognize { get; set; }

    private readonly Tesseract _ocr = new("C:\\Code\\AOCI\\OpenCV_app\\recognizy\\OCR", "eng",
        OcrEngineMode.TesseractLstmCombined);

    private CascadeClassifier _cascadeClassifier =
        new(@"C:\Code\AOCI\OpenCV_app\recognizy\Faces\haarcascade_frontalface_default.xml");

    private void RecTextBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var output = _origImg.Clone();
        output.ROI = RectForRecognize != null ? RectForRecognize : Rectangle.Empty;

        _ocr.SetImage(output); //фрагмент изображения, содержащий текст
        _ocr.Recognize(); //распознание текста
        var words = _ocr.GetCharacters(); //получение найденных символов

        var strBuilder = new StringBuilder();
        for (var j = 0; j < words.Length; j++)
        {
            strBuilder.Append(words[j].Text);
        }

        UpdateInfo(true, strBuilder.ToString());
    }

    private void RecFaces_OnClick(object sender, RoutedEventArgs e)
    {
        if (_imageLoaded)
        {
            UpdateInfo(true, "Face recognition start...");
            using (var ugray = new Mat())
            {
                CvInvoke.CvtColor(_origImg, ugray, ColorConversion.Bgr2Gray);
                listOfFaces.AddRange(_cascadeClassifier.DetectMultiScale(ugray, 1.1, 10, new Size(20, 20)));
            }
            UpdateInfo(false, "Count of face: "+ listOfFaces.Count);

                var output = _origImg.Clone();
                foreach (var t in listOfFaces)
                {
                    output.Draw(t, new Bgr(Color.GreenYellow), 5);
                }

                Image.Source = Filter.ImageSourceFromBitmap(output.Mat);
        }
        else if (_videoLoaded)
        {
        }
    }
}