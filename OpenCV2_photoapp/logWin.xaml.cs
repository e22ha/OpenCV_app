using System.Windows;

namespace OpenCV2_photoapp;

public partial class logWin : Window
{
    public logWin()
    {
        InitializeComponent();
        Loaded += Window_Loaded;
    }

    public void Write(string msg)
    {
        ListBox.Items.Add(msg);
    }


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // задаем позицию и размеры окна в 80% от размеров экрана
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        
        Width = screenWidth * 20 / 100;
        Height = screenHeight * 80 / 100;

        Left = (screenWidth - Width) / 6*5;
        Top = (screenHeight - Height) / 2;
    }

}