using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using Path = System.IO.Path;


namespace OpenCV_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Go_CLick(object sender, RoutedEventArgs e)
        {
            ImageBox_result.Source = ImageBox_sourse.Source;
        }

        private void SaveImage(object sender, MouseButtonEventArgs e)
        {
            var sourceFile = ((BitmapImage)ImageBox_sourse.Source).UriSource.AbsolutePath;
            var defaultFileName = "OpenCV_" //потом можно будет добавить какой метод использовался для обработки
                                  +Path.GetFileNameWithoutExtension(sourceFile) +
                                  "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
    
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif",
                FileName = defaultFileName
            };
            if (saveFileDialog.ShowDialog() != true) return;
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)ImageBox_result.Source));
            using var stream = new FileStream(saveFileDialog.FileName, FileMode.Create);
            encoder.Save(stream);
        }

        private void LoadImage(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ImageBox_sourse.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }
    }
}