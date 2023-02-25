using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;




namespace OpenCV2_photoapp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private logWin l;
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
        // var img = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        // var result = new Mat();
        // CvInvoke.ExtractChannel(img, result, 1);
        //
        // Image.Source = Filter.BitmapSourceFromHBitmap(result);
    }
    
    public void BW_filter(object sender, bool e)
    {
        MessageBox.Show("BW_on");
        // var img = CvInvoke.Imread(((BitmapImage)Image.Source).UriSource.LocalPath);
        // var result = new Mat();
        // CvInvoke.Decolor(img, result, result);
        //
        // Image.Source = Filter.BitmapSourceFromHBitmap(result);
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
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}