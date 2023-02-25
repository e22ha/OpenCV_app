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
    
    public void RGB_filter(object sender, bool e)
    {
        l.Write("RGB_on");
        if (originalMat == null) originalMat = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        var img = originalMat;
        var channels = new VectorOfMat();
        CvInvoke.Split(img, channels);

        var green = channels[0];
        var blue = channels[0];
        var red = channels[0];
        
        if ((bool)BCheckBox.IsChecked & (bool)GCheckBox.IsChecked & (bool)RCheckBox.IsChecked)
        {
            CvInvoke.Merge(channels, img);
            filteredMat = img;
        }
        else if ((bool)BCheckBox.IsChecked & (bool)GCheckBox.IsChecked )
        {
            var sum = new Mat();
            CvInvoke.Add(channels[0], channels[1], sum);
            CvInvoke.CvtColor(sum, sum, ColorConversion.Gray2Bgr);
            filteredMat = sum;
        }
        else if ((bool)GCheckBox.IsChecked & (bool)RCheckBox.IsChecked )
        {
            var sum = new Mat();
            CvInvoke.Add(channels[1], channels[2], sum);
            CvInvoke.CvtColor(sum, sum, ColorConversion.Gray2Bgr);
            filteredMat = sum;
        }       
        else if ((bool)RCheckBox.IsChecked & (bool)BCheckBox.IsChecked )
        {
            var sum = new Mat();
            CvInvoke.Add(channels[2], channels[0], sum);
            CvInvoke.CvtColor(sum, sum, ColorConversion.Gray2Bgr);
            filteredMat = sum;
        }
        else if ((bool)BCheckBox.IsChecked)
        {
            CvInvoke.CvtColor(blue, blue, ColorConversion.Gray2Bgr);
            filteredMat = blue;
        }
        else if ((bool)GCheckBox.IsChecked)
        {
            CvInvoke.CvtColor(green, green, ColorConversion.Gray2Bgr);
            filteredMat = green;
        }
        else if ((bool)RCheckBox.IsChecked)
        {
            CvInvoke.CvtColor(red, red, ColorConversion.Gray2Bgr);
            filteredMat = red;
        }
        else
        {
            filteredMat = img;
        }


        l.Write("RGB_apply");
        Image.Source = Filter.BitmapSourceFromHBitmap(filteredMat);
    }
    
    public void BW_filter(object sender, bool e)
    {
        l.Write("BW_on");
        if (originalMat == null) originalMat = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        var img = originalMat;
        Image<Bgr, byte> image = img.Clone().ToImage<Bgr, byte>();;
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
        Load_image();
    }

    private void Load_image()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };
        if (openFileDialog.ShowDialog() != true) return;
        Image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
        originalMat = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        l.Write("img load "+ Path.GetFileNameWithoutExtension(openFileDialog.FileName));
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CheckBox_OnChecked(object sender, RoutedEventArgs e)
    {
        RGB_filter(sender, true);
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