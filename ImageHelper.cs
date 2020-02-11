using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace TesseractDemo
{
    public class ImageHelper
    {

        public Bitmap Crop(Bitmap bmp, Point p1,Point p2)
        {

            Point location = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Size size = new Size(Math.Abs(p1.X - p2.X) + 1, Math.Abs(p1.Y - p2.Y) + 1);

            Rectangle rectangle = new Rectangle(location, size);

            // create filter
            Crop filter = new Crop(rectangle);
            // apply the filter
            Bitmap newImage = filter.Apply(bmp);

            return newImage;
        }

        public Bitmap Resize(Bitmap bmp, int newWidth, int newHeight)
        {

            // create filter
            ResizeNearestNeighbor filter = new ResizeNearestNeighbor(newWidth, newHeight);
            // apply the filter
            Bitmap newImage = filter.Apply(bmp);

            return newImage;
        }

        //SetGrayscale
        public Bitmap SetGrayscale(Bitmap img)
        {

            // create grayscale filter (BT709)
            Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
            // apply the filter
            Bitmap grayImage = filter.Apply(img);

            return grayImage;
        }


        public Bitmap GaussianBlur(Bitmap bSource)
        {
            GaussianBlur gaussianBlur = new GaussianBlur(4, 11);//11

            Bitmap bTemp = gaussianBlur.Apply(bSource);

            return bTemp;
        }

        public Bitmap SetToBW(Bitmap bmp, int ithreshold)
        {
            // create filter
            Threshold filter = new Threshold(ithreshold);
            // apply the filter
            filter.ApplyInPlace(bmp);
            return bmp;
        }

        public Bitmap Invert(Bitmap img)
        {
            Int32 th = 150;//150
            Int32 countBlack = 0;
            Int32 countWhite = 0;

            //轉成24bppRgb (for Set Pixel
            Bitmap bmp = img.Clone(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb); 

            for (var x = 0; x < bmp.Width; x++)
            {
                for (var y = 0; y < bmp.Height; y++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    if (pixel.R < th && pixel.G < th && pixel.B < th)
                    {
                        bmp.SetPixel(x, y, Color.Black);
                        countBlack++;
                    }
                    else
                    {
                        bmp.SetPixel(x, y, Color.White);
                        countWhite++;
                    }
                }
            }

            if (countBlack > countWhite)
            {
                //reverse
                for (var x = 0; x < bmp.Width; x++)
                {
                    for (var y = 0; y < bmp.Height; y++)
                    {
                        var pixel = bmp.GetPixel(x, y);
                        if (pixel.R < 220 && pixel.G < 220 && pixel.B < 220)
                        {
                            bmp.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            bmp.SetPixel(x, y, Color.Black);
                        }
                    }
                }
            }

            //轉回成8bppIndex
            Bitmap bmp2 = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            return bmp2;
        }

    }
}
