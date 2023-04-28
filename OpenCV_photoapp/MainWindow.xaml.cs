using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System.Windows.Controls;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using static Emgu.CV.CvEnum.HoughModes;
using Image = System.Windows.Controls.Image;
using Point = System.Drawing.Point;
using PointCollection = Emgu.CV.PointCollection;


namespace OpenCV_photoapp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private Mat _originalMat;
    private Mat _filteredMat;
    private Mat _mathMaskImage;
    private int _mathTypeOp = 1;
    private int _winTypeOp = 1;
    private bool _imageLoaded;
    private bool _filterApplied;
    private Point _center;

    public MainWindow()
    {
        InitializeComponent();
        Window_Loaded();
        _originalMat = new Mat();
        _filteredMat = new Mat();
        _mathMaskImage = new Mat();
        _imageLoaded = false;
        _filterApplied = false;
    }

    private void Window_Loaded()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        Width = screenWidth * 0.5;
        Height = screenHeight * 0.5;

        Left = screenWidth/2 - Width/2;
        Top = screenHeight/2 - Height/2;
    }



    private void ToggleFilter_Click(object sender, RoutedEventArgs e)
    {
        Image.Source = (bool)ToggleFilter.IsChecked switch
        {
            true => Filter.ImageSourceFromBitmap(_originalMat),
            false => Filter.ImageSourceFromBitmap(_filterApplied ? _filteredMat : _originalMat)
        };
    }


    private void Click_image(object sender, MouseButtonEventArgs e)
    {
        FilterPanel.IsEnabled = Load_image();
    }

    private bool Load_image()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return _imageLoaded;
        var imageSource = new BitmapImage(new Uri(openFileDialog.FileName));
        Image.Source = imageSource;
        _origImg = new Image<Bgr, byte>(((BitmapImage)Image.Source).UriSource.LocalPath);
        _originalMat = _origImg.Mat;
        _filteredMat = new Mat();
        ToggleFilter.IsChecked = false;
        return _imageLoaded = true;
    }

    private void Load_image(object sender, RoutedEventArgs e)
    {
        FilterPanel.IsEnabled = Load_image();
    }


    private void RGB_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyRgbFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyRgbFilter()
    {
        var image = _originalMat.Clone().ToImage<Bgr, byte>();

        var b = (bool)BCheckBox.IsChecked ? 1 : 0;
        var g = (bool)GCheckBox.IsChecked ? 1 : 0;
        var r = (bool)RCheckBox.IsChecked ? 1 : 0;

        _filteredMat = Filter.RGBChSwitch(image, b, g, r).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void RgbFilterCheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
        if (RGBFilterControl.Value)
            ApplyRgbFilter();
    }


    private void BW_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyBwFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyBwFilter()
    {
        var image = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.BWImage(image).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }


    private void Sepia_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplySepiaFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplySepiaFilter()
    {
        var image = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.Sepia(image).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }


    private void ApplyBcFilter()
    {
        var image = _originalMat.Clone().ToImage<Bgr, byte>();

        var contrastValue = ContrastSlider.Slider.Value / 10;
        var brightnessValue = BrightnessSlider.Slider.Value / 100 * 255;

        _filteredMat = Filter.ContrastBrightness(image, contrastValue, brightnessValue).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void BrightnessContrastSlider_OnValueChanged(object sender,
        RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
    {
        if (BCFilterControl.Value)
            ApplyBcFilter();
    }

    private void BrightnessContrast_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyBcFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }


    private void HCV_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyHsvFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyHsvFilter()
    {
        var img = _originalMat.Clone();

        var hue = HueSlider.Slider.Value;
        var saturation = SaturationSlider.Slider.Value;
        var value = ValueSlider.Slider.Value;

        _filteredMat = Filter.HcvImage(img, hue, saturation, value).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void HSVSlider_OnValueChanged(object sender,
        RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
    {
        if (HcvFilterControl.Value)
            ApplyHsvFilter();
    }


    private void Blur_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyBlurFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyBlurFilter()
    {
        var img = _originalMat.Clone();

        _filteredMat = Filter.Blur(img).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }


    private void LoadImageForMath_OnClick(object? sender, RoutedEventArgs? e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return;
        _mathMaskImage = CvInvoke.Imread(openFileDialog.FileName);
        ApplyMathFilter();
    }

    private void MathRadioButton_OnClick(object sender, RoutedEventArgs e)
    {
        var rb = (RadioButton)sender;
        _mathTypeOp = rb.Name switch
        {
            "AddRadioButton" => 1,
            "ExceptRadioButton" => 2,
            "IntersectRadioButton" => 3,
            _ => _mathTypeOp
        };
        ApplyMathFilter();
    }

    private void Math_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                LoadImageForMath_OnClick(sender, null);
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyMathFilter()
    {
        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        switch (_mathTypeOp)
        {
            case 1:
                img = Filter.Exception(img.Clone(), _mathMaskImage.Clone().ToImage<Bgr, byte>());
                break;
            case 2:
                img = Filter.Addition(img.Clone(), _mathMaskImage.Clone().ToImage<Bgr, byte>(), 0.5, 0.5);
                break;
            case 3:
                img = Filter.Intersection(img.Clone(), _mathMaskImage.Clone().ToImage<Bgr, byte>());
                break;
        }

        _filteredMat = img.ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }


    private void ApplyWinFilter()
    {
        var img = _originalMat.Clone().ToImage<Gray, byte>();

        img = Filter.WinFilter(img.Clone(), _winTypeOp);

        _filteredMat = img.ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void Win_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyWinFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void WinRadioButton_OnClick(object sender, RoutedEventArgs e)
    {
        var rb = (RadioButton)sender;
        _winTypeOp = rb.Name switch
        {
            "EdgeRadioButton" => 1,
            "SharpenRadioButton" => 2,
            "EmbosRadioButton" => 3,
            _ => _winTypeOp
        };
        ApplyWinFilter();
    }


    private void Watercolor_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                LoadImageForWaterColor_OnClick(sender, null);
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyWatercolorFilter()
    {
        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        var watercolorImage = Filter.ContrastBrightness(img, 1, -50);

        watercolorImage = Filter.Blur(watercolorImage.Clone().ToBitmap().ToMat());

        _filteredMat = Filter.Addition(watercolorImage, _mathMaskImage.Clone().ToImage<Bgr, byte>(),
            kSlider.Slider.Value * 0.1, (10 - kSlider.Slider.Value) * 0.1).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void LoadImageForWaterColor_OnClick(object sender, RoutedEventArgs? e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return;
        _mathMaskImage = CvInvoke.Imread(openFileDialog.FileName);
        ApplyWatercolorFilter();
    }

    private void KSlider_OnValueChanged(object sender,
        RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
    {
        if (WatercolorFilterControl.Value)
            ApplyWatercolorFilter();
    }


    private void Cartoon_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyCartoonFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyCartoonFilter()
    {
        var img = _originalMat.Clone().ToImage<Gray, byte>();

        _filteredMat = Filter.Blur(_originalMat).ToBitmap().ToMat();

        _filteredMat = Filter.Cartoon(img, _filteredMat.Clone()).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }


    private void Save_image(object sender, RoutedEventArgs routedEventArgs)
    {
        var defaultFileName = "OpenCV_" + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
            FileName = defaultFileName
        };
        if (saveFileDialog.ShowDialog() != true) return;

        _filteredMat.Save(saveFileDialog.FileName);
    }

    private void Scale_btn(object sender, RoutedEventArgs e)
    {
        var scaleSlider = new WindowWithSlider("Scale", 0, 5, 1, 1);
        scaleSlider.ValueChanged += ApplyScale;
        scaleSlider.Closing += (s, args) => scaleSlider.ValueChanged -= ApplyScale;
        scaleSlider.Show();
    }

    private void ApplyScale(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var scaleSlider = (WindowWithSlider)sender;
        var sX = scaleSlider.Value[0];
        var sY = scaleSlider.Value[1];

        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.BinScaleFilter(img, sX, sY).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }


    private void ApplyShear(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var shearSlider = (WindowWithSlider)sender;
        var sX = shearSlider.Value[0];
        var sY = shearSlider.Value[1];

        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.shear_filter(img, sX, sY).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }


    private void Shear_btn(object sender, RoutedEventArgs e)
    {
        var shearSlider = new WindowWithSlider("Shear", -1, 1, 0, 0);
        shearSlider.ValueChanged += ApplyShear;
        shearSlider.Closing += (s, args) => shearSlider.ValueChanged -= ApplyScale;
        shearSlider.Show();
    }

    private void Rotate_btn(object sender, RoutedEventArgs e)
    {
        _rotateMode = true;
        var rotateSlider = new WindowWithSlider("Rotate", -180, 180, 0);
        rotateSlider.ValueChanged += ApplyRotate;
        rotateSlider.Closing += (s, args) =>
        {
            rotateSlider.ValueChanged -= ApplyRotate;
            _rotateMode = false;
        };
        rotateSlider.Show();
    }

    private void ApplyRotate(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var rotateSlider = (WindowWithSlider)sender;
        var sX = rotateSlider.Value[0];

        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.BinRotateImage(img, sX, _center).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void FlipH_btn(object sender, RoutedEventArgs e)
    {
        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.Flip(img, true, false).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void FlipV_btn(object sender, RoutedEventArgs e)
    {
        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.Flip(img, false, true).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void FlipB_btn(object sender, RoutedEventArgs e)
    {
        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.Flip(img, true, true).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void kaleidoscope_btn(object sender, RoutedEventArgs e)
    {
        var img = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.ApplyKaleidoscopeEffect(img, 4).ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private void Homography_btn(object sender, RoutedEventArgs e)
    {
        // Создаем новое окно для выбора точек
        var pointsWindow = new PointSelectionWindow(_originalMat.ToImage<Bgr, byte>())
        {
            Owner = this // Устанавливаем текущее окно владельцем для нового окна
        };
        pointsWindow.ShowDialog(); // Открываем окно для выбора точек

        var srcPoints = pointsWindow.GetSourcePoints();

        var destPoints = new PointF[]
        {
            // плоскость, на которую осуществляется проекция,
            // задаётся четыремя точками
            new PointF(0, 0),
            new PointF(0, _originalMat.Height - 1),
            new PointF(_originalMat.Width - 1, _originalMat.Height - 1),
            new PointF(_originalMat.Width - 1, 0)
        };
        if (srcPoints == null || destPoints == null)
        {
            return;
        }

        // Выполняем гомографическое преобразование
        var homographyMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, destPoints);
        var destImage = new Mat();
        CvInvoke.WarpPerspective(_originalMat, destImage, homographyMatrix, _originalMat.Size);

        // Открываем новое окно для отображения результатов
        CvInvoke.Imshow("Homography Result", destImage);
        CvInvoke.WaitKey(0);
    }


    private void FindFilterControl_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyFindFilter();
                break;
            case false:
                Image.Source = Filter.ImageSourceFromBitmap(_originalMat);
                break;
        }
    }

    private void ApplyFindFilter()
    {
        var img = _originalMat.Clone().ToImage<Gray, byte>();

        var threshold = new Gray(100);
        var color = new Gray(255);

        var smoothGaussian = img.SmoothGaussian(5);
        var thresholdBinary = smoothGaussian.ThresholdBinary(threshold, color);

        CvInvoke.CvtColor(img, img, ColorConversion.Gray2Bgr);
        var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(thresholdBinary,
            contours,
            null,
            RetrType.List,
            ChainApproxMethod.ChainApproxSimple);

        // Определим форму каждого контура и нарисуем его в соответствующем цвете
        for (var i = 0; i < contours.Size; i++)
        {
            var contour = contours[i];
            var area = CvInvoke.ContourArea(contour);
            if (area < 256)
            {
                continue;
            }

            var epsilon = CvInvoke.ArcLength(contour, true) * 0.05;
            var approx = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(
                contour, approx,
                epsilon,
                true);

            var edges = PointCollection.PolyLine(approx.ToArray(), true);
            switch (approx.Size)
            {
                // Треугольник
                case 3:
                    CvInvoke.DrawContours(img, contours, i, new MCvScalar(0, 255, 0), 2);
                    break;
                // Четырехугольник
                case 4:
                    const int delta = 10;
                    for (var j = 0; j < edges.Length; j++)
                    {
                        var an = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                        if (an is < 90 - delta or > 90 + delta) break;
                    }

                    CvInvoke.DrawContours(img, contours, i, new MCvScalar(255, 0, 0), 2);
                    break;
            }
        }

        // Круг
        var circles = new List<CircleF>(
            CvInvoke.HoughCircles(
                smoothGaussian, Gradient,
                dp: 1.0, minDist: 250, 100, 36, minRadius: 10, maxRadius: 250)
        );
        foreach (var c in circles)
        {
            var center = new Point((int)c.Center.X, (int)c.Center.Y);
            CvInvoke.Circle(img, center, (int)c.Radius, new MCvScalar(0, 0, 255), 2);
        }

        // Отобразим изображение с выделенными объектами
        CvInvoke.Imshow("Contours Image", img);


        _filteredMat = thresholdBinary.ToBitmap().ToMat();

        Image.Source = Filter.ImageSourceFromBitmap(_filteredMat);
    }

    private bool _rotateMode;
    private bool _colorGet;
    private byte _colorRangeBase = 20;
    private Image<Bgr, byte> _origImg;
    private Image<Gray, byte> _hueChannel;

    private void Image_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_rotateMode)
        {
            var image = sender as Image;
            _center.X = (int)e.GetPosition(image).X;
            _center.Y = (int)e.GetPosition(image).Y;

            // MessageBox.Show("center: " + _center.X + ", " + _center.Y+";\n"
            //                 +"image Source size: "+ image.Source.Height + " x " + image.Source.Width+"\n"
            //                 +"image size: "+ Image.Height + " x " + Image.Width+"\n"
            //                 + _originalMat.Height + _originalMat.Width);
        }

        else if (_colorGet)
        {
            var image = sender as Image;
            var point = e.GetPosition(image);

            if (image.Source is not BitmapSource bitmapSource)
                return;

            var x = (int)(point.X / image.ActualWidth * bitmapSource.PixelWidth);
            var y = (int)(point.Y / image.ActualHeight * bitmapSource.PixelHeight);

            var pixel = new byte[4];
            var bitmap = new WriteableBitmap(bitmapSource);
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);

            // Вызов метода с индексом цветового канала
            ColorRange(pixel[0]);
        }
    }

    private void ColorRange_OnClick(object sender, RoutedEventArgs e)
    {
        ColorRange(30);
        _colorGet = true;
    }

    private void ColorRange(byte color)
    {
        var hsvImage = _origImg.Convert<Hsv, byte>();
        _hueChannel = hsvImage.Split()[0];

        const byte rangeDelta = 10;
        var resultImage = _hueChannel.InRange(new Gray(color - rangeDelta), new Gray(color + rangeDelta));

        CvInvoke.Imshow("Result", resultImage);
    }
}