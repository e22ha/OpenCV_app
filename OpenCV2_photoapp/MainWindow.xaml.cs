using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System.Windows.Controls;


namespace OpenCV2_photoapp;

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

        Width = screenWidth * 50 / 100;
        Height = screenHeight * 50 / 100;

        Left = (screenWidth - Width) / 6 * 1;
        Top = (screenHeight - Height) / 2;
    }


    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void ToggleFilter_Click(object sender, RoutedEventArgs e)
    {
        Image.Source = (bool)ToggleFilter.IsChecked switch
        {
            true => Filter.BitmapSourceFromHBitmap(_originalMat),
            false => Filter.BitmapSourceFromHBitmap(_filterApplied ? _filteredMat : _originalMat)
        };
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
        if (openFileDialog.ShowDialog() != true) return _imageLoaded;
        Image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
        _originalMat = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        _filteredMat = new Mat();
        ToggleFilter.IsChecked = false;
        return _imageLoaded = true;
    }

    private void Load_image(object sender, RoutedEventArgs e)
    {
        Filter_panel.IsEnabled = Load_image();
    }


    private void RGB_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyRgbFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
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

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
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
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
                break;
        }
    }

    private void ApplyBwFilter()
    {
        var image = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.BWImage(image).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
    }


    private void Sepia_filter(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplySepiaFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
                break;
        }
    }

    private void ApplySepiaFilter()
    {
        var image = _originalMat.Clone().ToImage<Bgr, byte>();

        _filteredMat = Filter.Sepia(image).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
    }


    private void ApplyBcFilter()
    {
        var image = _originalMat.Clone().ToImage<Bgr, byte>();

        var contrastValue = ContrastSlider.Slider.Value / 10;
        var brightnessValue = BrightnessSlider.Slider.Value / 100 * 255;

        _filteredMat = Filter.ContrastBrightness(image, contrastValue, brightnessValue).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
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
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
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
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
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

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
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
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
                break;
        }
    }

    private void ApplyBlurFilter()
    {
        var img = _originalMat.Clone();

        _filteredMat = Filter.Blur(img).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
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
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
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

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
    }


    private void ApplyWinFilter()
    {
        var img = _originalMat.Clone().ToImage<Gray, byte>();

        img = Filter.WinFilter(img.Clone(), _winTypeOp);

        _filteredMat = img.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
    }

    private void Win_filter_OnSwitchChanged(object sender, bool e)
    {
        switch (e)
        {
            case true:
                ApplyWinFilter();
                break;
            case false:
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
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
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
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

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
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
                Image.Source = Filter.BitmapSourceFromHBitmap(_originalMat);
                break;
        }
    }

    private void ApplyCartoonFilter()
    {
        var img = _originalMat.Clone().ToImage<Gray, byte>();

        _filteredMat = Filter.Blur(_originalMat).ToBitmap().ToMat();

        _filteredMat = Filter.Cartoon(img, _filteredMat.Clone()).ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
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
        var scaleSlider = new WindowWithSlider("Scale");
        scaleSlider.ValueChanged += (s, args) => ApplyScale(s, args);
        scaleSlider.Closing += (s, args) => scaleSlider.ValueChanged -= (s, args) => ApplyScale(s, args);
        scaleSlider.Show();
    }

    private void ApplyScale(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var scaleSlider = (WindowWithSlider)sender;
        var sX = scaleSlider.Value[0];
        var sY = scaleSlider.Value[1];

        var img = _originalMat.Clone().ToImage<Bgr, byte>();
        var newImage = new Image<Bgr, byte>((int)(img.Width * sX), (int)(img.Height * sY));
        for (var x = 0; x < img.Width; x++)
        {
            for (var y = 0; y < img.Height; y++)
            {
                var newX = (int)(x * sX);
                var newY = (int)(y * sY);

                newImage[newY, newX] = img[y, x];
            }
        }

        _filteredMat = newImage.ToBitmap().ToMat();

        Image.Source = Filter.BitmapSourceFromHBitmap(_filteredMat);
    }
}