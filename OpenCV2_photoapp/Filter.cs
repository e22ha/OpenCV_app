using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
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

                r = between0255(r * contrastValue);
                g = between0255(g * contrastValue);
                b = between0255(b * contrastValue);

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

    public static Image<Bgr, byte> shear_filter(Image<Bgr, byte> img, double sX, double sY)
    {
        var newImage = new Image<Bgr, byte>(img.Width, img.Height);
        for (var x = 0; x < img.Width; x++)
        {
            for (var y = 0; y < img.Height; y++)
            {
                var newX = Convert.ToInt32(x + sX * (img.Width - y));
                var newY = Convert.ToInt32(y + sY * (img.Height - x));

                if (newX < 0) newX = 0;
                if (newX >= newImage.Width) newX = newImage.Width - 1;

                if (newY < 0) newY = 0;
                if (newY >= newImage.Height) newY = newImage.Height - 1;

                newImage[newY, newX] = img[y, x];
            }
        }

        return newImage;
    }


    public static Image<Bgr, byte> ScaleFilter(Image<Bgr, byte> img, double sX, double sY)
    {
        var newImage = new Image<Bgr, byte>(Convert.ToInt32(img.Width * sX), Convert.ToInt32(img.Height * sY));
        for (var x = 0; x < img.Width; x++)
        {
            for (var y = 0; y < img.Height; y++)
            {
                var newX = Convert.ToInt32(x * sX);
                var newY = Convert.ToInt32(y * sY);

                if (newX < 0) newX = 0;
                if (newX >= newImage.Width) newX = newImage.Width - 1;

                if (newY < 0) newY = 0;
                if (newY >= newImage.Height) newY = newImage.Height - 1;

                newImage[newY, newX] = img[y, x];
            }
        }

        return newImage;
    }

    public static Image<Bgr, byte> BinScaleFilter(Image<Bgr, byte> img, double sX, double sY)
    {
        var newImage = new Image<Bgr, byte>(Convert.ToInt32(img.Width * sX), Convert.ToInt32(img.Height * sY));
        for (var x = 0; x < newImage.Width - 1; x++)
        {
            double X = (int)(x / sX);
            var baseX = Math.Floor(X);
            if (baseX >= img.Width - 1) continue;

            for (var y = 0; y < newImage.Height - 1; y++)
            {
                double Y = (int)(y / sY);

                var baseY = Math.Floor(Y);

                if (baseY >= img.Height - 1) continue;

                var rX = X - baseX;
                var rY = Y - baseY;

                var irX = 1 - rX;
                var irY = 1 - rY;

                var px1 = new Bgr
                {
                    Blue = img.Data[(int)baseY, (int)baseX, 0] * irX +
                           img.Data[(int)baseY, (int)(baseX + 1), 0] * rX,
                    Green = img.Data[(int)baseY, (int)baseX, 1] * irX +
                            img.Data[(int)baseY, (int)(baseX + 1), 1] * rX,
                    Red = img.Data[(int)baseY, (int)baseX, 2] * irX +
                          img.Data[(int)baseY, (int)(baseX + 1), 2] * rX
                };

                var px2 = new Bgr
                {
                    Blue = img.Data[(int)(baseY + 1), (int)baseX, 0] * irX +
                           img.Data[(int)(baseY + 1), (int)(baseX + 1), 0] * rX,
                    Green = img.Data[(int)(baseY + 1), (int)baseX, 1] * irX +
                            img.Data[(int)(baseY + 1), (int)(baseX + 1), 1] * rX,
                    Red = img.Data[(int)(baseY + 1), (int)baseX, 2] * irX +
                          img.Data[(int)(baseY + 1), (int)(baseX + 1), 2] * rX
                };

                var px = new Bgr
                {
                    Blue = px1.Blue * irY + px2.Blue * rY,
                    Green = px1.Green * irY + px2.Green * rY,
                    Red = px1.Red * irY + px2.Red * rY
                };

                newImage[y, x] = px;
            }
        }

        return newImage;
    }
    public static Image<Bgr, byte> RotateImage(Image<Bgr, byte> img, double angle)
    {
        var centerX = img.Width / 2.0;
        var centerY = img.Height / 2.0;
        var radians = angle * Math.PI / 180.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);

        var newImage = new Image<Bgr, byte>(img.Width, img.Height);

        for (var y = 0; y < img.Height; y++)
        {
            for (var x = 0; x < img.Width; x++)
            {
                var newX = (x - centerX) * cos - (y - centerY) * sin + centerX;
                var newY = (x - centerX) * sin + (y - centerY) * cos + centerY;

                if (!(newX >= 0) || !(newX < newImage.Width) || !(newY >= 0) || !(newY < newImage.Height)) continue;

                var b = img.Data[y, x, 0];
                var g = img.Data[y, x, 1];
                var r = img.Data[y, x, 2];
                newImage.Data[(int)newY, (int)newX, 0] = b;
                newImage.Data[(int)newY, (int)newX, 1] = g;
                newImage.Data[(int)newY, (int)newX, 2] = r;
            }
        }
        return newImage;
    }
    
    public static Image<Bgr, byte> BinRotateImage(Image<Bgr, byte> img, double angle)
    {
        var centerX = img.Width / 2.0;
        var centerY = img.Height / 2.0;
        var radians = angle * Math.PI / 180.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);

        var newImage = new Image<Bgr, byte>(img.Width, img.Height);

        for (var y = 0; y < newImage.Height; y++)
        {
            for (var x = 0; x < newImage.Width; x++)
            {
                var newX = (x - centerX) * cos - (y - centerY) * sin + centerX;
                var newY = (x - centerX) * sin + (y - centerY) * cos + centerY;

                if (!(newX >= 0) || !(newX < img.Width - 1) || !(newY >= 0) || !(newY < img.Height - 1)) continue;

                var x1 = (int)Math.Floor(newX);
                var y1 = (int)Math.Floor(newY);
                var x2 = x1 + 1;
                var y2 = y1 + 1;

                var dx = newX - x1;
                var dy = newY - y1;

                var b1 = img.Data[y1, x1, 0];
                var g1 = img.Data[y1, x1, 1];
                var r1 = img.Data[y1, x1, 2];

                var b2 = img.Data[y1, x2, 0];
                var g2 = img.Data[y1, x2, 1];
                var r2 = img.Data[y1, x2, 2];

                var b3 = img.Data[y2, x1, 0];
                var g3 = img.Data[y2, x1, 1];
                var r3 = img.Data[y2, x1, 2];

                var b4 = img.Data[y2, x2, 0];
                var g4 = img.Data[y2, x2, 1];
                var r4 = img.Data[y2, x2, 2];

                var b = (byte)((1 - dx) * (1 - dy) * b1 + dx * (1 - dy) * b2 + (1 - dx) * dy * b3 + dx * dy * b4);
                var g = (byte)((1 - dx) * (1 - dy) * g1 + dx * (1 - dy) * g2 + (1 - dx) * dy * g3 + dx * dy * g4);
                var r = (byte)((1 - dx) * (1 - dy) * r1 + dx * (1 - dy) * r2 + (1 - dx) * dy * r3 + dx * dy * r4);

                newImage.Data[y, x, 0] = b;
                newImage.Data[y, x, 1] = g;
                newImage.Data[y, x, 2] = r;
            }
        }

        return newImage;
    }


}