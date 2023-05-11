using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using OpenCV_photoapp;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Differ
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

        private void FImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img) fimage = LoadImage(img, out firstImage);
        }
        private void SImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img) simage = LoadImage(img, out secondImage);
        }

        private static bool fimage;
        private static bool simage;
        
        private static Image<Bgr, byte> firstImage;
        private static Image<Bgr, byte> secondImage;


        private static bool LoadImage(Image img, out Image<Bgr, byte> i)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };
            if (openFileDialog.ShowDialog() != true)
            {
                i = null;
                return false;
            }

            var uriString = openFileDialog.FileName;
            var imageSource = new BitmapImage(new Uri(uriString));
            i = new Image<Bgr, byte>(uriString);
            img.Source = imageSource;
            return true;
        }

        private void LucasKanadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var detector = new GFTTDetector(40, 0.01, 5, 3, true);

            var GFP1 = detector.Detect(firstImage.Convert<Gray, byte>().Mat);

            //создание массива характерных точек исходного изображения (только позиции)
            var srcPoints = new PointF[GFP1.Length];
            for (var i = 0; i < GFP1.Length; i++) srcPoints[i] = GFP1[i].Point;

            byte[] status; //статус точек (найдены/не найдены)
            float[] trackErrors; //ошибки
            
            //вычисление позиций характерных точек на новом изображении методом Лукаса-Канаде
            CvInvoke.CalcOpticalFlowPyrLK(
                firstImage.Convert<Gray, byte>().Mat, //исходное изображение
                secondImage.Convert<Gray, byte>().Mat,//изменённое изображение
                srcPoints, //массив характерных точек исходного изображения
                new System.Drawing.Size(20, 20), //размер окна поиска
                5, //уровни пирамиды
                new MCvTermCriteria(20, 1), //условие остановки вычисления оптического потока
                 out var destPoints, //позиции характерных точек на новом изображении
                 out status, //содержит 1 в элементах, для которых поток был найден
                out trackErrors //содержит ошибки
                );

            //вычисление матрицы гомографии
            var homographyMatrix = CvInvoke.FindHomography(destPoints, srcPoints, RobustEstimationAlgorithm.LMEDS);

            var destImage = new Image<Bgr, byte>(firstImage.Size);
            CvInvoke.WarpPerspective(secondImage, destImage, homographyMatrix, destImage.Size);

            var output1 = firstImage.Clone();

            foreach (var p in GFP1)
            {
                var center = new System.Drawing.Point((int)p.Point.X, (int)p.Point.Y);
                CvInvoke.Circle(output1, center, 3, new MCvScalar(0, 0, 255), 2);
            }

            foreach (var p in destPoints)
            {
                var center = new System.Drawing.Point((int)p.X, (int)p.Y);
                CvInvoke.Circle(destImage, center, 3, new MCvScalar(0, 0, 255), 2);
            }

            FImage.Source = Filter.ImageSourceFromBitmap(output1.Resize(350, 300, Inter.Linear).Mat);
            SImage.Source = Filter.ImageSourceFromBitmap(destImage.Resize(350, 300, Inter.Linear).Mat);
        }

        private void CompareBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var detector = new GFTTDetector(40, 0.01, 5, 3, true);

            var baseImgGray = firstImage.Convert<Gray, byte>();
            var twistedImgGray = secondImage.Convert<Gray, byte>();

            //генератор описания ключевых точек
            var descriptor = new Brisk();
            
            //поскольку в данном случае необходимо посчитать обратное преобразование
            //базой будет являться изменённое изображение
            var gfp1 = new VectorOfKeyPoint();
            var baseDesc = new UMat();
            var bimg = twistedImgGray.Mat.GetUMat(AccessType.Read);
            
            var gfp2 = new VectorOfKeyPoint();
            var twistedDesc = new UMat();
            var timg = baseImgGray.Mat.GetUMat(AccessType.Read);
            
            //получение необработанной информации о характерных точках изображений
            detector.DetectRaw(bimg, gfp1);
            
            //генерация описания характерных точек изображений
            descriptor.Compute(bimg, gfp1, baseDesc);
            detector.DetectRaw(timg, gfp2);
            descriptor.Compute(timg, gfp2, twistedDesc);

            //класс позволяющий сравнивать описания наборов ключевых точек
            var matcher = new BFMatcher(DistanceType.L2);

            //массив для хранения совпадений характерных точек
            var matches = new VectorOfVectorOfDMatch();
            //добавление описания базовых точек
            matcher.Add(baseDesc);
            //сравнение с описанием изменённых
            matcher.KnnMatch(twistedDesc, matches, 2, null);
            //3й параметр - количество ближайших соседей среди которых осуществляется поиск совпадений
            //4й параметр - маска, в данном случае не нужна

            var mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(255));
            Features2DToolbox.VoteForUniqueness(matches, 0.8, mask);

            var res = firstImage.Clone();
            Features2DToolbox.DrawMatches(secondImage, gfp1, firstImage, gfp2, matches, res, new MCvScalar(255, 0,0), new MCvScalar(255, 0, 0), mask);
                CompareResult.Source = Filter.ImageSourceFromBitmap(res.Resize(700, 300, Inter.Linear).Mat);

                var homography =
                    Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(gfp1, gfp2, matches, mask, 2);

            var destImage = new Image<Bgr, byte>(firstImage.Size);
            CvInvoke.WarpPerspective(secondImage, destImage, homography, destImage.Size);

            SImage.Source = Filter.ImageSourceFromBitmap(destImage.Resize(350, 300, Inter.Linear).Mat);
        }
    }
}