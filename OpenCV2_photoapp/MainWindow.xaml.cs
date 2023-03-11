using System;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using System.Linq;
using System.Windows.Controls;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;


namespace OpenCV2_photoapp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private logWin l;
    private Mat originalMat = null;
    private Mat filteredMat = null;
    private Mat mathMaskImage = null;
    private int _mathTypeOp;

    public MainWindow()
    {
        l = new logWin();
        l.Show();
        l.Write("showLog");

        InitializeComponent();
        Loaded += Window_Loaded;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // задаем позицию и размеры окна в 80% от размеров экрана
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        Width = screenWidth * 50 / 100;
        Height = screenHeight * 50 / 100;

        Left = (screenWidth - Width) / 6 * 1;
        Top = (screenHeight - Height) / 2;
    }


    private void RGB_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyRgbFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
                break;
        }
    }

    private void ApplyRgbFilter()
    {
        if (originalMat == null)
        {
            l.Write("none");
            return;
        }

        l.Write("RGB_on");
        var img = originalMat;

        var image = img.Clone().ToImage<Bgr, byte>();

        var SomeChImg = new Image<Bgr, byte>(img.Size);

        var b = (bool)BCheckBox.IsChecked ? 1 : 0;
        var g = (bool)GCheckBox.IsChecked ? 1 : 0;
        var r = (bool)RCheckBox.IsChecked ? 1 : 0;

        for (var y = 0; y < img.Size.Height; y++)
        {
            for (var x = 0; x < img.Size.Width; x++)
            {
                SomeChImg.Data[x, y, 0] = (byte)(image.Data[x, y, 0] * b);
                SomeChImg.Data[x, y, 1] = (byte)(image.Data[x, y, 1] * g);
                SomeChImg.Data[x, y, 2] = (byte)(image.Data[x, y, 2] * r);
            }
        }

        filteredMat = SomeChImg.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
        l.Write("RGB_apply");
    }

    private void BW_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyBwFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
                break;
        }
    }

    private void ApplyBwFilter()
    {
        if (originalMat == null)
        {
            l.Write("none");
            return;
        }

        l.Write("BW_on");
        var img = originalMat;
        var image = img.Clone().ToImage<Bgr, byte>();
        var grayImage = new Image<Gray, byte>(img.Size);

        for (var y = 0; y < img.Size.Height; y++)
        {
            for (var x = 0; x < img.Size.Width; x++)
            {
                grayImage.Data[x, y, 0] = (byte)(image.Data[x, y, 0] * 0.114 + image.Data[x, y, 1] * 0.587 +
                                                 image.Data[x, y, 2] * 0.299);
            }
        }

        filteredMat = grayImage.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }

    private void Click_image(object sender, MouseButtonEventArgs e)
    {
        Filter_panel.IsEnabled = Load_image();
    }

    private bool Load_image()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return false;
        Image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
        originalMat = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        l.Write("img load " + Path.GetFileNameWithoutExtension(openFileDialog.FileName));
        return true;
    }

    private void LoadImageForMath_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return;
        mathMaskImage = CvInvoke.Imread(openFileDialog.FileName, ImreadModes.Color);
        l.Write("mask load " + Path.GetFileNameWithoutExtension(openFileDialog.FileName));
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RgbFilterCheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
        ApplyRgbFilter();
    }

    private void Sepia_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplySepiaFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
                break;
        }
    }

    private void ApplySepiaFilter()
    {
        if (originalMat == null)
        {
            l.Write("ApplySepiaFilter: none");
            return;
        }

        l.Write("Sepia_on");
        var img = originalMat;
        var image = img.Clone().ToImage<Bgr, byte>();
        ;
        var SepiaImg = new Image<Bgr, byte>(img.Size);

        for (var y = 0; y < img.Size.Height; y++)
        {
            for (var x = 0; x < img.Size.Width; x++)
            {
                SepiaImg.Data[x, y, 0] = between0255((byte)(image.Data[x, y, 0] * 0.393));
                SepiaImg.Data[x, y, 1] = between0255((byte)(image.Data[x, y, 1] * 0.686));
                SepiaImg.Data[x, y, 2] = between0255((byte)(image.Data[x, y, 2] * 0.131));
            }
        }

        filteredMat = SepiaImg.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }

    private void ApplyBCFilter()
    {
        if (originalMat == null)
        {
            l.Write("none");
            return;
        }

        l.Write("BC_on");
        var img = originalMat.Clone();
        var image = img.Clone().ToImage<Bgr, byte>();

        
        // Задаем значение контраста и яркости
        double contrastValue = ContrastSlider.Slider.Value/100; // значение контраста от 0 до 1
        l.Write(contrastValue.ToString());
        double brightnessValue = BrightnessSlider.Slider.Value/100 *255; // значение яркости от -1 до 1
        l.Write(brightnessValue.ToString());

        // Создаем новое изображение того же размера, что и исходное изображение
        Image<Bgr, byte> outputImg = new Image<Bgr, byte>(image.Size);

        // Проходим по каждому пикселю исходного изображения
        for (int y = 0; y < image.Rows; y++)
        {
            for (int x = 0; x < image.Cols; x++)
            {
                // Получаем значения цветовых каналов для текущего пикселя
                byte b = image.Data[y, x, 0];
                byte g = image.Data[y, x, 1];
                byte r = image.Data[y, x, 2];


                double Red = r / 255.0f;
                double Green = g / 255.0f;
                double Blue = b / 255.0f;
                r = (byte)((((Red - 0.5f) * contrastValue) + 0.5f) * 255.0f);
                g = (byte)((((Green - 0.5f) * contrastValue) + 0.5f) * 255.0f);
                b = (byte)((((Blue - 0.5f) * contrastValue) + 0.5f) * 255.0f);

                r = between0255((byte)(r + brightnessValue));
                g = between0255((byte)(g + brightnessValue));
                b = between0255((byte)(b + brightnessValue));


                outputImg.Data[y, x, 0] = between0255(b);
                outputImg.Data[y, x, 1] = between0255(g);
                outputImg.Data[y, x, 2] = between0255(r);
            }
        }
        
        filteredMat = outputImg.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }

    private static byte between0255(byte b)
    {
        return byte.Min(byte.Max(b, 0), 255);
    }

    private void nofliterClick(object sender, RoutedEventArgs e)
    {
        if ((bool)ToggleFilter.IsChecked)
        {
            Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
        }
        else if ((bool)!ToggleFilter.IsChecked)
        {
            Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
        }
    }


    private void BrightnessContrastSlider_OnValueChanged(object sender,
        RoutedPropertyChangedEventArgs<double> routedpropertychangedeventargs)
    {
        ApplyBCFilter();
    }

    private void BrightnessContrast_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyBCFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
                break;
        }
    }

    private void HCV_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyHSVFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
                break;
        }
    }


    private void ApplyHSVFilter()
    {
        if (originalMat == null)
        {
            l.Write("none");
            return;
        }

        l.Write("HSV_on");
        var img = originalMat.Clone();


        CvInvoke.CvtColor(img, img, ColorConversion.Bgr2Hsv);

        var hue = HueSlider.Slider.Value;
        var saturation = SaturationSlider.Slider.Value;
        var value = ValueSlider.Slider.Value;

        var image = img.Clone().ToImage<Hsv, byte>();

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                image.Data[x, y, 0] = (byte)hue;
                image.Data[x, y, 1] = (byte)saturation;
                image.Data[x, y, 0] += (byte)value;
            }
        }

        filteredMat = image.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }

    private void HSVSlider_OnValueChanged(object sender,
        RoutedPropertyChangedEventArgs<double> routedpropertychangedeventargs)
    {
        ApplyHSVFilter();
    }

    private void Blur_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyBlurFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
                break;
        }
    }

    private void ApplyBlurFilter()
    {
        if (originalMat == null)
        {
            l.Write("none");
            return;
        }

        l.Write("Blur_on");
        var img = originalMat;

        var outputImage = new Mat();

        var coreSize = (int)SliderCore.Slider.Value;

        CvInvoke.Blur(img, outputImage, new Size(coreSize, coreSize), new Point(-1, -1), BorderType.Default);

        filteredMat = outputImage;

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
        l.Write("Blur_apply");
    }

    private void SliderCore_OnValueChanged(object sender,
        RoutedPropertyChangedEventArgs<double> routedpropertychangedeventargs)
    {
        ApplyBlurFilter();
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
                ApplyMathFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
                break;
        }
    }

    private void ApplyMathFilter()
    {
        if (originalMat == null || mathMaskImage == null)
        {
            l.Write("none");
            return;
        }
        
        l.Write("Math_on");
        var img = originalMat.Clone();

        switch (_mathTypeOp)
        {
            case 1:
                l.Write("type 1");
                CvInvoke.BitwiseNot(img, mathMaskImage.Clone());
                break;
            case 2:
                l.Write("type 2");
                CvInvoke.BitwiseXor(img, mathMaskImage.Clone(), img);
                break;
            case 3:
                l.Write("type 3");
                CvInvoke.BitwiseAnd(img, mathMaskImage.Clone(), img);
                break;
            default:
                l.Write("type 0");
                break;
        }

        filteredMat = img;

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
        l.Write("Math_apply");
    }
}