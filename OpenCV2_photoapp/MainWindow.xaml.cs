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
    private Mat originalMat;
    private Mat filteredMat = null;
    private Mat mathMaskImage = null;
    private int _mathTypeOp;
    private int _winTypeOp = 1;

    public MainWindow()
    {
        l = new logWin();

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
        if (originalMat == null) return;

        var image = originalMat.Clone().ToImage<Bgr, byte>();

        var b = (bool)BCheckBox.IsChecked ? 1 : 0;
        var g = (bool)GCheckBox.IsChecked ? 1 : 0;
        var r = (bool)RCheckBox.IsChecked ? 1 : 0;

        filteredMat = Filter.RGBChSwitch(image, b, g, r).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
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
        if (originalMat == null) return;

        var image = originalMat.Clone().ToImage<Bgr, byte>();

        filteredMat = Filter.BWImage(image).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }


//method for call load method with return result into state of enabled filter panel 
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
        return true;
    }

    private void LoadImageForMath_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return;
        mathMaskImage = CvInvoke.Imread(openFileDialog.FileName);
        ApplyMathFilter();
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
        if (originalMat == null) return;

        var image = originalMat.Clone().ToImage<Bgr, byte>();

        filteredMat = Filter.Sepia(image).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }

    private void ApplyBCFilter()
    {
        if (originalMat == null) return;

        var image = originalMat.Clone().ToImage<Bgr, byte>();

        //TODO: set right value range 
        var contrastValue = ContrastSlider.Slider.Value / 100;
        var brightnessValue = BrightnessSlider.Slider.Value / 100 * 255;

        filteredMat = Filter.ContrastBrightness(image, contrastValue, brightnessValue).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
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
        if (originalMat == null) return;

        var img = originalMat.Clone();

        var hue = HueSlider.Slider.Value;
        var saturation = SaturationSlider.Slider.Value;
        var value = ValueSlider.Slider.Value;

        filteredMat = Filter.HcvImage(img, hue, saturation, value).ToBitmap().ToMat();

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
        if (originalMat == null) return;

        var img = originalMat.Clone();

        filteredMat = Filter.Blur(img).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
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
        if (originalMat == null || mathMaskImage == null) return;

        var img = originalMat.Clone().ToImage<Bgr, byte>();

        switch (_mathTypeOp)
        {
            case 1:
                img = Filter.Exception(img.Clone(), mathMaskImage.Clone().ToImage<Bgr, byte>());
                break;
            case 2:
                img = Filter.Addition(img.Clone(), mathMaskImage.Clone().ToImage<Bgr, byte>(), 0.5, 0.5);
                break;
            case 3:
                img = Filter.Exception(img.Clone(), mathMaskImage.Clone().ToImage<Bgr, byte>());
                break;
        }

        filteredMat = img.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }

    private void ApplyWinFilter()
    {
        if (originalMat == null) return;

        var img = originalMat.Clone().ToImage<Gray, byte>();


        img = Filter.WinFilter(img.Clone(), _winTypeOp);


        filteredMat = img.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }

    private void Win_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyWinFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(originalMat);
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
}