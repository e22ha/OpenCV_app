using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
    private Image<Bgr, byte> _origImg;
    private Image<Bgr, byte> _origImg_BoundingBox;

    private readonly List<Rectangle> _listOfRectangles = new();

    private readonly List<Rectangle> _listOfFaces = new();
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

        Image.Source = imageSource;
        _origImg = new Image<Bgr, byte>(uriString);
        return true;
    }

    private bool Load_video()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Video files (*.mp4;*.gif;)|*.mp4;*.gif;"
        };
        if (openFileDialog.ShowDialog() != true) return false;
        videoString = openFileDialog.FileName;
        UpdateInfo(true, "Video\nPath to video: " + videoString + ";");
        _capture = new VideoCapture(videoString);
        var framrate = _capture.Get(CapProp.Fps);

        timer.Interval = new TimeSpan(0, 0, (int)(1 / framrate));
        return true;
    }

    private DispatcherTimer timer = new DispatcherTimer();
    private Mat frame;

    private void RecFacesProcessFrame(object sender, EventArgs e)
    {
        frame = _capture.QueryFrame(); //Read(frame);

        if (frame.IsEmpty)
        {
            UpdateInfo(false, "frame is empty");
            return;
        }

        _listOfFaces.Clear();

        UpdateInfo(true, "Face recognition start...");
        using (var ugray = new Mat())
        {
            CvInvoke.CvtColor(frame, ugray, ColorConversion.Bgr2Gray);
            _listOfFaces.AddRange(_cascadeClassifier.DetectMultiScale(ugray, 1.1, 10, new Size(20, 20)));
        }

        UpdateInfo(false, "Count of face: " + _listOfFaces.Count);

        var output = frame.Clone().ToImage<Bgr, byte>();
        foreach (var t in _listOfFaces)
        {
            // output.Draw(t, new Bgr(Color.GreenYellow), 5);
            output.ROI = t;
            var small = frame.ToImage<Bgr, byte>().Resize(t.Width, t.Height, Inter.Nearest); //создание
            //копирование изображения small на изображение res с использованием маски копирования mask
            CvInvoke.cvCopy(small, output, small.Split()[0]);
            output.ROI = Rectangle.Empty;
        }
        Image.Dispatcher.Invoke(() => { Image.Source = Filter.ImageSourceFromBitmap(output.Mat); });


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
        thresh._Dilate(5);

        var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(thresh, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
        UpdateInfo(false, "contours count: " + contours.Size.ToString());

        _origImg_BoundingBox = _origImg.Copy();
        var temp = 0;
        for (var i = 0; i < contours.Size; i++)
        {
            if (!(CvInvoke.ContourArea(contours[i]) > 50)) continue;
            var rect = CvInvoke.BoundingRectangle(contours[i]);
            _listOfRectangles.Add(rect);
            _origImg_BoundingBox.Draw(rect, new Bgr(Color.Blue), 1);
            temp++;
        }

        UpdateInfo(false, "contoursLargeArea count: " + temp);

        Image.Source = Filter.ImageSourceFromBitmap(_origImg_BoundingBox.Mat);
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
        if (_listOfRectangles.Count <= 0) return r;
        r = _listOfRectangles.Find(i =>
            i.Top < pos.Y &
            i.Bottom > pos.Y &
            i.Left < pos.X &
            i.Right > pos.X);
        return r;
    }

    private Rectangle RectForRecognize { get; set; }

    private readonly Tesseract _ocr = new("C:\\Code\\AOCI\\OpenCV_app\\recognizy\\OCR", "eng",
        OcrEngineMode.TesseractLstmCombined);

    private readonly CascadeClassifier _cascadeClassifier =
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
                _listOfFaces.AddRange(_cascadeClassifier.DetectMultiScale(ugray, 1.1, 10, new Size(20, 20)));
            }

            UpdateInfo(false, "Count of face: " + _listOfFaces.Count);

            var output = _origImg.Clone();
            foreach (var t in _listOfFaces)
            {
                output.Draw(t, new Bgr(Color.GreenYellow), 5);
            }

            Image.Source = Filter.ImageSourceFromBitmap(output.Mat);
        }
        else if (_videoLoaded)
        {
            UpdateInfo(true, "Video face recognition start...");
            timer.Tick += RecFacesProcessFrame;
            timer.Start();
        }
    }

    private string videoString;
    private VideoCapture _capture;

    private void StopBtn_OnClick(object sender, RoutedEventArgs e)
    {
        _capture.Stop();
        timer.Stop();
    }

    private void RecAction_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_videoLoaded) return;
        timer.Tick += RecActionProcessFrame;
        timer.Start();
    }

    private void RecActionProcessFrame(object sender, EventArgs e)
    {
        var mat = new Mat();
        _capture.Retrieve(mat);

        var current = mat.ToImage<Gray, byte>();

        var foregroundMask = current.CopyBlank();
        _subtractor.Apply(current, foregroundMask);

        foregroundMask._ThresholdBinary(new Gray(120), new Gray(255));

        //foregroundMask.Erode(3);
        //foregroundMask.Dilate(4);

        foregroundMask = FilterMask(foregroundMask);

        var contours = new VectorOfVectorOfPoint();

        CvInvoke.FindContours(foregroundMask, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
        var output = mat.ToImage<Bgr, byte>().Copy();

        for (var i = 0; i < contours.Size; i++)
        {
            if (!(CvInvoke.ContourArea(contours[i], false) > 100)) continue; //игнорирование маленьких контуров
            var rect = CvInvoke.BoundingRectangle(contours[i]);
            output.Draw(rect, new Bgr(Color.Lime), 4);
        }

        Image.Source = Filter.ImageSourceFromBitmap(foregroundMask.Mat);
        // Image.Source = Filter.ImageSourceFromBitmap(output.Mat);
    }

    BackgroundSubtractorMOG2 _subtractor = new BackgroundSubtractorMOG2(500, 16, true);

    private static Image<Gray, byte> FilterMask(Image<Gray, byte> mask)
    {
        var anchor = new System.Drawing.Point(-1, -1);
        var borderValue = new MCvScalar(1);
        // создание структурного элемента заданного размера и формы для морфологических операций
        var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2, 2), anchor);
        // заполнение небольших тёмных областей
        var closing = mask.MorphologyEx(MorphOp.Close, kernel, anchor, 3, BorderType.Default, borderValue);
        // удаление шумов
        var opening = closing.MorphologyEx(MorphOp.Open, kernel, anchor, 3, BorderType.Default, borderValue);
        // расширение для слияния небольших смежных областей
        opening._Erode(2);
        opening._Dilate(10);
        opening._Erode(8);

        // пороговое преобразование для удаления теней
        var threshold = opening.ThresholdBinary(new Gray(240), new Gray(255));
        return threshold;
    }
}