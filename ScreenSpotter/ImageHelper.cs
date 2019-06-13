using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NLog;

namespace ScreenSpotter
{
    class ImageHelper
    {

        public static Image cropImage(Image img, int[] rectCoor)
        {
            Rectangle cropArea = new Rectangle(rectCoor[0], rectCoor[1], rectCoor[2], rectCoor[3]);
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        public static Tuple<int, List<Rectangle>> ImageProcessing(Image img, Image imgSource)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            int[] quad = new int[91];
            int carCount = 0;

            Bitmap bmp = new Bitmap(img);
            Bitmap bmpSource = new Bitmap(imgSource);
            List<Rectangle> listOfFoundRect = new List<Rectangle>();
            
            int best = 0, bestTemp = 0, rsum = 0;

            int squareCount = 0, carOn = 0;
            for (int a = 1; a < 508; a = a + 39)
            {
                for (int b = 1; b < 274; b = b + 39)
                {
                    if ((-278 * a + 133 * b + 2224 <= 0) && (-260 * a + 450 * b + 20800 >= 0) ||
                    (-278 * (a + 38) + 133 * b + 2224 <= 0) && (-260 * (a + 38) + 450 * b + 20800 >= 0) ||
                    (-278 * a + 133 * (b + 38) + 2224 <= 0) && (-260 * a + 450 * (b + 38) + 20800 >= 0) ||
                    (-278 * (a + 38) + 133 * (b + 38) + 2224 <= 0) && (-260 * (a + 38) + 450 * (b + 38) + 20800 >= 0))
                    {
                        int squareDist = 0;
                        best = 0;
                        for (int i = a; i < a + 45; i = i + 9)
                        {
                            for (int j = b; j < b + 45; j = j + 9)
                            {
                                for (int g = i; g < i + 9; g = g + 3)
                                {
                                    for (int f = j; f < j + 9; f = f + 3)
                                    {
                                        int xbest = 0, ybest = 0;

                                        bestTemp = Math.Abs(bmpSource.GetPixel(i + 3, j + 3).R - bmp.GetPixel(g, f).R) +
                                                        Math.Abs(bmpSource.GetPixel(i + 3, j + 3).G - bmp.GetPixel(g, f).G) +
                                                        Math.Abs(bmpSource.GetPixel(i + 3, j + 3).B - bmp.GetPixel(g, f).B);
                                        if ((bestTemp <= best) || ((g == i) && (f == j)))
                                        {
                                            xbest = g;
                                            ybest = f;

                                            best = bestTemp;
                                            rsum = rsum + best;
                                        }
                                        
                                    }
                                }
                                squareDist = squareDist + best;
                            }
                        }
                        if (squareDist >= 1300)
                        {
                            Rectangle rect = new Rectangle(a, b, 39, 39); 
                            listOfFoundRect.Add(rect);
                            carOn++;
                            logger.Trace("Машина найдена в квадрате с координатами: x = " + a.ToString() + ", y = " + b.ToString());

                        }
                        quad[squareCount] = squareDist;
                        

                        squareCount++;
                    }
                }
            }


            if (carOn >= 1)
            {
                carCount++;
                logger.Trace("Машина замечена в " + carOn.ToString() + " квадратах");
            }

            return Tuple.Create(carCount, listOfFoundRect);


        }
        
    }
}
