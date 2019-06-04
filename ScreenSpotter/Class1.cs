using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ScreenSpotter
{
    class Class1
    {
        public void ImageProcessing(Image img)
        {
            for(int i = 1; i <= img.Width; i++)
            {

            }
        }

        public static Image cropImage(Image img, int[] rectCoor)
        {
            
            Rectangle cropArea = new Rectangle(rectCoor[0], rectCoor[1], rectCoor[2], rectCoor[3]);
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }
    }

    struct Pixels
    {
        public int x;
        public int y;
        public int r;

    }
}
