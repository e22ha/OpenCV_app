using System;
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




namespace OpenCV2_photoapp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private logWin l;
    private Mat originalMat = null;
    private Mat filteredMat = null;

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

        Left = (screenWidth - Width) / 6*1;
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
        l.Write("img load "+ Path.GetFileNameWithoutExtension(openFileDialog.FileName));
        return true;
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
        l.Write("Sepia_on");
        if (originalMat == null) originalMat = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        var img = originalMat;
        Image<Bgr, byte> image = img.Clone().ToImage<Bgr, byte>();;
        var SepiaImg = new Image<Bgr, byte>(img.Size);

        for (var y = 0; y < img.Size.Height; y++)
        {
            for (var x = 0; x < img.Size.Width; x++)
            {
                SepiaImg.Data[x, y, 0] = (byte)(image.Data[x, y, 0] * 0.393 + image.Data[x, y, 1] * 0.769 +
                                                 image.Data[x, y, 2] * 0.189);
                SepiaImg.Data[x, y, 1] = (byte)(image.Data[x, y, 0] * 0.349 + image.Data[x, y, 1] * 0.686 +
                                                 image.Data[x, y, 2] * 0.168);
                SepiaImg.Data[x, y, 2] = (byte)(image.Data[x, y, 0] * 0.272 + image.Data[x, y, 1] * 0.534 +
                                                 image.Data[x, y, 2] * 0.131);
                
            }
        }
        
        filteredMat = SepiaImg.ToBitmap().ToMat();
        
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
}