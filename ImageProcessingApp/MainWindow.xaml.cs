using System.Windows;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ImageProcessingApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Image<Bgr, byte> _originalImage;
    private Image<Bgr, byte> _resultImage;
    private Rectangle _selectedRegion;

    public MainWindow(Image<Bgr, byte> originalImage, Image<Bgr, byte> resultImage, Rectangle selectedRegion)
    {
        _originalImage = originalImage;
        _resultImage = resultImage;
        _selectedRegion = selectedRegion;
        InitializeComponent();
    }

        
}