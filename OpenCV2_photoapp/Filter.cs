using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace OpenCV2_photoapp;

public class Filter
{
    public static ImageSource BitmapSourceFromHBitmap(Mat result)
    {
        var bitmap = result.ToBitmap();
        var bitmapSourceFromHBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            nint.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        return bitmapSourceFromHBitmap;
    }

    public static Image<Gray, byte> BWImage(Image<Bgr, byte> image)
    {
        var grayImage = new Image<Gray, byte>(image.Size);
        for (var y = 0; y < image.Size.Height; y++)
        {
            for (var x = 0; x < image.Size.Width; x++)
            {
                grayImage.Data[x, y, 0] = (byte)(image.Data[x, y, 0] * 0.114 + image.Data[x, y, 1] * 0.587 +
                                                 image.Data[x, y, 2] * 0.299);
            }
        }

        return grayImage;
    }

    public static Image<Bgr, byte> RGBChSwitch(Image<Bgr, byte> image, int b, int g, int r)
    {
        var outputImage = new Image<Bgr, byte>(image.Size);
        for (var y = 0; y < image.Size.Height; y++)
        {
            for (var x = 0; x < image.Size.Width; x++)
            {
                outputImage.Data[x, y, 0] = (byte)(image.Data[x, y, 0] * b);
                outputImage.Data[x, y, 1] = (byte)(image.Data[x, y, 1] * g);
                outputImage.Data[x, y, 2] = (byte)(image.Data[x, y, 2] * r);
            }
        }

        return outputImage;
    }

    public static Image<Bgr, byte> ContrastBrightness(Image<Bgr, byte> image, double contrastValue,
        double brightnessValue)
    {
        // Создаем новое изображение того же размера, что и исходное изображение
        var outputImg = new Image<Bgr, byte>(image.Size);

        // Проходим по каждому пикселю исходного изображения
        for (var y = 0; y < image.Rows; y++)
        {
            for (var x = 0; x < image.Cols; x++)
            {
                // Получаем значения цветовых каналов для текущего пикселя
                var b = image.Data[y, x, 0];
                var g = image.Data[y, x, 1];
                var r = image.Data[y, x, 2];

                r = between0255(r  * contrastValue);
                g = between0255(g * contrastValue);
                b = between0255(b  * contrastValue );

                r = between0255(r + brightnessValue);
                g = between0255(g + brightnessValue);
                b = between0255(b + brightnessValue);

                outputImg.Data[y, x, 0] = b;
                outputImg.Data[y, x, 1] = g;
                outputImg.Data[y, x, 2] = r;
            }
        }

        return outputImg;
    }

    public static Image<Hsv, byte> HcvImage(Mat img, double hue, double saturation, double value)
    {
        var image = img.Clone().ToImage<Hsv, byte>();

        CvInvoke.CvtColor(img, img, ColorConversion.Bgr2Hsv);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                image.Data[x, y, 0] = (byte)hue;
                image.Data[x, y, 1] = (byte)saturation;
                image.Data[x, y, 0] += (byte)value;
            }
        }

        return image;
    }

    public static Image<Bgr, byte> Sepia(Image<Bgr, byte> image)
    {
        var outputImage = new Image<Bgr, byte>(image.Size);

        for (var y = 0; y < image.Size.Height; y++)
        {
            for (var x = 0; x < image.Size.Width; x++)
            {
                outputImage.Data[x, y, 0] = between0255(image.Data[x, y, 0] * 0.393);
                outputImage.Data[x, y, 1] = between0255(image.Data[x, y, 1] * 0.686);
                outputImage.Data[x, y, 2] = between0255(image.Data[x, y, 2] * 0.131);
            }
        }

        return outputImage;
    }

    public static Image<Bgr, byte> Addition(Image<Bgr, byte> img1, Image<Bgr, byte> img2, double k1, double k2)
    {
        var outputImage = img1.Clone();

        img2 = img2.Resize(outputImage.Size.Width, outputImage.Size.Height, Inter.Area);

        for (var y = 0; y < outputImage.Size.Height; y++)
        for (var x = 0; x < outputImage.Size.Width; x++)
        {
            outputImage.Data[y, x, 0] = between0255(outputImage.Data[y, x, 0] * k1 + img2.Data[y, x, 0] * k2);
            outputImage.Data[y, x, 1] = between0255(outputImage.Data[y, x, 1] * k2 + img2.Data[y, x, 1] * k2);
            outputImage.Data[y, x, 2] = between0255(outputImage.Data[y, x, 2] * k2 + img2.Data[y, x, 2] * k2);
        }

        return outputImage;
    }

    public static Image<Bgr, byte> Exception(Image<Bgr, byte> img1, Image<Bgr, byte> img2)
    {
        var outputImage = img1.Clone();
        img2 = img2.Resize(outputImage.Size.Width, outputImage.Size.Height, Inter.Area);

        for (var y = 0; y < outputImage.Size.Height; y++)
        for (var x = 0; x < outputImage.Size.Width; x++)
        {
            outputImage.Data[y, x, 0] = between0255(outputImage.Data[y, x, 0] - img2.Data[y, x, 0]);
            outputImage.Data[y, x, 1] = between0255(outputImage.Data[y, x, 1] - img2.Data[y, x, 1]);
            outputImage.Data[y, x, 2] = between0255(outputImage.Data[y, x, 2] - img2.Data[y, x, 2]);
        }

        return outputImage;
    }

    public static Image<Bgr, byte> Intersection(Image<Bgr, byte> img1, Image<Bgr, byte> img2)
    {
        var outputImage = img1.Clone();
        
        img2 = img2.Resize(outputImage.Size.Width, outputImage.Size.Height, Inter.Area);

        for (var y = 0; y < outputImage.Size.Height; y++)
        for (var x = 0; x < outputImage.Size.Width; x++)
        {
            outputImage.Data[y, x, 0] = between0255(outputImage.Data[y, x, 0] * img2.Data[y, x, 0] * 0.04);
            outputImage.Data[y, x, 1] = between0255(outputImage.Data[y, x, 0] * img2.Data[y, x, 0] * 0.04);
            outputImage.Data[y, x, 2] = between0255(outputImage.Data[y, x, 0] * img2.Data[y, x, 0] * 0.04);
        }

        return outputImage;
    }

    public static byte between0255(double res)
    {
        res = Math.Max(res, 0);
        res = Math.Min(res, 255);
        return (byte)res;
    }
    

    public static Image<Bgr, byte> Blur(Mat img)
    {
        var blur = img.Clone().ToBitmap().ToImage<Bgr, byte>();

        var outputImage = blur.CopyBlank();

        var list = new List<byte>();
        
        for (var c = 0; c < 3; c++)
        for (var y = 1; y < blur.Size.Height - 1; y++)
        for (var x = 1; x < blur.Size.Width - 1; x++)
        {
            list.Clear();

            for (var i = -1; i < 2; i++)
            for (var j = -1; j < 2; j++)
            {
                list.Add(blur.Data[i + y, j + x, c]);
            }

            list.Sort();

            outputImage.Data[y, x, c] = list[list.Count / 2];
        }

        return outputImage;
    }

    public static Image<Gray, byte> WinFilter(Image<Gray, byte> clone, int winTypeOp)
    {
        var gray = clone.Convert<Gray, byte>();
        var result = gray.CopyBlank();

        var window = winTypeOp switch
        {
            1 => new int[3, 3] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } },
            2 => new int[3, 3] { { -4, -2, 0 }, { -2, 1, 2 }, { 0, 2, 4 } },
            3 => new int[3, 3] { { 0, 0, 0 }, { -4, 4, 0 }, { 0, 0, 0 } },
        };

        for (var y = 1; y < gray.Size.Height - 1; y++)
        for (var x = 1; x < gray.Size.Width - 1; x++)
        {
            var r = 0;

            for (var i = -1; i < 2; i++)
            for (var j = -1; j < 2; j++)
            {
                r += gray.Data[i + y, j + x, 0] * window[i + 1, j + 1];
            }

            result.Data[y, x, 0] = between0255(r);
        }

        return result;
    }

    public static Image<Bgr, byte> Cartoon(Image<Gray, byte> img, Mat clone)
    {
        var outputImage = clone.ToImage<Bgr, byte>().Clone();

        var edges = img.Convert<Gray, byte>();
        edges = edges.ThresholdAdaptive(new Gray(100), AdaptiveThresholdType.MeanC, ThresholdType.Binary, 3,
            new Gray(0.03));

        for (var y = 0; y < clone.Size.Height; y++)
        for (var x = 0; x < clone.Size.Width; x++)
        {
            outputImage.Data[y, x, 0] = between0255(outputImage.Data[y, x, 0] * 0.12 * edges.Data[y, x, 0] * 0.12);
            outputImage.Data[y, x, 1] = between0255(outputImage.Data[y, x, 1] * 0.12 * edges.Data[y, x, 0] * 0.12);
            outputImage.Data[y, x, 2] = between0255(outputImage.Data[y, x, 2] * 0.12 * edges.Data[y, x, 0] * 0.12);
        }

        return outputImage;
    }
}