using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Aruco;

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
    
    /*Вывод значений одного из трёх цветовых каналов по выбору пользователя.
    Вывод чёрно-белой верспп изображенпя. 
    Вывод Sepia версии изображения.
    Вывод изображения с возможностью изменен11я его яркости и контраста. 
    Вывод результатов логических операций 
        «дополнение», 
        «исключение» и «пересечение»,
        с возможностью выбора изображения для соответствующей операции. 
    Вывод изображения преобразованного в формат HSV, с возможностью изменения значений HSV. 
    Вывод изображений с применештем к ним медианного размытия. 
    Вывод изображений с применением к ним оконного фильтра,
        с возможностью  изменения матрицы фильтра из формы приложения. 
    Вывод изображений с применением к ним «акварельного фильтра»,
        а так же, возможностью выбора яркости,
        контраста и параметров смешивания изображений.
    Вывод изображений с применением к ним «cartoon filter» и 
        возможностью изменения порога преобразования изображения.*/

    public enum Filters
    {
        RGB,
        BW,
        Sepia,
        BC,
        Math,
        HSV,
        Win,
        Aqua,
        Caartoon
    }
}